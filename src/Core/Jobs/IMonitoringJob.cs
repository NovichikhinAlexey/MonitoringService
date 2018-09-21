using System.Threading.Tasks;

namespace Core.Jobs
{
    public interface IMonitoringJob
    {
        Task CheckAPIs();
        Task CheckJobs();
    }
}
