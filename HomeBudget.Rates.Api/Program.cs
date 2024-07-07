using System;
using System.Collections.Generic;
using System.Reflection;

using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
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
using Google.Protobuf.WellKnownTypes;
using HomeBudget.Rates.Api.Middlewares;

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
services.AddControllers(o =>
{
    o.Conventions.Add(new SwaggerControllerDocConvention());
});

// services.AddJwtAuthentication(configuration);
await services.SetUpDiAsync(configuration, environment);
services.AddAutoMapper(new List<Assembly>
{
    ApiRatesMappingProfiles.GetExecutingAssembly(),
    CurrencyRatesMappingProfiles.GetExecutingAssembly(),
    ExchangeMappingProfiles.GetExecutingAssembly(),
});

services.AddExceptionHandler<BadExternalApiRequestExceptionHandler>();
services.AddExceptionHandler<GlobalExceptionHandler>();
services.AddProblemDetails();

services
    .SetUpHealthCheck(configuration, Environment.GetEnvironmentVariable("ASPNETCORE_URLS"))
    .AddValidatorsFromAssemblyContaining<HomeBudget.Rates.Api.Program>()
    .AddResponseCaching()
    .SetupSwaggerGen();

services.AddHeaderPropagation(options =>
{
    options.Headers.Add(HttpHeaderKeys.HostService, HostServiceOptions.Name);
    options.Headers.Add(HttpHeaderKeys.CorrelationId);
});

// Add relevant services for OTel to function
services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: environment.ApplicationName))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("Microsoft.AspNetCore.Hosting")
        .AddMeter("Microsoft.AspNetCore.Routing")
        .AddMeter("Microsoft.AspNetCore.Diagnostics")
        .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
        .AddMeter("Microsoft.AspNetCore.Http.Connections")
        .AddMeter("Microsoft.Extensions.Diagnostics.HealthChecks")
        .SetMaxMetricStreams(OpenTelemetryOptions.MaxMetricStreams)
        .SetMaxMetricPointsPerMetricStream(OpenTelemetryOptions.MaxMetricPointsPerMetricStream)
        .AddPrometheusExporter()
    );

configuration.InitializeLogger(environment, webAppBuilder);

// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
var webApp = webAppBuilder.Build();

// Map the /metrics endpoint
webApp.UseOpenTelemetryPrometheusScrapingEndpoint();

// webApp.SetUpSecurity();
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