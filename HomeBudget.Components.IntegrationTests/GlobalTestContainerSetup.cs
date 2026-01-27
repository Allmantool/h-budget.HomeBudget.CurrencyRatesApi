using System.Threading.Tasks;

using NUnit.Framework;

namespace HomeBudget.Components.IntegrationTests
{
    [SetUpFixture]
    public class GlobalTestContainerSetup
    {
        internal static TestContainersService TestContainersService { get; private set; }

        [OneTimeSetUp]
        public async Task SetupAsync()
        {
            TestContainersService = await TestContainersService.InitAsync();
        }

        [OneTimeTearDown]
        public async Task TeardownAsync()
        {
            if (TestContainersService != null)
            {
                await TestContainersService.DisposeAsync();
            }
        }
    }
}
