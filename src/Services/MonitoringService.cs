using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;
using Core.Repositories;
using Core.Services;

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

        public async Task<IMonitoringObject> GetByNameAsync(string serviceName)
        {
            IMonitoringObject mObject = await _monitorObjectRepository.GetByNameAsync(serviceName) ??
                await _apiMonitoringObjectRepository.GetByNameAsync(serviceName);

            return mObject;
        }

        public async Task<IEnumerable<IMonitoringObject>> GetCurrentSnapshotAsync()
        {
            var inMemmory = await _monitorObjectRepository.GetAllAsync();

            return inMemmory;
        }

        public async Task MuteAsync(string serviceName, int minutes)
        {
            IMonitoringObject mObject = await GetByNameAsync(serviceName);
            mObject.SkipCheckUntil = DateTime.UtcNow.AddMinutes(minutes);
            await InsertAsync(mObject);
        }

        public async Task PingAsync(IMonitoringObject mObject)
        {
            await InsertAsync(mObject);
        }

        public async Task RemoveByNameAsync(string serviceName)
        {
            await _monitorObjectRepository.RemoveByNameAsync(serviceName);
            await _apiMonitoringObjectRepository.RemoveByNameAsync(serviceName);
        }

        public async Task RemoveByUrlAsync(string url)
        {
            await _monitorObjectRepository.RemoveByUrlAsync(url);
            await _apiMonitoringObjectRepository.RemoveByUrlAsync(url);
        }

        public async Task UnmuteAsync(string serviceName)
        {
            IMonitoringObject mObject = await GetByNameAsync(serviceName);
            mObject.SkipCheckUntil = null;
            await InsertAsync(mObject);
        }

        private async Task InsertAsync(IMonitoringObject mObject)
        {
            await _monitorObjectRepository.InsertAsync(mObject);
        }
    }
}
