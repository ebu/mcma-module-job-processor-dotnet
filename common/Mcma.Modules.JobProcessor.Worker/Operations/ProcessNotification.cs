using System;
using System.Threading.Tasks;
using Mcma.Client.Resources;
using Mcma.Model;
using Mcma.Model.Jobs;
using Mcma.Modules.JobProcessor.Common;
using Mcma.Modules.JobProcessor.Worker.EventLogging;
using Mcma.Modules.JobProcessor.Worker.Requests;
using Mcma.Serialization;
using Mcma.Worker;

namespace Mcma.Modules.JobProcessor.Worker.Operations
{
    internal class ProcessNotification : McmaWorkerOperation<NotificationData>
    {
        public ProcessNotification(IResourceManagerProvider resourceManagerProvider, IDataController dataController)
        {
            ResourceManagerProvider = resourceManagerProvider ?? throw new ArgumentNullException(nameof(resourceManagerProvider));
            DataController = dataController ?? throw new ArgumentNullException(nameof(dataController));
        }
        
        private IResourceManagerProvider ResourceManagerProvider { get; }

        private IDataController DataController { get; }

        public override string Name => nameof(ProcessNotification);

        protected override async Task ExecuteAsync(McmaWorkerRequestContext requestContext, NotificationData notificationData)
        {
            var jobId = notificationData.JobId;
            var jobExecutionId = notificationData.JobExecutionId;
            var notification = notificationData.Notification;
            var notificationContent = notification.Content.ToMcmaObject<JobBase>();

            var logger = requestContext.Logger;
            var resourceManager = ResourceManagerProvider.Get(requestContext.Tracker);
            var jobEventLogger = new JobEventLogger(logger, resourceManager);
            
            var mutex = await DataController.CreateMutexAsync(jobId, requestContext.RequestId);

            Job job;
            JobExecution jobExecution;
            
            await mutex.LockAsync();
            try
            {
                job = await DataController.GetJobAsync(jobId);
                if (job == null)
                    throw new McmaException($"Job with id '{jobId}' not found.");

                jobExecution = await DataController.GetExecutionAsync(jobExecutionId);
                if (jobExecution == null)
                    throw new McmaException($"JobExecution with id '{jobExecutionId}' not found.");

                // not updating job if it already was marked as completed or failed.
                if (jobExecution.Status == JobStatus.Completed ||
                    jobExecution.Status == JobStatus.Failed ||
                    jobExecution.Status == JobStatus.Canceled)
                {
                    logger.Warn($"Ignoring notification for job that would change state from {jobExecution.Status} to {notificationContent.Status}");
                    return;
                }

                if (job.Status != notificationContent.Status)
                {
                    logger.Info($"Job changed status from {job.Status} to {notificationContent.Status}: {job.Id}");

                    if (notificationContent.Status == JobStatus.Scheduled ||
                        notificationContent.Status == JobStatus.Running)
                    {
                        jobExecution.ActualStartDate ??= DateTime.UtcNow;
                    }
                    else if (notificationContent.Status == JobStatus.Failed ||
                             notificationContent.Status == JobStatus.Canceled ||
                             notificationContent.Status == JobStatus.Completed)
                    {
                        jobExecution.ActualEndDate ??= DateTime.UtcNow;

                        jobExecution.ActualDuration =
                            jobExecution.ActualStartDate.HasValue
                                ? (long)(jobExecution.ActualEndDate.Value - jobExecution.ActualStartDate.Value).TotalMilliseconds
                                : 0;
                    }
                }

                jobExecution.Status = notificationContent.Status;
                jobExecution.Error = notificationContent.Error;
                jobExecution.Progress = notificationContent.Progress;
                jobExecution.JobOutput = notificationContent.JobOutput;
                
                await DataController.UpdateExecutionAsync(jobExecution);

                job.Status = jobExecution.Status;
                job.Error = jobExecution.Error;
                job.Progress = jobExecution.Progress;
                job.JobOutput = jobExecution.JobOutput;

                await DataController.UpdateJobAsync(job);
            }
            finally
            {
                await mutex.UnlockAsync();
            }

            await jobEventLogger.LogJobEventAsync(job, jobExecution);

            await resourceManager.SendNotificationAsync(job, job.NotificationEndpoint);
        }
    }
}
