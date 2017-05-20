using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface ISlackNotifier
    {
        Task WarningAsync(string message);
        Task ErrorAsync(string message);
    }
}
