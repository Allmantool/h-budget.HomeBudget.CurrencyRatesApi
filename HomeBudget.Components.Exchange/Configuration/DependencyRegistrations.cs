using Microsoft.Extensions.DependencyInjection;

using HomeBudget.Components.Exchange.Services;
using HomeBudget.Components.Exchange.Services.Interfaces;

namespace HomeBudget.Components.Exchange.Configuration
{
    public static class DependencyRegistrations
    {
        public static IServiceCollection RegisterExchangeIoCDependency(
            this IServiceCollection services)
        {
            return services.AddScoped<IExchangeService, ExchangeService>();
        }
    }
}
