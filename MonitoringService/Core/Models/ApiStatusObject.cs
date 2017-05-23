using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Models
{
    public interface IApiStatusObject
    {
        string Env { get; set; }
        string Version { get; set; }
    }

    //{"Version":"1.45.2855.0","Env":"slot=A"}
    public class ApiStatusObject : IApiStatusObject
    {
        public string Env { get; set; }
        public string Version { get; set; }
    }
}
