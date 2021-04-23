using System;
using Mcma.Modules.JobProcessor.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Modules.JobProcessor.PeriodicJobChecker
{
    public static class JobCheckerServiceCollectionExtensions
    {
        public static IServiceCollection AddPeriodicJobChecker<T>(this IServiceCollection services, Action<JobCheckerOptions> configure = null) where T : class, IJobCheckerTrigger
        {
            if (configure != null)
                services.Configure(configure);
            
            return services.AddSingleton<IJobChecker, JobChecker>().AddSingleton<IJobCheckerTrigger, T>();
        }
    }
}