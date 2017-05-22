using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MonitoringService.Models
{
    [DataContract]
    public class UrlMonitoringObjectModel
    {
        [DataMember(Name= "serviceName")]
        public string ServiceName { get; set; }

        [DataMember(Name = "url")]
        [Url(ErrorMessage ="It should be a valid url")]
        public string Url { get; set; }
    }
}
