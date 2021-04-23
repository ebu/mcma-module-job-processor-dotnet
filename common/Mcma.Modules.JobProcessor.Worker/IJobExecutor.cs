using System.Threading.Tasks;
using Mcma.Modules.JobProcessor.Worker.Requests;
using Mcma.Worker;

namespace Mcma.Modules.JobProcessor.Worker
{
    internal interface IJobExecutor
    {
        Task<Job> StartExecutionAsync(McmaWorkerRequestContext requestContext, JobReference jobReference, Job job);

        Task<Job> CancelExecutionAsync(McmaWorkerRequestContext requestContext, JobReference jobReference, Job job);
    }
}