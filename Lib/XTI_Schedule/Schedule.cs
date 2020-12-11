using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Linq;

namespace XTI_Schedule
{
    public sealed class Schedule
    {
        private readonly ScheduleOptions options;

        public Schedule(ScheduleOptions options)
        {
            this.options = options ?? new ScheduleOptions();
        }

        public bool IsInSchedule(DateTime dateTime)
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

        private bool isInTimeRange(DateTime dateTime, TimeRangeOptions timeRange)
        {
            var time = dateTime.Hour * 100 + dateTime.Minute;
            return time >= timeRange.StartTime && time <= timeRange.EndTime;
        }
    }
}
