using Mcma.Utility;

namespace Mcma.Modules.JobProcessor.Common
{
    public class DataControllerOptions
    {
        public string PublicUrl { get; set; } = McmaEnvironmentVariables.Get("PUBLIC_URL", false);
    }
}