using Core.Services;
using System;
using Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Repositories;
using System.Collections;

namespace Services
{
    public class MonitoringService : IMonitoringService
    {
        private readonly IMonitoringObjectRepository _monitorObjectRepository;
        private readonly IApiMonitoringObjectRepository _apiMonitoringObjectRepository;

        public MonitoringService(IMonitoringObjectRepository monitorObjectRepository,
            IApiMonitoringObjectRepository apiMonitoringObjectRepository)
        {
            _monitorObjectRepository = monitorObjectRepository;
            _apiMonitoringObjectRepository = apiMonitoringObjectRepository;
        }

        public async Task<IMonitoringObject> GetByName(string serviceName)
        {
            IMonitoringObject mObject = await _monitorObjectRepository.GetByNameAsync(serviceName) ??
                await _apiMonitoringObjectRepository.GetByNameAsync(serviceName);

            return mObject;
        }

        public async Task<IEnumerable<IMonitoringObject>> GetCurrentSnapshot()
        {
            var inMemmory = await _monitorObjectRepository.GetAllAsync();

            return inMemmory;
        }

        public async Task Mute(string serviceName, int minutes)
        {
            IMonitoringObject mObject = await GetByName(serviceName);
            mObject.SkipCheckUntil = DateTime.UtcNow.AddMinutes(minutes);
            await InsertAsync(mObject);
        }

        public async Task Ping(IMonitoringObject mObject)
        {
            await InsertAsync(mObject);
        }

        public async Task Remove(string serviceName)
        {
            await _monitorObjectRepository.RemoveAsync(serviceName);
            await _apiMonitoringObjectRepository.RemoveAsync(serviceName);
        }

        public async Task Unmute(string serviceName)
        {
            IMonitoringObject mObject = await GetByName(serviceName);
            mObject.SkipCheckUntil = null;
            await InsertAsync(mObject);
        }

        private async Task InsertAsync(IMonitoringObject mObject)
        {
            await _monitorObjectRepository.InsertAsync(mObject);
        }
    }
}
