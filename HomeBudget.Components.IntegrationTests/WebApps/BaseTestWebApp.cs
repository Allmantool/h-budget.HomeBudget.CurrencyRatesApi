using System;
using System.Threading.Tasks;

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
        private IntegrationTestWebApplicationFactory<TEntryPoint> WebFactory { get; set; }
        internal TestContainersService TestContainersService { get; private set; } = GlobalTestContainerSetup.TestContainersService;

        internal RestClient RestHttpClient { get; set; }

        public async Task<bool> InitAsync()
        {
            try
            {
                if (WebFactory is not null && WebFactory.IsInitialized)
                {
                    return true;
                }

                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", HostEnvironments.Integration);
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", HostEnvironments.Integration);

                var testProperties = TestContext.CurrentContext.Test.Properties;
                var testCategory = testProperties.Get("Category") as string;

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

        public async Task ShutdownAsync()
        {
            if (RestHttpClient is not null)
            {
                RestHttpClient.Dispose();
            }

            if (WebFactory != null)
            {
                await WebFactory.DisposeAsync();
                WebFactory = null;
            }
        }
    }
}
