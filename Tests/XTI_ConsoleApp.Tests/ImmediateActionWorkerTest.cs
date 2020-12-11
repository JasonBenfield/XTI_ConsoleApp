using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XTI_ConsoleApp.Tests
{
    public sealed class ImmediateActionWorkerTest
    {
        private static readonly Counter counter = new Counter();

        [Test]
        public async Task ShouldRunImmediateAction()
        {
            await BuildHost().RunConsoleAsync();
            Assert.That(counter.Value, Is.EqualTo(1));
        }

        private IHostBuilder BuildHost()
        {
            return Host.CreateDefaultBuilder(new string[] { })
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.Sources.Clear();
                    config.AddInMemoryCollection(new[]
                    {
                        KeyValuePair.Create("AppAction:ImmediateActions:0:GroupName", "Test"),
                        KeyValuePair.Create("AppAction:ImmediateActions:0:ActionName", "Run")
                    });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTestConsoleAppServices(hostContext.Configuration);
                    services.AddSingleton(_ => counter);
                });
        }
    }
}
