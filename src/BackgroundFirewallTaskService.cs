using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MinimalFirewall.TypedObjects;

namespace MinimalFirewall
{
    // Helper Definitions 
    public readonly struct TaskResult
    {
        public bool RequiresCacheInvalidation { get; }
        public bool RequiresWildcardRefresh { get; }

        public TaskResult(bool cache, bool wildcard)
        {
            RequiresCacheInvalidation = cache;
            RequiresWildcardRefresh = wildcard;
        }

        public static TaskResult None => new(false, false);
        public static TaskResult CacheOnly => new(true, false);
        public static TaskResult WildcardOnly => new(false, true);
    }

    public interface IFirewallTaskHandler
    {
        Task<TaskResult> HandleAsync(object payload);
    }

    // The Main Service Class
    public class BackgroundFirewallTaskService : IDisposable
    {
        private readonly BlockingCollection<FirewallTask> _taskQueue = new();
        private readonly Task _worker;
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        // Dependencies
        private readonly FirewallActionsService _actionsService;
        private readonly UserActivityLogger _activityLogger;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly FirewallDataService _dataService;

        private readonly Dictionary<FirewallTaskType, IFirewallTaskHandler> _handlers;

        public event Action<int>? QueueCountChanged;
        public event Action<string>? StatusChanged;
        public event Action? WildcardRulesChanged;

        public BackgroundFirewallTaskService(
            FirewallActionsService actionsService,
            UserActivityLogger activityLogger,
            WildcardRuleService wildcardRuleService,
            FirewallDataService dataService)
        {
            _actionsService = actionsService;
            _activityLogger = activityLogger;
            _wildcardRuleService = wildcardRuleService;
            _dataService = dataService;

            _handlers = RegisterHandlers();

            _worker = Task.Run(ProcessQueueAsync, _cancellationTokenSource.Token);
        }

        public void EnqueueTask(FirewallTask task)
        {
            if (!_taskQueue.IsAddingCompleted)
            {
                _taskQueue.Add(task);
                SafeInvoke(QueueCountChanged, _taskQueue.Count);
            }
        }

        private async Task ProcessQueueAsync()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    FirewallTask? task = null;

                    // 1. Smart Waiting Logic:
                    // If queue is empty, we wait UP TO 1500ms for a new item.
                    // If an item arrives, TryTake returns true immediately (no lag).
                    // If 1500ms passes, it returns false, and we update status to "Ready".
                    if (_taskQueue.Count == 0)
                    {
                        if (!_taskQueue.TryTake(out task, 1500, _cancellationTokenSource.Token))
                        {
                            // Timed out => No tasks came in for 1.5s. Safe to set Ready.
                            SafeInvoke(StatusChanged, "Ready");

                            // Now block indefinitely until work actually arrives
                            task = _taskQueue.Take(_cancellationTokenSource.Token);
                        }
                    }
                    else
                    {
                        // Queue has items, grab the next one immediately
                        task = _taskQueue.Take(_cancellationTokenSource.Token);
                    }

                    // 2. Process the task
                    if (task != null)
                    {
                        await RunTaskLogicAsync(task);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
            }
        }

        private async Task RunTaskLogicAsync(FirewallTask task)
        {
            SafeInvoke(StatusChanged, task.Description);
            TaskResult result = TaskResult.None;

            try
            {
                if (_handlers.TryGetValue(task.TaskType, out var handler))
                {
                    result = await handler.HandleAsync(task.Payload);
                }
                else
                {
                    _activityLogger.LogDebug($"[Warning] No handler registered for task type: {task.TaskType}");
                }
            }
            catch (OperationCanceledException)
            {
                throw; // Allow main loop to catch this
            }
            catch (COMException comEx)
            {
                _activityLogger.LogException($"BackgroundTask-{task.TaskType}-COM", comEx);
            }
            catch (InvalidComObjectException invComEx)
            {
                _activityLogger.LogException($"BackgroundTask-{task.TaskType}-InvalidCOM", invComEx);
            }
            catch (Exception ex)
            {
                _activityLogger.LogException($"BackgroundTask-{task.TaskType}", ex);
            }
            finally
            {
                if (result.RequiresCacheInvalidation)
                {
                    _dataService.InvalidateRuleCache();
                    _activityLogger.LogDebug($"[Cache] Invalidated MFW Rules cache after task: {task.TaskType}");
                }

                if (result.RequiresWildcardRefresh)
                {
                    SafeInvoke(WildcardRulesChanged);
                }

                SafeInvoke(QueueCountChanged, _taskQueue.Count);
            }
        }

        // Helper to prevent subscriber errors (UI crashes) from killing the service
        private void SafeInvoke(Action? action)
        {
            try { action?.Invoke(); } catch { /* Ignore UI update errors */ }
        }

        private void SafeInvoke<T>(Action<T>? action, T param)
        {
            try { action?.Invoke(param); } catch { /* Ignore UI update errors */ }
        }

        // Map the TaskType enum 
        private Dictionary<FirewallTaskType, IFirewallTaskHandler> RegisterHandlers()
        {
            var map = new Dictionary<FirewallTaskType, IFirewallTaskHandler>();
            // Standard Rule Handlers 
            map[FirewallTaskType.ApplyApplicationRule] = new ActionHandler<ApplyApplicationRulePayload>(
                p => _actionsService.ApplyApplicationRuleChange(p.AppPaths, p.Action, p.WildcardSourcePath), TaskResult.CacheOnly);
            map[FirewallTaskType.ApplyServiceRule] = new ActionHandler<ApplyServiceRulePayload>(
                p => _actionsService.ApplyServiceRuleChange(p.ServiceName, p.Action, p.AppPath), TaskResult.CacheOnly);
            map[FirewallTaskType.ApplyUwpRule] = new ActionHandler<ApplyUwpRulePayload>(
                p => _actionsService.ApplyUwpRuleChange(p.UwpApps, p.Action), TaskResult.CacheOnly);
            map[FirewallTaskType.CreateAdvancedRule] = new ActionHandler<CreateAdvancedRulePayload>(
                p => _actionsService.CreateAdvancedRule(p.ViewModel, p.InterfaceTypes, p.IcmpTypesAndCodes), TaskResult.CacheOnly);
            map[FirewallTaskType.DeleteApplicationRules] = new ActionHandler<DeleteRulesPayload>(
                p => _actionsService.DeleteApplicationRules(p.RuleIdentifiers), TaskResult.CacheOnly);
            map[FirewallTaskType.DeleteUwpRules] = new ActionHandler<DeleteRulesPayload>(
                p => _actionsService.DeleteUwpRules(p.RuleIdentifiers), TaskResult.CacheOnly);
            map[FirewallTaskType.DeleteAdvancedRules] = new ActionHandler<DeleteRulesPayload>(
                p => _actionsService.DeleteAdvancedRules(p.RuleIdentifiers), TaskResult.CacheOnly);
            map[FirewallTaskType.ProcessPendingConnection] = new ActionHandler<ProcessPendingConnectionPayload>(
               p => _actionsService.ProcessPendingConnection(p.PendingConnection, p.Decision, p.Duration, p.TrustPublisher), TaskResult.CacheOnly);
            map[FirewallTaskType.DeleteForeignRule] = new ActionHandler<ForeignRuleChangePayload>(
                p => _actionsService.DeleteForeignRule(p.Change), TaskResult.CacheOnly);

            map[FirewallTaskType.DisableForeignRule] = new ActionHandler<ForeignRuleChangePayload>(
                p => _actionsService.DisableForeignRule(p.Change, p.Acknowledge), TaskResult.CacheOnly);

            map[FirewallTaskType.EnableForeignRule] = new ActionHandler<ForeignRuleChangePayload>(
                p => _actionsService.EnableForeignRule(p.Change, p.Acknowledge), TaskResult.CacheOnly);

            map[FirewallTaskType.QuarantineForeignRule] = new ActionHandler<ForeignRuleChangePayload>(
               p => _actionsService.QuarantineForeignRule(p.Change), TaskResult.CacheOnly);
            map[FirewallTaskType.SetGroupEnabledState] = new ActionHandler<SetGroupEnabledStatePayload>(
               p => _actionsService.SetGroupEnabledState(p.GroupName, p.IsEnabled), TaskResult.CacheOnly);
            map[FirewallTaskType.DeleteGroup] = new AsyncActionHandler<string>(
                async p => await _actionsService.DeleteGroupAsync(p), TaskResult.CacheOnly);
            map[FirewallTaskType.AcceptForeignRule] = new ActionHandler<ForeignRuleChangePayload>(
                p => _actionsService.AcceptForeignRule(p.Change), TaskResult.None);
            map[FirewallTaskType.AcceptAllForeignRules] = new ActionHandler<AllForeignRuleChangesPayload>(
                p => _actionsService.AcceptAllForeignRules(p.Changes), TaskResult.None);
            map[FirewallTaskType.AddWildcardRule] = new ActionHandler<WildcardRule>(
                p => _wildcardRuleService.AddRule(p), TaskResult.WildcardOnly);
            map[FirewallTaskType.UpdateWildcardRule] = new ActionHandler<UpdateWildcardRulePayload>(
                p => _actionsService.UpdateWildcardRule(p.OldRule, p.NewRule), new TaskResult(true, true));
            map[FirewallTaskType.RemoveWildcardRule] = new ActionHandler<DeleteWildcardRulePayload>(
                p => _actionsService.RemoveWildcardRule(p.Wildcard), new TaskResult(true, true));
            map[FirewallTaskType.DeleteWildcardRules] = new ActionHandler<DeleteWildcardRulePayload>(
                p => _actionsService.DeleteRulesForWildcard(p.Wildcard), TaskResult.CacheOnly);
            map[FirewallTaskType.RemoveWildcardDefinitionOnly] = new ActionHandler<DeleteWildcardRulePayload>(
                p => _actionsService.RemoveWildcardDefinitionOnly(p.Wildcard), TaskResult.WildcardOnly);
            map[FirewallTaskType.DeleteAllMfwRules] = new ActionHandler<object>(
                _ => _actionsService.DeleteAllMfwRules(), TaskResult.CacheOnly);
            map[FirewallTaskType.ImportRules] = new AsyncActionHandler<ImportRulesPayload>(
                async p => await _actionsService.ImportRulesAsync(p.JsonContent, p.Replace), TaskResult.CacheOnly);
            return map;
        }

        public void Dispose()
        {
            _taskQueue.CompleteAdding();
            _cancellationTokenSource.Cancel();
            try
            {
                _worker.Wait(2000);
            }
            catch (AggregateException) { /* Expected */ }
            catch (Exception) { /* Expected */ }

            _cancellationTokenSource.Dispose();
            _taskQueue.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    // Helper Classes
    public class ActionHandler<T> : IFirewallTaskHandler
    {
        private readonly Action<T> _action;
        private readonly TaskResult _result;

        public ActionHandler(Action<T> action, TaskResult result)
        {
            _action = action;
            _result = result;
        }

        public Task<TaskResult> HandleAsync(object payload)
        {
            // Simplified pattern matching works for both specific types and generic objects
            if (payload is T typedPayload)
            {
                _action(typedPayload);
                return Task.FromResult(_result);
            }

            return Task.FromResult(TaskResult.None);
        }
    }

    public class AsyncActionHandler<T> : IFirewallTaskHandler
    {
        private readonly Func<T, Task> _func;
        private readonly TaskResult _result;

        public AsyncActionHandler(Func<T, Task> func, TaskResult result)
        {
            _func = func;
            _result = result;
        }

        public async Task<TaskResult> HandleAsync(object payload)
        {
            if (payload is T typedPayload)
            {
                await _func(typedPayload);
                return _result;
            }
            return TaskResult.None;
        }
    }
}