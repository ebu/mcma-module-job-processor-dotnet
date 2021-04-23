using Mcma.Data.DocumentDatabase.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Mcma.Modules.JobProcessor.Common
{
    public abstract class JobResourceQueryBuilder<T> : CustomQueryBuilder<JobResourceQueryParameters, T>
    {
        public sealed override string Name => CustomQueryNames.JobResourceQueryName;
    }
}