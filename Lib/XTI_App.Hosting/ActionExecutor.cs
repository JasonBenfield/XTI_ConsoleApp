using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog;

namespace XTI_App.Hosting
{
    public sealed class ActionExecutor
    {
        private readonly IServiceProvider sp;
        private readonly string groupName;
        private readonly string actionName;
        private readonly Func<AppApiAction<EmptyRequest, EmptyActionResult>, Task> execute;

        public ActionExecutor
        (
            IServiceProvider sp,
            string groupName,
            string actionName,
            Func<AppApiAction<EmptyRequest, EmptyActionResult>, Task> execute
        )
        {
            this.sp = sp;
            this.groupName = groupName;
            this.actionName = actionName;
            this.execute = execute;
        }

        public async Task Run()
        {
            using var scope = sp.CreateScope();
            var session = scope.ServiceProvider.GetService<TempLogSession>();
            var path = scope.ServiceProvider.GetService<XtiBasePath>()
                .Value()
                .WithGroup(groupName)
                .WithAction(actionName)
                .Format();
            AppApiAction<EmptyRequest, EmptyActionResult> action;
            try
            {
                var api = scope.ServiceProvider.GetService<AppApi>();
                action = api
                    .Group(groupName)
                    .Action<EmptyRequest, EmptyActionResult>(actionName);
                var isOptional = await action.IsOptional();
                if (isOptional)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                await session.StartRequest(path);
                await session.LogException(AppEventSeverity.Values.CriticalError, ex, $"Unexpected error in {path}");
                await session.EndRequest();
                return;
            }
            try
            {
                await session.StartRequest(path);
                await execute(action);
            }
            catch (Exception ex)
            {
                await session.LogException(AppEventSeverity.Values.CriticalError, ex, $"Unexpected error in {path}");
            }
            finally
            {
                await session.EndRequest();
            }
        }
    }
}
