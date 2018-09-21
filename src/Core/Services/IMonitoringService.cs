using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Services
{
    public interface IMonitoringService
    {
        //In future may require async
        Task Ping(IMonitoringObject mObject);
        Task<IEnumerable<IMonitoringObject>> GetCurrentSnapshot();
        Task Mute(string serviceName, int minutes);
        Task Unmute(string serviceName);
        Task Remove(string serviceName);
        Task<IMonitoringObject> GetByName(string serviceName);
    }
}
