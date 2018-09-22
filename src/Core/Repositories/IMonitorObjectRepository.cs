using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Repositories
{
    public interface IMonitoringObjectRepository
    {
        Task InsertAsync(IMonitoringObject mObject);
        Task<IEnumerable<IMonitoringObject>> GetAllAsync();
        Task<IMonitoringObject> GetByNameAsync(string serviceName);
        Task RemoveByNameAsync(string serviceName);
        Task RemoveByUrlAsync(string url);
    }
}
