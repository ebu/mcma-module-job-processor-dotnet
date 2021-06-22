using Mcma.Utility;

namespace Mcma.Modules.JobProcessor.Kubernetes.Common
{
    public static class KafkaCronJobEnvironmentVariables
    {
        public static readonly string CronJobStateTopic = McmaEnvironmentVariables.Get("KAFKA_CRON_JOB_STATE_TOPIC", false);

        public static readonly string CronJobExecutionTopicPrefix =
            McmaEnvironmentVariables.Get("KAFKA_CRON_JOB_EXECUTION_TOPIC_PREFIX", false) ?? "JobProcessor.CronJobs.Execution";
    }
}