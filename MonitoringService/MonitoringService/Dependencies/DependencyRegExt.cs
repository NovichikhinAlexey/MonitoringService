using Microsoft.Extensions.DependencyInjection;
using Common.Log;
using Lykke.SettingsReader;
using AzureStorage.Tables;
using AzureStorage.Blob;
using Core;
using Core.Jobs;
using Core.Repositories;
using Core.Services;
using Core.Settings;
using Services;
using Repositories;

namespace MonitoringService.Dependencies
{
    public static class DependencyRegExt
    {
        public static void RegDependencies(this IServiceCollection collection)
        {
            collection.AddSingleton<IMonitoringJob, MonitoringJob>();
            collection.AddSingleton<IMonitoringObjectRepository>(new MonitoringObjectRepository());
            collection.AddSingleton<IMonitoringService, Services.MonitoringService>();
            collection.AddSingleton<IUrlMonitoringService, UrlMonitoringService>();
            collection.AddSingleton<IIsAliveService, IsAliveService>();
            collection.AddSingleton<IBackUpService, BackUpService>();
        }

        public static void RegisterAzureStorages(
            this IServiceCollection services,
            IReloadingManager<SettingsWrapper> settingsManager,
            ILog log)
        {
            var monitoringObjectStorage = AzureTableStorage<ApiMonitoringObjectEntity>.Create(
                settingsManager.ConnectionString(i => i.MonitoringService.Db.DataConnectionString),
                Constants.ApiMonitoringObjectTable,
                log);
            services.AddSingleton<IApiMonitoringObjectRepository>(provider =>
                new ApiMonitoringObjectRepository(monitoringObjectStorage));

            var errorStorage = AzureTableStorage<ApiHealthCheckErrorEntity>.Create(
                settingsManager.ConnectionString(i => i.MonitoringService.Db.DataConnectionString),
                Constants.ApiHealthCheckErrorTable,
                log);
            services.AddSingleton<IApiHealthCheckErrorRepository>(provider =>
                new ApiHealthCheckErrorRepository(errorStorage));

            var blobStorage = AzureBlobStorage.Create(
                settingsManager.ConnectionString(i => i.MonitoringService.Db.DataConnectionString));
            services.AddSingleton<IBackUpRepository>(provider =>
                new BackUpRepository(Constants.BackUpBlobName, blobStorage, log));
        }
    }
}
