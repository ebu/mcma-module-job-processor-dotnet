using System;
using System.Threading.Tasks;
using Mcma.Kafka;
using Mcma.Modules.JobProcessor.PeriodicJobChecker;

namespace Mcma.Modules.JobProcessor.Kubernetes.PeriodicJobChecker
{
    public class JobCheckerMessageProcessor : IKafkaConsumerMessageProcessor
    {
        public JobCheckerMessageProcessor(IJobChecker jobChecker)
            => JobChecker = jobChecker ?? throw new ArgumentNullException(nameof(jobChecker));

        private IJobChecker JobChecker { get; }

        public Task ProcessAsync(string requestId, string message) => JobChecker.CheckJobsAsync(requestId);
    }
}