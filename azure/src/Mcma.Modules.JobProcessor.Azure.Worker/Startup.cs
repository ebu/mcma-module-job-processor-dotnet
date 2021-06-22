using Mcma.Functions.Azure.Worker;
using Mcma.Modules.JobProcessor.Azure.Common;
using Mcma.Modules.JobProcessor.Azure.Worker;
using Mcma.Modules.JobProcessor.Common;
using Mcma.Modules.JobProcessor.Worker;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Mcma.Modules.JobProcessor.Azure.Worker
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
            => builder.Services
                      .AddCosmosDbDataController()
                      .AddSingleton<IJobCheckerTrigger, LogicAppWorkflowCheckerTrigger>()
                      .AddMcmaAzureFunctionWorker("job-processor-worker",
                                                  workerBuilder => workerBuilder.AddJobProcessorOperations());

    }
}