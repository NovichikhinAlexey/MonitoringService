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
            var errors = new List<ApiHealthCheckError>();
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
                    }
                    catch (OperationCanceledException)
                    {
                        GenerateError(errors, now, "Timeout", monitoringItem);
                    }
                    catch (SimpleHttpResponseException e)
                    {
                        GenerateError(errors, now, e.StatusCode.ToString(), monitoringItem);
                    }
                    catch (Exception e)
                    {
                        GenerateError(errors, now, $"Unexpected exception: {e.GetBaseException().Message}", monitoringItem);
                    }
                });

                recipientChecks.Add(task);
            }

            await Task.WhenAll(recipientChecks);

            foreach (var error in errors)
            {
                await _apiHealthCheckErrorRepository.InsertAsync(error);
            }

            foreach (var api in apisMonitoring)
            {
                await _monitoringService.Ping(api);
            }
        }

        #region Private

        private void GenerateError(
            List<ApiHealthCheckError> errors,
            DateTime now,
            string errorMessage,
            IMonitoringObject mObject)
        {
            var  error = new ApiHealthCheckError()
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
