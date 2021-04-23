using System;
using Mcma.Modules.JobProcessor.Common;
using Mcma.MongoDb;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Modules.JobProcessor.Generic.Common
{
    public static class DataControllerServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDbDataController(this IServiceCollection services,
                                                                  Action<DataControllerOptions> configureDataController,
                                                                  Action<MongoDbTableOptions> configureMongoDb)
            =>
                services
                    .AddMcmaMongoDb(configureMongoDb)
                    .AddDataController(configureDataController);
    }
}