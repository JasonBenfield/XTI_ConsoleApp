using Microsoft.Extensions.Hosting;
using System.IO;

namespace XTI_App.Hosting
{
    public sealed class XtiBasePath
    {
        private readonly AppKey appKey;
        private readonly IHostEnvironment hostEnv;

        public XtiBasePath(AppKey appKey, IHostEnvironment hostEnv)
        {
            this.appKey = appKey;
            this.hostEnv = hostEnv;
        }

        public XtiPath Value()
        {
            string versionKey;
            if (hostEnv.IsProduction())
            {
                var appDir = new DirectoryInfo(hostEnv.ContentRootPath);
                versionKey = appDir.Name;
            }
            else
            {
                versionKey = AppVersionKey.Current.DisplayText;
            }
            return new XtiPath(appKey.Name, versionKey);
        }
    }
}
