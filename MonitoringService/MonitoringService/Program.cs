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
            var end = new ManualResetEvent(false);
            CancellationTokenSource cts = new CancellationTokenSource();
            //backUpService.RestoreBackupAsync().Wait();
            Task.Run(async () => 
            {
                int secondsDelay = settings.MonitoringJobFrequency;

                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                       await job.Execute();
                    }
                    catch (Exception e)
                    {
                        log.WriteErrorAsync("MonitoringService", "Program", "MonitoringJob", e, DateTime.UtcNow).Wait();
                    }

                    await Task.Delay(secondsDelay * 1000);
                }

                end.WaitOne();
            });

            AssemblyLoadContext.Default.Unloading += ctx =>
            {
                Console.WriteLine("SIGTERM recieved");
                cts.Cancel();
            };

            host.Run();

            log.WriteInfoAsync("MonitoringService", "Program", "Main", "Monitoring Service has been stopped", DateTime.UtcNow).Wait();
            //backUpService.CreateBackupAsync().Wait();
            end.Set();
        }
    }
}
