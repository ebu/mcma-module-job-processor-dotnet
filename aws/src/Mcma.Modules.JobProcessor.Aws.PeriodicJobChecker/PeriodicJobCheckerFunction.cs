using Amazon.Lambda.Core;
using Mcma.Modules.JobProcessor.Aws.Common;
using Mcma.Functions.Aws;
using Mcma.Logging.Aws.CloudWatch;
using Mcma.Modules.JobProcessor.PeriodicJobChecker;
using Mcma.Serialization.Aws;
using Mcma.WorkerInvoker.Aws.Lambda;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(McmaLambdaSerializer))]

namespace Mcma.Modules.JobProcessor.Aws.PeriodicJobChecker
{
    public class JobProcessorPeriodicJobChecker : McmaLambdaFunction<PeriodicJobCheckerHandler>
    {
        protected override void Configure(IServiceCollection services)
            => services.AddMcmaCloudWatchLogging("job-processor-periodic-job-checker")
                       .AddMcmaLambdaWorkerInvoker()
                       .AddDynamoDbDataController()
                       .AddPeriodicJobChecker<CloudWatchEventsJobCheckerTrigger>();
    }
}