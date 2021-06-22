using Amazon.Lambda.Core;
using Mcma.Modules.JobProcessor.Aws.Common;
using Mcma.Functions.Aws;
using Mcma.Logging.Aws.CloudWatch;
using Mcma.Modules.JobProcessor.PeriodicJobCleanup;
using Mcma.Serialization.Aws;
using Mcma.WorkerInvoker.Aws.Lambda;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(McmaLambdaSerializer))]

namespace Mcma.Modules.JobProcessor.Aws.PeriodicJobCleanup
{
    public class PeriodicJobCleanupFunction : McmaLambdaFunction<PeriodicJobCleanupHandler>
    {
        protected override void Configure(IServiceCollection services)
            => services.AddMcmaCloudWatchLogging("job-processor-periodic-job-cleanup")
                       .AddDynamoDbDataController()
                       .AddMcmaLambdaWorkerInvoker()
                       .AddPeriodicJobCleaner();
    }
}