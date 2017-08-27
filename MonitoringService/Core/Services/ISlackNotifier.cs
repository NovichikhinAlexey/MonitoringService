using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface ISlackNotifier
    {
        Task SendWarningMsgAsync(string message);
        Task SendMonitorMsgAsync(string message);
    }
}
