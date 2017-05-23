using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IApiMonitoringObjectRepository
    {
        Task<IEnumerable<IMonitoringObject>> GetAll();
        Task Insert(IMonitoringObject aObject);
        Task<IMonitoringObject> GetByName(string serviceName);
        Task Remove(string serviceName);
    }
}
