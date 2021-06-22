namespace Mcma.Modules.JobProcessor.Kubernetes.Common
{
    public class SetCronJobEnabledRequest
    {
        public CronJobType CronJobType { get; set; }
        
        public bool Enabled { get; set; }
    }
}