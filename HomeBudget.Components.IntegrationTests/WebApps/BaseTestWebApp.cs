using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting.Server.Features;
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
        private IntegrationTestWebApplicationFactory<TEntryPoint> WebFactory { get; set; }
        internal static TestContainersService TestContainersService { get; set; }

        internal RestClient RestHttpClient { get; set; }

        public async Task<bool> InitAsync(int workersMaxAmount = 1)
        {
            try
            {
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

                var server = WebFactory.Server;
                var addresses = server.Features.Get<IServerAddressesFeature>();
                var realAddress = addresses.Addresses.FirstOrDefault();
                var baseAddress = realAddress is null ?
                    WebFactory.ClientOptions.BaseAddress
                    : new Uri(realAddress);

                var clientOptions = new WebApplicationFactoryClientOptions
                {
                    BaseAddress = baseAddress,
                    AllowAutoRedirect = true,
                    HandleCookies = true
                };

                var baseClient = WebFactory.CreateClient(clientOptions);
                baseClient.Timeout = TimeSpan.FromMinutes(BaseTestWebAppOptions.WebClientTimeoutInMinutes);

                var httpClient = WebFactory.CreateClient(new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false,
                    BaseAddress = new Uri("http://localhost:7064")
                });

                RestHttpClient = new RestClient(httpClient);

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static async Task<bool> StartContainersAsync()
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
        }
    }
}
