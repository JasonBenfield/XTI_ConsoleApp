using MainDB.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XTI_App;
using XTI_App.Hosting;
using XTI_AuthenticatorClient.Extensions;
using XTI_ConsoleApp;
using XTI_Core;
using XTI_Schedule;
using XTI_Secrets.Extensions;
using XTI_TempLog;
using XTI_TempLog.Extensions;

namespace XTI_ServiceApp.Extensions
{
    public static class ServiceAppExtensions
    {
        public static void AddXtiServiceAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddXtiDataProtection();
            services.AddAppDbContextForSqlServer(configuration);
            services.AddSingleton<Clock, UtcClock>();
            services.Configure<AppOptions>(configuration.GetSection(AppOptions.App));
            services.Configure<AppActionOptions>(configuration.GetSection(AppActionOptions.AppAction));
            services.AddScoped(sp =>
            {
                var hostEnv = sp.GetService<IHostEnvironment>();
                var appKey = sp.GetService<AppKey>();
                return new AppDataFolder()
                    .WithHostEnvironment(hostEnv)
                    .WithSubFolder("ServiceApps")
                    .WithSubFolder(appKey.Name.DisplayText);
            });
            services.AddSingleton<CurrentSession>();
            services.AddSingleton<XtiBasePath>();
            services.AddSingleton(sp => sp.GetService<XtiBasePath>().Value());
            services.AddScoped<IActionRunnerFactory, ActionRunnerFactory>();
            services.AddTempLogServices();
            services.AddAuthenticatorClientServices(configuration);
            services.AddFileSecretCredentials();
            services.AddSingleton<IAppEnvironmentContext, ServiceAppEnvironmentContext>();
            services.AddHostedService<ServiceAppWorker>();
        }
    }
}
