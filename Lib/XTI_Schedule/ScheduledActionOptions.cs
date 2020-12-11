namespace XTI_Schedule
{
    public sealed class ScheduledActionOptions
    {
        public string GroupName { get; set; }
        public string ActionName { get; set; }
        public int Interval { get; set; }
        public ScheduleOptions Schedule { get; set; }
    }
}
