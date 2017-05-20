using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public class MonitoringObject
    {
        public string ServiceName{ get; set; }
        public string Version { get; set; }
        public DateTime LastTime { get; set; }
        public DateTime? SkipCheckUntil { get; set; }
    }
}
