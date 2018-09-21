using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Services
{
    public interface IUrlMonitoringService
    {
        Task MonitorAsync(IMonitoringObject aObject);

        Task<IEnumerable<IMonitoringObject>> GetAllAsync();
    }
}
