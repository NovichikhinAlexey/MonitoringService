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
        private IApiMonitoringObjectRepository _apiMonitoringObjectRepository;

        public UrlMonitoringService(IApiMonitoringObjectRepository apiMonitoringObjectRepository)
        {
            _apiMonitoringObjectRepository = apiMonitoringObjectRepository;
        }

        public async Task MonitorAsync(IMonitoringObject aObject)
        {
            aObject.LastTime = DateTime.UtcNow;
            await _apiMonitoringObjectRepository.InsertAsync(aObject);
        }

        public async Task<IEnumerable<IMonitoringObject>> GetAllAsync()
        {
            return await _apiMonitoringObjectRepository.GetAllAsync();
        }
    }
}
