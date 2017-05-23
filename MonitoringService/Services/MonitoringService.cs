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
            var result = new List<IMonitoringObject>();
            var inMemmory = await _monitorObjectRepository.GetAllAsync();
            var azure = await _apiMonitoringObjectRepository.GetAllAsync();
            result.AddRange(inMemmory);
            result.AddRange(azure);

            return result;
        }

        public async Task Mute(string serviceName, int minutes)
        {
            IMonitoringObject mObject = await GetByName(serviceName);
            mObject.SkipCheckUntil = DateTime.UtcNow.AddMinutes(minutes);
            await Insert(mObject);
        }

        public async Task Ping(IMonitoringObject mObject)
        {
            await _monitorObjectRepository.InsertAsync(mObject);
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
            await Insert(mObject);
        }

        private async Task Insert(IMonitoringObject mObject)
        {
            if (string.IsNullOrEmpty(mObject.Url))
            {
                await _monitorObjectRepository.InsertAsync(mObject);
            }
            else
            {
                await _apiMonitoringObjectRepository.InsertAsync(mObject);
            }
        }
    }
}
