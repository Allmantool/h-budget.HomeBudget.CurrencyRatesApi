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
            var telemetryEndpoint =
                configuration.GetValue<string>("ObservabilityOptions:TelemetryEndpoint");

            if (string.IsNullOrWhiteSpace(telemetryEndpoint))
            {
                return false;
            }

            var resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: HostServiceOptions.Name,
                    serviceInstanceId: Environment.MachineName)
                .AddAttributes(new Dictionary<string, object>
                {
                    [OpenTelemetryTags.DeploymentEnvironment] = environment.EnvironmentName
                });

            services
                .AddOpenTelemetry()
                .WithTracing(traceBuilder =>
                {
                    traceBuilder
                        .SetResourceBuilder(resourceBuilder)
                        .AddSource(Observability.ActivitySourceName)
                        .AddSource(HostServiceOptions.Name)
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
                            options.RecordException = true;
                            options.Filter = httpContext =>
                            {
                                var path = httpContext.Request.Path;
                                return !path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase)
                                       && !path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase);
                            };

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
                        .AddHttpClientInstrumentation(options =>
                        {
                            options.RecordException = true;
                        })
                        .AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(telemetryEndpoint);
                            o.Protocol = OtlpExportProtocol.Grpc;
                        });
                })
                .WithMetrics(metrics => metrics
                    .SetResourceBuilder(resourceBuilder)
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
