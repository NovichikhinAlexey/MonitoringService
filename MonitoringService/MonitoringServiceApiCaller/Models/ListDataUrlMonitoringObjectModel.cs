// Code generated by Microsoft (R) AutoRest Code Generator 1.0.1.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Lykke.MonitoringServiceApiCaller.Models
{
    using Lykke.MonitoringServiceApiCaller;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public partial class ListDataUrlMonitoringObjectModel
    {
        /// <summary>
        /// Initializes a new instance of the ListDataUrlMonitoringObjectModel
        /// class.
        /// </summary>
        public ListDataUrlMonitoringObjectModel()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the ListDataUrlMonitoringObjectModel
        /// class.
        /// </summary>
        public ListDataUrlMonitoringObjectModel(IList<UrlMonitoringObjectModel> data = default(IList<UrlMonitoringObjectModel>))
        {
            Data = data;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public IList<UrlMonitoringObjectModel> Data { get; set; }

    }
}
