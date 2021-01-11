using System;
using System.Linq;
using XTI_Core;

namespace XTI_Schedule
{
    public sealed class Schedule
    {
        private readonly ScheduleOptions options;

        public Schedule(ScheduleOptions options)
        {
            this.options = options ?? new ScheduleOptions();
        }

        public bool IsInSchedule(DateTimeOffset dateTime)
        {
            if (!options.IsUtc)
            {
                dateTime = dateTime.ToLocalTime();
            }
            var isInSchedule = false;
            var weeklyTimeRanges = options.WeeklyTimeRanges ?? new WeeklyTimeRangeOptions[] { };
            foreach (var weeklyTimeRange in weeklyTimeRanges)
            {
                if (weeklyTimeRange.DaysOfWeek.Any(d => d == dateTime.DayOfWeek))
                {
                    var timeRanges = weeklyTimeRange.TimeRanges ?? new TimeRangeOptions[] { };
                    if (timeRanges.Any(tr => isInTimeRange(dateTime, tr)))
                    {
                        isInSchedule = true;
                    }
                }
            }
            return isInSchedule;
        }

        private bool isInTimeRange(DateTimeOffset dateTime, TimeRangeOptions timeRangeOptions)
        {
            var date = new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, dateTime.Offset);
            var timeRange = TimeRange.Between
            (
                date.AddHours(timeRangeOptions.StartTime / 100).AddMinutes(timeRangeOptions.StartTime % 100),
                date.AddHours(timeRangeOptions.EndTime / 100).AddMinutes(timeRangeOptions.EndTime % 100)
            );
            return timeRange.IsInRange(dateTime);
        }
    }
}
