using Core.Services;
using System;
using Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Repositories;

namespace Services
{
    public class MonitoringService : IMonitoringService
    {
        
        private IMonitoringObjectRepository _monitorObjectRepository;

        public MonitoringService(IMonitoringObjectRepository monitorObjectRepository)
        {
            _monitorObjectRepository = monitorObjectRepository;
        }

        public async Task<IEnumerable<MonitoringObject>> GetCurrentSnapshot()
        {
            return await _monitorObjectRepository.GetAll();
            
        }

        public async Task Ping(MonitoringObject mObject)
        {
            await _monitorObjectRepository.Insert(mObject);
        }
    }
}
