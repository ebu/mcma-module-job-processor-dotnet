using Amazon.Lambda.Core;
using Mcma.Aws.Client;
using Mcma.Aws.Functions;
using Mcma.Aws.Functions.ApiHandler;
using Mcma.Aws.Functions.Worker;
using Mcma.Modules.JobProcessor.Aws.Common;
using Mcma.Aws.Lambda;
using Mcma.Client;
using Mcma.Modules.JobProcessor.Common;
using Mcma.Modules.JobProcessor.Worker;
using Mcma.Worker.Common;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(McmaLambdaSerializer))]

namespace Mcma.Modules.JobProcessor.Aws.Worker
{
    public class JobProcessorWorker : McmaLambdaFunction<McmaLambdaWorker, McmaWorkerRequest>
    {
        protected override void Configure(IServiceCollection services)
        {
            services
                .AddMcmaClient(clientBuilder => clientBuilder.Auth.AddAws4Auth())
                .AddDynamoDbDataController()
                .AddSingleton<IJobCheckerTrigger, CloudWatchEventsJobCheckerTrigger>()
                .AddMcmaAwsLambdaWorker("job-processor-worker",
                                        builder => builder.AddJobProcessorOperations());
        }
    }
}
