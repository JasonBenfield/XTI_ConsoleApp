using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
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
            while (!stoppingToken.IsCancellationRequested)
            {
                var actionExecutor = new ActionExecutor
                (
                    sp,
                    options.GroupName,
                    options.ActionName,
                    a =>
                    {
                        var clock = sp.GetService<Clock>();
                        var schedule = new Schedule(options.Schedule);
                        var scheduledAction = new ScheduledAction(clock, schedule, a);
                        return scheduledAction.TryExecute();
                    }
                );
                await actionExecutor.Run();
                await Task.Delay(options.Interval, stoppingToken);
            }
        }
    }
}
