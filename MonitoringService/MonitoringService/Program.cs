using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Core.Jobs;
using System.Linq.Expressions;

using System.Threading;
using System.Runtime.Loader;
using Common.Log;
using Core.Settings;
using Core.Services;
using MonitoringService.Utils;

namespace MonitoringService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            var backUpService = Startup.ServiceProvider.GetService<IBackUpService>();
            var settings = Startup.ServiceProvider.GetService<IBaseSettings>();
            var log = Startup.ServiceProvider.GetService<ILog>();
            IMonitoringJob job = Startup.ServiceProvider.GetService<IMonitoringJob>();

            #region InProcessJobs

            var end = new ManualResetEvent(false);
            CancellationTokenSource cts = new CancellationTokenSource();
            List<Task> jobs = new List<Task>()
            {
                InProcessJobHelper.StartJob(async () => { await job.CheckJobs(); }, cts.Token, settings.MonitoringJobFrequencyInSeconds, log),
                InProcessJobHelper.StartJob(async () => { await job.CheckAPIs(); }, cts.Token, settings.MonitoringApiFrequencyInSeconds, log)
            };

            #endregion

            AssemblyLoadContext.Default.Unloading += ctx =>
            {
                Console.WriteLine("SIGTERM recieved");
                cts.Cancel();
                try
                {
                    Task.WaitAll(jobs.ToArray());
                }
                catch { }
                end.WaitOne();
            };

            host.Run();
            log.WriteInfoAsync("MonitoringService", "Program", "Main", "Monitoring Service has been stopped", DateTime.UtcNow).Wait();
            end.Set();
        }
    }
}
