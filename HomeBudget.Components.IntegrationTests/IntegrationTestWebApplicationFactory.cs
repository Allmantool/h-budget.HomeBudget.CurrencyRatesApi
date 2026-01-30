using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;

using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Components.IntegrationTests.MockServices;
using HomeBudget.Components.IntegrationTests.Models;
using HomeBudget.Rates.Api.Configuration;
using HomeBudget.Rates.Api.Constants;

namespace HomeBudget.Components.IntegrationTests
{
    internal class IntegrationTestWebApplicationFactory<TStartup>
        (Func<TestContainersConnections> webHostInitializationCallback) : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        private static int _counter;
        private readonly int _id = Interlocked.Increment(ref _counter);

        private TestContainersConnections _containersConnections;

        internal IConfiguration Configuration { get; private set; }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            TestContext.Progress.WriteLine($"[WebFactory {_id}] CreateHost()");
            return base.CreateHost(builder);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                _ = services.SetDiForConnectionsAsync().ConfigureAwait(false).GetAwaiter().GetResult();

                services.AddScoped<INationalBankRatesProvider, MockNationalBankRatesProvider>();

                services.AddHttpClient("test-default").SetHandlerLifetime(TimeSpan.FromMinutes(10));
            });

            builder.ConfigureAppConfiguration((_, conf) =>
            {
                conf.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{HostEnvironments.Integration}.json"));
                conf.AddEnvironmentVariables();

                Configuration = conf.Build();

                _containersConnections = webHostInitializationCallback.Invoke();

                conf.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["DatabaseOptions:ConnectionString"] = _containersConnections.MsSqlContainer,
                    ["DatabaseOptions:RedisConnectionString"] = $"{_containersConnections.RedisContainer},allowAdmin=true"
                });

                Configuration = conf.Build();
            });

            builder.UseEnvironment(HostEnvironments.Integration);

            TestContext.Progress.WriteLine($"[WebFactory {_id}] ConfigureWebHost()");
            base.ConfigureWebHost(builder);
        }

        protected override void Dispose(bool disposing)
        {
            TestContext.Progress.WriteLine(
                $"[WebFactory {_id}] Dispose(disposing={disposing})");

            base.Dispose(disposing);
        }
    }
}
