using System;
using System.Threading.Tasks;
using Mcma.Data;
using Mcma.Data.DocumentDatabase.Queries;
using Mcma.Modules.JobProcessor.Common;

namespace Mcma.Modules.JobProcessor.Aws.Common
{
    public class DynamoDbJobResourceQueryExecutor : IJobResourceQueryExecutor
    {
        public DynamoDbJobResourceQueryExecutor(IDocumentDatabaseTable dbTable)
        {
            DbTable = dbTable ?? throw new ArgumentNullException(nameof(dbTable));
        }
        
        private IDocumentDatabaseTable DbTable { get; }

        public async Task<QueryResults<Job>> QueryAsync(JobResourceQueryParameters queryParameters, string pageStartToken = null)
        {
            queryParameters.PartitionKey = "/jobs";
            
            return await DbTable.CustomQueryAsync<Job, JobResourceQueryParameters>(new CustomQuery<JobResourceQueryParameters>
            {
                Name = CustomQueryNames.JobResourceQueryName,
                Parameters = queryParameters,
                PageStartToken = pageStartToken
            });
        }
    }
}