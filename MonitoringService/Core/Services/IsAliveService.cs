using Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IIsAliveService
    {
        Task<IApiStatusObject> GetStatusAsync(string url, CancellationToken cancellationToken);
    }
}
