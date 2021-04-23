using System.Threading.Tasks;
using Hangfire.Server;

namespace Mcma.Modules.JobProcessor.Generic.Common
{
    public interface IHangfireJobChecker
    {
        Task CheckJobsAsync(PerformContext performContext);
    }
}