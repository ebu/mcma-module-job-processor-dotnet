using System.Threading.Tasks;

namespace Mcma.Modules.JobProcessor.PeriodicJobChecker
{
    public interface IJobChecker
    {
        Task CheckJobsAsync(string requestId);
    }
}