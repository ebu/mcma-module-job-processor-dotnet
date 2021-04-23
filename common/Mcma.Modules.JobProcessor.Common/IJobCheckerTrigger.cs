using System.Threading.Tasks;

namespace Mcma.Modules.JobProcessor.Common
{
    public interface IJobCheckerTrigger
    {
        Task EnableAsync();
        
        Task DisableAsync();
    }
}