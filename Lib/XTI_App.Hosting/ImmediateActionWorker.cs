using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Schedule;
using XTI_TempLog;

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = sp.CreateScope();
            var path = $"{options.GroupName}/{options.ActionName}";
            var session = scope.ServiceProvider.GetService<TempLogSession>();
            await session.StartRequest(path);
            var api = scope.ServiceProvider.GetService<AppApi>();
            var action = api
                .Group(options.GroupName)
                .Action<EmptyRequest, EmptyActionResult>(options.ActionName);
            await action.Execute(new EmptyRequest());
            await session.EndRequest();
        }
    }
}
