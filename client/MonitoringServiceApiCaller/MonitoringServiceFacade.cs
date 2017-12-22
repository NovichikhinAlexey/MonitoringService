using Lykke.MonitoringServiceApiCaller.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.MonitoringServiceApiCaller
{
    /// <summary>
    /// MonitoringServiceFacade uses HttpClient. Consider using it as singleton. 
    /// </summary>
    public class MonitoringServiceFacade
    {
        private readonly IMonitoringService _monitoringService;

        public MonitoringServiceFacade(string monitoringServiceUrl)
        {
            _monitoringService = new MonitoringService(new Uri(monitoringServiceUrl));
        }

        public MonitoringServiceFacade(IMonitoringService monitoringService)
        {
            _monitoringService = monitoringService;
        }

        public async Task<IEnumerable<MonitoringObjectModel>> GetAll()
        {
            ListDataMonitoringObjectModel model = await _monitoringService.ApiMonitoringGetAsync();

            return model.Data;
        }

        public async Task Ping(MonitoringObjectPingModel pingModel)
        {
            await _monitoringService.ApiMonitoringPingPostAsync(pingModel);
        }

        public async Task<MonitoringObjectModel> GetService(string serviceName)
        {
            MonitoringObjectModel result = await _monitoringService.ApiMonitoringByServiceNameGetAsync(serviceName);

            return result;
        }

        public async Task RemoveService(string serviceName)
        {
            await _monitoringService.ApiMonitoringRemoveByServiceNameDeleteAsync(serviceName);
        }

        public async Task Mute(MonitoringObjectMuteModel muteModel)
        {
            await _monitoringService.ApiMonitoringMutePostAsync(muteModel);
        }

        public async Task Unmute(MonitoringObjectUnmuteModel unmuteModel)
        {
            await _monitoringService.ApiMonitoringUnmutePostAsync(unmuteModel);
        }

        public async Task MonitorUrl(UrlMonitoringObjectModel urlMonitoringObjectModel)
        {
            await _monitoringService.ApiUrlMonitoringMonitorPostAsync(urlMonitoringObjectModel);
        }
    }
}
