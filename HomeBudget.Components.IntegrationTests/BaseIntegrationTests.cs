using System;
using System.Diagnostics;
using System.Threading.Tasks;

using NUnit.Framework;

using HomeBudget.Components.IntegrationTests.Constants;

namespace HomeBudget.Components.IntegrationTests
{
    public abstract class BaseIntegrationTests
    {
        private static bool _initialized;
        internal TestContainersService TestContainers { get; private set; }

        [OneTimeSetUp]
        public virtual async Task SetupAsync()
        {
            if (_initialized && TestContainers is not null && TestContainers.IsReadyForUse)
            {
                return;
            }

            var maxWait = TimeSpan.FromMinutes(BaseTestContainerOptions.StopTimeoutInMinutes);
            var sw = Stopwatch.StartNew();

            TestContainers = await TestContainersService.InitAsync();

            while (TestContainers is null || !TestContainers.IsReadyForUse)
            {
                if (sw.Elapsed > maxWait)
                {
                    Assert.Fail(
                        $"TestContainersService did not start within the allowed timeout of {maxWait.TotalSeconds} seconds."
                    );
                }

                await Task.Delay(TimeSpan.FromSeconds(ComponentTestOptions.TestContainersWaitingInSeconds));
            }

            sw.Stop();

            await Task.Delay(TimeSpan.FromSeconds(ComponentTestOptions.TestContainersWaitingInSeconds));

            _initialized = true;
        }

        [OneTimeTearDown]
        public async Task TerminateAsync()
        {
            // await OperationsTestWebApp.ResetAsync();
        }
    }
}
