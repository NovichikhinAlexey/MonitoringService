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
        private readonly IApiHealthCheckErrorRepository _apiHealthCheckErrorRepository;
        private readonly object _failedChecksLock = new object();

        public MonitoringJob(
            IMonitoringService monitoringService,
            IBaseSettings settings,
            IApiMonitoringObjectRepository apiMonitoringObjectRepository,
            IApiHealthCheckErrorRepository apiHealthCheckErrorRepository,
            IIsAliveService isAliveService,
            ILog log)
        {
            _log = log;
            _monitoringService = monitoringService;
            _settings = settings;
            _isAliveService = isAliveService;
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
            List<ApiHealthCheckError> failedChecks = new List<ApiHealthCheckError>(apisMonitoring.Count());
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

            List<Task> recipientChecks = new List<Task>(apisMonitoring.Count());
            foreach (var item in pendingHttpChecks)
            {
                string serviceName = requestServiceMapping[item].ServiceName;

                var task = Task.Run(async () =>
                {
                    try
                    {
                        IApiStatusObject statusObject = await item;
                        requestServiceMapping[item].Version = statusObject.Version;
                        requestServiceMapping[item].LastTime = now;
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
                        GenerateError(failedChecks, now, $"Unexpected exception: {e.GetBaseException().Message}", serviceName);
                    }
                });

                recipientChecks.Add(task);
            }

            await Task.WhenAll(recipientChecks);

            foreach (var error in failedChecks)
            {
                IMonitoringObject mObject = serviceNameMonitoringObjectMapping[error.ServiceName];
                await _apiHealthCheckErrorRepository.InsertAsync(error);
                await _log.WriteMonitorAsync(
                    nameof(MonitoringJob),
                    nameof(CheckAPIs),
                    $"Service url check failed for {error.ServiceName}(URL:{mObject.Url}), reason: {error.LastError}!");
            }

            foreach (var api in apisMonitoring)
            {
                await _monitoringService.Ping(api);
            }
        }

        #region Private

        private void GenerateError( List<ApiHealthCheckError> errors, DateTime now, string errorMessage, string serviceName)
        {
            var  error = new ApiHealthCheckError()
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
