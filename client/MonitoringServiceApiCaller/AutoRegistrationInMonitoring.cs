using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using Common.Log;
using Lykke.MonitoringServiceApiCaller.Models;

namespace Lykke.MonitoringServiceApiCaller
{
    public static class AutoRegistrationInMonitoring
    {
        public async static Task RegisterAsync(IConfigurationRoot configuration, string monitoringServiceUrl, ILog log)
        {
            try
            {
                string envVariableName = "MyMonitoringUrl";
                string myMonitoringUrl = configuration[envVariableName];
                if (string.IsNullOrWhiteSpace(myMonitoringUrl))
                {
                    myMonitoringUrl = "0.0.0.0";
                    log.WriteInfo("Auto-registration in monitoring", "", $"{envVariableName} environment variable is not found. Using {myMonitoringUrl} for monitoring registration");
                }
                var monitoringService = new MonitoringServiceFacade(monitoringServiceUrl);
                await monitoringService.MonitorUrl(
                    new UrlMonitoringObjectModel
                    {
                        Url = myMonitoringUrl,
                        ServiceName = PlatformServices.Default.Application.ApplicationName,
                    });
                log.WriteInfo("Auto-registration in monitoring", "", $"Auto-registered in Monitoring on {myMonitoringUrl}");
            }
            catch (Exception ex)
            {
                log.WriteError("Auto-registration in monitoring", "", ex);
            }
        }
    }
}
