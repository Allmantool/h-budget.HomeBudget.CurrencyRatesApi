﻿using System;
using System.Reflection;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Sentry;
using Serilog.Configuration;
using Serilog.Events;
using Sentry.Extensibility;
using Sentry.Infrastructure;

using HomeBudget.Core.Options;

namespace HomeBudget.Rates.Api.Extensions
{
    public static class SentryExtensions
    {
        public static LoggerConfiguration AddAndConfigureSentry(
            this LoggerSinkConfiguration loggerConfiguration,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            return AddAndConfigureSentry(
                loggerConfiguration,
                configuration,
                environment,
                new SentryConfigurationOptions());
        }

        public static LoggerConfiguration AddAndConfigureSentry(
            this LoggerSinkConfiguration loggerConfiguration,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            SentryConfigurationOptions sentryOptions)
        {
            return loggerConfiguration.Sentry(sentrySerilogOptions =>
            {
                if (!TryIfSentryConfigurationValid(sentryOptions, configuration, out var verifiedOptions))
                {
                    sentrySerilogOptions.Dsn = string.Empty;
                    return;
                }

                var version = Assembly.GetExecutingAssembly().GetName().Version;

                if (version != null)
                {
                    sentrySerilogOptions.Release = version.ToString();
                }

                if (environment.IsUnderDevelopment())
                {
                    sentrySerilogOptions.Debug = true;
                    sentrySerilogOptions.DiagnosticLogger = new TraceDiagnosticLogger(SentryLevel.Debug);
                }

                sentrySerilogOptions.Environment = environment.EnvironmentName;
                sentrySerilogOptions.Dsn = verifiedOptions.Dns;
                sentrySerilogOptions.TracesSampleRate = environment.IsUnderDevelopment() ? 1.0 : 0.3;
                sentrySerilogOptions.IsGlobalModeEnabled = true;
                sentrySerilogOptions.AttachStacktrace = true;
                sentrySerilogOptions.SendDefaultPii = environment.IsUnderDevelopment(); // Disable sending PII for security (e.g., user emails)
                sentrySerilogOptions.MinimumBreadcrumbLevel = environment.IsUnderDevelopment() ? LogEventLevel.Debug : LogEventLevel.Information;
                sentrySerilogOptions.MinimumEventLevel = LogEventLevel.Warning;
                sentrySerilogOptions.DiagnosticLevel = SentryLevel.Error;
            });
        }

        public static IHostBuilder AddAndConfigureSentry(this IHostBuilder hostBuilder)
        {
            return AddAndConfigureSentry(hostBuilder, new SentryConfigurationOptions());
        }

        public static IHostBuilder AddAndConfigureSentry(this IHostBuilder hostBuilder, SentryConfigurationOptions sentryOptions)
        {
            return hostBuilder.ConfigureServices((hostBuilderContext, services) =>
            {
                if (!TryIfSentryConfigurationValid(sentryOptions, hostBuilderContext.Configuration, out var verifiedOptions))
                {
                    return;
                }

                var environment = hostBuilderContext.HostingEnvironment;
                services.AddLogging(logging =>
                {
                    logging.AddSentry(sentryLoggingOptions =>
                    {
                        var version = Assembly.GetExecutingAssembly().GetName().Version;

                        if (version != null)
                        {
                            sentryLoggingOptions.Release = version.ToString();
                        }

                        if (environment.IsUnderDevelopment())
                        {
                            sentryLoggingOptions.Debug = true;
                            sentryLoggingOptions.DiagnosticLogger = new TraceDiagnosticLogger(SentryLevel.Debug);
                        }

                        sentryLoggingOptions.Environment = environment.EnvironmentName;
                        sentryLoggingOptions.Dsn = verifiedOptions.Dns;
                        sentryLoggingOptions.TracesSampleRate = environment.IsUnderDevelopment() ? 1.0 : 0.3;
                        sentryLoggingOptions.IsGlobalModeEnabled = true;
                        sentryLoggingOptions.AttachStacktrace = true;
                        sentryLoggingOptions.SendDefaultPii = environment.IsUnderDevelopment();
                        sentryLoggingOptions.MinimumBreadcrumbLevel = environment.IsUnderDevelopment() ? LogLevel.Debug : LogLevel.Information;
                        sentryLoggingOptions.MinimumEventLevel = LogLevel.Warning;
                        sentryLoggingOptions.DiagnosticLevel = SentryLevel.Error;
                    });
                });
            });
        }

        public static IWebHostBuilder AddAndConfigureSentry(this IWebHostBuilder webHostBuilder)
        {
            return AddAndConfigureSentry(webHostBuilder, new SentryConfigurationOptions());
        }

        public static IWebHostBuilder AddAndConfigureSentry(this IWebHostBuilder webHostBuilder, SentryConfigurationOptions sentryOptions)
        {
            return webHostBuilder.UseSentry((webHostBuilderContext, sentryAspNetCoreOptions) =>
            {
                if (!TryIfSentryConfigurationValid(sentryOptions, webHostBuilderContext.Configuration, out var verifiedOptions))
                {
                    sentryAspNetCoreOptions.Dsn = string.Empty;
                    return;
                }

                var environment = webHostBuilderContext.HostingEnvironment;

                var version = Assembly.GetExecutingAssembly().GetName().Version;

                if (version != null)
                {
                    sentryAspNetCoreOptions.Release = version.ToString();
                }

                if (environment.IsUnderDevelopment())
                {
                    sentryAspNetCoreOptions.Debug = true;
                    sentryAspNetCoreOptions.DiagnosticLogger = new TraceDiagnosticLogger(SentryLevel.Debug);
                }

                sentryAspNetCoreOptions.Environment = environment.EnvironmentName;
                sentryAspNetCoreOptions.Dsn = verifiedOptions.Dns;
                sentryAspNetCoreOptions.TracesSampleRate = environment.IsUnderDevelopment() ? 1.0 : 0.3;
                sentryAspNetCoreOptions.IsGlobalModeEnabled = true;
                sentryAspNetCoreOptions.AttachStacktrace = true;
                sentryAspNetCoreOptions.SendDefaultPii = environment.IsUnderDevelopment(); // Disable sending PII for security (e.g., user emails)
                sentryAspNetCoreOptions.MaxRequestBodySize = environment.IsUnderDevelopment() ? RequestSize.Always : RequestSize.Small;
                sentryAspNetCoreOptions.MinimumBreadcrumbLevel = environment.IsUnderDevelopment() ? LogLevel.Debug : LogLevel.Information;
                sentryAspNetCoreOptions.MinimumEventLevel = LogLevel.Warning;
                sentryAspNetCoreOptions.DiagnosticLevel = SentryLevel.Error;
            });
        }

        private static bool TryIfSentryConfigurationValid(
            SentryConfigurationOptions sentryOptions,
            IConfiguration configuration,
            out SentryConfigurationOptions verifiedOptions)
        {
            verifiedOptions = null;

            var sentryDns = sentryOptions.Dns ?? configuration["Sentry:Dsn"];

            var isNotNullOrEmptyDns = !string.IsNullOrWhiteSpace(sentryDns);

            var isValidUri = Uri.TryCreate(sentryDns, UriKind.Absolute, out var uri)
                             && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

            var isOptionsValid = isNotNullOrEmptyDns && isValidUri;

            if (isOptionsValid)
            {
                verifiedOptions = new SentryConfigurationOptions
                {
                    Dns = sentryDns
                };
            }

            return isOptionsValid;
        }
    }
}
