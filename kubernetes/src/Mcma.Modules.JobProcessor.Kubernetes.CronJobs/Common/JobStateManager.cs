using System.Collections.Concurrent;
using Mcma.Modules.JobProcessor.Kubernetes.Common;
using Quartz;

namespace Mcma.Modules.JobProcessor.Kubernetes.CronJobs.Common
{
    internal class JobStateManager : IJobStateManager
    {
        private ConcurrentDictionary<JobKey, bool> States { get; } = new();

        public bool IsJobEnabled(JobKey jobKey) => States.GetOrAdd(jobKey, _ => true);

        public void SetJobEnabled(SetCronJobEnabledRequest request) =>
            States.AddOrUpdate(JobKey.Create(request.CronJobType.ToString()), request.Enabled, (_, _) => request.Enabled);
    }
}