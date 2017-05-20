using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IMonitoringService
    {
        //In future may require async
        Task Ping(MonitoringObject mObject);
        Task<IEnumerable<MonitoringObject>> GetCurrentSnapshot();
    }
}
