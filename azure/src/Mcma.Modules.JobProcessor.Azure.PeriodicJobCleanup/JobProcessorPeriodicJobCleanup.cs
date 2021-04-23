using System.Threading.Tasks;
using Mcma.Modules.JobProcessor.PeriodicJobCleanup;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Mcma.Modules.JobProcessor.Azure.PeriodicJobCleanup
{
    public class JobProcessorPeriodicJobCleanup
    {
        public JobProcessorPeriodicJobCleanup(IJobCleaner jobCleaner)
        {
            JobCleaner = jobCleaner;
        }
        
        private IJobCleaner JobCleaner { get; }

        [FunctionName(nameof(JobProcessorPeriodicJobCleanup))]
        public async Task Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
            HttpRequest request,
            ExecutionContext executionContext)
        {
            await JobCleaner.CleanupJobsAsync(executionContext.InvocationId.ToString());
        }

    }
}