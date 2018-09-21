using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Repositories
{
    public interface IApiMonitoringObjectRepository
    {
        Task<IEnumerable<IMonitoringObject>> GetAllAsync();
        Task InsertAsync(IMonitoringObject aObject);
        Task<IMonitoringObject> GetByNameAsync(string serviceName);
        Task RemoveByNameAsync(string serviceName);
        Task RemoveByUrlAsync(string url);
    }
}
