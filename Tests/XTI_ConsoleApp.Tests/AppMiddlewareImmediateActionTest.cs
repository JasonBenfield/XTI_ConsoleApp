using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_TempLog;
using XTI_TempLog.Fakes;

namespace XTI_ConsoleApp.Tests
{
    public sealed class AppMiddlewareImmediateActionTest
    {
        [Test]
        public async Task ShouldStartSession()
        {
            var host = await runService();
            var tempLog = host.Services.GetService<TempLog>();
            var clock = host.Services.GetService<Clock>();
            var startSessionFiles = tempLog.StartSessionFiles(clock.Now());
            Assert.That(startSessionFiles.Count(), Is.EqualTo(1), "Should start session");
        }

        [Test]
        public async Task ShouldStartRequest()
        {
            var host = await runService();
            var tempLog = host.Services.GetService<TempLog>();
            var clock = host.Services.GetService<Clock>();
            var startRequests = await getStartRequests(host.Services);
            var api = host.Services.GetService<TestApi>();
            startRequests = startRequests.Where(r => api.Test.RunContinuously.Path.Equals(r.Path)).ToArray();
            Assert.That(startRequests.Length, Is.GreaterThan(0), "Should start request");
        }

        private Task<StartRequestModel[]> getStartRequests(IServiceProvider services)
        {
            var tempLog = services.GetService<TempLog>();
            var clock = (FakeClock)services.GetService<Clock>();
            var files = tempLog.StartRequestFiles(clock.Now()).ToArray();
            return deserializeLogFiles<StartRequestModel>(files);
        }

        private async Task<T[]> deserializeLogFiles<T>(IEnumerable<ITempLogFile> logFiles)
        {
            var logObjects = new List<T>();
            foreach (var logFile in logFiles)
            {
                var deserialized = await logFile.Read();
                var logObject = JsonSerializer.Deserialize<T>(deserialized);
                logObjects.Add(logObject);
            }
            return logObjects.ToArray();
        }

        [Test]
        public async Task ShouldEndRequest()
        {
            var host = await runService();
            var tempLog = host.Services.GetService<TempLog>();
            var clock = host.Services.GetService<Clock>();
            var endRequestFiles = tempLog.EndRequestFiles(clock.Now());
            Assert.That(endRequestFiles.Count(), Is.EqualTo(1), "Should end request");
        }

        [Test]
        public async Task ShouldEndSession()
        {
            var host = await runService();
            var tempLog = host.Services.GetService<TempLog>();
            var clock = host.Services.GetService<Clock>();
            var endSessionFiles = tempLog.EndSessionFiles(clock.Now());
            Assert.That(endSessionFiles.Count(), Is.EqualTo(1), "Should end session");
        }

        private async Task<IHost> runService()
        {
            var host = BuildHost().Build();
            return await runHost(host);
        }

        private static async Task<IHost> runHost(IHost host)
        {
            var envContext = (FakeAppEnvironmentContext)host.Services.GetService<IAppEnvironmentContext>();
            envContext.Environment = new AppEnvironment
            (
                "test.user",
                "AppMiddleware",
                "my-computer",
                "Windows 10",
                "Service"
            );
            var _ = Task.Run(() => host.StartAsync());
            var counter = host.Services.GetService<Counter>();
            while (counter.ContinuousValue == 0)
            {
                await Task.Delay(100);
            }
            await host.StopAsync();
            return host;
        }

        private IHostBuilder BuildHost()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            return Host.CreateDefaultBuilder(new string[] { })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();
                    config.AddInMemoryCollection(new[]
                    {
                        KeyValuePair.Create("AppAction:ImmediateActions:0:GroupName", "Test"),
                        KeyValuePair.Create("AppAction:ImmediateActions:0:ActionName", "RunContinuously")
                    });
                })
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTestServiceAppServices(hostContext.Configuration);
                });
        }
    }
}
