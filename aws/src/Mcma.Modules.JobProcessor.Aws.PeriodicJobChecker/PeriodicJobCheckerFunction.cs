using Amazon.Lambda.Core;
using Mcma.Aws.CloudWatch;
using Mcma.Aws.Functions;
using Mcma.Modules.JobProcessor.Aws.Common;
using Mcma.Aws.Lambda;
using Mcma.Aws.WorkerInvoker;
using Mcma.Modules.JobProcessor.PeriodicJobChecker;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(McmaLambdaSerializer))]

namespace Mcma.Modules.JobProcessor.Aws.PeriodicJobChecker
{
    public class JobProcessorPeriodicJobChecker : McmaLambdaFunction<PeriodicJobCheckerHandler>
    {
        protected override void Configure(IServiceCollection services)
            => services.AddMcmaCloudWatchLogging("job-processor-periodic-job-cleanup")
                       .AddMcmaLambdaWorkerInvoker()
                       .AddDynamoDbDataController()
                       .AddPeriodicJobChecker<CloudWatchEventsJobCheckerTrigger>();
    }
}