using System;
using System.Linq;
using Mcma.Kafka;
using Mcma.Modules.JobProcessor.Kubernetes.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Mcma.Modules.JobProcessor.Kubernetes.CronJobs.ExecutionProducer
{
    public static class JobExecutionServiceCollectionExtensions
    {
        public static IServiceCollection AddCronJobExecutionProducer(this IServiceCollection services, IConfiguration configuration)
            => services.AddSingletonKafkaProducer<string, string>()
                       .AddQuartz(q =>
                       {
                           q.UseMicrosoftDependencyInjectionJobFactory();
                           foreach (var jobType in Enum.GetValues(typeof(CronJobType)).OfType<CronJobType>())
                           {
                               var cronSchedule = configuration[$"CronJobs:{jobType}:Expression"];
                               if (string.IsNullOrWhiteSpace(cronSchedule))
                                   throw new McmaException($"Cron schedule not found for job type {jobType}");
                               if (!CronExpression.IsValidExpression(cronSchedule))
                                   throw new McmaException($"Invalid cron expression '{cronSchedule}' found for job type {jobType}");

                               var jobKey = JobKey.Create(jobType.ToString());
                               q.AddJob<JobExecutionMessageProducer>(jobKey);
                               q.AddTrigger(opts => opts.ForJob(jobKey).WithIdentity($"{jobType}-trigger").WithCronSchedule(cronSchedule));
                           }
                       })
                       .AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
    }
}