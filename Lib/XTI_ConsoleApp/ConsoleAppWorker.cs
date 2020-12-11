using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_App.Hosting;
using XTI_Schedule;

namespace XTI_ConsoleApp
{
    public sealed class ConsoleAppWorker : BackgroundService
    {
        private readonly IServiceProvider sp;
        private readonly AppActionOptions options;

        public ConsoleAppWorker(IServiceProvider sp, IOptions<AppActionOptions> options)
        {
            this.sp = sp;
            this.options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var worker = new AppMiddleware(sp, options.ImmediateActions, options.ScheduledActions, options.AlwaysRunningActions);
            await worker.Start(stoppingToken);
            var lifetime = sp.GetService<IHostApplicationLifetime>();
            lifetime.StopApplication();
        }
    }
}
