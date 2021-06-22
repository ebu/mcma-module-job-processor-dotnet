using System;
using System.Threading.Tasks;
using Mcma.Data.DocumentDatabase;
using Mcma.Data.DocumentDatabase.Queries;
using Mcma.Model;
using Mcma.Model.Jobs;

namespace Mcma.Modules.JobProcessor.Common
{
    public class JobResourceQueryExecutor : IJobResourceQueryExecutor
    {
        public JobResourceQueryExecutor(IDocumentDatabaseTable dbTable)
        {
            DbTable = dbTable ?? throw new ArgumentNullException(nameof(dbTable));
        }
        
        private IDocumentDatabaseTable DbTable { get; }

        private static Query<T> BuildQuery<T>(JobResourceQueryParameters queryParameters, string pageStartToken) where T : JobBase
        {
            var (partitionKey, status, from, to, ascending, limit) = queryParameters;
            var query = new Query<T>
            {
                Path = partitionKey,
                SortBy = nameof(McmaResource.DateCreated),
                SortAscending = ascending ?? false,
                PageSize = limit,
                PageStartToken = pageStartToken
            };

            if (status.HasValue)
                query.AddFilterExpression(
                    new FilterCriteria<T, JobStatus>(j => j.Status, BinaryOperator.EqualTo, status.Value));
            
            if (from.HasValue)
                query.AddFilterExpression(
                    new FilterCriteria<T, DateTimeOffset?>(j => j.DateCreated, BinaryOperator.GreaterThanOrEqualTo, from.Value));
            
            if (to.HasValue)
                query.AddFilterExpression(
                    new FilterCriteria<T, DateTimeOffset?>(j => j.DateCreated, BinaryOperator.LessThanOrEqualTo, to.Value));

            return query;
        }

        public async Task<QueryResults<Job>> QueryAsync(JobResourceQueryParameters queryParameters, string pageStartToken = null)
        {
            queryParameters.PartitionKey = "/jobs";
            return await DbTable.QueryAsync(BuildQuery<Job>(queryParameters, pageStartToken));
        }
    }
}