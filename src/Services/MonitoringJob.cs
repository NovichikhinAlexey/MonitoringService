using Common.Log;
using Core.Exceptions;
using Core.Jobs;
using Core.Models;
using Core.Repositories;
using Core.Services;
using Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class MonitoringJob : IMonitoringJob
    {
        private readonly IMonitoringService _monitoringService;
        private readonly IBaseSettings _settings;
        private readonly ILog _log;
        private readonly IIsAliveService _isAliveService;
        private readonly INotifyingLimitSettings _notifyingLimitSettings;
        private readonly IApiHealthCheckErrorRepository _apiHealthCheckErrorRepository;
        private readonly object _lock = new object();

        public MonitoringJob(
            IMonitoringService monitoringService,
            IBaseSettings settings,
            IApiHealthCheckErrorRepository apiHealthCheckErrorRepository,
            IIsAliveService isAliveService,
            INotifyingLimitSettings notifyingLimitSettings,
            ILog log)
        {
            _log = log;
            _monitoringService = monitoringService;
            _settings = settings;
            _isAliveService = isAliveService;
            _notifyingLimitSettings = notifyingLimitSettings;
            _apiHealthCheckErrorRepository = apiHealthCheckErrorRepository;
        }

        public async Task CheckJobs()
        {
            DateTime now = DateTime.UtcNow;
            List<IMonitoringObject> jobsMonitoring = await GetMonitoringObjectsForProcessing(x => string.IsNullOrEmpty(x.Url));
            List<IMonitoringObject> fireNotificationsFor = new List<IMonitoringObject>();

            foreach (var @object in jobsMonitoring)
            {
                if (now - @object.LastTime > TimeSpan.FromSeconds(_settings.MaxTimeDifferenceInSeconds))
                {
                    fireNotificationsFor.Add(@object);
                }
            }

            fireNotificationsFor.ForEach(async service =>
            {
                var timeDiff = now - service.LastTime;
                string formattedDiff = timeDiff.ToString(@"hh\:mm\:ss");
                await _log.WriteMonitorAsync(
                    nameof(MonitoringJob),
                    nameof(CheckJobs),
                    $"No updates from {service.ServiceName} within {formattedDiff}!");
            });
        }

        public async Task CheckAPIs()
        {
            List<IMonitoringObject> apisMonitoring = await GetMonitoringObjectsForProcessing(x => !string.IsNullOrEmpty(x.Url));

            DateTime now = DateTime.UtcNow;
            List<Task> recipientChecks = new List<Task>(apisMonitoring.Count);
            var issues = new List<ApiHealthCheckError>();
            foreach (var monitoringItem in apisMonitoring)
            {
                var task = Task.Run(async () =>
                {
                    try
                    {
                        CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.PingTimeoutInSeconds));
                        IApiStatusObject statusObject = await _isAliveService.GetStatusAsync(monitoringItem.Url, cts.Token);
                        monitoringItem.Version = statusObject.Version;
                        monitoringItem.LastTime = now;

                        HandleResilience(issues, statusObject.IssueIndicators, monitoringItem);
                    }
                    catch (OperationCanceledException)
                    {
                        GenerateError(issues, now, "Timeout", monitoringItem);
                    }
                    catch (SimpleHttpResponseException e)
                    {
                        GenerateError(issues, now, e.StatusCode.ToString(), monitoringItem);
                    }
                    catch (Exception e)
                    {
                        GenerateError(issues, now, $"Unexpected exception: {e.GetBaseException().Message}", monitoringItem);
                    }
                });

                recipientChecks.Add(task);
            }

            await Task.WhenAll(recipientChecks);

            foreach (var issue in issues)
            {
                await _apiHealthCheckErrorRepository.InsertAsync(issue);
            }

            foreach (var api in apisMonitoring)
            {
                await _monitoringService.Ping(api);
            }
        }

        #region Private

        /// <summary>If api send any failing indicators, they are added to resilience output</summary>
        private void HandleResilience(
            ICollection<ApiHealthCheckError> issues,
            IEnumerable<IssueIndicatorObject> issueIndicators,
            IMonitoringObject mObject)
        {
            var indicators = _notifyingLimitSettings.CheckAndUpdateLimits(mObject.ServiceName, issueIndicators);

            if (indicators.Count == 0)
                return;

            string errorMessage = string.Join("; ", indicators.Select(o => o.Type + ": " + o.Value));

            lock (_lock)
            {
                issues.Add(new ApiHealthCheckError()
                {
                    Date = DateTime.UtcNow,
                    LastError = errorMessage,
                    ServiceName = mObject.ServiceName,
                });
            }

            _log.WriteMonitor(
                nameof(MonitoringJob),
                nameof(CheckAPIs),
                $"Service url check failed for {mObject.ServiceName} (URL:{mObject.Url}), reason: {errorMessage}!");
        }

        private void GenerateError(
            List<ApiHealthCheckError> errors,
            DateTime now,
            string errorMessage,
            IMonitoringObject mObject)
        {
            var error = new ApiHealthCheckError()
            {
                Date = now,
                LastError = errorMessage,
                ServiceName = mObject.ServiceName,
            };

            lock(errors)
            {
                errors.Add(error);
            }

            _log.WriteMonitor(
                nameof(MonitoringJob),
                nameof(CheckAPIs),
                $"Service url check failed for {mObject.ServiceName}(URL:{mObject.Url}), reason: {errorMessage}!");
        }

        private async Task<List<IMonitoringObject>> GetMonitoringObjectsForProcessing(Func<IMonitoringObject, bool> filter)
        {
            var now = DateTime.UtcNow;
            Func<IMonitoringObject, bool> decoratedFilter = (@object) => !(@object.SkipCheckUntil > now) && filter(@object);
            var allMonitoringObjects = await _monitoringService.GetCurrentSnapshot();
            var filteredObjects = allMonitoringObjects.Where(@object => decoratedFilter(@object));

            return filteredObjects.ToList();
        }

        #endregion
    }
}
