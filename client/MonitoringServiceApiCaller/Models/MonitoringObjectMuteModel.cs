namespace Lykke.MonitoringServiceApiCaller.Models
{
    using Newtonsoft.Json;

    public class MonitoringObjectMuteModel
    {
        [JsonProperty(PropertyName = "serviceName")]
        public string ServiceName { get; set; }

        [JsonProperty(PropertyName = "minutes")]
        public int? Minutes { get; set; }
    }
}
