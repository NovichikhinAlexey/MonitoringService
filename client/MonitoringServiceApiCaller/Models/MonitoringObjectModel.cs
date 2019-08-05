namespace Lykke.MonitoringServiceApiCaller.Models
{
    using Newtonsoft.Json;

    public class MonitoringObjectModel
    {
        [JsonProperty(PropertyName = "serviceName")]
        public string ServiceName { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonProperty(PropertyName = "lastPing")]
        public System.DateTime? LastPing { get; set; }

        [JsonProperty(PropertyName = "skipUntil")]
        public System.DateTime? SkipUntil { get; set; }

        [JsonProperty(PropertyName = "url")]
        public string Url { get; set; }

        [JsonProperty(PropertyName = "envInfo")]
        public string EnvInfo { get; set; }
    }
}
