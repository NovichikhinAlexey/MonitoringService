namespace Lykke.MonitoringServiceApiCaller.Models
{
    using Newtonsoft.Json;

    public class UrlMonitoringObjectModel
    {
        [JsonProperty(PropertyName = "serviceName")]
        public string ServiceName { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }
    }
}
