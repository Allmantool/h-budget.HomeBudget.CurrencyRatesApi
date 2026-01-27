using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using RestSharp;

using HomeBudget.Components.IntegrationTests.Constants;
using HomeBudget.Components.IntegrationTests.Models;
using HomeBudget.Rates.Api.Constants;

namespace HomeBudget.Components.IntegrationTests.WebApps
{
    internal abstract class BaseTestWebApp<TEntryPoint> : BaseTestWebAppDispose
        where TEntryPoint : class
    {
        private bool _disposed;
        private IntegrationTestWebApplicationFactory<TEntryPoint> WebFactory { get; set; }
        internal TestContainersService TestContainersService { get; set; }

        internal RestClient RestHttpClient { get; set; }

        public async Task<bool> InitAsync()
        {
            try
            {
                ObjectDisposedException.ThrowIf(_disposed, nameof(BaseTestWebApp<TEntryPoint>));

                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", HostEnvironments.Integration);
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", HostEnvironments.Integration);

                var testProperties = TestContext.CurrentContext.Test.Properties;
                var testCategory = testProperties.Get("Category") as string;

                if (!TestTypes.Integration.Equals(testCategory, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                TestContainersService = await TestContainersService.InitAsync();
                await StartContainersAsync();

                WebFactory = new IntegrationTestWebApplicationFactory<TEntryPoint>(
                    () => new TestContainersConnections
                    {
                        MsSqlContainer = TestContainersService.MsSqlDbContainer.GetConnectionString(),
                        RedisContainer = TestContainersService.CacheContainer.GetConnectionString()
                    });

                var baseClient = WebFactory.CreateDefaultClient();
                baseClient.Timeout = TimeSpan.FromMinutes(BaseTestWebAppOptions.WebClientTimeoutInMinutes);

                RestHttpClient = new RestClient(baseClient);

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> StartContainersAsync()
        {
            if (TestContainersService is null)
            {
                return false;
            }

            return TestContainersService.IsReadyForUse;
        }

        protected override async ValueTask DisposeAsyncCoreAsync()
        {
            if (TestContainersService != null)
            {
                await TestContainersService.DisposeAsync();
            }

            if (WebFactory != null)
            {
                await WebFactory.DisposeAsync();
            }

            RestHttpClient?.Dispose();

            _disposed = true;
        }
    }
}
