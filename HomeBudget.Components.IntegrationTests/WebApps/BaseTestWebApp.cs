using System;
using System.Threading.Tasks;

using NUnit.Framework;
using RestSharp;

using HomeBudget.Components.IntegrationTests.Constants;
using HomeBudget.Rates.Api.Constants;

namespace HomeBudget.Components.IntegrationTests.WebApps
{
    internal abstract class BaseTestWebApp<TEntryPoint> : BaseTestWebAppDispose
        where TEntryPoint : class
    {
        private IntegrationTestWebApplicationFactory<TEntryPoint> WebFactory { get; }
        private TestContainersService TestContainersService { get; set; }

        internal RestClient RestHttpClient { get; }

        protected BaseTestWebApp()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", HostEnvironments.Integration);

            var testProperties = TestContext.CurrentContext.Test.Properties;
            var testCategory = testProperties.Get("Category") as string;

            if (!TestTypes.Integration.Equals(testCategory, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            WebFactory = new IntegrationTestWebApplicationFactory<TEntryPoint>(
                async () =>
                {
                    TestContainersService = new TestContainersService(WebFactory?.Configuration);

                    await StartAsync();
                });

            var httpClient = WebFactory.CreateClient();

            RestHttpClient = new RestClient(
                httpClient,
                new RestClientOptions(new Uri("http://localhost:6064")));
        }

        public async Task StartAsync()
        {
            await TestContainersService.UpAndRunningContainersAsync();
        }

        public async Task StopAsync()
        {
            await TestContainersService.StopAsync();
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
