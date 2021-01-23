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

namespace XTI_ConsoleApp.Tests
{
    public sealed class ScheduledActionWorkerTest
    {
        [Test]
        public async Task ShouldRunScheduledAction()
        {
            var host = BuildHost().Build();
            setTimeWithinSchedule(host.Services);
            var counter = host.Services.GetService<Counter>();
            var _ = Task.Run(() => host.Run());
            await delay();
            Assert.That(counter.ContinuousValue, Is.GreaterThan(0));
            Console.WriteLine($"Counter value: {counter.ContinuousValue}");
            await host.StopAsync();
        }

        [Test]
        public async Task ShouldRunOnce_WhenTypeIsPeriodicUntilSuccess()
        {
            var host = BuildHost().Build();
            setTimeWithinSchedule(host.Services);
            var counter = host.Services.GetService<Counter>();
            var _ = Task.Run(() => host.Run());
            await delay();
            Assert.That(counter.UntilSuccessValue, Is.EqualTo(1));
            await host.StopAsync();
        }

        [Test]
        public async Task ShouldStartRequest()
        {
            var host = BuildHost().Build();
            setTimeWithinSchedule(host.Services);
            var counter = host.Services.GetService<Counter>();
            var _ = Task.Run(() => host.Run());
            await delay();
            var startRequests = await getStartRequests(host.Services);
            var api = host.Services.GetService<TestApi>();
            var startRequest = startRequests.FirstOrDefault(r => api.Test.RunContinuously.Path.Equals(r.Path));
            Assert.That(startRequest, Is.Not.Null, "Should add start request");
            await host.StopAsync();
        }

        [Test]
        public async Task ShouldEndRequest()
        {
            var host = BuildHost().Build();
            setTimeWithinSchedule(host.Services);
            var counter = host.Services.GetService<Counter>();
            var _ = Task.Run(() => host.Run());
            await delay();
            var startRequests = await getStartRequests(host.Services);
            var endRequests = await getEndRequests(host.Services);
            var endRequest = endRequests.FirstOrDefault(r => r.RequestKey == startRequests[0].RequestKey);
            Assert.That(endRequest, Is.Not.Null, "Should end request");
            await host.StopAsync();
        }

        [Test]
        public async Task ShouldLogException()
        {
            var host = BuildHost().Build();
            setTimeWithinSchedule(host.Services);
            var testOptions = host.Services.GetService<TestOptions>();
            testOptions.IsOptional = false;
            testOptions.ThrowException = true;
            var counter = host.Services.GetService<Counter>();
            var _ = Task.Run(() => host.Run());
            await delay();
            var startRequests = await getStartRequests(host.Services);
            var api = host.Services.GetService<TestApi>();
            var startRequest = startRequests.FirstOrDefault(r => api.Test.OptionalRun.Path.Equals(r.Path));
            var logEvents = await getLogEvents(host.Services);
            logEvents = logEvents.Where(evt => evt.RequestKey == startRequest.RequestKey).ToArray();
            Assert.That(logEvents.Count(), Is.GreaterThan(0), "Should log exceptions");
            Assert.That(logEvents[0].Severity, Is.EqualTo(AppEventSeverity.Values.CriticalError), "Should log critical error");
            await host.StopAsync();
        }

        private static void setTimeWithinSchedule(IServiceProvider services)
        {
            var clock = (FakeClock)services.GetService<Clock>();
            clock.Set(new DateTime(2020, 10, 16, 13, 30, 0, DateTimeKind.Utc));
        }

        [Test]
        public async Task ShouldNotRunScheduledAction_WhenNotInSchedule()
        {
            var host = BuildHost().Build();
            var clock = (FakeClock)host.Services.GetService<Clock>();
            clock.Set(new DateTime(2020, 10, 16, 14, 30, 0, DateTimeKind.Utc));
            var counter = host.Services.GetService<Counter>();
            var _ = Task.Run(() => host.RunAsync());
            await delay();
            Assert.That(counter.ContinuousValue, Is.EqualTo(0));
            await host.StopAsync();
        }

        [Test]
        public async Task ShouldRunOptionalAction_WhenItIsNotOptional()
        {
            var host = BuildHost().Build();
            setTimeWithinSchedule(host.Services);
            var tempLog = host.Services.GetService<TempLog>();
            var counter = host.Services.GetService<Counter>();
            var testOptions = host.Services.GetService<TestOptions>();
            testOptions.IsOptional = false;
            var _ = Task.Run(() => host.Run());
            await delay();
            fastForward(host.Services, TimeSpan.FromMinutes(1));
            var startRequests = await getStartRequests(host.Services);
            var api = host.Services.GetService<TestApi>();
            startRequests = startRequests.Where(r => api.Test.OptionalRun.Path.Equals(r.Path)).ToArray();
            Assert.That(startRequests.Count(), Is.GreaterThan(0), "Should start request when action is not optional");
            Assert.That(counter.OptionalValue, Is.GreaterThan(0));
            await host.StopAsync();
        }

        [Test]
        public async Task ShouldNotRunOptionalAction_WhenItIsOptional()
        {
            var host = BuildHost().Build();
            setTimeWithinSchedule(host.Services);
            var tempLog = host.Services.GetService<TempLog>();
            var counter = host.Services.GetService<Counter>();
            var testOptions = host.Services.GetService<TestOptions>();
            testOptions.IsOptional = true;
            var _ = Task.Run(() => host.Run());
            await delay();
            fastForward(host.Services, TimeSpan.FromMinutes(1));
            var startRequests = await getStartRequests(host.Services);
            var api = host.Services.GetService<TestApi>();
            startRequests = startRequests.Where(r => api.Test.OptionalRun.Path.Equals(r.Path)).ToArray();
            Assert.That(startRequests.Count(), Is.EqualTo(0), "Should not start request for an optional action");
            Assert.That(counter.OptionalValue, Is.EqualTo(0), "Should not run an optional action");
            await host.StopAsync();
        }

        private void fastForward(IServiceProvider services, TimeSpan howLong)
        {
            var clock = (FakeClock)services.GetService<Clock>();
            clock.Add(howLong);
        }

        private Task<StartRequestModel[]> getStartRequests(IServiceProvider services)
        {
            var tempLog = services.GetService<TempLog>();
            var clock = (FakeClock)services.GetService<Clock>();
            var files = tempLog.StartRequestFiles(clock.Now()).ToArray();
            return deserializeLogFiles<StartRequestModel>(files);
        }

        private Task<EndRequestModel[]> getEndRequests(IServiceProvider services)
        {
            var tempLog = services.GetService<TempLog>();
            var clock = (FakeClock)services.GetService<Clock>();
            return deserializeLogFiles<EndRequestModel>
            (
                tempLog.EndRequestFiles(clock.Now()).ToArray()
            );
        }

        private Task<LogEventModel[]> getLogEvents(IServiceProvider services)
        {
            var tempLog = services.GetService<TempLog>();
            var clock = (FakeClock)services.GetService<Clock>();
            return deserializeLogFiles<LogEventModel>
            (
                tempLog.LogEventFiles(clock.Now()).ToArray()
            );
        }

        private async Task<T[]> deserializeLogFiles<T>(IEnumerable<ITempLogFile> logFiles)
        {
            var logObjects = new List<T>();
            foreach (var logFile in logFiles)
            {
                var deserialized = await logFile.Read();
                if (!string.IsNullOrWhiteSpace(deserialized))
                {
                    var logObject = JsonSerializer.Deserialize<T>(deserialized);
                    logObjects.Add(logObject);
                }
            }
            return logObjects.ToArray();
        }

        private static Task delay() => Task.Delay(500);

        private IHostBuilder BuildHost()
        {
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");
            return Host.CreateDefaultBuilder(new string[] { })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();
                    config.AddInMemoryCollection(new[]
                    {
                        KeyValuePair.Create("AppAction:ScheduledActions:0:GroupName", "Test"),
                        KeyValuePair.Create("AppAction:ScheduledActions:0:ActionName", "RunContinuously"),
                        KeyValuePair.Create("AppAction:ScheduledActions:0:Interval", "100"),
                        KeyValuePair.Create("AppAction:ScheduledActions:0:Schedule:WeeklyTimeRanges:0:DaysOfWeek:0", "Friday"),
                        KeyValuePair.Create("AppAction:ScheduledActions:0:Schedule:WeeklyTimeRanges:0:TimeRanges:0:StartTime", "900"),
                        KeyValuePair.Create("AppAction:ScheduledActions:0:Schedule:WeeklyTimeRanges:0:TimeRanges:0:EndTime", "1000"),
                        KeyValuePair.Create("AppAction:ScheduledActions:1:GroupName", "Test"),
                        KeyValuePair.Create("AppAction:ScheduledActions:1:ActionName", "OptionalRun"),
                        KeyValuePair.Create("AppAction:ScheduledActions:1:Interval", "100"),
                        KeyValuePair.Create("AppAction:ScheduledActions:1:Schedule:WeeklyTimeRanges:0:DaysOfWeek:0", "Friday"),
                        KeyValuePair.Create("AppAction:ScheduledActions:1:Schedule:WeeklyTimeRanges:0:TimeRanges:0:StartTime", "900"),
                        KeyValuePair.Create("AppAction:ScheduledActions:1:Schedule:WeeklyTimeRanges:0:TimeRanges:0:EndTime", "1000"),
                        KeyValuePair.Create("AppAction:ScheduledActions:2:GroupName", "Test"),
                        KeyValuePair.Create("AppAction:ScheduledActions:2:ActionName", "RunUntilSuccess"),
                        KeyValuePair.Create("AppAction:ScheduledActions:2:Type", "PeriodicUntilSuccess"),
                        KeyValuePair.Create("AppAction:ScheduledActions:2:Interval", "1"),
                        KeyValuePair.Create("AppAction:ScheduledActions:2:Schedule:WeeklyTimeRanges:0:DaysOfWeek:0", "Friday"),
                        KeyValuePair.Create("AppAction:ScheduledActions:2:Schedule:WeeklyTimeRanges:0:TimeRanges:0:StartTime", "900"),
                        KeyValuePair.Create("AppAction:ScheduledActions:2:Schedule:WeeklyTimeRanges:0:TimeRanges:0:EndTime", "1000")
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
