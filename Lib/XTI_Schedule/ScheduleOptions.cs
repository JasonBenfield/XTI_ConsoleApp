namespace XTI_Schedule
{
    public sealed class ScheduleOptions
    {
        public WeeklyTimeRangeOptions[] WeeklyTimeRanges { get; set; } = new WeeklyTimeRangeOptions[] { };
        public bool IsUtc { get; set; }
    }
}
