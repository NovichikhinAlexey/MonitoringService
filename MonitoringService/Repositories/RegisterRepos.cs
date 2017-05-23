using System;
using Core;
using Core.Repositories;
using Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using AzureStorage.Tables;
using AzureStorage.Queue;
using Common.Log;
using AzureStorage.Tables.Templates.Index;
using Repositories;
using Lykke.Logs;
using Repositories.Log;

namespace AzureRepositories
{
    public static class RegisterReposExt
    {
        public static void RegisterAzureLogs(this IServiceCollection services, IBaseSettings settings)
        {
            var logToTable = new Lykke.Logs.LykkeLogToAzureStorage("MonitoringService",
                new AzureTableStorage<LogEntity>(settings.Db.LogsConnectionString, "Errors", null));

            services.AddSingleton(logToTable);
            services.AddTransient<LogToConsole>();
            services.AddSingleton<ILog, LogToTableAndConsole>();
        }

        public static void RegisterAzureStorages(this IServiceCollection services, IBaseSettings settings)
        {
            services.AddSingleton<IApiMonitoringObjectRepository>(provider => new ApiMonitoringObjectRepository(
                new AzureTableStorage<ApiMonitoringObjectEntity>(settings.Db.DataConnectionString, Constants.ApiMonitoringObjectTable,
                provider.GetService<ILog>())));

            services.AddSingleton<IApiHealthCheckErrorRepository>(provider => new ApiHealthCheckErrorRepository(
                new AzureTableStorage<ApiHealthCheckErrorEntity>(settings.Db.DataConnectionString, Constants.ApiHealthCheckErrorTable,
                provider.GetService<ILog>())));

            var blobStorage = new AzureStorage.Blob.AzureBlobStorage(settings.Db.DataConnectionString);
            services.AddSingleton<IBackUpRepository>(provider => new BackUpRepository(Constants.BackUpBlobName, blobStorage, provider.GetService<ILog>()));
        }
    }
}
