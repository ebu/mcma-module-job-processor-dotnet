using System;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Mcma.Aws.Functions;
using Mcma.Modules.JobProcessor.PeriodicJobCleanup;

namespace Mcma.Modules.JobProcessor.Aws.PeriodicJobCleanup
{
    public class PeriodicJobCleanupHandler : IMcmaLambdaFunctionHandler
    {
        public PeriodicJobCleanupHandler(IJobCleaner jobCleaner)
        {
            JobCleaner = jobCleaner ?? throw new ArgumentNullException(nameof(jobCleaner));
        }
        
        private IJobCleaner JobCleaner { get; }

        public async Task ExecuteAsync(ILambdaContext context) => await JobCleaner.CleanupJobsAsync(context.AwsRequestId);
    }
}