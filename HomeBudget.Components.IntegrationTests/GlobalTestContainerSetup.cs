using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace HomeBudget.Components.IntegrationTests
{
    [SetUpFixture]
    internal class GlobalTestContainerSetup
    {
        private static int _counter;
        private readonly int _id = Interlocked.Increment(ref _counter);

        internal static TestContainersService TestContainersService { get; private set; }

        [OneTimeSetUp]
        public async Task SetupAsync()
        {
            TestContext.Progress.WriteLine($"[WebApp {_id}] OneTimeSetUp START ({GetType().Name})");

            TestContainersService = await TestContainersService.InitAsync();

            TestContext.Progress.WriteLine($"[WebApp {_id}] OneTimeSetUp END");
        }

        [OneTimeTearDown]
        public async Task TeardownAsync()
        {
            TestContext.Progress.WriteLine($"[WebApp {_id}] OneTimeTearDown START");

            if (TestContainersService != null)
            {
                await TestContainersService.DisposeAsync();
            }

            TestContext.Progress.WriteLine($"[WebApp {_id}] OneTimeTearDown END");
        }
    }
}
