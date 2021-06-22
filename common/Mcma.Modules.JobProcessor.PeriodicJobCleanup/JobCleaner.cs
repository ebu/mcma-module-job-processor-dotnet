using System;
using System.Linq;
using System.Threading.Tasks;
using Mcma.Logging;
using Mcma.Model;
using Mcma.Model.Jobs;
using Mcma.Modules.JobProcessor.Common;
using Mcma.WorkerInvoker;
using Microsoft.Extensions.Options;

namespace Mcma.Modules.JobProcessor.PeriodicJobCleanup
{
    public class JobCleaner : IJobCleaner
    {
        public JobCleaner(ILoggerProvider loggerProvider,
                          IDataController dataController,
                          IMcmaWorkerInvoker workerInvoker,
                          IOptions<JobCleanerOptions> options)
        {
            LoggerProvider = loggerProvider ?? throw new ArgumentNullException(nameof(loggerProvider));
            DataController = dataController ?? throw new ArgumentNullException(nameof(dataController));
            WorkerInvoker = workerInvoker ?? throw new ArgumentNullException(nameof(workerInvoker));
            Options = options.Value ?? new JobCleanerOptions();
        }

        private ILoggerProvider LoggerProvider { get; }

        private IDataController DataController { get; }

        private IMcmaWorkerInvoker WorkerInvoker { get; }

        private JobCleanerOptions Options { get; }
        public async Task CleanupJobsAsync(string requestId)
        {
            var tracker = new McmaTracker
            {
                Id = Guid.NewGuid().ToString(),
                Label = $"Periodic Job Cleanup - {DateTime.UtcNow:O}"
            };

            var logger = LoggerProvider.Get(requestId, tracker);
            try
            {
                var jobRetentionPeriodInDays = Options.JobRetentionPeriodInDays;
                
                logger.Info($"Job Retention Period set to {jobRetentionPeriodInDays} days");

                if (!jobRetentionPeriodInDays.HasValue || jobRetentionPeriodInDays.Value <= 0)
                {
                    logger.Info("Exiting");
                    return;
                }

                var retentionDateLimit = DateTime.UtcNow - TimeSpan.FromDays(jobRetentionPeriodInDays.Value);

                var completedJobs = await DataController.QueryJobsAsync(new JobResourceQueryParameters {Status = JobStatus.Completed});
                var failedJobs = await DataController.QueryJobsAsync(new JobResourceQueryParameters {Status = JobStatus.Failed});
                var canceledJobs = await DataController.QueryJobsAsync(new JobResourceQueryParameters {Status = JobStatus.Canceled});

                var jobs = completedJobs.Results.Concat(failedJobs.Results).Concat(canceledJobs.Results).ToArray();

                logger.Info($"Deleting {jobs.Length} jobs older than {retentionDateLimit:O}");

                foreach (var job in jobs)
                    await DeleteJobAsync(job);
            }
            catch (Exception error)
            {
                logger.Error(error);
                throw;
            }
            finally
            {
                logger.FunctionEnd(requestId);
            }
        }

        private async Task DeleteJobAsync(Job job)
        {
            await WorkerInvoker.InvokeAsync("DeleteJob",
                                            new
                                            {
                                                jobId = job.Id
                                            },
                                            job.Tracker);
        }
    }
}