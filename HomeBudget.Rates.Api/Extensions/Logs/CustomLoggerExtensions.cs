using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using System.Threading.Channels;
using Elastic.Apm.SerilogEnricher;
using Elastic.Channels;
using Elastic.Ingest.Elasticsearch;
using Elastic.Ingest.Elasticsearch.DataStreams;
using Elastic.Serilog.Sinks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.OpenTelemetry;

using HomeBudget.Core.Constants;
using HomeBudget.Core.Options;
using HomeBudget.Rates.Api.Constants;

namespace HomeBudget.Rates.Api.Extensions.Logs
{
    internal static class CustomLoggerExtensions
    {
        public static Logger InitializeLogger(
            this IConfiguration configuration,
            IWebHostEnvironment environment,
            ILoggingBuilder loggingBuilder,
            ConfigureHostBuilder configureHostBuilder)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProperty(LoggerTags.ServiceName, HostServiceOptions.Name)
                .Enrich.WithProperty(LoggerTags.Environment, environment)
                .Enrich.WithProperty(LoggerTags.HostService, environment.EnvironmentName)
                .Enrich.WithProperty(LoggerTags.TraceId, Activity.Current?.TraceId.ToString())
                .Enrich.WithProperty(LoggerTags.SpanId, Activity.Current?.SpanId.ToString())
                .Enrich.WithExceptionDetails()
                .Enrich.WithSpan()
                .Enrich.WithElasticApmCorrelationInfo()
                .WriteTo.Debug()
                .WriteTo.Console()
                .WriteTo.AddAndConfigureSentry(configuration, environment)
                .WriteTo.OpenTelemetry(o =>
                {
                    o.Endpoint = configuration.GetSection("ObservabilityOptions:LogsEndpoint")?.Value;
                    o.Protocol = OtlpProtocol.Grpc;
                })
                .TryAddSeqSupport(configuration)
                .TryAddElasticSearchSupport(configuration, environment)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(logger);

            configureHostBuilder.UseSerilog(logger);

            loggingBuilder.AddOpenTelemetry(options =>
            {
                options.IncludeScopes = true;
                options.ParseStateValues = true;
                options.IncludeFormattedMessage = true;
                options.AddOtlpExporter();
            });

            // SelfLog.Enable(msg => File.AppendAllText("serilog-rates-api-selflog.txt", msg));
            Log.Logger = logger;

            return logger;
        }

        private static LoggerConfiguration TryAddSeqSupport(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            try
            {
                var seqOptions = configuration.GetSection(ConfigurationSectionKeys.SeqOptions)?.Get<SeqOptions>();

                if (!seqOptions.IsEnabled)
                {
                    return loggerConfiguration;
                }

                var seqUrl = seqOptions.Uri?.ToString() ?? Environment.GetEnvironmentVariable("SEQ_URL");

                loggerConfiguration.WriteTo.Seq(seqUrl);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine($"Failed to configure Seq sink: {ex}");
            }

            return loggerConfiguration;
        }

        private static LoggerConfiguration TryAddElasticSearchSupport(
            this LoggerConfiguration loggerConfiguration,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            try
            {
                var elasticOptions = configuration.GetSection(ConfigurationSectionKeys.ElasticSearchOptions)?.Get<ElasticSearchOptions>();

                if (!elasticOptions.IsEnabled)
                {
                    return loggerConfiguration;
                }

                var elasticNodeUrl = (elasticOptions.Uri?.ToString() ?? Environment.GetEnvironmentVariable(EnvironmentsVariables.AspNetCoreUrls)) ?? string.Empty;

                return string.IsNullOrWhiteSpace(elasticNodeUrl)
                    ? loggerConfiguration
                    : loggerConfiguration
                        .Enrich.WithElasticApmCorrelationInfo()
                        .WriteTo.Elasticsearch(
                            new List<Uri>
                            {
                                new(elasticNodeUrl)
                            },
                            opt => opt.ConfigureElasticSink(environment));
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine($"Elasticsearch sink initialization failed: {ex}");
            }

            return loggerConfiguration;
        }

        private static void ConfigureElasticSink(this ElasticsearchSinkOptions options, IHostEnvironment environment)
        {
            var formattedExecuteAssemblyName = typeof(Program).Assembly.GetName().Name;
            var dateIndexPostfix = DateTime.UtcNow.ToString(DateFormats.ElasticSearch, CultureInfo.InvariantCulture);
            var baseStreamName = $"{formattedExecuteAssemblyName}-{environment.EnvironmentName}-{dateIndexPostfix}";

            var formattedStreamName = baseStreamName
                .Replace(".", "-", StringComparison.OrdinalIgnoreCase)
                .ToUpperInvariant();

            options.DataStream = new DataStreamName(formattedStreamName);
            options.BootstrapMethod = BootstrapMethod.Failure;
            options.MinimumLevel = LogEventLevel.Debug;
            options.ConfigureChannel = channelOpts =>
            {
                channelOpts.BufferOptions = new BufferOptions
                {
                    BoundedChannelFullMode = BoundedChannelFullMode.DropNewest,
                };
            };
        }
    }
}
