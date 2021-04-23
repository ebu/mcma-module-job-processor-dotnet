using System;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Modules.JobProcessor.PeriodicJobCleanup
{
    public static class JobCleanerServiceCollectionExtensions
    {
        public static IServiceCollection AddJobCleaner(this IServiceCollection services, Action<JobCleanerOptions> configure = null)
        {
            if (configure != null)
                services.Configure(configure);
            
            return services.AddSingleton<IJobCleaner, JobCleaner>();
        }
    }
}