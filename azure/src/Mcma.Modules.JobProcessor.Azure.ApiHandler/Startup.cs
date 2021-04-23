using Mcma.Azure.Client.AzureAD.ManagedIdentity;
using Mcma.Azure.Functions.ApiHandler;
using Mcma.Modules.JobProcessor.Azure.ApiHandler;
using Mcma.Modules.JobProcessor.Azure.Common;
using Mcma.Azure.WorkerInvoker;
using Mcma.Client;
using Mcma.Modules.JobProcessor.ApiHandler;
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
