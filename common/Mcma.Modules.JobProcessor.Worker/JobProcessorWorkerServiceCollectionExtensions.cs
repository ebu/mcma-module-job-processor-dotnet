using Mcma.Modules.JobProcessor.Worker.Operations;
using Mcma.Worker;

namespace Mcma.Modules.JobProcessor.Worker
{
    public static class JobProcessorWorkerWorkerBuilderExtensions
    {
        public static McmaWorkerBuilder AddJobProcessorOperations(this McmaWorkerBuilder workerBuilder)
            => workerBuilder
               .AddOperation<StartJob>()
               .AddOperation<CancelJob>()
               .AddOperation<RestartJob>()
               .AddOperation<FailJob>()
               .AddOperation<DeleteJob>()
               .AddOperation<ProcessNotification>();
    }
}