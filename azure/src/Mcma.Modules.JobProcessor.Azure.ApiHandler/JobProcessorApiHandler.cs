using System;
using System.Threading.Tasks;
using Mcma.Api.Azure.FunctionApp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace Mcma.Modules.JobProcessor.Azure.ApiHandler
{
    public class JobProcessorApiHandler
    {
        public JobProcessorApiHandler(IAzureFunctionApiController controller)
        {
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
        }

        private IAzureFunctionApiController Controller { get; }

        [FunctionName(nameof(JobProcessorApiHandler))]
        public Task<IActionResult> ExecuteAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, Route = "{*resourcePath}")] HttpRequest request,
            string resourcePath,
            ExecutionContext executionContext)
        {
            return Controller.HandleRequestAsync(request, executionContext);
        }
    }
}
