using System;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IApiHealthCheckError
    {
        string ServiceName { get; set; }
        string LastError { get; set; }
        DateTime Date { get; set; }
    }

    public class ApiHealthCheckError : IApiHealthCheckError
    {
        public string ServiceName { get; set; }
        public string LastError { get; set; }
        public DateTime Date { get; set; }
    }

    public interface IApiHealthCheckErrorRepository
    {
        Task<IApiHealthCheckError> GetByIdAsync(string serviceName);

        Task InsertAsync(IApiHealthCheckError service);
    }
}
