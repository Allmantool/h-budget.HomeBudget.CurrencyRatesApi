using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

using HomeBudget.Rates.Api.Constants;
using HomeBudget.Rates.Api.Configuration;
using Microsoft.Extensions.Hosting;

namespace HomeBudget.Components.IntegrationTests
{
    public class IntegrationTestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        private readonly Action _webHostInitializationCallback;
        internal IConfiguration Configuration { get; private set; }

        public IntegrationTestWebApplicationFactory(Action webHostInitializationCallback)
        {
            _webHostInitializationCallback = webHostInitializationCallback;
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", HostEnvironments.Integration);
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            return base.CreateHostBuilder();
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return base.CreateWebHostBuilder();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                _ = services.SetDiForConnectionsAsync().GetAwaiter().GetResult();
            });

            builder.ConfigureAppConfiguration((_, conf) =>
            {
                conf.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{HostEnvironments.Integration}.json"));
                conf.AddEnvironmentVariables();

                Configuration = conf.Build();

                _webHostInitializationCallback?.Invoke();
            });

            base.ConfigureWebHost(builder);
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            return base.CreateHost(builder);
        }

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            return base.CreateServer(builder);
        }
    }
}
