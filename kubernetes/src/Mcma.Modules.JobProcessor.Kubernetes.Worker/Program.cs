using Mcma.Client;
using Mcma.Logging.Serilog;
using Mcma.Modules.JobProcessor.Common;
using Mcma.Modules.JobProcessor.Kubernetes.Common;
using Mcma.Modules.JobProcessor.Worker;
using Mcma.Storage.LocalFileSystem;
using Mcma.Worker;
using Mcma.Worker.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Mcma.Modules.JobProcessor.Kubernetes.Worker
{
    public class Program
    {
        public static void Main(string[] args)
            => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    LocalFileSystemHelper.AddTypes();

                    services.AddMcmaSerilogLogging(loggerConfig => loggerConfig.WriteTo.Console(), "job-processor-worker")
                            .AddMongoDbDataController()
                            .AddMcmaClient()
                            .AddKafkaJobCheckerTrigger()
                            .AddMcmaWorker(builder => builder.AddJobProcessorOperations())
                            .AddMcmaKafkaWorkerService();
                });
    }
}
