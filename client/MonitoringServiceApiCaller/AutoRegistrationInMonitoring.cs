using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using Common.Log;
using Lykke.MonitoringServiceApiCaller.Models;

namespace Lykke.MonitoringServiceApiCaller
{
    /// <summary>
    /// Class for auto-registration in monitoring service
    /// </summary>
    public static class AutoRegistrationInMonitoring
    {
        private const string _myMonitoringUrlEnvVarName = "MyMonitoringUrl";
        private const string _missingEnvVarUrl = "0.0.0.0";
        private const string _myMonitoringNameEnvVarName = "MyMonitoringName";
        private const string _disableAutoRegistrationEnvVarName = "DisableAutoRegistrationInMonitoring";

        /// <summary>
        /// Registers calling application in monitoring service based on application url from environemnt variable.
        /// </summary>
        /// <param name="configuration">Application configuration that is used for environemnt variable search.</param>
        /// <param name="monitoringServiceUrl">Monitoring service url.</param>
        /// <param name="log">ILog implementation. LogToConsole is used on case this parmeter is null.</param>
        /// <returns></returns>
        public async static Task RegisterAsync(
            IConfigurationRoot configuration,
            string monitoringServiceUrl,
            ILog log)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            string disableAutoRegistrationStr = configuration[_disableAutoRegistrationEnvVarName];
            if (bool.TryParse(disableAutoRegistrationStr, out bool disableAutoRegistration) && disableAutoRegistration)
                return;

            if (string.IsNullOrWhiteSpace(monitoringServiceUrl))
                throw new ArgumentException("Argument is empty", nameof(monitoringServiceUrl));

            if (log == null)
                log = new LogToConsole();

            try
            {
                string myMonitoringUrl = configuration[_myMonitoringUrlEnvVarName];
                if (string.IsNullOrWhiteSpace(myMonitoringUrl))
                {
                    myMonitoringUrl = _missingEnvVarUrl;
                    log.WriteMonitor("Auto-registration in monitoring", "", $"{_myMonitoringUrlEnvVarName} environment variable is not found. Using {myMonitoringUrl} for monitoring registration");
                }
                string myMonitoringName = configuration[_myMonitoringNameEnvVarName];
                if (string.IsNullOrWhiteSpace(myMonitoringName))
                    myMonitoringName = PlatformServices.Default.Application.ApplicationName;
                var monitoringService = new MonitoringServiceFacade(monitoringServiceUrl);
                await monitoringService.MonitorUrl(
                    new UrlMonitoringObjectModel
                    {
                        Url = myMonitoringUrl,
                        ServiceName = myMonitoringName,
                    });
                log.WriteMonitor("Auto-registration in monitoring", "", $"Auto-registered in Monitoring with name {myMonitoringName} on {myMonitoringUrl}");
            }
            catch (Exception ex)
            {
                log.WriteError("Auto-registration in monitoring", "", ex);
            }
        }
    }
}
