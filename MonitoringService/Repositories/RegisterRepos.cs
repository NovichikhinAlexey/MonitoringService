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

namespace AzureRepositories
{
    public static class RegisterReposExt
    {
        public static void RegisterAzureStorages(this IServiceCollection services, IBaseSettings settings)
        {
            services.AddSingleton<IApiMonitoringObjectRepository>(provider => new ApiMonitoringObjectRepository(
                new AzureTableStorage<ApiMonitoringObjectEntity>(settings.Db.SharedConnectionString, Constants.ApiMonitoringObjectTable,
                provider.GetService<ILog>())));
        }
    }
}
