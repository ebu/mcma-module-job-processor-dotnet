using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Mcma.Modules.JobProcessor.Kubernetes.Common;
using Mcma.Modules.JobProcessor.Kubernetes.CronJobs.Common;
using Microsoft.Extensions.Options;
using Quartz;

namespace Mcma.Modules.JobProcessor.Kubernetes.CronJobs.ExecutionProducer
{
    internal class JobExecutionMessageProducer : IJob
    {
        public JobExecutionMessageProducer(IJobStateManager jobStateManager,
                                              IProducer<string, string> producer,
                                              IOptions<KafkaCronJobOptions> options)
        {
            JobStateManager = jobStateManager ?? throw new ArgumentNullException(nameof(jobStateManager));
            Producer = producer ?? throw new ArgumentNullException(nameof(producer));
            Options = options.Value ?? new KafkaCronJobOptions();
        }

        private IJobStateManager JobStateManager { get; }

        private IProducer<string, string> Producer { get; }

        private KafkaCronJobOptions Options { get; }

        public async Task Execute(IJobExecutionContext context)
        {
            if (JobStateManager.IsJobEnabled(context.JobDetail.Key) && Enum.TryParse<CronJobType>(context.JobDetail.Key.Name, true, out var jobType))
                await Producer.ProduceAsync(Options.GetJobExecutionTopic(jobType),
                                            new Message<string, string> { Key = context.FireInstanceId, Value = string.Empty });
        }
    }
}