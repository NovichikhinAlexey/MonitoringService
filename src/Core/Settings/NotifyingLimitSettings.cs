using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Models;
using Core.Services;

namespace Core.Settings
{
    public class NotifyingLimitSettings : INotifyingLimitSettings
    {
        private List<LimitMonitor> _limits = new List<LimitMonitor>();
        private object _lockObj = new object();
        /// <summary>
        /// Seconds
        /// </summary>
        private readonly int _defaultLimit = 3600;

        public List<IssueIndicatorObject> CheckAndUpdateLimits(string serviceName, 
            IEnumerable<IssueIndicatorObject> issueIndicators)
        {
            var result = new List<IssueIndicatorObject>();
            if (issueIndicators == null)
                return result;

            lock (_lockObj)
            {
                foreach (var indicator in issueIndicators)
                {
                    var limit = _limits.FirstOrDefault(x => x.ServiceName == serviceName && x.Indicator == indicator.Type);
                    if (limit == null)
                    {
                        limit = new LimitMonitor
                        {
                            ServiceName = serviceName,
                            Indicator = indicator.Type,
                            LastNotify = null,
                            TimeLimit = _defaultLimit
                        };
                        _limits.Add(limit);
                    }

                    if (DateTime.Now.Subtract(limit.LastNotify ?? DateTime.MinValue).TotalSeconds < limit.TimeLimit)
                        continue;

                    result.Add(indicator);
                    limit.LastNotify = DateTime.Now;
                }
            }

            return result;
        }

        internal class LimitMonitor
        {
            internal string ServiceName { get; set; }
            internal string Indicator { get; set; }
            internal DateTime? LastNotify { get; set; }
            /// <summary>
            /// Seconds
            /// </summary>
            internal int TimeLimit { get; set; }
        }
    }
}
