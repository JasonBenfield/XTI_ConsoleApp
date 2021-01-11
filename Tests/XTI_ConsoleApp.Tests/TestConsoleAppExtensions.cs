using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XTI_App.Api;
using XTI_App.Hosting;
using XTI_ConsoleApp.Fakes;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_TempLog;
using XTI_TempLog.Fakes;

namespace XTI_ConsoleApp.Tests
{
    public static class TestConsoleAppExtensions
    {
        public static void AddTestConsoleAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<Clock, FakeClock>();
            services.AddSingleton<Counter>();
            services.AddSingleton<TestOptions>();
            services.AddScoped<IAppApiUser, AppApiSuperUser>();
            services.AddScoped(_ => TestAppKey.Key);
            services.AddScoped<AppApi, TestApi>();
            services.AddFakeConsoleAppServices(configuration);
        }
    }
}
