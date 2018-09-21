using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.MonitoringServiceApiCaller.Models;
using Refit;

namespace Lykke.MonitoringServiceApiCaller
{
    [PublicAPI]
    public interface IUrlMonitoring
    {
        [Get("/api/UrlMonitoring")]
        Task<ListDataUrlMonitoringObjectModel> Get();

        [Post("/api/UrlMonitoring/monitor")]
        Task Monitor(UrlMonitoringObjectModel model);
    }
}
