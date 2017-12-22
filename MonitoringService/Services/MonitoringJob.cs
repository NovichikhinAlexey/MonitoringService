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
        private readonly object _failedChecksLock = new object();
        private readonly object _resilienceLockObj = new object();

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
            IEnumerable<IMonitoringObject> jobsMonitoring = (await GetMonitoringObjectsForProcessing(x => string.IsNullOrEmpty(x.Url)))?.ToList();
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
            IEnumerable<IMonitoringObject> apisMonitoring = (await GetMonitoringObjectsForProcessing(x => !string.IsNullOrEmpty(x.Url)))?.ToList();
            List<Task<IApiStatusObject>> pendingHttpChecks = new List<Task<IApiStatusObject>>(apisMonitoring.Count());
            IDictionary<string, IMonitoringObject> serviceNameMonitoringObjectMapping = apisMonitoring.ToDictionary(x => x.ServiceName);
            IDictionary<Task<IApiStatusObject>, IMonitoringObject> requestServiceMapping = new Dictionary<Task<IApiStatusObject>, IMonitoringObject>();
            DateTime now = DateTime.UtcNow;

            foreach (var api in apisMonitoring)
            {
                CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(_settings.PingTimeoutInSeconds));

                Task<IApiStatusObject> task = _isAliveService.GetStatusAsync(api.Url, cts.Token);
                pendingHttpChecks.Add(task);
                requestServiceMapping[task] = api;
            }

            var recipientChecks = new List<Task>(apisMonitoring.Count());
            var failedChecks = new List<ApiHealthCheckError>(apisMonitoring.Count());
            var resilienceChecks = new List<ApiHealthCheckError>();
            foreach (var item in pendingHttpChecks)
            {
                var serviceName = requestServiceMapping[item].ServiceName;

                var task = Task.Run(async () =>
                {
                    try
                    {
                        IApiStatusObject statusObject = await item;
                        requestServiceMapping[item].Version = statusObject.Version;
                        requestServiceMapping[item].LastTime = now;

                        HandleResilience(resilienceChecks, statusObject.IssueIndicators, serviceName);
                    }
                    catch (OperationCanceledException)
                    {
                        GenerateError(failedChecks, now, "Timeout", serviceName);
                    }
                    catch (SimpleHttpResponseException e)
                    {
                        GenerateError(failedChecks, now, e.StatusCode.ToString(), serviceName);
                    }
                    catch (Exception e)
                    {
                        await _log.WriteErrorAsync("MonitoringJob", "CheckAPIs", "", e, DateTime.UtcNow);
                        GenerateError(failedChecks, now, $"Unexpected exception: {e.Message}", serviceName);
                    }
                });

                recipientChecks.Add(task);
            }

            await Task.WhenAll(recipientChecks);

            foreach (var error in failedChecks)
            {
                IMonitoringObject mObject = serviceNameMonitoringObjectMapping[error.ServiceName];
                await _apiHealthCheckErrorRepository.InsertAsync((IApiHealthCheckError)error);
                await _log.WriteMonitorAsync(
                    nameof(MonitoringJob),
                    nameof(CheckAPIs),
                    $"Service url check failed for {error.ServiceName} (URL:{mObject.Url}), reason: {error.LastError}!");
            }

            foreach (var issue in resilienceChecks)
            {
                var mObject = serviceNameMonitoringObjectMapping[issue.ServiceName];
                await _apiHealthCheckErrorRepository.InsertAsync((IApiHealthCheckError)issue);
                await _log.WriteMonitorAsync(
                    nameof(MonitoringJob),
                    nameof(CheckAPIs),
                    $"Health check failed for {issue.ServiceName} (URL:{mObject.Url}), reason: {issue.LastError}!");
            }

            foreach (var api in apisMonitoring)
            {
                await _monitoringService.Ping(api);
            }
        }

        #region Private

        /// <summary>
        /// If api send any failing indicators, they are added to resilience output
        /// </summary>
        /// <param name="issues"></param>
        /// <param name="issueIndicators"></param>
        /// <param name="serviceName"></param>
        private void HandleResilience(ICollection<ApiHealthCheckError> issues, 
            IEnumerable<IssueIndicatorObject> issueIndicators, string serviceName)
        {
            var indicators = _notifyingLimitSettings.CheckAndUpdateLimits(serviceName, issueIndicators);

            if (indicators.Count == 0)
                return;

            lock (_resilienceLockObj)
            {
                issues.Add(new ApiHealthCheckError()
                {
                    Date = DateTime.UtcNow,
                    LastError = string.Join("; ", indicators.Select(o => o.Type + ": " + o.Value)),
                    ServiceName = serviceName,
                });
            }
        }

        private void GenerateError(List<ApiHealthCheckError> errors, DateTime now, string errorMessage, string serviceName)
        {
            var error = new ApiHealthCheckError()
            {
                Date = now,
                LastError = errorMessage,
                ServiceName = serviceName,
            };

            lock (_failedChecksLock)
            {
                errors.Add(error);
            }
        }

        private async Task<IEnumerable<IMonitoringObject>> GetMonitoringObjectsForProcessing(Func<IMonitoringObject, bool> filter)
        {
            var now = DateTime.UtcNow;
            Func<IMonitoringObject, bool> decoratedFilter = (@object) => !(@object.SkipCheckUntil > now) && filter(@object);
            var allMonitoringObjects = await _monitoringService.GetCurrentSnapshot();
            var filteredObjects = allMonitoringObjects.Where(@object => decoratedFilter(@object));

            return filteredObjects;
        }

        #endregion
    }
}
