using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Mcma.Functions.Aws;
using Mcma.Modules.JobProcessor.PeriodicJobChecker;

namespace Mcma.Modules.JobProcessor.Aws.PeriodicJobChecker
{
    public class PeriodicJobCheckerHandler : IMcmaLambdaFunctionHandler
    {
        public PeriodicJobCheckerHandler(IJobChecker jobChecker)
        {
            JobChecker = jobChecker ?? throw new ArgumentNullException(nameof(jobChecker));
        }

        private IJobChecker JobChecker { get; }

        public async Task ExecuteAsync(ILambdaContext context) => await JobChecker.CheckJobsAsync(context.AwsRequestId);
    }
}