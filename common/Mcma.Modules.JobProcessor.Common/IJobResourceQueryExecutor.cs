using System.Threading.Tasks;

namespace Mcma.Modules.JobProcessor.Common
{
    public interface IJobResourceQueryExecutor
    {
        Task<QueryResults<Job>> QueryAsync(JobResourceQueryParameters queryParameters, string pageStartToken = null);
    }
}