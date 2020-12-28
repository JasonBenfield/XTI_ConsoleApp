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

        public enum Results
        {
            None,
            Succeeded,
            Error,
            NotRequired
        }

        public async Task<Results> Run()
        {
            using var scope = sp.CreateScope();
            var result = await verifyActionIsRequired(scope.ServiceProvider);
            if (result == Results.None)
            {
                result = await run(scope.ServiceProvider);
            }
            return result;
        }

        private async Task<Results> verifyActionIsRequired(IServiceProvider services)
        {
            var result = Results.None;
            var session = services.GetService<TempLogSession>();
            try
            {
                var action = getApiAction(services);
                var isOptional = await action.IsOptional();
                if (isOptional)
                {
                    result = Results.NotRequired;
                }
            }
            catch (Exception ex)
            {
                var path = getPath(services);
                await session.StartRequest(path);
                await session.LogException(AppEventSeverity.Values.CriticalError, ex, $"Unexpected error in {path}");
                await session.EndRequest();
                result = Results.Error;
            }
            return result;
        }

        private async Task<Results> run(IServiceProvider services)
        {
            Results result = Results.None;
            var session = services.GetService<TempLogSession>();
            var path = getPath(services);
            try
            {
                await session.StartRequest(path);
                var action = getApiAction(services);
                await execute(action);
                result = Results.Succeeded;
            }
            catch (Exception ex)
            {
                result = Results.Error;
                await session.LogException
                (
                    AppEventSeverity.Values.CriticalError,
                    ex,
                    $"Unexpected error in {path}"
                );
            }
            finally
            {
                await session.EndRequest();
            }
            return result;
        }

        private string getPath(IServiceProvider services)
        {
            return services.GetService<XtiBasePath>()
                .Value()
                .WithGroup(groupName)
                .WithAction(actionName)
                .Format();
        }

        private AppApiAction<EmptyRequest, EmptyActionResult> getApiAction(IServiceProvider services)
        {
            var api = services.GetService<AppApi>();
            return api
                .Group(groupName)
                .Action<EmptyRequest, EmptyActionResult>(actionName);
        }
    }
}
