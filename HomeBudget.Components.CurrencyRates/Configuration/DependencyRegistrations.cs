using Microsoft.Extensions.DependencyInjection;

using HomeBudget.Components.CurrencyRates.Providers;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Components.CurrencyRates.Services;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;

namespace HomeBudget.Components.CurrencyRates.Configuration
{
    public static class DependencyRegistrations
    {
        public static IServiceCollection RegisterCurrencyRatedIoCDependency(
            this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            return services
                .AddScoped<IConfigSettingsProvider, ConfigSettingsProvider>()
                .AddScoped<IConfigSettingsServices, ConfigSettingsServices>()
                .AddScoped<ICurrencyRatesWriteProvider, CurrencyRatesWriteProvider>()
                .AddScoped<INationalBankRatesProvider, NationalBankRatesProvider>()
                .AddScoped<ICurrencyRatesReadProvider, CurrencyRatesReadProvider>()
                .AddScoped<ICurrencyRatesService, CurrencyRatesService>()
                .RegisterNationalApiHttpClient(serviceProvider)
                .AddMediatR(configuration =>
                {
                    configuration.RegisterServicesFromAssembly(typeof(DependencyRegistrations).Assembly);
                });
        }
    }
}
