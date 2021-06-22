using System;
using System.Threading.Tasks;
using Mcma.Kafka;
using Mcma.Modules.JobProcessor.Kubernetes.Common;
using Mcma.Modules.JobProcessor.Kubernetes.CronJobs.Common;
using Mcma.Serialization;
using Newtonsoft.Json.Linq;

namespace Mcma.Modules.JobProcessor.Kubernetes.CronJobs.StateConsumer
{
    internal class JobStateUpdater : IKafkaConsumerMessageProcessor
    {
        public JobStateUpdater(IJobStateManager stateManager)
        {
            JobStateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        }

        private IJobStateManager JobStateManager { get; }
        
        public Task ProcessAsync(string requestId, string message)
        {
            JobStateManager.SetJobEnabled(JObject.Parse(message).ToMcmaObject<SetCronJobEnabledRequest>());
            return Task.CompletedTask;
        }
    }
}