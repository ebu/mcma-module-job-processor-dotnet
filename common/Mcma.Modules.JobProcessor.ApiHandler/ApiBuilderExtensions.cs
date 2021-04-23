using Mcma.Api;

namespace Mcma.Modules.JobProcessor.ApiHandler
{
    public static class ApiBuilderExtensions
    {
        public static McmaApiBuilder AddJobProcessorRoutes(this McmaApiBuilder apiBuilder)
            => apiBuilder.AddRouteCollection<JobRoutes>().AddRouteCollection<JobExecutionRoutes>();
    }
}