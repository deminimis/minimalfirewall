// File: BackgroundFirewallTaskService.cs
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

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

        public event Action<int>? QueueCountChanged;

        public BackgroundFirewallTaskService(FirewallActionsService actionsService, UserActivityLogger activityLogger, WildcardRuleService wildcardRuleService)
        {
            _actionsService = actionsService;
            _activityLogger = activityLogger;
            _wildcardRuleService = wildcardRuleService;
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
                try
                {
                    await Task.Run(() =>
                    {
                        switch (task.TaskType)
                        {
                            case FirewallTaskType.ApplyApplicationRule:
                                if (task.Payload is ApplyApplicationRulePayload p1) _actionsService.ApplyApplicationRuleChange(p1.AppPaths, p1.Action, p1.WildcardSourcePath);
                                break;
                            case FirewallTaskType.ApplyServiceRule:
                                if (task.Payload is ApplyServiceRulePayload p2) _actionsService.ApplyServiceRuleChange(p2.ServiceName, p2.Action);
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
                                if (task.Payload is string p7) _actionsService.DeleteGroupAsync(p7).Wait();
                                break;
                            case FirewallTaskType.DeleteWildcardRules:
                                if (task.Payload is DeleteWildcardRulePayload p8) _actionsService.DeleteRulesForWildcard(p8.Wildcard);
                                break;
                            case FirewallTaskType.ProcessPendingConnection:
                                if (task.Payload is ProcessPendingConnectionPayload p9) _actionsService.ProcessPendingConnection(p9.PendingConnection, p9.Decision, p9.Duration, p9.TrustPublisher);
                                break;
                            case FirewallTaskType.AcceptForeignRule:
                                if (task.Payload is ForeignRuleChangePayload p10) _actionsService.AcceptForeignRule(p10.Change);
                                break;
                            case FirewallTaskType.AcknowledgeForeignRule:
                                if (task.Payload is ForeignRuleChangePayload p11) _actionsService.AcknowledgeForeignRule(p11.Change);
                                break;
                            case FirewallTaskType.DeleteForeignRule:
                                if (task.Payload is ForeignRuleChangePayload p12) _actionsService.DeleteForeignRule(p12.Change);
                                break;
                            case FirewallTaskType.AcceptAllForeignRules:
                                if (task.Payload is AllForeignRuleChangesPayload p13) _actionsService.AcceptAllForeignRules(p13.Changes);
                                break;
                            case FirewallTaskType.AcknowledgeAllForeignRules:
                                if (task.Payload is AllForeignRuleChangesPayload p14) _actionsService.AcknowledgeAllForeignRules(p14.Changes);
                                break;
                            case FirewallTaskType.CreateAdvancedRule:
                                if (task.Payload is CreateAdvancedRulePayload p15) _actionsService.CreateAdvancedRule(p15.ViewModel, p15.InterfaceTypes, p15.IcmpTypesAndCodes);
                                break;
                            case FirewallTaskType.AddWildcardRule:
                                if (task.Payload is WildcardRule p16) _wildcardRuleService.AddRule(p16);
                                break;
                        }
                    }, _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _activityLogger.LogException($"BackgroundTask-{task.TaskType}", ex);
                }
                finally
                {
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
