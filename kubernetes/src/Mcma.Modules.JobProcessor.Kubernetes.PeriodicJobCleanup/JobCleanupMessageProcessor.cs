using System;
using System.Threading.Tasks;
using Mcma.Kafka;
using Mcma.Modules.JobProcessor.PeriodicJobCleanup;

namespace Mcma.Modules.JobProcessor.Kubernetes.PeriodicJobCleanup
{
    public class JobCleanupMessageProcessor : IKafkaConsumerMessageProcessor
    {
        public JobCleanupMessageProcessor(IJobCleaner jobCleaner)
            => JobCleaner = jobCleaner ?? throw new ArgumentNullException(nameof(jobCleaner));

        private IJobCleaner JobCleaner { get; }

        public Task ProcessAsync(string requestId, string message) => JobCleaner.CleanupJobsAsync(requestId);
    }
}