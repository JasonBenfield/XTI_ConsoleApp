using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Schedule;

namespace XTI_App.Hosting
{
    public sealed class ImmediateActionWorker : BackgroundService
    {
        private readonly IServiceProvider sp;
        private readonly ImmediateActionOptions options;

        public ImmediateActionWorker(IServiceProvider sp, ImmediateActionOptions options)
        {
            this.sp = sp;
            this.options = options;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var actionExecutor = new ActionRunner
            (
                sp,
                options.GroupName,
                options.ActionName,
                a => a.Execute(new EmptyRequest())
            );
            return actionExecutor.Run();
        }
    }
}
