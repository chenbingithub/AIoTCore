using System;

namespace AIoT.Core.BackgroundJobs
{
    public class PeriodicBackgroundWorkerContext
    {
        public IServiceProvider ServiceProvider { get; }

        public PeriodicBackgroundWorkerContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}