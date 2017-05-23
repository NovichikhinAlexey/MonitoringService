using Core.Models;
using Core.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IUrlMonitoringService
    {
        Task Monitor(IMonitoringObject aObject);

        Task<IEnumerable<IMonitoringObject>> GetAll();
    }
}
