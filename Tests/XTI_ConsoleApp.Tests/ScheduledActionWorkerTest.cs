using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XTI_Core;
using XTI_Core.Fakes;

namespace XTI_ConsoleApp.Tests
{
    public sealed class ScheduledActionWorkerTest
    {
        [Test]
        public async Task ShouldRunScheduledAction()
        {
            var host = BuildHost().Build();
            var clock = (FakeClock)host.Services.GetService<Clock>();
            clock.Set(new DateTime(2020, 10, 16, 13, 30, 0, DateTimeKind.Utc));
            var counter = host.Services.GetService<Counter>();
            var _ = Task.Run(() => host.Run());
            await Task.Delay(2000);
            Assert.That(counter.Value, Is.GreaterThan(0));
            Console.WriteLine($"Counter value: {counter.Value}");
            await host.StopAsync();
        }

        [Test]
        public async Task ShouldNotRunScheduledAction()
        {
            var host = BuildHost().Build();
            var clock = (FakeClock)host.Services.GetService<Clock>();
            clock.Set(new DateTime(2020, 10, 16, 14, 30, 0, DateTimeKind.Utc));
            var counter = host.Services.GetService<Counter>();
            var _ = Task.Run(() => host.RunAsync());
            await Task.Delay(2000);
            Assert.That(counter.Value, Is.EqualTo(0));
            await host.StopAsync();
        }

        private IHostBuilder BuildHost()
        {
            return Host.CreateDefaultBuilder(new string[] { })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();
                    config.AddInMemoryCollection(new[]
                    {
                        KeyValuePair.Create("AppAction:ScheduledActions:0:GroupName", "Test"),
                        KeyValuePair.Create("AppAction:ScheduledActions:0:ActionName", "Run"),
                        KeyValuePair.Create("AppAction:ScheduledActions:0:Interval", "500"),
                        KeyValuePair.Create("AppAction:ScheduledActions:0:Schedule:WeeklyTimeRanges:0:DaysOfWeek:0", "Friday"),
                        KeyValuePair.Create("AppAction:ScheduledActions:0:Schedule:WeeklyTimeRanges:0:TimeRanges:0:StartTime", "900"),
                        KeyValuePair.Create("AppAction:ScheduledActions:0:Schedule:WeeklyTimeRanges:0:TimeRanges:0:EndTime", "1000")
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
