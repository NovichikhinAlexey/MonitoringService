namespace Lykke.MonitoringServiceApiCaller.Models
{
    using Newtonsoft.Json;

    public class MonitoringObjectPingModel
    {
        [JsonProperty(PropertyName = "serviceName")]
        public string ServiceName { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }
    }
}
