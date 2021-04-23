using System.Threading.Tasks;

namespace Mcma.Modules.JobProcessor.PeriodicJobCleanup
{
    public interface IJobCleaner
    {
        Task CleanupJobsAsync(string requestId);
    }
}