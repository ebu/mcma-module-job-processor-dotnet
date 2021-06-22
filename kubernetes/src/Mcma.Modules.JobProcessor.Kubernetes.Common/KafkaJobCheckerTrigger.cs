using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Mcma.Modules.JobProcessor.Common;
using Mcma.Serialization;
using Microsoft.Extensions.Options;

namespace Mcma.Modules.JobProcessor.Kubernetes.Common
{
    public class KafkaJobCheckerTrigger : IJobCheckerTrigger
    {
        public KafkaJobCheckerTrigger(IProducer<string, string> producer, IOptions<KafkaCronJobOptions> options)
        {
            Producer = producer ?? throw new ArgumentNullException(nameof(producer));

            Options = options.Value ?? new KafkaCronJobOptions();
            if (string.IsNullOrWhiteSpace(Options.CronJobStateTopic))
                throw new McmaException("No Kafka topic specified for producing cron job state messages.");
        }
        
        private IProducer<string, string> Producer { get; }
        
        private KafkaCronJobOptions Options { get; }
        
        private Task ProduceAsync(bool enabled)
            => Producer.ProduceAsync(Options.CronJobStateTopic,
                                     new Message<string, string>
                                     {
                                         Key = Guid.NewGuid().ToString(),
                                         Value = new SetCronJobEnabledRequest { CronJobType = CronJobType.JobChecker, Enabled = enabled }.ToMcmaJson().ToString()
                                     });

        public Task EnableAsync() => ProduceAsync(true);

        public Task DisableAsync() => ProduceAsync(false);
    }
}