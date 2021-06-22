using Mcma.Logging.Serilog;
using Mcma.Modules.JobProcessor.Kubernetes.Common;
using Mcma.Modules.JobProcessor.PeriodicJobChecker;
using Mcma.WorkerInvoker.Kafka;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Mcma.Modules.JobProcessor.Kubernetes.PeriodicJobChecker
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
                                       services.AddMcmaSerilogLogging(loggerConfig => loggerConfig.WriteTo.Console(), "job-processor-periodic-job-checker")
                                               .AddMongoDbDataController()
                                               .AddMcmaKafkaWorkerInvoker()
                                               .AddPeriodicJobChecker<KafkaJobCheckerTrigger>()
                                               .AddKafkaCronJobExecutionService<JobCheckerMessageProcessor>(CronJobType.JobChecker));
    }
}