using System;
using System.Threading.RateLimiting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using HomeBudget.Components.CurrencyRates.Clients.Limiters;
using HomeBudget.Components.CurrencyRates.Providers;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Components.CurrencyRates.Services;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Limiters;
using HomeBudget.Core.Options;

namespace HomeBudget.Components.CurrencyRates.Configuration
{
    public static class DependencyRegistrations
    {
        public static IServiceCollection RegisterCurrencyRatesIoCDependency(
            this IServiceCollection services)
        {
            return services
                .AddScoped<IConfigSettingsProvider, ConfigSettingsProvider>()
                .AddScoped<IConfigSettingsServices, ConfigSettingsServices>()
                .AddScoped<ICurrencyRatesWriteProvider, CurrencyRatesWriteProvider>()
                .AddScoped<INationalBankRatesProvider, NationalBankRatesProvider>()
                .AddScoped<ICurrencyRatesReadProvider, CurrencyRatesReadProvider>()
                .AddScoped<ICurrencyRatesService, CurrencyRatesService>()
                .AddSingleton<IHttpClientRateLimiter>((sp) =>
                {
                    var options = sp.GetRequiredService<IOptions<HttpClientOptions>>();
                    var httpClientOptions = options.Value;

                    var limiter = new TokenBucketRateLimiter(
                        new TokenBucketRateLimiterOptions
                        {
                            TokenLimit = httpClientOptions.RateLimiterBurstLimit,
                            TokensPerPeriod = httpClientOptions.RateLimiterTokensPerPeriod,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(httpClientOptions.RateLimiterReplenishmentSeconds),
                            QueueLimit = httpClientOptions.RateLimiterQueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            AutoReplenishment = true
                        });

                    return new NationalBankHttpClientRateLimiter(limiter);
                })
                .AddMediatR(configuration =>
                {
                    configuration.RegisterServicesFromAssembly(typeof(DependencyRegistrations).Assembly);
                });
        }
    }
}
