using JetBrains.Annotations;

namespace Lykke.MonitoringServiceApiCaller
{
    [PublicAPI]
    public interface IMonitoringServiceClient
    {
        IMonitoring Monitoring { get; }

        IUrlMonitoring UrlMonitoring { get; }
    }
}