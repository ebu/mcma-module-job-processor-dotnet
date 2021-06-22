using System.Threading.Tasks;
using Mcma.Model;
using Mcma.Model.Jobs;

namespace Mcma.Modules.JobProcessor.Common
{
    public interface IJobResourceQueryExecutor
    {
        Task<QueryResults<Job>> QueryAsync(JobResourceQueryParameters queryParameters, string pageStartToken = null);
    }
}