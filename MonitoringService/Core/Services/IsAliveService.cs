using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IIsAliveService
    {
        Task<IApiStatusObject> GetStatusAsync(string url, CancellationToken cancellationToken);
    }
}
