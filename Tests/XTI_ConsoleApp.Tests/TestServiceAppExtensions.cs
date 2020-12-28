using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XTI_App.Api;
using XTI_ConsoleApp.Fakes;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_TempLog;
using XTI_TempLog.Fakes;

namespace XTI_ConsoleApp.Tests
{
    public static class TestServiceAppExtensions
    {
        public static void AddTestServiceAppServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<Clock, FakeClock>();
            services.AddSingleton<Counter>();
            services.AddSingleton<TestOptions>();
            services.AddSingleton<IAppEnvironmentContext, FakeAppEnvironmentContext>();
            services.AddScoped<IAppApiUser, AppApiSuperUser>();
            services.AddScoped(_ => TestAppKey.Key);
            services.AddScoped<TestApi>();
            services.AddScoped<AppApi>(sp => sp.GetService<TestApi>());
            services.AddFakeServiceAppServices(configuration);
        }
    }
}
