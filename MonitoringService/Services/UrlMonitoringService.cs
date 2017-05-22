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

        public async Task Monitor(IApiMonitoringObject aObject)
        {
            await _apiMonitoringObjectRepository.Insert(aObject);
        }

        public async Task<IEnumerable<IApiMonitoringObject>> GetAll()
        {
            return await _apiMonitoringObjectRepository.GetAll();
        }
    }
}
