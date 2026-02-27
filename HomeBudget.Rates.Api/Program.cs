using System;

using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

using HomeBudget.Components.CurrencyRates.MapperProfileConfigurations;
using HomeBudget.Components.Exchange.MapperProfileConfigurations;
using HomeBudget.Core.Constants;
using HomeBudget.Rates.Api.Configuration;
using HomeBudget.Rates.Api.Constants;
using HomeBudget.Rates.Api.Exceptions.Handlers;
using HomeBudget.Rates.Api.Extensions;
using HomeBudget.Rates.Api.Extensions.Logs;
using HomeBudget.Rates.Api.MapperProfileConfigurations;

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

services.AddControllers(o =>
{
    o.Conventions.Add(new SwaggerControllerDocConvention());
});

// services.AddJwtAuthentication(configuration);
await services.SetUpDiAsync(configuration, environment);
services.AddAutoMapper(
    cfg =>
    {
        cfg.AddMaps(typeof(Program).Assembly);
        cfg.AddMaps(ApiRatesMappingProfiles.GetExecutingAssembly());
        cfg.AddMaps(CurrencyRatesMappingProfiles.GetExecutingAssembly());
        cfg.AddMaps(ExchangeMappingProfiles.GetExecutingAssembly());
    });

services.AddExceptionHandler<BadExternalApiRequestExceptionHandler>();
services.AddExceptionHandler<GlobalExceptionHandler>();
services.AddProblemDetails();

services
    .SetUpHealthCheck(configuration, Environment.GetEnvironmentVariable(EnvironmentsVariables.AspNetCoreUrls))
    .AddValidatorsFromAssemblyContaining<HomeBudget.Rates.Api.Program>()
    .AddResponseCaching()
    .SetupSwaggerGen();

services.AddSignalR();

services.AddHeaderPropagation(options =>
{
    options.Headers.Add(HttpHeaderKeys.HostService, HostServiceOptions.Name);
    options.Headers.Add(HttpHeaderKeys.CorrelationId);
});

services.TryAddTracingSupport(environment, configuration);

services
    .AddAllElasticApm()
    .AddLogging(loggerBuilder => configuration.InitializeLogger(environment, loggerBuilder, webAppBuilder.Host));

webHost.AddAndConfigureSentry();

var webApp = webAppBuilder.Build();

var webAppLifetie = webApp.Lifetime;

if (!environment.IsProduction())
{
    webAppLifetie.ApplicationStarted.Register(() =>
    {
        Console.WriteLine("🚀 Host STARTED");
    });

    webAppLifetie.ApplicationStopping.Register(() =>
    {
        Console.WriteLine("🛑 Host STOPPING");
    });

    webAppLifetie.ApplicationStopped.Register(() =>
    {
        Console.WriteLine("💀 Host STOPPED");
    });
}

// Map the /metrics endpoint
webApp.UseOpenTelemetryPrometheusScrapingEndpoint();

// webApp.SetUpSecurity();
webApp.SetUpBaseApplication(services, environment, configuration);

// webApp.UseHttpsRedirection();
var executionAppName = typeof(HomeBudget.Rates.Api.Program).Assembly.GetName().Name;

try
{
    Log.Information("The app '{0}' is about to start.", executionAppName);

    await webApp.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, $"Application terminated unexpectedly, failed to start {executionAppName}");
    await Log.CloseAndFlushAsync();
}

// To add visibility for integration tests
namespace HomeBudget.Rates.Api
{
    public partial class Program
    {
        protected Program() { }
    }
}