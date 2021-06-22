using Mcma.Kafka;
using Mcma.Logging.Serilog;
using Mcma.Modules.JobProcessor.Kubernetes.Common;
using Mcma.Modules.JobProcessor.Kubernetes.CronJobs.ExecutionProducer;
using Mcma.Modules.JobProcessor.Kubernetes.CronJobs.StateConsumer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Serilog;

namespace Mcma.Modules.JobProcessor.Kubernetes.CronJobs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                                       services.AddMcmaSerilogLogging(loggerConfig => loggerConfig.WriteTo.Console(), "job-processor-cron-jobs")
                                               .AddCronJobStateConsumer()
                                               .AddCronJobExecutionProducer(hostContext.Configuration));
    }
}