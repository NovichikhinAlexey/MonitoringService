using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Rest;
using AsyncFriendlyStackTrace;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.MonitoringServiceApiCaller.Models;

namespace Lykke.MonitoringServiceApiCaller
{
    /// <summary>
    /// Class for auto-registration in monitoring service
    /// </summary>
    [PublicAPI]
    public static class AutoRegistrationInMonitoring
    {
        private const string MyMonitoringUrlEnvVarName = "MyMonitoringUrl";
        private const string MissingEnvVarUrl = "0.0.0.0";
        private const string MyMonitoringNameEnvVarName = "MyMonitoringName";
        private const string DisableAutoRegistrationEnvVarName = "DisableAutoRegistrationInMonitoring";
        private const string PodNameEnvVarName = "ENV_INFO";

        /// <summary>
        /// Registers calling application in monitoring service based on application url from environemnt variable.
        /// </summary>
        /// <param name="configuration">Application configuration that is used for environemnt variable search.</param>
        /// <param name="monitoringServiceUrl">Monitoring service url.</param>
        /// <param name="log">ILog implementation. LogToConsole is used on case this parmeter is null.</param>
        /// <returns></returns>
        [Obsolete("User RegisterInMonitoringServiceAsync extension method")]
        public static async Task RegisterAsync(
            IConfigurationRoot configuration,
            string monitoringServiceUrl,
            ILog log)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            if (log == null)
                throw new ArgumentNullException(nameof(log));

            string disableAutoRegistrationStr = configuration[DisableAutoRegistrationEnvVarName];
            if (bool.TryParse(disableAutoRegistrationStr, out bool disableAutoRegistration) && disableAutoRegistration)
            {
                log.WriteMonitor("Auto-registration in monitoring", "", $"Auto-registration is disabled");
                return;
            }

            if (string.IsNullOrWhiteSpace(monitoringServiceUrl))
                throw new ArgumentNullException(nameof(monitoringServiceUrl));

            string podTag = configuration[PodNameEnvVarName] ?? "";

            try
            {
                string myMonitoringUrl = configuration[MyMonitoringUrlEnvVarName];
                if (string.IsNullOrWhiteSpace(myMonitoringUrl))
                {
                    myMonitoringUrl = MissingEnvVarUrl;
                    log.WriteMonitor(
                        "Auto-registration in monitoring",
                        podTag,
                        $"{MyMonitoringUrlEnvVarName} environment variable is not found. Using {myMonitoringUrl} for monitoring registration");
                }
                string myMonitoringName = configuration[MyMonitoringNameEnvVarName];
                if (string.IsNullOrWhiteSpace(myMonitoringName))
                    myMonitoringName = PlatformServices.Default.Application.ApplicationName;
                var monitoringService = new MonitoringServiceFacade(monitoringServiceUrl);

                try
                {
                    var monitoringRegistration = await monitoringService.GetService(myMonitoringName);
                    if (monitoringRegistration.Url == myMonitoringUrl)
                    {
                        log.WriteMonitor("Auto-registration in monitoring", podTag, $"Service is already registered in monitoring with such url. Skipping.");
                        return;
                    }

                    if (monitoringRegistration.Url != MissingEnvVarUrl)
                    {
                        log.WriteMonitor("Auto-registration in monitoring", podTag, $"There is a registration for {myMonitoringName} in monitoring service!");

                        myMonitoringUrl = MissingEnvVarUrl;
                        string instanceTag = string.IsNullOrEmpty(podTag) ? Guid.NewGuid().ToString() : podTag;
                        myMonitoringName = $"{myMonitoringName}-{instanceTag}";
                    }
                }
                catch (HttpOperationException)
                {
                    //Duplicated registration is not found - proceed with usual registration
                }

                await monitoringService.MonitorUrl(
                    new UrlMonitoringObjectModel
                    {
                        Url = myMonitoringUrl,
                        ServiceName = myMonitoringName,
                    });
                log.WriteMonitor("Auto-registration in monitoring", podTag, $"Auto-registered in Monitoring with name {myMonitoringName} on {myMonitoringUrl}");
            }
            catch (Exception ex)
            {
                log.WriteMonitor("Auto-registration in monitoring", podTag, ex.ToAsyncString());
            }
        }

        /// <summary>
        /// Registers calling application in monitoring service based on application url from environemnt variable.
        /// </summary>
        /// <param name="configuration">Application configuration that is used for environemnt variable search.</param>
        /// <param name="monitoringServiceUrl">Monitoring service url.</param>
        /// <param name="healthNotifier">Health notifier</param>
        /// <returns></returns>
        public static async Task RegisterInMonitoringServiceAsync(
            [NotNull] this IConfigurationRoot configuration,
            [NotNull] string monitoringServiceUrl,
            [NotNull] IHealthNotifier healthNotifier)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            var disableAutoRegistrationStr = configuration[DisableAutoRegistrationEnvVarName];
            if (bool.TryParse(disableAutoRegistrationStr, out bool disableAutoRegistration) && disableAutoRegistration)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(monitoringServiceUrl))
            {
                throw new ArgumentException("Argument is empty", nameof(monitoringServiceUrl));
            }

            if (healthNotifier == null)
            {
                throw new ArgumentNullException(nameof(healthNotifier));
            }

            var podTag = configuration[PodNameEnvVarName] ?? string.Empty;

            try
            {
                var myMonitoringUrl = configuration[MyMonitoringUrlEnvVarName];
                if (string.IsNullOrWhiteSpace(myMonitoringUrl))
                {
                    myMonitoringUrl = MissingEnvVarUrl;
                    healthNotifier.Notify(
                        $"{MyMonitoringUrlEnvVarName} environment variable is not found. Using {myMonitoringUrl} for monitoring registration",
                        podTag);
                }

                var myMonitoringName = configuration[MyMonitoringNameEnvVarName];
                if (string.IsNullOrWhiteSpace(myMonitoringName))
                {
                    myMonitoringName = PlatformServices.Default.Application.ApplicationName;
                }

                var monitoringService = new MonitoringServiceFacade(monitoringServiceUrl);

                try
                {
                    var monitoringRegistration = await monitoringService.GetService(myMonitoringName);
                    if (monitoringRegistration.Url == myMonitoringUrl)
                    {
                        return;
                    }

                    healthNotifier.Notify($"There is a registration for {myMonitoringName} in monitoring service!", podTag);

                    myMonitoringUrl = MissingEnvVarUrl;
                    var instanceTag = string.IsNullOrEmpty(podTag) ? Guid.NewGuid().ToString() : podTag;
                    myMonitoringName = $"{myMonitoringName}-{instanceTag}";
                }
                catch (HttpOperationException)
                {
                    //Duplicated registration is not found - proceed with usual registration
                }

                await monitoringService.MonitorUrl(
                    new UrlMonitoringObjectModel
                    {
                        Url = myMonitoringUrl,
                        ServiceName = myMonitoringName,
                    });
                healthNotifier.Notify($"Auto-registered in Monitoring with name {myMonitoringName} on {myMonitoringUrl}", podTag);
            }
            catch (Exception ex)
            {
                healthNotifier.Notify(ex.ToAsyncString(), podTag);
            }
        }
    }
}
