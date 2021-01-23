using XTI_App.Api;
using XTI_TempLog;

namespace XTI_App.Hosting
{
    public interface IActionRunnerFactory
    {
        IAppApi CreateAppApi();
        TempLogSession CreateTempLogSession();
        XtiPath CreateXtiPath();
    }
}
