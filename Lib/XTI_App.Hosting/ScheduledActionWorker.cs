using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;
using XTI_Schedule;

namespace XTI_App.Hosting
{
    public sealed class ScheduledActionWorker : BackgroundService
    {
        private readonly IServiceProvider sp;
        private readonly ScheduledActionOptions options;

        public ScheduledActionWorker(IServiceProvider sp, ScheduledActionOptions options)
        {
            this.sp = sp;
            this.options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var periodicSucceeded = false;
            while (!stoppingToken.IsCancellationRequested)
            {
                var clock = sp.GetService<Clock>();
                var schedule = new Schedule(options.Schedule);
                if (schedule.IsInSchedule(clock.Now()))
                {
                    if (options.Type != ScheduledActionTypes.PeriodicUntilSuccess || !periodicSucceeded)
                    {
                        var actionExecutor = new ActionRunner
                        (
                            sp,
                            options.GroupName,
                            options.ActionName,
                            a => a.Execute(new EmptyRequest())
                        );
                        var result = await actionExecutor.Run();
                        if (result == ActionRunner.Results.Succeeded)
                        {
                            periodicSucceeded = true;
                        }
                    }
                }
                else
                {
                    periodicSucceeded = false;
                }
                await Task.Delay(options.Interval, stoppingToken);
            }
        }
    }
}
