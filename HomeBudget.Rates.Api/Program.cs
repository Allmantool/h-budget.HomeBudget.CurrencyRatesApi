using System;
using System.Collections.Generic;
using System.Reflection;

using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

using HomeBudget.Components.CurrencyRates.MapperProfileConfigurations;
using HomeBudget.Rates.Api.Configuration;
using HomeBudget.Rates.Api.Constants;
using HomeBudget.Rates.Api.Extensions;
using HomeBudget.Rates.Api.Extensions.Logs;

var webAppBuilder = WebApplication.CreateBuilder(args);

// Add services to the container.
var services = webAppBuilder.Services;
var environment = webAppBuilder.Environment;
var configuration = webAppBuilder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true)
    .Build();

var webHost = webAppBuilder.WebHost;

webHost
    .ConfigureKestrel(opt =>
    {
        opt.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(1);
    })
    .ConfigureAppConfiguration((hostContext, builder) =>
    {
        if (hostContext.HostingEnvironment.IsEnvironment(HostEnvironments.Docker))
        {
            builder.AddUserSecrets<HomeBudget.Rates.Api.Program>();
        }
    });

// This method gets called by the runtime. Use this method to add services to the container.
services.AddControllers();

await services.SetUpDiAsync(configuration);
services.AddAutoMapper(new List<Assembly>
{
    typeof(HomeBudget.Rates.Api.Program).Assembly,
    CurrencyRatesComponentMappingProfiles.GetExecutingAssembly(),
});

services
    .SetUpHealthCheck(configuration, Environment.GetEnvironmentVariable("ASPNETCORE_URLS"))
    .AddValidatorsFromAssemblyContaining<HomeBudget.Rates.Api.Program>()
    .AddResponseCaching()
    .SetupSwaggerGen();

configuration.InitializeLogger(environment, webAppBuilder.Host);

// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
var webApp = webAppBuilder.Build();

webApp.SetUpBaseApplication(services, environment, configuration);

// webApp.UseHttpsRedirection();
var executionAppName = typeof(HomeBudget.Rates.Api.Program).Assembly.GetName().Name;

try
{
    Log.Information("The app '{0}' is about to start.", executionAppName);

    webApp.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, $"Application terminated unexpectedly, failed to start {executionAppName}");
    Log.CloseAndFlush();
}

// To add visibility for integration tests
namespace HomeBudget.Rates.Api
{
    public partial class Program
    {
        protected Program() { }
    }
}