using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Mcma.Modules.JobProcessor.Aws.Common;
using Mcma.Client;
using Mcma.Client.Aws;
using Mcma.Functions.Aws;
using Mcma.Functions.Aws.ApiHandler;
using Mcma.Modules.JobProcessor.ApiHandler;
using Mcma.Serialization.Aws;
using Mcma.WorkerInvoker.Aws.Lambda;
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
