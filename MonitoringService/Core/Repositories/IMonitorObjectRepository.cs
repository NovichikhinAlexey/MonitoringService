using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IMonitoringObjectRepository
    {
        Task Insert(IMonitoringObject mObject);
        Task<IEnumerable<IMonitoringObject>> GetAll();
        Task<IMonitoringObject> GetByName(string serviceName);
        Task Remove(string serviceName);
    }
}
