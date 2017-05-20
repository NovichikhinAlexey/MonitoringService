using MonitoringServiceApiCaller.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonitoringServiceApiCaller
{
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
    }
}
