namespace Lykke.MonitoringServiceApiCaller.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class ListDataMonitoringObjectModel
    {
        [JsonProperty(PropertyName = "data")]
        public IList<MonitoringObjectModel> Data { get; set; }
    }
}
