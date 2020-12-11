using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;
using XTI_Schedule;
using XTI_TempLog;

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
                using var scope = sp.CreateScope();
                var session = scope.ServiceProvider.GetService<TempLogSession>();
                var appEnv = await scope.ServiceProvider.GetService<IAppEnvironmentContext>().Value();
                var path = scope.ServiceProvider.GetService<XtiBasePath>()
                    .Value()
                    .WithGroup(options.GroupName)
                    .WithAction(options.ActionName)
                    .Format();
                try
                {
                    await session.StartRequest(path);
                    var clock = scope.ServiceProvider.GetService<Clock>();
                    var schedule = new Schedule(options.Schedule);
                    var api = scope.ServiceProvider.GetService<AppApi>();
                    var action = api
                        .Group(options.GroupName)
                        .Action<EmptyRequest, EmptyActionResult>(options.ActionName);
                    var scheduledAction = new ScheduledAction(clock, schedule, action);
                    await scheduledAction.TryExecute();
                    await session.EndRequest();
                }
                catch (Exception ex)
                {
                    await session.LogException(AppEventSeverity.Values.CriticalError, ex, $"Unexpected error in {path}");
                }
                await Task.Delay(options.Interval, stoppingToken);
            }
        }
    }
}
