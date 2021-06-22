using Amazon.Lambda.Core;
using Mcma.Modules.JobProcessor.Aws.Common;
using Mcma.Client;
using Mcma.Client.Aws;
using Mcma.Functions.Aws;
using Mcma.Functions.Aws.Worker;
using Mcma.Modules.JobProcessor.Common;
using Mcma.Modules.JobProcessor.Worker;
using Mcma.Serialization.Aws;
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
