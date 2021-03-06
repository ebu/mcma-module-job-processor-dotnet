using System;
using System.Linq;
using System.Threading.Tasks;
using Mcma.Logging;
using Mcma.Model;
using Mcma.Model.Jobs;
using Mcma.Modules.JobProcessor.Common;
using Mcma.WorkerInvoker;
using Microsoft.Extensions.Options;

namespace Mcma.Modules.JobProcessor.PeriodicJobChecker
{
    public class JobChecker : IJobChecker
    {
        public JobChecker(ILoggerProvider loggerProvider,
                          IDataController dataController,
                          IMcmaWorkerInvoker workerInvoker,
                          IJobCheckerTrigger checkerTrigger,
                          IOptions<JobCheckerOptions> options)
        {
            LoggerProvider = loggerProvider ?? throw new ArgumentNullException(nameof(loggerProvider));
            DataController = dataController ?? throw new ArgumentNullException(nameof(dataController));
            WorkerInvoker = workerInvoker ?? throw new ArgumentNullException(nameof(workerInvoker));
            CheckerTrigger = checkerTrigger ?? throw new ArgumentNullException(nameof(checkerTrigger));
            Options = options.Value ?? new JobCheckerOptions();
        }

        private ILoggerProvider LoggerProvider { get; }

        private IDataController DataController { get; }

        private IMcmaWorkerInvoker WorkerInvoker { get; }

        private IJobCheckerTrigger CheckerTrigger { get; }

        private JobCheckerOptions Options { get; }

        public async Task CheckJobsAsync(string requestId)
        {
            var tracker = new McmaTracker
            {
                Id = Guid.NewGuid().ToString(),
                Label = $"Periodic Job Checker - {DateTime.UtcNow:O}"
            };

            var logger = LoggerProvider.Get(requestId, tracker);
            logger.FunctionStart(requestId);
            try
            {
                await CheckerTrigger.DisableAsync();

                var newJobs = await DataController.QueryJobsAsync(new JobResourceQueryParameters { Status = JobStatus.New });
                var queuedJobs = await DataController.QueryJobsAsync(new JobResourceQueryParameters { Status = JobStatus.Queued });
                var scheduledJobs = await DataController.QueryJobsAsync(new JobResourceQueryParameters { Status = JobStatus.Scheduled });
                var runningJobs = await DataController.QueryJobsAsync(new JobResourceQueryParameters { Status = JobStatus.Running });

                var jobs = newJobs.Results.Concat(queuedJobs.Results).Concat(scheduledJobs.Results).Concat(runningJobs.Results).ToArray();

                logger.Info($"Found {jobs.Length} active jobs");

                var activeJobsCount = 0;
                var failedJobsCount = 0;
                var now = DateTime.UtcNow;

                foreach (var job in jobs)
                {
                    var deadlinePassed = false;
                    var timeoutPassed = false;

                    var defaultTimeout = Options.DefaultJobTimeoutInMinutes;

                    if (job.Deadline != null)
                    {
                        defaultTimeout = null;
                        if (job.Deadline < now)
                            deadlinePassed = true;
                    }

                    var timeout = job.Timeout ?? defaultTimeout;
                    if (timeout.HasValue)
                    {
                        var jobExecution = (await DataController.GetExecutionsAsync(job.Id)).Results.FirstOrDefault();

                        var startDate = jobExecution?.ActualStartDate ?? jobExecution?.DateCreated ?? job.DateCreated;

                        var timePassedInMinutes = (now - startDate)?.TotalMinutes;
                        if (timePassedInMinutes > timeout)
                            timeoutPassed = true;
                    }

                    if (deadlinePassed)
                    {
                        await FailJobAsync(job,
                                           new ProblemDetail
                                           {
                                               ProblemType = "uri://mcma.ebu.ch/rfc7807/job-processor/job-deadline-passed",
                                               Title = "Job failed to complete before deadline",
                                               Detail = $"Job missed deadline of {job.Deadline:O}"
                                           });
                        failedJobsCount++;
                    }
                    else if (timeoutPassed)
                    {
                        await FailJobAsync(job,
                                           new ProblemDetail
                                           {
                                               ProblemType = "uri://mcma.ebu.ch/rfc7807/job-processor/job-timeout-passed",
                                               Title = "Job failed to complete before timeout limit",
                                               Detail = $"Job timed out after {timeout} minutes"
                                           });
                        failedJobsCount++;
                    }
                    else
                        activeJobsCount++;
                }

                logger.Info($"Failed {failedJobsCount} jobs due to deadline or timeout constraints");

                if (activeJobsCount > 0)
                {
                    logger.Info($"There are {activeJobsCount} active jobs remaining");
                    await CheckerTrigger.EnableAsync();
                }
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

        private async Task FailJobAsync(Job job, ProblemDetail error)
        {
            await WorkerInvoker.InvokeAsync("FailJob",
                                            new
                                            {
                                                jobId = job.Id,
                                                error
                                            },
                                            job.Tracker);
        }
    }
}