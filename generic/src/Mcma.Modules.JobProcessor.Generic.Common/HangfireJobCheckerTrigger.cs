using System;
using System.Threading.Tasks;
using Hangfire;
using Mcma.Modules.JobProcessor.Common;
using Mcma.Modules.JobProcessor.PeriodicJobChecker;
using Microsoft.Extensions.Options;

namespace Mcma.Modules.JobProcessor.Generic.Common
{
    public class HangfireJobCheckerTrigger : IJobCheckerTrigger
    {
        public HangfireJobCheckerTrigger(IBackgroundJobClient backgroundJobClient, IOptions<JobCheckerOptions> options)
        {
            BackgroundJobClient = backgroundJobClient ?? throw new ArgumentNullException(nameof(backgroundJobClient));
            Options = options.Value ?? new JobCheckerOptions();
        }
        
        private IBackgroundJobClient BackgroundJobClient { get; }

        private JobCheckerOptions Options { get; }

        public Task EnableAsync()
        {
            BackgroundJobClient.Schedule<IHangfireJobChecker>(jobChecker => jobChecker.CheckJobsAsync(null),
                                                              TimeSpan.FromMinutes(Options.DefaultJobTimeoutInMinutes ?? 1));
            return Task.CompletedTask;
        }

        public Task DisableAsync()
        {
            return Task.CompletedTask;
        }
    }
}