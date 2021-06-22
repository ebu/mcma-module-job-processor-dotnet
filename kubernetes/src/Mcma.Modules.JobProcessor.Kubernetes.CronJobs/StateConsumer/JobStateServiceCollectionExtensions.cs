using Mcma.Kafka;
using Mcma.Modules.JobProcessor.Kubernetes.Common;
using Mcma.Modules.JobProcessor.Kubernetes.CronJobs.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Modules.JobProcessor.Kubernetes.CronJobs.StateConsumer
{
    public static class JobStateServiceCollectionExtensions
    {
        public static IServiceCollection AddCronJobStateConsumer(this IServiceCollection services)
            => services.AddSingleton<IJobStateManager, JobStateManager>()
                       .AddKafkaConsumerService<JobStateUpdater>(topic: KafkaCronJobEnvironmentVariables.CronJobStateTopic);
    }
}