using Mcma.Logging.Azure.ApplicationInsights;
using Mcma.Modules.JobProcessor.Azure.Common;
using Mcma.Modules.JobProcessor.Azure.PeriodicJobCleanup;
using Mcma.Modules.JobProcessor.PeriodicJobCleanup;
using Mcma.WorkerInvoker.Azure.QueueStorage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Mcma.Modules.JobProcessor.Azure.PeriodicJobCleanup
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
            => builder.Services
                      .AddMcmaAppInsightsLogging("job-processor-periodic-job-cleanup")
                      .AddMcmaQueueWorkerInvoker()
                      .AddCosmosDbDataController()
                      .AddPeriodicJobCleaner();
    }
}
