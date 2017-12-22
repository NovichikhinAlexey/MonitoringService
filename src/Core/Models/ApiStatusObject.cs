namespace Core.Models
{
    public interface IApiStatusObject
    {
        string Version { get; set; }
    }

    //{"Version":"1.45.2855.0","Env":"slot=A"}
    public class ApiStatusObject : IApiStatusObject
    {
        public string Version { get; set; }
    }
}
