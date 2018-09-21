using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;
using Core.Repositories;

namespace Services
{
    public class MonitoringObjectRepository : IMonitoringObjectRepository
    {
        private readonly ConcurrentDictionary<string, IMonitoringObject> _monitoringDictionary;

        public MonitoringObjectRepository()
        {
            _monitoringDictionary = new ConcurrentDictionary<string, IMonitoringObject>();
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

        public Task RemoveByNameAsync(string serviceName)
        {
            _monitoringDictionary.TryRemove(serviceName, out _);

            return Task.CompletedTask;
        }

        public Task RemoveByUrlAsync(string url)
        {
            IMonitoringObject objToDelete = null;
            foreach (var monitoringObject in _monitoringDictionary.Values)
            {
                if (monitoringObject.Url != url)
                    continue;

                objToDelete = monitoringObject;
                break;
            }

            if (objToDelete != null)
                _monitoringDictionary.TryRemove(objToDelete.ServiceName, out _);

            return Task.CompletedTask;
        }
    }
}
