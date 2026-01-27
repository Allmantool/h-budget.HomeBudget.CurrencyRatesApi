using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Polly;
using Polly.Extensions.Http;
using Refit;

using HomeBudget.Components.CurrencyRates.Clients;
using HomeBudget.Core.Models;
using HomeBudget.Core.Options;
using HomeBudget.Rates.Api.Exceptions.Handlers;

namespace HomeBudget.Rates.Api.Configuration
{
    internal static class RefitHttpClientConfiguration
    {
        public static IServiceCollection RegisterNationalApiHttpClient(
            this IServiceCollection services,
            IServiceProvider serviceProvider)
        {
            var externalResourceUrls = serviceProvider.GetRequiredService<IOptions<ExternalResourceUrls>>().Value;

            var httpClientOptions = serviceProvider.GetRequiredService<IOptions<HttpClientOptions>>().Value;

            services
                .AddRefitClient<INationalBankApiClient>(_ => GetRefitSettings())
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = externalResourceUrls.NationalBankUrl;
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.Timeout = TimeSpan.FromSeconds(httpClientOptions.TimeoutInSeconds);
                })
                .ConfigurePrimaryHttpMessageHandler(() => CreateSocketsHttpHandler(httpClientOptions))
                .AddPolicyHandler(GetRetryPolicy(serviceProvider))
                .AddHttpMessageHandler<HttpLoggingHandler>()
                .SetHandlerLifetime(TimeSpan.FromMinutes(httpClientOptions.HandlerLifetimeInMinutes))
                .AddHeaderPropagation();

            return services;
        }

        private static RefitSettings GetRefitSettings()
        {
            return new RefitSettings
            {
                CollectionFormat = CollectionFormat.Multi,
                ContentSerializer = GetHttpContentSerializer(),
                HttpMessageHandlerFactory = () => new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
                }
            };
        }

        private static IHttpContentSerializer GetHttpContentSerializer()
        {
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                Formatting = Formatting.Indented,
            };

            return new NewtonsoftJsonContentSerializer(serializerSettings);
        }

        private static SocketsHttpHandler CreateSocketsHttpHandler(
            HttpClientOptions options)
        {
            return new SocketsHttpHandler
            {
                MaxConnectionsPerServer = options.MaxConcurrentRequests,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                PooledConnectionLifetime = TimeSpan.FromMinutes(options.HandlerLifetimeInMinutes),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(options.PooledConnectionIdleTimeoutInMinutes),
                SslOptions =
                {
                    RemoteCertificateValidationCallback = (_, _, _, _) => true
                }
            };
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider serviceProvider)
        {
            var pollyRetryOptions = serviceProvider.GetRequiredService<IOptions<PollyRetryOptions>>().Value;

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(
                    pollyRetryOptions.RetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(pollyRetryOptions.SleepDurationInSeconds, retryAttempt)));
        }
    }
}
