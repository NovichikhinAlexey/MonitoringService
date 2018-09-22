using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Core.Repositories;
using Core.Services;

namespace Services
{
    public class BackUpService : IBackUpService
    {
        private readonly IMonitoringObjectRepository _monitoringObjectRepository;
        private readonly ILog _log;
        private readonly IApiMonitoringObjectRepository _apiMonitoringObjectRepository;

        public BackUpService(
            IMonitoringObjectRepository monitoringObjectRepository,
            IApiMonitoringObjectRepository apiMonitoringObjectRepository,
            ILog log)
        {
            _monitoringObjectRepository = monitoringObjectRepository;
            _apiMonitoringObjectRepository = apiMonitoringObjectRepository;
            _log = log;
        }

        public async Task CreateBackupAsync()
        {
            await _log.WriteInfoAsync("BackUpService", "CreateBackupAsync", "","Creating a backup", DateTime.UtcNow);
            var all = await _monitoringObjectRepository.GetAllAsync();
            var tasks = new List<Task>(all.Count());

            foreach (var item in all)
            {
                var task = _apiMonitoringObjectRepository.InsertAsync(item);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            await _log.WriteInfoAsync("BackUpService", "CreateBackupAsync", "", "Backup has been created", DateTime.UtcNow);
        }

        public async Task RestoreBackupAsync()
        {
            var all = await _apiMonitoringObjectRepository.GetAllAsync();

            foreach (var item in all)
            {
                await _monitoringObjectRepository.InsertAsync(item);
            }

            await _log.WriteInfoAsync("BackUpService", "RestoreBackupAsync", "", "Backup has been restored", DateTime.UtcNow);
        }
    }
}
