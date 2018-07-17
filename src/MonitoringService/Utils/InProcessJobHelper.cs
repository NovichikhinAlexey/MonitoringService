using Common.Log;
using System;
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
                        log.WriteError("InProcessJobHelper", "StartJob", e);
                    }

                    await Task.Delay(frequencyInSeconds * 1000, cToken);
                }
            });
        }
    }
}
