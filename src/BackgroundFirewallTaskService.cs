using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using MinimalFirewall.TypedObjects;

namespace MinimalFirewall
{
    public class BackgroundFirewallTaskService : IDisposable
    {
        private readonly BlockingCollection<FirewallTask> _taskQueue = new BlockingCollection<FirewallTask>();
        private readonly Task _worker;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly FirewallActionsService _actionsService;
        private readonly UserActivityLogger _activityLogger;
        private readonly WildcardRuleService _wildcardRuleService;
        private readonly FirewallDataService _dataService;

        public event Action<int>? QueueCountChanged;

        public BackgroundFirewallTaskService(FirewallActionsService actionsService, UserActivityLogger activityLogger, WildcardRuleService wildcardRuleService, FirewallDataService dataService)
        {
            _actionsService = actionsService;
            _activityLogger = activityLogger;
            _wildcardRuleService = wildcardRuleService;
            _dataService = dataService;
            _worker = Task.Run(ProcessQueueAsync, _cancellationTokenSource.Token);
        }

        public void EnqueueTask(FirewallTask task)
        {
            if (!_taskQueue.IsAddingCompleted)
            {
                _taskQueue.Add(task);
                QueueCountChanged?.Invoke(_taskQueue.Count);
            }
        }

        private async Task ProcessQueueAsync()
        {
            foreach (var task in _taskQueue.GetConsumingEnumerable(_cancellationTokenSource.Token))
            {
                bool requiresCacheInvalidation = true;

                try
                {
                    await Task.Run(async () =>
                    {
                        switch (task.TaskType)
                        {
                            case FirewallTaskType.ApplyApplicationRule:
                                if (task.Payload is ApplyApplicationRulePayload p1) _actionsService.ApplyApplicationRuleChange(p1.AppPaths, p1.Action, p1.WildcardSourcePath);
                                break;
                            case FirewallTaskType.ApplyServiceRule:
                                if (task.Payload is ApplyServiceRulePayload p2) _actionsService.ApplyServiceRuleChange(p2.ServiceName, p2.Action, p2.AppPath);
                                break;
                            case FirewallTaskType.ApplyUwpRule:
                                if (task.Payload is ApplyUwpRulePayload p3) _actionsService.ApplyUwpRuleChange(p3.UwpApps, p3.Action);
                                break;
                            case FirewallTaskType.DeleteApplicationRules:
                                if (task.Payload is DeleteRulesPayload p4) _actionsService.DeleteApplicationRules(p4.RuleIdentifiers);
                                break;
                            case FirewallTaskType.DeleteUwpRules:
                                if (task.Payload is DeleteRulesPayload p5) _actionsService.DeleteUwpRules(p5.RuleIdentifiers);
                                break;
                            case FirewallTaskType.DeleteAdvancedRules:
                                if (task.Payload is DeleteRulesPayload p6) _actionsService.DeleteAdvancedRules(p6.RuleIdentifiers);
                                break;
                            case FirewallTaskType.DeleteGroup:
                                if (task.Payload is string p7) await _actionsService.DeleteGroupAsync(p7);
                                break;
                            case FirewallTaskType.DeleteWildcardRules:
                                if (task.Payload is DeleteWildcardRulePayload p8) _actionsService.DeleteRulesForWildcard(p8.Wildcard);
                                break;
                            case FirewallTaskType.ProcessPendingConnection:
                                if (task.Payload is ProcessPendingConnectionPayload p9) _actionsService.ProcessPendingConnection(p9.PendingConnection, p9.Decision, p9.Duration, p9.TrustPublisher);

                                break;
                            case FirewallTaskType.AcceptForeignRule:
                                if (task.Payload is ForeignRuleChangePayload p10) _actionsService.AcceptForeignRule(p10.Change);
                                requiresCacheInvalidation = false;
                                break;
                            case FirewallTaskType.DeleteForeignRule:
                                if (task.Payload is ForeignRuleChangePayload p12) _actionsService.DeleteForeignRule(p12.Change);
                                break;
                            case FirewallTaskType.AcceptAllForeignRules:
                                if (task.Payload is AllForeignRuleChangesPayload p13) _actionsService.AcceptAllForeignRules(p13.Changes);
                                requiresCacheInvalidation = false;
                                break;
                            case FirewallTaskType.CreateAdvancedRule:
                                if (task.Payload is CreateAdvancedRulePayload p15) _actionsService.CreateAdvancedRule(p15.ViewModel, p15.InterfaceTypes, p15.IcmpTypesAndCodes);
                                break;
                            case FirewallTaskType.AddWildcardRule:
                                if (task.Payload is WildcardRule p16) _wildcardRuleService.AddRule(p16);

                                requiresCacheInvalidation = false;
                                break;
                            case FirewallTaskType.SetGroupEnabledState:
                                if (task.Payload is SetGroupEnabledStatePayload p17) _actionsService.SetGroupEnabledState(p17.GroupName, p17.IsEnabled);
                                break;
                            case FirewallTaskType.UpdateWildcardRule:
                                if (task.Payload is UpdateWildcardRulePayload p18) _actionsService.UpdateWildcardRule(p18.OldRule, p18.NewRule);
                                break;
                            case FirewallTaskType.RemoveWildcardRule:
                                if (task.Payload is DeleteWildcardRulePayload p19) _actionsService.RemoveWildcardRule(p19.Wildcard);
                                break;
                            case FirewallTaskType.RemoveWildcardDefinitionOnly:
                                if (task.Payload is DeleteWildcardRulePayload p20) _actionsService.RemoveWildcardDefinitionOnly(p20.Wildcard);
                                requiresCacheInvalidation = false;
                                break;
                            case FirewallTaskType.DeleteAllMfwRules:
                                _actionsService.DeleteAllMfwRules();
                                break;
                            case FirewallTaskType.ImportRules:
                                if (task.Payload is ImportRulesPayload p21) await _actionsService.ImportRulesAsync(p21.JsonContent, p21.Replace);
                                break;
                            default:
                                requiresCacheInvalidation = false;
                                break;
                        }
                    }, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    requiresCacheInvalidation = false;
                    break;
                }
                catch (COMException comEx)
                {
                    requiresCacheInvalidation = false;
                    _activityLogger.LogException($"BackgroundTask-{task.TaskType}-COM", comEx);
                }
                catch (InvalidComObjectException invComEx)
                {
                    requiresCacheInvalidation = false;
                    _activityLogger.LogException($"BackgroundTask-{task.TaskType}-InvalidCOM", invComEx);
                }
                catch (Exception ex)
                {
                    requiresCacheInvalidation = false;
                    _activityLogger.LogException($"BackgroundTask-{task.TaskType}", ex);
                }
                finally
                {
                    if (requiresCacheInvalidation)
                    {
                        _dataService.InvalidateMfwRuleCache();
                        _activityLogger.LogDebug($"[Cache] Invalidated MFW Rules cache after task: {task.TaskType}");
                    }
                    QueueCountChanged?.Invoke(_taskQueue.Count);
                }
            }
        }

        public void Dispose()
        {
            _taskQueue.CompleteAdding();
            _cancellationTokenSource.Cancel();
            try
            {
                _worker.Wait(2000);
            }
            catch (OperationCanceledException) { }
            catch (AggregateException) { }

            _cancellationTokenSource.Dispose();
            _taskQueue.Dispose();
        }
    }
}
