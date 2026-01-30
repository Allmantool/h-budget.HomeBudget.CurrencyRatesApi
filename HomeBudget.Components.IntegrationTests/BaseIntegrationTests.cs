using System;
using System.Diagnostics;
using System.Threading.Tasks;

using NUnit.Framework;

using HomeBudget.Components.IntegrationTests.Constants;

namespace HomeBudget.Components.IntegrationTests
{
    public abstract class BaseIntegrationTests
    {
        internal TestContainersService TestContainers { get; private set; } = GlobalTestContainerSetup.TestContainersService;

        [OneTimeSetUp]
        public virtual async Task SetupAsync()
        {
            if (TestContainers is not null && TestContainers.IsReadyForUse)
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
        }

        [OneTimeTearDown]
        public virtual async Task TerminateAsync()
        {
            // await OperationsTestWebApp.ResetAsync();
        }
    }
}
