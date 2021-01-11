using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XTI_Schedule;

namespace XTI_App.Hosting
{
    public sealed class MultiActionRunner
    {
        private readonly IServiceProvider sp;
        private readonly ImmediateActionOptions[] immediateActions;
        private readonly ScheduledActionOptions[] scheduledActions;
        private readonly AlwaysRunningActionOptions[] alwaysRunningActions;

        public MultiActionRunner
        (
            IServiceProvider sp,
            ImmediateActionOptions[] immediateActions,
            ScheduledActionOptions[] scheduledActions,
            AlwaysRunningActionOptions[] alwaysRunningActions
        )
        {
            this.sp = sp;
            this.immediateActions = immediateActions;
            this.scheduledActions = scheduledActions;
            this.alwaysRunningActions = alwaysRunningActions;
        }

        public async Task Start(CancellationToken stoppingToken)
        {
            using var scope = sp.CreateScope();
            var factory = scope.ServiceProvider.GetService<IActionRunnerFactory>();
            var session = factory.CreateTempLogSession();
            await session.StartSession();
            var tasks = getTasks(stoppingToken);
            await Task.WhenAll(tasks);
            await session.EndSession();
        }

        private Task[] getTasks(CancellationToken stoppingToken)
        {
            var tasks = new List<Task>();
            foreach (var immediateActionOptions in immediateActions)
            {
                var worker = new ImmediateActionWorker(sp, immediateActionOptions);
                var task = worker.StartAsync(stoppingToken);
                tasks.Add(task);
            }
            foreach (var alwaysRunningActionOptions in alwaysRunningActions)
            {
                var scheduledActionOptions = new ScheduledActionOptions
                {
                    GroupName = alwaysRunningActionOptions.GroupName,
                    ActionName = alwaysRunningActionOptions.ActionName,
                    Interval = alwaysRunningActionOptions.Interval,
                    Schedule = new ScheduleOptions
                    {
                        WeeklyTimeRanges = new[]
                        {
                            new WeeklyTimeRangeOptions
                            {
                                DaysOfWeek = new[]
                                {
                                    DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday,DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday
                                },
                                TimeRanges = new [] { new TimeRangeOptions { StartTime = 0, EndTime = 2400 } }
                            }
                        }
                    }
                };
                var worker = new ScheduledActionWorker(sp, scheduledActionOptions);
                var task = worker.StartAsync(stoppingToken);
                tasks.Add(task);
            }
            foreach (var scheduledActionOptions in scheduledActions)
            {
                var worker = new ScheduledActionWorker(sp, scheduledActionOptions);
                var task = worker.StartAsync(stoppingToken);
                tasks.Add(task);
            }
            return tasks.ToArray();
        }
    }
}
