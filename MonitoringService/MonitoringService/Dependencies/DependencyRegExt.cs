using Core.Jobs;
using Core.Repositories;
using Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitoringService.Dependencies
{
    public static class DependencyRegExt
    {
        public static void RegDependencies(this IServiceCollection collection)
        {
            collection.AddSingleton<IMonitoringJob, MonitoringJob>();
            collection.AddSingleton<IMonitoringObjectRepository>(new MonitoringObjectRepository());
            collection.AddSingleton<IQueueFactory, QueueFactory>();
            collection.AddSingleton<IMonitoringService, Services.MonitoringService>();
            collection.AddSingleton<ISlackNotifier, SlackNotifier>();
            collection.AddSingleton<IUrlMonitoringService, UrlMonitoringService>();
            collection.AddSingleton<IIsAliveService, IsAliveService>();
        }
    }
}
