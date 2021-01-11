using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_App.Hosting;
using XTI_Schedule;

namespace XTI_ServiceApp
{
    public sealed class ServiceAppWorker : BackgroundService
    {
        private readonly IServiceProvider sp;
        private readonly AppActionOptions options;

        public ServiceAppWorker(IServiceProvider sp, IOptions<AppActionOptions> options)
        {
            this.sp = sp;
            this.options = options.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var worker = new MultiActionRunner
            (
                sp,
                options.ImmediateActions,
                options.ScheduledActions,
                options.AlwaysRunningActions
            );
            return worker.Start(stoppingToken);
        }
    }
}
