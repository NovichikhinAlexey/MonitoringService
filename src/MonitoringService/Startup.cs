using System;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using Core.Jobs;
using Core.Services;
using Core.Settings;
using Lykke.AzureQueueIntegration;
using Lykke.Common;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Logs;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonitoringService.Dependencies;
using MonitoringService.Utils;

namespace MonitoringService
{
    public class Startup
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public ILog Log { get; private set; }
        public IConfigurationRoot Configuration { get; }
        public IContainer ApplicationContainer { get; private set; }

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
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

                var settingsManager = Configuration.LoadSettings<SettingsWrapper>(o =>
                    {
                        o.SetConnString(s => s.SlackNotifications.AzureQueue.ConnectionString);
                        o.SetQueueName(s => s.SlackNotifications.AzureQueue.QueueName);
                        o.SenderName = $"{AppEnvironment.Name} {AppEnvironment.Version}";
                    });
                Log = CreateLogWithSlack(services, settingsManager);

                var builder = new ContainerBuilder();
                builder.RegisterInstance(Log)
                    .As<ILog>()
                    .SingleInstance();

                var settings = settingsManager.CurrentValue;
                services.AddSingleton<IBaseSettings>(settings.MonitoringService);

                services.RegisterAzureStorages(settingsManager, Log);
                services.RegDependencies();

                builder.Populate(services);

                ApplicationContainer = builder.Build();

                return new AutofacServiceProvider(ApplicationContainer);
            }
            catch (Exception ex)
            {
                Log?.WriteFatalError(nameof(Startup), nameof(ConfigureServices), ex);
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
                appLifetime.ApplicationStopped.Register(CleanUp);
            }
            catch (Exception ex)
            {
                Log?.WriteFatalError(nameof(Startup), nameof(Configure), ex);
                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                // NOTE: Service not yet recieve and process requests here
                var backupService = ApplicationContainer.Resolve<IBackUpService>();
                await backupService.RestoreBackupAsync();

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

                Log.WriteMonitor("", "", "Started");
            }
            catch (Exception ex)
            {
                Log.WriteFatalError(nameof(Startup), nameof(StartApplication), ex);
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
                await backupService.CreateBackupAsync();
            }
            catch (Exception ex)
            {
                if (Log != null)
                    Log.WriteFatalError(nameof(Startup), nameof(StopApplication), ex);
                throw;
            }
        }

        private void CleanUp()
        {
            try
            {
                // NOTE: Service can't recieve and process requests here, so you can destroy all resources

                if (Log != null)
                    Log.WriteMonitor("", "", "Terminating");

                ApplicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (Log != null)
                {
                    Log.WriteFatalError(nameof(Startup), nameof(CleanUp), ex);
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
                console.WriteWarning(nameof(Startup), nameof(CreateLogWithSlack), "Table loggger is not inited");
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

            var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, true, console);

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
