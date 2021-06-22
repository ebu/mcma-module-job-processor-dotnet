using Amazon.DynamoDBv2.DocumentModel;
using Mcma.Data.Aws.DynamoDb;
using Mcma.Model;
using Mcma.Modules.JobProcessor.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Modules.JobProcessor.Aws.Common
{
    public static class DataControllerServiceCollectionExtensions
    {
        public static IServiceCollection AddDynamoDbDataController(this IServiceCollection services, bool? consistentRead = null)
            =>
                services
                    .AddMcmaDynamoDb(
                        opts =>
                        {
                            opts.ConsistentGet = consistentRead;
                            opts.ConsistentQuery = consistentRead;
                        },
                        builder =>
                            builder
                                .AddCustomQueryBuilder<JobResourceQueryParameters, JobResourceQueryBuilder<QueryOperationConfig>>()
                                .AddAttributeMapping<JobBase>(
                                    "resource_status",
                                    (partitionKey, sortKey, resource) => $"{partitionKey}-{resource.Status}")
                                .AddAttributeMapping<JobBase>(
                                    "resource_created",
                                    (partitionKey, sortKey, resource) => resource.DateCreated?.ToUnixTimeMilliseconds()))
                    .AddDataController<DynamoDbJobResourceQueryExecutor>();
    }
}