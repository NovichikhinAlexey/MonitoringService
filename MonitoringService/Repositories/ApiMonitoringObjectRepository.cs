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
    public class ApiMonitoringObjectEntity : IApiMonitoringObject, ITableEntity
    {
        public string ServiceName { get; set; }
        public string Version { get; set; }
        public string Url { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }

        public static string GetPartitionKey()
        {
            return "ApiMonitoring";
        }

        public static ApiMonitoringObjectEntity GetApiMonitoringObjectEntity(IApiMonitoringObject mObject)
        {
            return new ApiMonitoringObjectEntity()
            {
                //ServiceName = ,
                //Url = ,
                //Version =,
            };
        }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            throw new NotImplementedException();
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            throw new NotImplementedException();
        }
    }

    public class ApiMonitoringObjectRepository : IApiMonitoringObjectRepository
    {
        public ApiMonitoringObjectRepository(INoSQLTableStorage<ApiMonitoringObjectEntity> table)
        { }

        public Task<IEnumerable<IApiMonitoringObject>> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}
