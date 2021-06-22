namespace Mcma.Modules.JobProcessor.Kubernetes.Common
{
    public class KafkaCronJobOptions
    {
        public string CronJobStateTopic { get; set; } = KafkaCronJobEnvironmentVariables.CronJobStateTopic;

        public string CronJobExecutionTopicPrefix { get; set; } = KafkaCronJobEnvironmentVariables.CronJobExecutionTopicPrefix;

        public string GetJobExecutionTopic(CronJobType jobCronJobType) => $"{CronJobExecutionTopicPrefix}.{jobCronJobType}";
    }
}