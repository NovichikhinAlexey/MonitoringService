using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IApiMonitoringObjectRepository
    {
        Task<IEnumerable<IMonitoringObject>> GetAllAsync();
        Task InsertAsync(IMonitoringObject aObject);
        Task<IMonitoringObject> GetByNameAsync(string serviceName);
        Task RemoveAsync(string serviceName);
    }
}
