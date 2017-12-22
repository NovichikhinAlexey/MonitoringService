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
    public class BackUpService : IBackUpService
    {
        private const string _backUpKey = "backup";
        //private readonly IBackUpRepository _backUpRepository;
        private readonly IMonitoringObjectRepository _monitoringObjectRepository;
        private readonly ILog _log;
        private readonly IApiMonitoringObjectRepository _apiMonitoringObjectRepository;

        public BackUpService(IMonitoringObjectRepository monitoringObjectRepository,
            IApiMonitoringObjectRepository apiMonitoringObjectRepository,
            IBackUpRepository backUpRepository,
            ILog log)
        {
            _monitoringObjectRepository = monitoringObjectRepository;
            _apiMonitoringObjectRepository = apiMonitoringObjectRepository;
            //ackUpRepository = backUpRepository;
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
            //List<IMonitoringObject> allObjects = (await _monitoringObjectRepository.GetAllAsync())?.ToList();

            //string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(allObjects);
            //await _backUpRepository.InsertAsync(new BackUp()
            //{
            //    Key = _backUpKey,
            //    SerializedObject = serialized
            //});
        }

        public async Task RestoreBackupAsync()
        {

            var all = await _apiMonitoringObjectRepository.GetAllAsync();

            foreach (var item in all)
            {
                await _monitoringObjectRepository.InsertAsync(item);
            }

            await _log.WriteInfoAsync("BackUpService", "RestoreBackupAsync", "", "Backup has been restored", DateTime.UtcNow);
            //IBackUp backUp = await _backUpRepository.GetAsync(_backUpKey);
            //if (backUp == null)
            //{
            //    return;
            //}

            //List<MonitoringObject> allObjects = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MonitoringObject>>(backUp.SerializedObject);
            //allObjects.ForEach(async @object => await _monitoringObjectRepository.InsertAsync(@object));
        }
    }
}
