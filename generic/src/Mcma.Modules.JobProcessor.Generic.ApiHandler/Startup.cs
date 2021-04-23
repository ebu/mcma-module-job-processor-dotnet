using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Mcma.Api;
using Mcma.Api.Routing.Defaults.Routes;
using Mcma.AspNetCore;
using Mcma.Client;
using Mcma.HangfireWorkerInvoker;
using Mcma.LocalFileSystem;
using Mcma.Modules.JobProcessor.ApiHandler;
using Mcma.Modules.JobProcessor.Generic.Common;
using Mcma.MongoDb;
using Mcma.Serilog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Mcma.Modules.JobProcessor.Generic.ApiHandler
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            LocalFileSystemHelper.AddTypes();

            var mcmaConfig = Configuration.GetSection("Mcma");

            services.AddMcmaSerilogLogging(mcmaConfig["ServiceName"],
                                           new LoggerConfiguration()
                                               .WriteTo.Console()
                                               .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
                                               .CreateLogger());

            services.AddMongoDbDataController(
                dataControllerOpts => dataControllerOpts.PublicUrl = mcmaConfig["PublicUrl"],
                mongoDbOpts => mcmaConfig.Bind("MongoDb", mongoDbOpts));

            services.AddMcmaHangfireWorkerInvoker(mcmaConfig["ServiceName"], config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170);
                config.UseSimpleAssemblyNameTypeSerializer();
                config.UseRecommendedSerializerSettings();
                config.UseMongoStorage(Configuration.GetConnectionString("Hangfire"),
                                       new MongoStorageOptions
                                       {
                                           MigrationOptions = new MongoMigrationOptions {MigrationStrategy = new DropMongoMigrationStrategy()}
                                       });
            });

            services.AddMcmaClient(builder => builder.ConfigureDefaults(mcmaConfig["ServicesUrl"]))
                    .AddMcmaApi(builder =>
                                    builder.Configure(opts => opts.PublicUrl = mcmaConfig["PublicUrl"])
                                           .AddJobProcessorRoutes());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseMcmaApiHandler();
        }
    }
}
