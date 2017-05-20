using Core.Jobs;
using Core.Models;
using Core.Repositories;
using Core.Services;
using Core.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class MonitoringJob : IMonitoringJob
    {
        private readonly IMonitoringObjectRepository _monitoringObjectRepository;
        private readonly IBaseSettings _settings;
        private readonly ISlackNotifier _slackNotifier;

        public MonitoringJob(IMonitoringObjectRepository monitoringObjectRepository,
            IBaseSettings settings,
            ISlackNotifier slackNotifier)
        {
            _settings = settings;
            _monitoringObjectRepository = monitoringObjectRepository;
            _slackNotifier = slackNotifier;
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
                string formattedDiff =  timeDiff.ToString(@"hh\:mm\:ss");
                await _slackNotifier.ErrorAsync($"No updates from {service.ServiceName} within {formattedDiff}!");
            });
        }
    }
}
