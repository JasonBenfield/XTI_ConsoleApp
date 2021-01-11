using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_Core;
using XTI_TempLog;

namespace XTI_App.Hosting
{
    public sealed class ActionRunner
    {
        private readonly IServiceProvider sp;
        private readonly string groupName;
        private readonly string actionName;
        private readonly Func<AppApiAction<EmptyRequest, EmptyActionResult>, Task> execute;

        public ActionRunner
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
            var factory = scope.ServiceProvider.GetService<IActionRunnerFactory>();
            var result = await verifyActionIsRequired(factory);
            if (result == Results.None)
            {
                result = await run(factory);
            }
            return result;
        }

        private async Task<Results> verifyActionIsRequired(IActionRunnerFactory factory)
        {
            var result = Results.None;
            var session = factory.CreateTempLogSession();
            try
            {
                var action = getApiAction(factory);
                var isOptional = await action.IsOptional();
                if (isOptional)
                {
                    result = Results.NotRequired;
                }
            }
            catch (Exception ex)
            {
                var path = getPath(factory);
                await session.StartRequest(path);
                await session.LogException(AppEventSeverity.Values.CriticalError, ex, $"Unexpected error in {path}");
                await session.EndRequest();
                result = Results.Error;
            }
            return result;
        }

        private async Task<Results> run(IActionRunnerFactory factory)
        {
            Results result = Results.None;
            var session = factory.CreateTempLogSession();
            var path = getPath(factory);
            try
            {
                await session.StartRequest(path);
                var action = getApiAction(factory);
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

        private string getPath(IActionRunnerFactory factory)
        {
            return factory.CreateXtiPath()
                .WithNewGroup(groupName)
                .WithAction(actionName)
                .Format();
        }

        private AppApiAction<EmptyRequest, EmptyActionResult> getApiAction(IActionRunnerFactory factory)
        {
            var api = factory.CreateAppApi();
            return api
                .Group(groupName)
                .Action<EmptyRequest, EmptyActionResult>(actionName);
        }
    }
}
