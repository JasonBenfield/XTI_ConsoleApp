using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using XTI_App;
using XTI_AuthenticatorClient.Extensions;
using XTI_Secrets;
using XTI_TempLog;

namespace XTI_ConsoleApp
{
    public sealed class ServiceAppEnvironmentContext : IAppEnvironmentContext
    {
        private readonly SecretCredentialsFactory credentialsFactory;
        private readonly AuthenticatorOptions authOptions;

        public ServiceAppEnvironmentContext(SecretCredentialsFactory credentialsFactory, IOptions<AuthenticatorOptions> authOptions)
        {
            this.credentialsFactory = credentialsFactory;
            this.authOptions = authOptions.Value;
        }

        public async Task<AppEnvironment> Value()
        {
            var credentials = credentialsFactory.Create(authOptions.CredentialKey);
            var credentialsValue = await credentials.Value();
            var firstMacAddress = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .FirstOrDefault();
            return new AppEnvironment
            (
                credentialsValue.UserName,
                firstMacAddress,
                Environment.MachineName,
                $"{RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture}",
                AppType.Values.Service.DisplayText
            );
        }
    }
}
