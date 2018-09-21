using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using Lykke.HttpClientGenerator.Infrastructure;
using Lykke.MonitoringServiceApiCaller.Models;

namespace Lykke.MonitoringServiceApiCaller
{
    /// <summary>
    /// MonitoringServiceFacade uses HttpClient. Consider using it as singleton. 
    /// </summary>
    public class MonitoringServiceFacade : IMonitoringServiceClient
    {
        public IMonitoring Monitoring { get; }

        public IUrlMonitoring UrlMonitoring { get; }

        public MonitoringServiceFacade(string monitoringServiceUrl)
            : this(monitoringServiceUrl, null)
        {
        }

        public MonitoringServiceFacade(string monitoringServiceUrl, [CanBeNull] Func<HttpClientGeneratorBuilder, HttpClientGeneratorBuilder> builderConfigure)
        {
            var clientBuilder = HttpClientGenerator.HttpClientGenerator.BuildForUrl(monitoringServiceUrl)
                .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper());

            clientBuilder = builderConfigure?.Invoke(clientBuilder) ?? clientBuilder.WithoutRetries();

            var clientGenerator = clientBuilder.Create();
            Monitoring = clientGenerator.Generate<IMonitoring>();
            UrlMonitoring = clientGenerator.Generate<IUrlMonitoring>();
        }

        public async Task<IEnumerable<MonitoringObjectModel>> GetAll()
        {
            ListDataMonitoringObjectModel model = await Monitoring.Get();

            return model.Data;
        }

        public async Task Ping(MonitoringObjectPingModel pingModel)
        {
            await Monitoring.Ping(pingModel);
        }

        public async Task<MonitoringObjectModel> GetService(string serviceName)
        {
            return await Monitoring.GetByServiceName(serviceName);
        }

        public async Task RemoveService(string serviceName)
        {
            await Monitoring.RemoveByServiceName(serviceName);
        }

        public async Task Mute(MonitoringObjectMuteModel muteModel)
        {
            await Monitoring.Mute(muteModel);
        }

        public async Task Unmute(MonitoringObjectUnmuteModel unmuteModel)
        {
            await Monitoring.Unmute(unmuteModel);
        }

        public async Task MonitorUrl(UrlMonitoringObjectModel urlMonitoringObjectModel)
        {
            await UrlMonitoring.Monitor(urlMonitoringObjectModel);
        }
    }
}