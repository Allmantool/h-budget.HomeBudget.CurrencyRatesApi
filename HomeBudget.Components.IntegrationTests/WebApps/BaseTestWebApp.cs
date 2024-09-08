using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Testing;
using RestSharp;

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

            WebFactory = new IntegrationTestWebApplicationFactory<TEntryPoint>(
                async () =>
                {
                    TestContainersService = new TestContainersService(WebFactory?.Configuration);

                    await StartAsync();
                });

            var httpClient = WebFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("http://localhost:7064")
            });

            RestHttpClient = new RestClient(httpClient);
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
