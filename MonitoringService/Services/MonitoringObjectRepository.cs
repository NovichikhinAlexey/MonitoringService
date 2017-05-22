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
        private IDictionary<string, MonitoringObject> _monitoringDictionary;
        private readonly Guid _guid = Guid.NewGuid();

        public MonitoringObjectRepository()
        {
            _monitoringDictionary = new Dictionary<string, MonitoringObject>();
        }

        public async Task<IEnumerable<MonitoringObject>> GetAll()
        {
            return _monitoringDictionary.Values;
        }

        public async Task<MonitoringObject> GetByName(string serviceName)
        {
            MonitoringObject mObject;
            _monitoringDictionary.TryGetValue(serviceName, out mObject);

            return mObject;
        }

        public async Task Insert(MonitoringObject mObject)
        {
            _monitoringDictionary[mObject.ServiceName] = mObject;
        }
    }
}
