using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Jobs
{
    public interface IMonitoringJob
    {
        Task Execute();
    }
}
