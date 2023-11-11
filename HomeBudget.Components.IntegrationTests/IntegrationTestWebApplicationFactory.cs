using System;
using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

using HomeBudget.Rates.Api.Constants;
using HomeBudget.Rates.Api.Configuration;

namespace HomeBudget.Components.IntegrationTests
{
    public class IntegrationTestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        internal IConfiguration Configuration { get; private set; }

        public IntegrationTestWebApplicationFactory()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", HostEnvironments.Integration);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, conf) =>
            {
                conf.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{HostEnvironments.Integration}.json"));
                conf.AddEnvironmentVariables();

                Configuration = conf.Build();
            });

            builder.ConfigureTestServices(services =>
            {
                _ = services.SetDiForConnectionsAsync().GetAwaiter().GetResult();
            });

            base.ConfigureWebHost(builder);
        }
    }
}
