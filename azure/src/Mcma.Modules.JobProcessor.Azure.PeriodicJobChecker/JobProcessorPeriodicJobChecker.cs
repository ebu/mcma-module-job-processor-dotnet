using System;
using System.Threading.Tasks;
using Mcma.Modules.JobProcessor.PeriodicJobChecker;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Mcma.Modules.JobProcessor.Azure.PeriodicJobChecker
{
    public class JobProcessorPeriodicJobChecker
    {
        public JobProcessorPeriodicJobChecker(IJobChecker jobChecker)
        {
            JobChecker = jobChecker ?? throw new ArgumentNullException(nameof(jobChecker));
        }

        private IJobChecker JobChecker { get; }

        [FunctionName(nameof(JobProcessorPeriodicJobChecker))]
        public async Task ExecuteAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest request,
            ExecutionContext executionContext)
        {
            await JobChecker.CheckJobsAsync(executionContext.InvocationId.ToString());
        }
    }
}