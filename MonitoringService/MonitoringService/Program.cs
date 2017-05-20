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

            IMonitoringJob job = Startup.ServiceProvider.GetService<IMonitoringJob>();
            var end = new ManualResetEvent(false);
            CancellationTokenSource cts = new CancellationTokenSource();

            Task.Run(async () => 
            {
                while (!cts.IsCancellationRequested)
                {
                    await job.Execute();
                    await Task.Delay(60 * 1000);
                }

                end.WaitOne();
            });

            AssemblyLoadContext.Default.Unloading += ctx =>
            {
                Console.WriteLine("SIGTERM recieved");
                cts.Cancel();
            };

            host.Run();
            end.Set();
        }
    }
}
