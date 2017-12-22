using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using AzureStorage.Tables;
using Lykke.Logs;
using Lykke.SlackNotification.AzureQueue;
using Lykke.AzureQueueIntegration;
using Lykke.SettingsReader;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using MonitoringService.Dependencies;
using Core.Settings;
using Core.Services;
using Core.Jobs;
using MonitoringService.Utils;

namespace MonitoringService
{
    public class Startup
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();

        public ILog Log { get; private set; }
        public IConfigurationRoot Configuration { get; }
        public IContainer ApplicationContainer { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddSwaggerGen(c =>
                {
                    c.DefaultLykkeConfiguration("v1", "MonitoringService API");
                });
                // Add framework services.
                services.AddMvc();

                var settingsManager = Configuration.LoadSettings<SettingsWrapper>("SettingsUrl");
                Log = CreateLogWithSlack(services, settingsManager);

                var builder = new ContainerBuilder();
                builder.RegisterInstance(Log)
                    .As<ILog>()
                    .SingleInstance();

                var settings = settingsManager.CurrentValue;
                services.AddSingleton<IBaseSettings>(settings.MonitoringService);
                services.AddSingleton<INotifyingLimitSettings, NotifyingLimitSettings>();

                services.RegisterAzureStorages(settingsManager, Log);
                services.RegDependencies();

                builder.Populate(services);

                ApplicationContainer = builder.Build();

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(ConfigureServices), "", ex).GetAwaiter().GetResult();
                throw;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                    app.UseDeveloperExceptionPage();

                loggerFactory.AddConsole(Configuration.GetSection("Logging"));
                loggerFactory.AddDebug();

                app.UseCors((policyBuilder) =>
                {
                    policyBuilder.AllowAnyHeader();
                    policyBuilder.AllowAnyOrigin();
                });

                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");
                });

                app.UseLykkeMiddleware("MonitoringService", ex => new { Message = "Technical problem" });

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.RoutePrefix = "swagger/ui";
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });

                appLifetime.ApplicationStarted.Register(() => StartApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopping.Register(() => StopApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopped.Register(() => CleanUp().GetAwaiter().GetResult());
            }
            catch (Exception ex)
            {
                Log?.WriteFatalErrorAsync(nameof(Startup), nameof(Configure), "", ex).GetAwaiter().GetResult();
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Service not yet recieve and process requests here
                var backupService = ApplicationContainer.Resolve<IBackUpService>();
                backupService.RestoreBackupAsync().Wait();

                IMonitoringJob job = ApplicationContainer.Resolve<IMonitoringJob>();
                IBaseSettings baseSettings = ApplicationContainer.Resolve<IBaseSettings>();
                InProcessJobHelper.StartJob(
                    job.CheckJobs,
                    _cts.Token,
                    baseSettings.MonitoringJobFrequencyInSeconds,
                    Log);
                InProcessJobHelper.StartJob(
                    job.CheckAPIs,
                    _cts.Token,
                    baseSettings.MonitoringApiFrequencyInSeconds,
                    Log);

                await Log.WriteMonitorAsync("", "", "Started");
            }
            catch (Exception ex)
            {
                await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);
                throw;
            }
        }

        private async Task StopApplication()
        {
            try
            {
                _cts.Cancel();

                // NOTE: Service still can recieve and process requests here, so take care about it if you add logic here.
                var backupService = ApplicationContainer.Resolve<IBackUpService>();
                backupService.CreateBackupAsync().Wait();
            }
            catch (Exception ex)
            {
                if (Log != null)
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(StopApplication), "", ex);
                throw;
            }
        }

        private async Task CleanUp()
        {
            try
            {
                // NOTE: Service can't recieve and process requests here, so you can destroy all resources

                if (Log != null)
                    await Log.WriteMonitorAsync("", "", "Terminating");

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    await Log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                    (Log as IDisposable)?.Dispose();
                }
                throw;
            }
        }

        private ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<SettingsWrapper> settings)
        {
           var console = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(console);

            var dbLogConnectionStringManager = settings.Nested(x => x.MonitoringService.Db.LogsConnectionString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            if (string.IsNullOrEmpty(dbLogConnectionString))
            {
                console.WriteWarningAsync(nameof(Startup), nameof(CreateLogWithSlack), "Table loggger is not inited").Wait();
                return aggregateLogger;
            }

            if (dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}"))
                throw new InvalidOperationException($"LogsConnString {dbLogConnectionString} is not filled in settings");

            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager(
                AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "Errors", console),
                console);

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueSettings
            {
                ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            }, aggregateLogger);

            var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, console);

            // Creating azure storage logger, which logs own messages to concole log
            var azureStorageLogger = new LykkeLogToAzureStorage(
                persistenceManager,
                slackNotificationsManager,
                console);

            azureStorageLogger.Start();

            aggregateLogger.AddLog(azureStorageLogger);

            return aggregateLogger;
        }
    }
}
