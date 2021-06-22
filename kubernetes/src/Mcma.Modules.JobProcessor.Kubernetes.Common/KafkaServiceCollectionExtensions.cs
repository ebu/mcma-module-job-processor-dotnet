using System;
using Confluent.Kafka;
using Mcma.Kafka;
using Mcma.Modules.JobProcessor.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Modules.JobProcessor.Kubernetes.Common
{
    public static class KafkaServiceCollectionExtensions
    {
        public static IServiceCollection AddKafkaJobCheckerTrigger(this IServiceCollection services)
            => services.AddSingletonKafkaProducer<string, string>()
                       .AddSingleton<IJobCheckerTrigger, KafkaJobCheckerTrigger>();

        public static IServiceCollection AddKafkaCronJobExecutionService<T>(this IServiceCollection services,
                                                                            CronJobType jobType,
                                                                            Action<ConsumerConfig> configureConsumer = null)
            where T : class, IKafkaConsumerMessageProcessor
            => services.AddKafkaConsumerService<T>(KafkaCronJobEnvironmentVariables.CronJobStateTopic, configureConsumer);
    }
}