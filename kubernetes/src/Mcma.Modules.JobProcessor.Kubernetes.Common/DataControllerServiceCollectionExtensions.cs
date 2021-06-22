using System;
using Mcma.Data.MongoDB;
using Mcma.Modules.JobProcessor.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Modules.JobProcessor.Kubernetes.Common
{
    public static class DataControllerServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDbDataController(this IServiceCollection services,
                                                                  Action<DataControllerOptions> configureDataController = null,
                                                                  Action<MongoDbTableOptions> configureMongoDb = null)
            =>
                services
                    .AddMcmaMongoDb(configureMongoDb)
                    .AddDataController(configureDataController);
    }
}