using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using Core.Settings;
using MonitoringService.Dependencies;
using AzureRepositories;
using Core.Services;

namespace MonitoringService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }
        public static IServiceProvider ServiceProvider { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            var settings = GetSettings(Configuration);
            services.AddSingleton<IBaseSettings>(settings.MonitoringService);
            services.AddSingleton<ISlackNotificationSettings>(settings.SlackNotifications);
            services.RegisterAzureLogs(settings.MonitoringService);
            services.RegisterAzureStorages(settings.MonitoringService);
            services.RegDependencies();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "MonitoringService", Version = "v1" });
            });
            // Add framework services.
            services.AddMvc();

            ServiceProvider = services.BuildServiceProvider();
            return ServiceProvider;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime applicationLifetime)
        {
            applicationLifetime.ApplicationStopping.Register(() => 
            {
                var backupService = ServiceProvider.GetService<IBackUpService>();

                backupService.CreateBackupAsync();
            });
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

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MonitoringService");
            });
        }

        static SettingsWrapper GetSettings(IConfigurationRoot configuration)
        {
            var connectionString = configuration.GetConnectionString("ConnectionString");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new Exception("ConnectionString is empty");

            var settings = GeneralSettingsReader.ReadGeneralSettings<SettingsWrapper>(connectionString);

            return settings;
        }
    }
}
