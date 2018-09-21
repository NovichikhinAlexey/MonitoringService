namespace Lykke.MonitoringServiceApiCaller.Models
{
    using Newtonsoft.Json;

    public class MonitoringObjectUnmuteModel
    {
        [JsonProperty(PropertyName = "serviceName")]
        public string ServiceName { get; set; }
    }
}
