using Mcma.Data.Azure.CosmosDb;
using Mcma.Modules.JobProcessor.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Modules.JobProcessor.Azure.Common
{
    public static class DataControllerServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDbDataController(this IServiceCollection services, bool? consistentRead = null)
            =>
                services
                    .AddMcmaCosmosDb(
                        opts =>
                        {
                            opts.ConsistentGet = consistentRead;
                            opts.ConsistentQuery = consistentRead;
                        })
                    .AddDataController();
    }
}