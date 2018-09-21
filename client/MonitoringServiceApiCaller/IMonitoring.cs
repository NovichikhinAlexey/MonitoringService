using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.MonitoringServiceApiCaller.Models;
using Refit;

namespace Lykke.MonitoringServiceApiCaller
{
    [PublicAPI]
    public interface IMonitoring
    {
        [Get("/api/Monitoring")]
        Task<ListDataMonitoringObjectModel> Get();

        [Get("/api/Monitoring/{serviceName}")]
        Task<MonitoringObjectModel> GetByServiceName(string serviceName);

        [Post("/api/Monitoring/ping")]
        Task Ping(MonitoringObjectPingModel model);

        [Post("/api/Monitoring/mute")]
        Task Mute(MonitoringObjectMuteModel model);

        [Post("/api/Monitoring/unmute")]
        Task Unmute(MonitoringObjectUnmuteModel model);

        [Delete("/api/Monitoring/remove/{serviceName}")]
        Task RemoveByServiceName(string serviceName);

        [Delete("/api/Monitoring/removebyurl")]
        Task RemoveByUrl(string url);
    }
}
