using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IMonitoringObjectRepository
    {
        Task InsertAsync(IMonitoringObject mObject);
        Task<IEnumerable<IMonitoringObject>> GetAllAsync();
        Task<IMonitoringObject> GetByNameAsync(string serviceName);
        Task RemoveAsync(string serviceName);
    }
}
