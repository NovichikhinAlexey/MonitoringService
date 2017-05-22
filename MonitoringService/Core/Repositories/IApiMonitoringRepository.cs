using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IApiMonitoringObject
    {
        string ServiceName { get; set; }
        string Url { get; set; }
    }

    public class ApiMonitoringObject : IApiMonitoringObject
    {
        public string ServiceName { get; set; }
        public string Url { get; set; }
    }

    public interface IApiMonitoringObjectRepository
    {
        Task<IEnumerable<IApiMonitoringObject>> GetAll();

        Task Insert(IApiMonitoringObject aObject);
    }
}
