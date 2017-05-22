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

        public async Task Mute(string serviceName, int minutes)
        {
            MonitoringObject mObject = await _monitorObjectRepository.GetByName(serviceName);
            mObject.SkipCheckUntil = DateTime.UtcNow.AddMinutes(minutes);
            await _monitorObjectRepository.Insert(mObject);
        }

        public async Task Ping(MonitoringObject mObject)
        {
            await _monitorObjectRepository.Insert(mObject);
        }

        public async Task Unmute(string serviceName)
        {
            MonitoringObject mObject = await _monitorObjectRepository.GetByName(serviceName);
            mObject.SkipCheckUntil = null;
            await _monitorObjectRepository.Insert(mObject);
        }
    }
}
