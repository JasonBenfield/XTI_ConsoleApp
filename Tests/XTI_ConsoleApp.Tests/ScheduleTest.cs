using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using XTI_Core;
using XTI_Core.Fakes;
using XTI_Schedule;

namespace XTI_ConsoleApp.Tests
{
    public sealed class ScheduleTest
    {
        [Test]
        public void ShouldBeInScheduleByDayOfWeek()
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
            var result = input.Schedule.IsInSchedule(new DateTime(2020, 10, 19, 12, 0, 0));
            Assert.That(result, Is.True, "Should be scheduled by day of week");
        }

        [Test]
        public void ShouldNotBeInSchedule_WhenNotTheDayOfWeek()
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
            var result = input.Schedule.IsInSchedule(new DateTime(2020, 10, 20, 12, 0, 0));
            Assert.That(result, Is.False, "Should not be scheduled when not the day of week");
        }

        [Test]
        public void ShouldNotBeInSchedule_WhenNotInTimeRange()
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
                        new TimeRangeOptions { StartTime = 1000, EndTime = 1400 }
                    }
                }
            };
            var result = input.Schedule.IsInSchedule(new DateTime(2020, 10, 19, 8, 0, 0));
            Assert.That(result, Is.False, "Should not be in schedule");
        }

        [Test]
        public void ShouldBeInSchedule_WhenDateTimeIsInAnyOfTheOptionTimeRange()
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
                        new TimeRangeOptions { StartTime = 1000, EndTime = 1400 },
                        new TimeRangeOptions { StartTime = 1700, EndTime = 1900 }
                    }
                }
            };
            var result = input.Schedule.IsInSchedule(new DateTime(2020, 10, 19, 18, 0, 0));
            Assert.That(result, Is.True, "Should be in schedule");
        }

        [Test]
        public void ShouldBeInSchedule_WhenDateTimeIsInAnyOfTheOptionDaysOfTheWeek()
        {
            var input = setup();
            input.ScheduleOptions.WeeklyTimeRanges = new[]
            {
                new WeeklyTimeRangeOptions
                {
                    DaysOfWeek = new[]
                    {
                        DayOfWeek.Monday, DayOfWeek.Wednesday
                    },
                    TimeRanges = new []
                    {
                        new TimeRangeOptions { StartTime = 1000, EndTime = 1400 },
                        new TimeRangeOptions { StartTime = 1700, EndTime = 1900 }
                    }
                }
            };
            var result = input.Schedule.IsInSchedule(new DateTime(2020, 10, 21, 18, 0, 0));
            Assert.That(result, Is.True, "Should be in schedule");
        }

        [Test]
        public void ShouldConvertLocalTimeToUtc()
        {
            var input = setup();
            input.ScheduleOptions.IsUtc = false;
            input.ScheduleOptions.WeeklyTimeRanges = new[]
            {
                new WeeklyTimeRangeOptions
                {
                    DaysOfWeek = new[]
                    {
                        DayOfWeek.Monday, DayOfWeek.Wednesday
                    },
                    TimeRanges = new []
                    {
                        new TimeRangeOptions { StartTime = 2200, EndTime = 2400 }
                    }
                }
            };
            Assert.That
            (
                input.Schedule.IsInSchedule(new DateTime(2020, 10, 22, 3, 30, 0, DateTimeKind.Utc)),
                Is.True, "Should be in schedule"
            );
            Assert.That
            (
                input.Schedule.IsInSchedule(new DateTime(2020, 10, 22, 1, 30, 0, DateTimeKind.Utc)),
                Is.False, "Should not be in schedule"
            );
        }

        [Test]
        public void ShouldNotConvertLocalTimeToUtc()
        {
            var input = setup();
            input.ScheduleOptions.IsUtc = true;
            input.ScheduleOptions.WeeklyTimeRanges = new[]
            {
                new WeeklyTimeRangeOptions
                {
                    DaysOfWeek = new[]
                    {
                        DayOfWeek.Monday, DayOfWeek.Wednesday
                    },
                    TimeRanges = new []
                    {
                        new TimeRangeOptions { StartTime = 1700, EndTime = 1900 }
                    }
                }
            };
            Assert.That
            (
                input.Schedule.IsInSchedule(new DateTime(2020, 10, 21, 18, 0, 0, DateTimeKind.Utc)),
                Is.True, "Should be in schedule"
            );
            Assert.That
            (
                input.Schedule.IsInSchedule(new DateTime(2020, 10, 21, 20, 0, 0, DateTimeKind.Utc)),
                Is.False, "Should not be in schedule"
            );
        }

        private TestInput setup()
        {
            var services = new ServiceCollection();
            services.AddScoped<ScheduleOptions>();
            services.AddScoped<Schedule>();
            services.AddScoped<Clock, FakeClock>();
            var sp = services.BuildServiceProvider();
            return new TestInput(sp);
        }

        private sealed class TestInput
        {
            public TestInput(IServiceProvider sp)
            {
                Schedule = sp.GetService<Schedule>();
                ScheduleOptions = sp.GetService<ScheduleOptions>();
            }

            public Schedule Schedule { get; }
            public ScheduleOptions ScheduleOptions { get; }
        }
    }
}
