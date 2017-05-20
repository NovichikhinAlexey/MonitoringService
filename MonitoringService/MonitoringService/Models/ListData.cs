using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonitoringService.Models
{
    public class ListData<T>
    {
        public IEnumerable<T>  Data { get; set; }
    }
}
