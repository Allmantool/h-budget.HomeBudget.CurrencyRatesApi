﻿using System;
using System.IO;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Components.IntegrationTests.MockServices;
using HomeBudget.Rates.Api.Configuration;
using HomeBudget.Rates.Api.Constants;

namespace HomeBudget.Components.IntegrationTests
{
    public class IntegrationTestWebApplicationFactory<TStartup>
        (Action webHostInitializationCallback) : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        internal IConfiguration Configuration { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
               _ = services.SetDiForConnectionsAsync().GetAwaiter().GetResult();

               services.AddScoped<INationalBankRatesProvider, MockNationalBankRatesProvider>();
            });

            builder.ConfigureAppConfiguration((_, conf) =>
            {
                conf.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), $"appsettings.{HostEnvironments.Integration}.json"));
                conf.AddEnvironmentVariables();

                Configuration = conf.Build();

                webHostInitializationCallback?.Invoke();
            });

            base.ConfigureWebHost(builder);
        }
    }
}
