using Core.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Models;
using System.Threading.Tasks;

namespace Services
{
    public class MonitoringObjectRepository : IMonitoringObjectRepository
    {
        private IDictionary<string, IMonitoringObject> _monitoringDictionary;
        private readonly Guid _guid = Guid.NewGuid();

        public MonitoringObjectRepository()
        {
            _monitoringDictionary = new Dictionary<string, IMonitoringObject>();
        }

        public async Task<IEnumerable<IMonitoringObject>> GetAllAsync()
        {
            return _monitoringDictionary.Values;
        }

        public async Task<IMonitoringObject> GetByNameAsync(string serviceName)
        {
            IMonitoringObject mObject;
            _monitoringDictionary.TryGetValue(serviceName, out mObject);

            return mObject;
        }

        public async Task InsertAsync(IMonitoringObject mObject)
        {
            _monitoringDictionary[mObject.ServiceName] = mObject;
        }

        public async Task RemoveAsync(string serviceName)
        {
            _monitoringDictionary.Remove(serviceName);
        }
    }
}
