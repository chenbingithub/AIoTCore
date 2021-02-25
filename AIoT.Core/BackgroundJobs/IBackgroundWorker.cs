using Microsoft.Extensions.Hosting;
using Volo.Abp.DependencyInjection;

namespace AIoT.Core.BackgroundJobs
{
    /// <summary>
    /// Interface for a worker (thread) that runs on background to perform some tasks.
    /// </summary>
    public interface IBackgroundWorker : IHostedService, ISingletonDependency
    {

    }
}
