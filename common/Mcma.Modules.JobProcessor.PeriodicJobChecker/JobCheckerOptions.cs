using Mcma.Utility;

namespace Mcma.Modules.JobProcessor.PeriodicJobChecker
{
    public class JobCheckerOptions
    {
        public long? DefaultJobTimeoutInMinutes { get; set; } =
            long.TryParse(McmaEnvironmentVariables.Get("DEFAULT_JOB_TIMEOUT_IN_MINUTES", false), out var tmp)
                ? tmp
                : default(long?);
    }
}