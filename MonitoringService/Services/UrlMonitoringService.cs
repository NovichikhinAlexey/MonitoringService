using Core.Services;
using System;
using Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Repositories;

namespace Services
{
    public class UrlMonitoringService : IUrlMonitoringService
    {
        private readonly IApiMonitoringObjectRepository _apiMonitoringObjectRepository;
        private readonly IMonitoringObjectRepository _monitoringObjectRepository;

        public UrlMonitoringService(IApiMonitoringObjectRepository apiMonitoringObjectRepository, 
            IMonitoringObjectRepository monitoringObjectRepository)
        {
            _apiMonitoringObjectRepository = apiMonitoringObjectRepository;
            _monitoringObjectRepository = monitoringObjectRepository;
        }

        public async Task MonitorAsync(IMonitoringObject aObject)
        {
            aObject.LastTime = DateTime.UtcNow;
            await _apiMonitoringObjectRepository.InsertAsync(aObject);
            await _monitoringObjectRepository.InsertAsync(aObject);
        }

        public async Task<IEnumerable<IMonitoringObject>> GetAllAsync()
        {
            return await _apiMonitoringObjectRepository.GetAllAsync();
        }
    }
}
