// Code generated by Microsoft (R) AutoRest Code Generator 1.0.1.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Lykke.MonitoringServiceApiCaller.Models
{
    using Lykke.MonitoringServiceApiCaller;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class MonitoringObjectMuteModel
    {
        /// <summary>
        /// Initializes a new instance of the MonitoringObjectMuteModel class.
        /// </summary>
        public MonitoringObjectMuteModel()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the MonitoringObjectMuteModel class.
        /// </summary>
        public MonitoringObjectMuteModel(string serviceName = default(string), int? minutes = default(int?))
        {
            ServiceName = serviceName;
            Minutes = minutes;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "serviceName")]
        public string ServiceName { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "minutes")]
        public int? Minutes { get; set; }

    }
}