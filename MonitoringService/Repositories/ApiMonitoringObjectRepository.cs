using AzureStorage;
using Core.Repositories;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Core.Models;

namespace Repositories
{
    public class ApiMonitoringObjectEntity : TableEntity, IMonitoringObject
    {
        public string ServiceName
        {
            get
            {
                return this.RowKey;
            }

            set
            {
                this.RowKey = value;
            }
        }
        public string Version { get; set; }
        public string Url { get; set; }
        public DateTime LastTime { get; set; }
        public DateTime? SkipCheckUntil { get; set; }

        public static string GetPartitionKey()
        {
            return "ApiMonitoringObject";
        }

        public static ApiMonitoringObjectEntity GetApiMonitoringObjectEntity(IMonitoringObject mObject)
        {
            return new ApiMonitoringObjectEntity()
            {
                PartitionKey = GetPartitionKey(),
                ServiceName = mObject.ServiceName,
                Url = mObject.Url,
                LastTime = mObject.LastTime,
                SkipCheckUntil = mObject.SkipCheckUntil,
                Version = mObject.Version
            };
        }
    }

    public class ApiMonitoringObjectRepository : IApiMonitoringObjectRepository
    {
        private readonly INoSQLTableStorage<ApiMonitoringObjectEntity> _table;

        public ApiMonitoringObjectRepository(INoSQLTableStorage<ApiMonitoringObjectEntity> table)
        {
            _table = table;
        }

        public async Task<IEnumerable<IMonitoringObject>> GetAllAsync()
        {
            IEnumerable<IMonitoringObject> allApi = await _table.GetDataAsync(ApiMonitoringObjectEntity.GetPartitionKey());

            return allApi;
        }

        public async Task<IMonitoringObject> GetByNameAsync(string serviceName)
        {
            ApiMonitoringObjectEntity mObject = await _table.GetDataAsync(ApiMonitoringObjectEntity.GetPartitionKey(), serviceName);

            return mObject;
        }

        public async Task InsertAsync(IMonitoringObject aObject)
        {
            var entity = ApiMonitoringObjectEntity.GetApiMonitoringObjectEntity(aObject);

            await _table.InsertOrReplaceAsync(entity);
        }

        public async Task RemoveAsync(string serviceName)
        {
            await _table.DeleteIfExistAsync(ApiMonitoringObjectEntity.GetPartitionKey(), serviceName);
        }
    }
}
