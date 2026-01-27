using System.Threading.Tasks;

using NUnit.Framework;

using HomeBudget.Components.IntegrationTests.WebApps;

namespace HomeBudget.Components.IntegrationTests
{
    [SetUpFixture]
    public class GlobalTestContainerSetup
    {
        private GlobalWebApp _webAppSut;

        [OneTimeSetUp]
        public async Task SetupAsync()
        {
            _webAppSut = new GlobalWebApp();

            await _webAppSut.InitAsync();
        }

        [OneTimeTearDown]
        public async Task TeardownAsync()
        {
            if (_webAppSut != null)
            {
                await _webAppSut.DisposeAsync();
            }
        }
    }
}
