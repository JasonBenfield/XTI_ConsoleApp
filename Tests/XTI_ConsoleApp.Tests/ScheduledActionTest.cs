using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System;
using System.Threading.Tasks;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_Schedule;

namespace XTI_ConsoleApp.Tests
{
    public sealed class ScheduledActionTest
    {
        [Test]
        public async Task ShouldRun_WhenScheduled()
        {
            var input = setup();
            input.ScheduleOptions.WeeklyTimeRanges = new[]
            {
                new WeeklyTimeRangeOptions
                {
                    DaysOfWeek = new[]
                    {
                        DayOfWeek.Monday
                    },
                    TimeRanges = new []
                    {
                        new TimeRangeOptions { StartTime = 0, EndTime = 2400 }
                    }
                }
            };
            input.Clock.Set(new DateTime(2020, 10, 19, 12, 0, 0));
            await input.ScheduledAction.TryExecute();
            Assert.That(input.Counter.ContinuousValue, Is.EqualTo(1), "Should run when scheduled");
        }

        [Test]
        public async Task ShouldNotRun_WhenNotScheduled()
        {
            var input = setup();
            input.ScheduleOptions.WeeklyTimeRanges = new[]
            {
                new WeeklyTimeRangeOptions
                {
                    DaysOfWeek = new[]
                    {
                        DayOfWeek.Monday
                    },
                    TimeRanges = new []
                    {
                        new TimeRangeOptions { StartTime = 0, EndTime = 2400 }
                    }
                }
            };
            input.Clock.Set(new DateTime(2020, 10, 20, 12, 0, 0));
            await input.ScheduledAction.TryExecute();
            Assert.That(input.Counter.ContinuousValue, Is.EqualTo(0), "Should not run when not scheduled");
        }

        private TestInput setup()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices
                (
                    (hostContext, services) =>
                    {
                        services.AddTestServiceAppServices(hostContext.Configuration);
                        services.AddScoped<Schedule>();
                        services.AddScoped<ScheduleOptions>();
                        services.AddScoped(sp =>
                        {
                            var clock = sp.GetService<Clock>();
                            var schedule = sp.GetService<Schedule>();
                            var api = sp.GetService<TestApi>();
                            return new ScheduledAction(clock, schedule, api.Test.Run);
                        });
                    }
                )
                .Build();
            var scope = host.Services.CreateScope();
            return new TestInput(scope.ServiceProvider);
        }

        private sealed class TestInput
        {
            public TestInput(IServiceProvider sp)
            {
                Schedule = sp.GetService<Schedule>();
                ScheduleOptions = sp.GetService<ScheduleOptions>();
                Clock = (FakeClock)sp.GetService<Clock>();
                ScheduledAction = sp.GetService<ScheduledAction>();
                Counter = sp.GetService<Counter>();
            }

            public Schedule Schedule { get; }
            public ScheduleOptions ScheduleOptions { get; }
            public FakeClock Clock { get; }
            public ScheduledAction ScheduledAction { get; }
            public Counter Counter { get; }
        }
    }
}
