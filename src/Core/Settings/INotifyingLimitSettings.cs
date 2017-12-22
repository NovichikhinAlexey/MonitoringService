using System.Collections.Generic;
using Core.Models;

namespace Core.Services
{
    public interface INotifyingLimitSettings
    {
        List<IssueIndicatorObject> CheckAndUpdateLimits(string serviceName, IEnumerable<IssueIndicatorObject> issueIndicators);
    }
}