using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IMonitoringObjectRepository
    {
        Task Insert(MonitoringObject mObject);
        Task<IEnumerable<MonitoringObject>> GetAll();
        Task<MonitoringObject> GetByName(string serviceName);
    }
}
