using Mcma.Modules.JobProcessor.Kubernetes.Common;
using Quartz;

namespace Mcma.Modules.JobProcessor.Kubernetes.CronJobs.Common
{
    internal interface IJobStateManager
    {
        bool IsJobEnabled(JobKey jobKey);

        void SetJobEnabled(SetCronJobEnabledRequest request);
    }
}