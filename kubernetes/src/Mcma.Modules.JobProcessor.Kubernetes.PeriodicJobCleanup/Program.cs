using Mcma.Logging.Serilog;
using Mcma.Modules.JobProcessor.Kubernetes.Common;
using Mcma.Modules.JobProcessor.PeriodicJobCleanup;
using Mcma.WorkerInvoker.Kafka;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Mcma.Modules.JobProcessor.Kubernetes.PeriodicJobCleanup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                                       services.AddMcmaSerilogLogging(loggerConfig => loggerConfig.WriteTo.Console(), "job-processor-periodic-job-cleanup")
                                               .AddMongoDbDataController()
                                               .AddMcmaKafkaWorkerInvoker()
                                               .AddPeriodicJobCleaner()
                                               .AddKafkaCronJobExecutionService<JobCleanupMessageProcessor>(CronJobType.JobCleanup));
    }
}