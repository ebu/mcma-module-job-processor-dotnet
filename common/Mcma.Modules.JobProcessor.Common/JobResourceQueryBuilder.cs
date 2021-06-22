using Mcma.Data.DocumentDatabase.Queries.Custom;

namespace Mcma.Modules.JobProcessor.Common
{
    public abstract class JobResourceQueryBuilder<T> : CustomQueryBuilder<JobResourceQueryParameters, T>
    {
        public sealed override string Name => CustomQueryNames.JobResourceQueryName;
    }
}