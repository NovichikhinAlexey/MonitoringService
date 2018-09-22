namespace Lykke.MonitoringServiceApiCaller.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public class ListDataUrlMonitoringObjectModel
    {
        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public IList<UrlMonitoringObjectModel> Data { get; set; }
    }
}
