using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MonitoringService.Models
{
    [DataContract]
    public class MonitoringObjectModel
    {
        [DataMember(Name= "serviceName")]
        public string ServiceName { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "lastPing")]
        public DateTime LastPing { get; set; }

        [DataMember(Name = "skipUntil")]
        public DateTime? SkipUntil { get; set; }
    }

    [DataContract]
    public class MonitoringObjectPingModel
    {
        [DataMember(Name = "serviceName")]
        public string ServiceName { get; set; }

        [DataMember(Name = "version")]
        public string Version { get; set; }
    }

    [DataContract]
    public class MonitoringObjectMuteModel
    {
        [DataMember(Name = "serviceName")]
        public string ServiceName { get; set; }

        [DataMember(Name = "minutes")]
        public int Minutes { get; set; }
    }

    [DataContract]
    public class MonitoringObjectUnmuteModel
    {
        [DataMember(Name = "serviceName")]
        public string ServiceName { get; set; }
    }
}
