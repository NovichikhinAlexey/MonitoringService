using AzureStorage;
using Core.Repositories;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;

namespace Repositories
{
    public class ApiMonitoringObjectEntity : TableEntity, IApiMonitoringObject
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

        public static string GetPartitionKey()
        {
            return "ApiMonitoringObject";
        }

        public static ApiMonitoringObjectEntity GetApiMonitoringObjectEntity(IApiMonitoringObject mObject)
        {
            return new ApiMonitoringObjectEntity()
            {
                PartitionKey = GetPartitionKey(),
                ServiceName = mObject.ServiceName,
                Url = mObject.Url
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

        public async Task<IEnumerable<IApiMonitoringObject>> GetAll()
        {
            IEnumerable<IApiMonitoringObject> allApi = await _table.GetDataAsync(ApiMonitoringObjectEntity.GetPartitionKey());

            return allApi;
        }

        public async Task Insert(IApiMonitoringObject aObject)
        {
            var entity = ApiMonitoringObjectEntity.GetApiMonitoringObjectEntity(aObject);

            await _table.InsertOrReplaceAsync(entity);
        }
    }
}
