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
        private readonly IBackUpRepository _backUpRepository;
        private readonly IMonitoringObjectRepository _monitoringObjectRepository;

        public BackUpService(IMonitoringObjectRepository monitoringObjectRepository,
            IBackUpRepository backUpRepository,
            ILog log)
        {
            _monitoringObjectRepository = monitoringObjectRepository;
            _backUpRepository = backUpRepository;
        }

        public async Task CreateBackupAsync()
        {
            List<IMonitoringObject> allObjects = (await _monitoringObjectRepository.GetAllAsync())?.ToList();

            string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(allObjects);
            await _backUpRepository.InsertAsync(new BackUp()
            {
                Key = _backUpKey,
                SerializedObject = serialized
            });
        }

        public async Task RestoreBackupAsync()
        {
            IBackUp backUp = await _backUpRepository.GetAsync(_backUpKey);
            if (backUp == null)
            {
                return;
            }

            List<MonitoringObject> allObjects = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MonitoringObject>>(backUp.SerializedObject);
            allObjects.ForEach(async @object => await _monitoringObjectRepository.InsertAsync(@object));
        }
    }
}
