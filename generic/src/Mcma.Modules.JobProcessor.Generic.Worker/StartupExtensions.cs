using System;
using Hangfire;
using Hangfire.Logging;
using Hangfire.Logging.LogProviders;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Mcma.Client;
using Mcma.LocalFileSystem;
using Mcma.Modules.JobProcessor.Common;
using Mcma.Modules.JobProcessor.Generic.Common;
using Mcma.Modules.JobProcessor.Worker;
using Mcma.Serilog;
using Mcma.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Mcma.Modules.JobProcessor.Generic.Worker
{
    public static class JobProcessorWorkerServiceCollectionExtensions
    {
        public static IServiceCollection AddJobProcessorWorker(this IServiceCollection services, IConfiguration configuration)
        {
            LocalFileSystemHelper.AddTypes();

            var logger =
                new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                    .CreateLogger();

            var mcmaConfig = configuration.GetSection("Mcma");

            services.AddMcmaSerilogLogging(mcmaConfig["ServiceName"], logger);

            services.AddMongoDbDataController(
                dataControllerOpts => dataControllerOpts.PublicUrl = mcmaConfig["PublicUrl"],
                mongoDbOpts => mcmaConfig.Bind("MongoDb", mongoDbOpts));

            services.AddHangfire(config =>
                    {
                        config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
                        config.UseSimpleAssemblyNameTypeSerializer();
                        config.UseRecommendedSerializerSettings();
                        config.UseFilter(new AutomaticRetryAttribute { Attempts = 0 });
                        config.UseMongoStorage(configuration.GetConnectionString("Hangfire"),
                                               new MongoStorageOptions
                                               {
                                                   QueuePollInterval = mcmaConfig.GetValue("QueuePollInterval", TimeSpan.FromSeconds(3)),
                                                   MigrationOptions = new MongoMigrationOptions { MigrationStrategy = new DropMongoMigrationStrategy() },
                                                   UseNotificationsCollection = false
                                               });
                    })
                    .AddHangfireServer(opts => opts.Queues = new[] { mcmaConfig["ServiceName"] });

            services.AddMcmaClient(builder => builder.ConfigureDefaults(mcmaConfig["ServicesUrl"]));

            return services
                   .AddSingleton<IJobCheckerTrigger, HangfireJobCheckerTrigger>()
                   .AddMcmaWorker(builder => builder.AddJobProcessorOperations());
        }
    }
}