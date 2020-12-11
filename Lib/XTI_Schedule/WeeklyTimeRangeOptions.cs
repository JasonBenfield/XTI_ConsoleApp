using System;

namespace XTI_Schedule
{
    public sealed class WeeklyTimeRangeOptions
    {
        public DayOfWeek[] DaysOfWeek { get; set; } = new DayOfWeek[] { };
        public TimeRangeOptions[] TimeRanges { get; set; } = new TimeRangeOptions[] { };
    }
}
