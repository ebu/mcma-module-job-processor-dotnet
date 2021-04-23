using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Mcma.Aws.Client;
using Mcma.Aws.Functions;
using Mcma.Aws.Functions.ApiHandler;
using Mcma.Modules.JobProcessor.Aws.Common;
using Mcma.Aws.Lambda;
using Mcma.Aws.WorkerInvoker;
using Mcma.Client;
using Mcma.Modules.JobProcessor.ApiHandler;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(McmaLambdaSerializer))]

namespace Mcma.Modules.JobProcessor.Aws.ApiHandler
{
    public class JobProcessorApiHandler : McmaLambdaFunction<McmaLambdaApiHandler, APIGatewayProxyRequest, APIGatewayProxyResponse>
    {
        protected override void Configure(IServiceCollection services)
            => services.AddMcmaClient(clientBuilder => clientBuilder.Auth.AddAws4Auth())
                       .AddMcmaLambdaWorkerInvoker()
                       .AddDynamoDbDataController()
                       .AddMcmaLambdaApiHandler("job-processor-api-handler", apiBuilder => apiBuilder.AddJobProcessorRoutes());
    }
}
