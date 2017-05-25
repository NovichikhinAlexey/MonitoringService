using Common.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringService.Utils
{
    public static class InProcessJobHelper
    {
        public static Task StartJob(Func<Task> job, CancellationToken cToken, int frequencyInSeconds, ILog log)
        {
            return Task.Run(async () =>
            {
                while (!cToken.IsCancellationRequested)
                {
                    try
                    {
                        await job();
                    }
                    catch (Exception e)
                    {
                        log.WriteErrorAsync("MonitoringService", "Program", "MonitoringJob", e, DateTime.UtcNow).Wait();
                    }

                    await Task.Delay(frequencyInSeconds * 1000, cToken);
                }
            });
        }
    }
}
