using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XTI_App.Api;
using XTI_TempLog;

namespace XTI_App.Hosting
{
    public interface IActionRunnerFactory
    {
        AppApi CreateAppApi();
        TempLogSession CreateTempLogSession();
        XtiPath CreateXtiPath();
    }
}
