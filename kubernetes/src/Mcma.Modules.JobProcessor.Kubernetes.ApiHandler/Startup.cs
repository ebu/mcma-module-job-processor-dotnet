using Mcma.Api.AspNetCore;
using Mcma.Api.Http;
using Mcma.Client;
using Mcma.Logging.Serilog;
using Mcma.Modules.JobProcessor.ApiHandler;
using Mcma.Modules.JobProcessor.Kubernetes.Common;
using Mcma.Storage.LocalFileSystem;
using Mcma.WorkerInvoker.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Mcma.Modules.JobProcessor.Kubernetes.ApiHandler
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
            
            services.AddMcmaSerilogLogging(loggerConfig => loggerConfig.WriteTo.Console(), "job-processor-api-handler")
                    .AddMongoDbDataController()
                    .AddMcmaClient()
                    .AddMcmaKafkaWorkerInvoker()
                    .AddMcmaApi(builder => builder.AddJobProcessorRoutes());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseMcmaApiHandler();
        }
    }
}
