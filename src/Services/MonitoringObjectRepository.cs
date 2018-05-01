using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;
using Core.Repositories;

namespace Services
{
    public class MonitoringObjectRepository : IMonitoringObjectRepository
    {
        private readonly IDictionary<string, IMonitoringObject> _monitoringDictionary;
        private readonly Guid _guid = Guid.NewGuid();

        public MonitoringObjectRepository()
        {
            _monitoringDictionary = new Dictionary<string, IMonitoringObject>();
        }

        public Task<IEnumerable<IMonitoringObject>> GetAllAsync()
        {
            return Task.FromResult<IEnumerable<IMonitoringObject>>(_monitoringDictionary.Values);
        }

        public Task<IMonitoringObject> GetByNameAsync(string serviceName)
        {
            _monitoringDictionary.TryGetValue(serviceName, out IMonitoringObject mObject);

            return Task.FromResult(mObject);
        }

        public Task InsertAsync(IMonitoringObject mObject)
        {
            _monitoringDictionary[mObject.ServiceName] = mObject;

            return Task.CompletedTask;
        }

        public Task RemoveAsync(string serviceName)
        {
            _monitoringDictionary.Remove(serviceName);

            return Task.CompletedTask;
        }
    }
}
