namespace Mcma.Modules.JobProcessor.Worker.Requests
{
    public class NotificationData
    {
        public string JobId { get; set; }
        
        public string JobExecutionId { get; set; }

        public Notification Notification { get; set; }
    }
}
