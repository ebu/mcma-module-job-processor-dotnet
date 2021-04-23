using System;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Modules.JobProcessor.Common
{
    public static class DataControllerServiceCollectionExtensions
    {
        public static IServiceCollection AddDataController(this IServiceCollection services, Action<DataControllerOptions> configure = null)
            => services.AddDataController<JobResourceQueryExecutor>(configure);

        public static IServiceCollection AddDataController<T>(this IServiceCollection services, Action<DataControllerOptions> configure = null)
            where T : class, IJobResourceQueryExecutor
        {
            if (configure != null)
                services.Configure(configure);
            
            return services.AddSingleton<IJobResourceQueryExecutor, T>().AddSingleton<IDataController, DataController>();
        }
    }
}