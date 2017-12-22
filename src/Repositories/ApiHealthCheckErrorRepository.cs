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
    public class ApiHealthCheckErrorEntity : TableEntity, IApiHealthCheckError
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

        public string LastError { get; set; }
        public DateTime Date { get; set; }

        public static string GetPartitionKey()
        {
            return "ApiHealthCheckError";
        }

        public static ApiHealthCheckErrorEntity GetEntity(IApiHealthCheckError mObject)
        {
            return new ApiHealthCheckErrorEntity()
            {
                PartitionKey = GetPartitionKey(),
                ServiceName = mObject.ServiceName,
                Date = mObject.Date,
                LastError = mObject.LastError
            };
        }
    }

    public class ApiHealthCheckErrorRepository : IApiHealthCheckErrorRepository
    {
        private readonly INoSQLTableStorage<ApiHealthCheckErrorEntity> _table;

        public ApiHealthCheckErrorRepository(INoSQLTableStorage<ApiHealthCheckErrorEntity> table)
        {
            _table = table;
        }

        public async Task<IApiHealthCheckError> GetByIdAsync(string serviceName)
        {
            IApiHealthCheckError error = await _table.GetDataAsync(ApiHealthCheckErrorEntity.GetPartitionKey(), serviceName);

            return error;
        }

        public async Task InsertAsync(IApiHealthCheckError error)
        {
            var entity = ApiHealthCheckErrorEntity.GetEntity(error);

            await _table.InsertOrReplaceAsync(entity);
        }
    }
}
