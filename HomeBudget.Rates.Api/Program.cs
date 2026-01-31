using System;
using System.Collections.Generic;

using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

using HomeBudget.Components.CurrencyRates.MapperProfileConfigurations;
using HomeBudget.Components.Exchange.MapperProfileConfigurations;
using HomeBudget.Core;
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

// This method gets called by the runtime. Use this method to add services to the container.
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

services
    .AddOpenTelemetry()
    .ConfigureResource(r => r
        .AddService(HostServiceOptions.Name)
        .AddAttributes(new Dictionary<string, object>
        {
            [OpenTelemetryTags.DeploymentEnvironment] = environment.EnvironmentName
        }))
    .WithTracing(traceBuilder =>
    {
        traceBuilder
            .AddSource(Observability.ActivitySourceName)
            .AddAspNetCoreInstrumentation(options =>
            {
                options.EnrichWithHttpRequest = (activity, request) =>
                {
                    if (request.Headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var cid))
                    {
                        activity.SetTag(ActivityTags.CorrelationId, cid.ToString());
                    }
                };

                options.EnrichWithHttpResponse = (activity, response) =>
                {
                    activity.SetTag(ActivityTags.HttpStatusCode, response.StatusCode);
                };

                options.EnrichWithException = (activity, exception) =>
                {
                    activity.SetTag(ActivityTags.ExceptionMessage, exception.Message);
                };
            })
             .AddHttpClientInstrumentation()
             .AddSource(HostServiceOptions.Name)
             .AddOtlpExporter(o =>
             {
                 o.Endpoint = new Uri(configuration.GetSection("ObservabilityOptions:TelemetryEndpoint")?.Value);
                 o.Protocol = OtlpExportProtocol.Grpc;
             });
    })
    .ConfigureResource(resource => resource.AddService(serviceName: environment.ApplicationName))
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter(MetersTags.Hosting)
        .AddMeter(MetersTags.Routing)
        .AddMeter(MetersTags.Diagnostics)
        .AddMeter(MetersTags.Kestrel)
        .AddMeter(MetersTags.HttpConnections)
        .AddMeter(MetersTags.HealthChecks)
        .SetMaxMetricStreams(OpenTelemetryOptions.MaxMetricStreams)
        .AddPrometheusExporter()
    );

services.AddLogging(loggerBuilder => configuration.InitializeLogger(environment, loggerBuilder, webAppBuilder.Host));

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