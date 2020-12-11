namespace XTI_Schedule
{
    public sealed class AppActionOptions
    {
        public static readonly string AppAction = nameof(AppAction);
        public ImmediateActionOptions[] ImmediateActions { get; set; } = new ImmediateActionOptions[] { };
        public ScheduledActionOptions[] ScheduledActions { get; set; } = new ScheduledActionOptions[] { };
        public AlwaysRunningActionOptions[] AlwaysRunningActions { get; set; } = new AlwaysRunningActionOptions[] { };
    }
}
