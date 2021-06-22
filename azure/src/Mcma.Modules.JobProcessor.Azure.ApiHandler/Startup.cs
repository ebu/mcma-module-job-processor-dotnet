using Mcma.Modules.JobProcessor.Azure.ApiHandler;
using Mcma.Modules.JobProcessor.Azure.Common;
using Mcma.Client;
using Mcma.Client.Azure.AzureAD.ManagedIdentity;
using Mcma.Functions.Azure.ApiHandler;
using Mcma.Modules.JobProcessor.ApiHandler;
using Mcma.WorkerInvoker.Azure.QueueStorage;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Mcma.Modules.JobProcessor.Azure.ApiHandler
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
            => builder.Services
                      .AddMcmaClient(clientBuilder => clientBuilder.Auth.AddAzureADManagedIdentityAuth())
                      .AddMcmaQueueWorkerInvoker()
                      .AddCosmosDbDataController()
                      .AddMcmaAzureFunctionApiHandler("job-processor-api-handler", apiBuilder => apiBuilder.AddJobProcessorRoutes());
    }
}
