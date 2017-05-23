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
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services
{
    public class MonitoringJob : IMonitoringJob
    {
        private readonly IMonitoringService _monitoringService;
        private readonly IApiMonitoringObjectRepository _apiMonitoringObjectRepository;
        private readonly IBaseSettings _settings;
        private readonly ISlackNotifier _slackNotifier;
        private IApiHealthCheckErrorRepository _apiHealthCheckErrorRepository;
        private readonly IIsAliveService _isAliveService;

        public MonitoringJob(IMonitoringService monitoringService,
            IBaseSettings settings,
            ISlackNotifier slackNotifier,
            IApiMonitoringObjectRepository apiMonitoringObjectRepository,
            IApiHealthCheckErrorRepository apiHealthCheckErrorRepository,
            IIsAliveService isAliveService,
            ILog log)
        {
            _log = log;
            _monitoringService = monitoringService;
            _settings = settings;
            _slackNotifier = slackNotifier;
            _isAliveService = isAliveService;
            _apiHealthCheckErrorRepository = apiHealthCheckErrorRepository;
            _apiMonitoringObjectRepository = apiMonitoringObjectRepository;
        }

        public async Task Execute()
        {
            DateTime now = DateTime.UtcNow;
            IEnumerable<IMonitoringObject> objects = (await _monitoringService.GetCurrentSnapshot())?.
                Where(@object => !(@object.SkipCheckUntil > now));
            IEnumerable<IMonitoringObject> jobsMonitoring = objects.Where(x => string.IsNullOrEmpty(x.Url));
            IEnumerable<IMonitoringObject> apisMonitoring = objects.Except(jobsMonitoring);

            await CheckJobs(jobsMonitoring);
            await CheckAPIs(apisMonitoring);
        }

        #region Private

        private async Task CheckJobs(IEnumerable<IMonitoringObject> jobsMonitoring)
        {
            DateTime now = DateTime.UtcNow;
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
                await _slackNotifier.ErrorAsync($"No updates from {service.ServiceName} within {formattedDiff}!");
            });
        }

        private async Task CheckAPIs(IEnumerable<IMonitoringObject> apisMonitoring)
        {
            List<Task<IApiStatusObject>> pendingHttpChecks = new List<Task<IApiStatusObject>>(apisMonitoring.Count());
            List<ApiHealthCheckError> failedChecks = new List<ApiHealthCheckError>(apisMonitoring.Count());
            IDictionary<string, IMonitoringObject> serviceNameMonitoringObjectMapping = apisMonitoring.ToDictionary(x => x.ServiceName);
            IDictionary<Task<IApiStatusObject>, IMonitoringObject> requestServiceMapping = new Dictionary<Task<IApiStatusObject>, IMonitoringObject>();
            DateTime now = DateTime.UtcNow;

            foreach (var api in apisMonitoring)
            {
                CancellationTokenSource cts = new CancellationTokenSource(5500);

                Task<IApiStatusObject> task = _isAliveService.GetStatus(api.Url, cts.Token);
                pendingHttpChecks.Add(task);
                requestServiceMapping[task] = api;
                api.LastTime = now;
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
                    catch (OperationCanceledException e)
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
                await _apiHealthCheckErrorRepository.Insert((IApiHealthCheckError)error);
                await _slackNotifier.ErrorAsync($"Service url check failed for {error.ServiceName}(URL:{mObject.Url}), reason: {error.LastError}!");
            }

            foreach (var api in apisMonitoring)
            {
                await _apiMonitoringObjectRepository.Insert(api);
            }
        }

        private object failedChecksLock = new object();
        private ILog _log;

        private void GenerateError( List<ApiHealthCheckError> errors, DateTime now, string errorMessage, string serviceName)
        {
            var  error = new ApiHealthCheckError()
            {
                Date = now,
                LastError = errorMessage,
                ServiceName = serviceName,
            };

            lock (failedChecksLock)
            {
                errors.Add(error);
            }
        }

        #endregion
    }
}
