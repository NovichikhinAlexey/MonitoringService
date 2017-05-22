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
        private readonly IMonitoringObjectRepository _monitoringObjectRepository;
        private readonly IBaseSettings _settings;
        private readonly ISlackNotifier _slackNotifier;
        private readonly IApiMonitoringObjectRepository _apiMonitoringObjectRepository;
        private readonly HttpClient _httpClient;
        private IApiHealthCheckErrorRepository _apiHealthCheckErrorRepository;

        public MonitoringJob(IMonitoringObjectRepository monitoringObjectRepository,
            IBaseSettings settings,
            ISlackNotifier slackNotifier,
            IApiMonitoringObjectRepository apiMonitoringObjectRepository,
            IApiHealthCheckErrorRepository apiHealthCheckErrorRepository)
        {
            _settings = settings;
            _monitoringObjectRepository = monitoringObjectRepository;
            _slackNotifier = slackNotifier;
            _apiMonitoringObjectRepository = apiMonitoringObjectRepository;
            _httpClient = new HttpClient();
            _apiHealthCheckErrorRepository = apiHealthCheckErrorRepository;
        }

        public async Task Execute()
        {
            DateTime now = DateTime.UtcNow;
            IEnumerable<MonitoringObject> objects = await _monitoringObjectRepository.GetAll();
            List<MonitoringObject> fireNotificationsFor = new List<MonitoringObject>();

            foreach (var @object in objects)
            {
                if (@object.SkipCheckUntil > now)
                {
                    continue;
                }

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

            await CheckAPIs();
        }

        private async Task CheckAPIs()
        {
            IEnumerable<IApiMonitoringObject> allApis = await _apiMonitoringObjectRepository.GetAll();
            List<Task<HttpResponseMessage>> runningChecks = new List<Task<HttpResponseMessage>>(allApis.Count());
            List<ApiHealthCheckError> failedChecks = new List<ApiHealthCheckError>(allApis.Count());
            IDictionary<Task<HttpResponseMessage>, string> requestServiceMapping = new Dictionary<Task<HttpResponseMessage>, string>();
            DateTime now = DateTime.UtcNow;

            foreach (var api in allApis)
            {
                CancellationTokenSource cts = new CancellationTokenSource(5500);

                Task<HttpResponseMessage> task = _httpClient.GetAsync(api.Url, cts.Token);
                runningChecks.Add(task);
                requestServiceMapping[task] = api.ServiceName;
            }

            foreach (var item in runningChecks)
            {
                string serviceName = requestServiceMapping[item];

                try
                {
                    await item;
                }
                catch (OperationCanceledException e)
                {
                    failedChecks.Add(GenerateError(now, "Timeout", serviceName));
                }
                catch (Exception e)
                {
                    failedChecks.Add(GenerateError(now, e.Message, serviceName));
                }

                if (!item.IsFaulted && !item.IsCanceled
                    && item.Result != null && !item.Result.IsSuccessStatusCode)
                {
                    failedChecks.Add(GenerateError(now, item.Result.StatusCode.ToString(), serviceName));
                }
            }

            foreach (var error in failedChecks)
            {
                await _apiHealthCheckErrorRepository.Insert((IApiHealthCheckError)error);

                await _slackNotifier.ErrorAsync($"Service url check failed for {error.ServiceName}, reason: {error.LastError}!");
            }
        }

        private static ApiHealthCheckError GenerateError(DateTime now, string errorMessage, string serviceName)
        {
            return new ApiHealthCheckError()
            {
                Date = now,
                LastError = errorMessage,
                ServiceName = serviceName,
            };
        }
    }
}
