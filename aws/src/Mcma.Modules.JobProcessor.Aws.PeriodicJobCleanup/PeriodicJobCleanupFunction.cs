using Amazon.Lambda.Core;
using Mcma.Aws.CloudWatch;
using Mcma.Aws.Functions;
using Mcma.Modules.JobProcessor.Aws.Common;
using Mcma.Aws.Lambda;
using Mcma.Aws.WorkerInvoker;
using Mcma.Modules.JobProcessor.PeriodicJobCleanup;
using Mcma.Utility;
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
                       .AddJobCleaner();
    }
}