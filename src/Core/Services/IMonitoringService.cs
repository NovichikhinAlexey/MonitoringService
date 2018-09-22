using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Services
{
    public interface IMonitoringService
    {
        Task PingAsync(IMonitoringObject mObject);
        Task<IEnumerable<IMonitoringObject>> GetCurrentSnapshotAsync();
        Task MuteAsync(string serviceName, int minutes);
        Task UnmuteAsync(string serviceName);
        Task RemoveByNameAsync(string serviceName);
        Task RemoveByUrlAsync(string url);
        Task<IMonitoringObject> GetByNameAsync(string serviceName);
    }
}
