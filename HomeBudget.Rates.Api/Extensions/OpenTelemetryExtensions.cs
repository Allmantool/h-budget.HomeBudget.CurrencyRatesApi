using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using HomeBudget.Core;
using HomeBudget.Core.Constants;
using HomeBudget.Rates.Api.Constants;

namespace HomeBudget.Rates.Api.Extensions
{
    internal static class OpenTelemetryExtensions
    {
        public static bool TryAddTracingSupport(
            this IServiceCollection services,
            IWebHostEnvironment environment,
            IConfigurationRoot configuration)
        {
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
                       if (!environment.IsProduction())
                       {
                           return;
                       }

                       traceBuilder
                           .AddSource(Observability.ActivitySourceName)
                           .AddSqlClientInstrumentation(options =>
                           {
                               options.EnrichWithSqlCommand = (activity, obj) =>
                               {
                                   if (obj is SqlCommand cmd)
                                   {
                                       activity.SetTag("db.commandTimeout", cmd.CommandTimeout);
                                   }
                               };
                               options.RecordException = true;
                           })
                           .AddRedisInstrumentation()
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

            return true;
        }
    }
}
