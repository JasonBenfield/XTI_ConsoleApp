using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;

namespace XTI_Schedule
{
    public sealed class ScheduledAction
    {
        private readonly Clock clock;
        private readonly Schedule schedule;
        private readonly AppApiAction<EmptyRequest, EmptyActionResult> action;

        public ScheduledAction(Clock clock, Schedule schedule, AppApiAction<EmptyRequest, EmptyActionResult> action)
        {
            this.clock = clock;
            this.schedule = schedule;
            this.action = action;
        }

        public async Task TryExecute()
        {
            var now = clock.Now();
            if (schedule.IsInSchedule(now))
            {
                await action.Execute(new EmptyRequest());
            }
        }
    }
}
