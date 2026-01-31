using System;
using System.Diagnostics;
using System.Threading.Tasks;

using NUnit.Framework;

using HomeBudget.Components.IntegrationTests.Constants;

namespace HomeBudget.Components.IntegrationTests
{
    public abstract class BaseIntegrationTests : IAsyncDisposable
    {
        private bool _ownsContainers;
        private bool _disposed;

        internal TestContainersService TestContainers { get; private set; } = GlobalTestContainerSetup.TestContainersService;

        public virtual async Task SetupAsync()
        {
            ThrowIfDisposed();

            if (TestContainers is not null && TestContainers.IsReadyForUse)
            {
                return;
            }

            var maxWait = TimeSpan.FromMinutes(BaseTestContainerOptions.StopTimeoutInMinutes);
            var sw = Stopwatch.StartNew();

            TestContainers = await TestContainersService.InitAsync();
            _ownsContainers = true;

            while (!TestContainers.IsReadyForUse)
            {
                if (sw.Elapsed > maxWait)
                {
                    Assert.Fail(
                        $"TestContainersService did not start within the allowed timeout of {maxWait.TotalSeconds} seconds."
                    );
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(ComponentTestOptions.TestContainersWaitingInSeconds)
                );
            }

            sw.Stop();

            await Task.Delay(
                TimeSpan.FromSeconds(ComponentTestOptions.TestContainersWaitingInSeconds)
            );
        }

        public virtual async Task TerminateAsync()
        {
            await DisposeAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_ownsContainers && TestContainers is not null)
            {
                try
                {
                    await TestContainers.DisposeAsync();
                }
                catch (Exception ex)
                {
                    await TestContext.Error.WriteLineAsync(
                         $"[DisposeAsync] Failed to dispose TestContainers: {ex}"
                     );
                }
            }

            TestContainers = null;
            GC.SuppressFinalize(this);
        }

        protected void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
        }
    }
}
