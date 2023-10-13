using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

using HomeBudget.Components.CurrencyRates.Configuration;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Extensions;
using HomeBudget.Core.Models;
using HomeBudget.DataAccess.Dapper.Extensions;

namespace HomeBudget.Rates.Api.Configuration
{
    internal static class DependencyRegistrations
    {
        public static async Task<IServiceCollection> SetUpDiAsync(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services
                .Configure<DatabaseConnectionOptions>(configuration.GetSection(ConfigurationSectionKeys.DatabaseOptions))
                .Configure<CacheStoreOptions>(configuration.GetSection(ConfigurationSectionKeys.CacheStoreOptions))
                .Configure<ExternalResourceUrls>(configuration.GetSection(ConfigurationSectionKeys.ExternalResourceUrls))
                .Configure<PollyRetryOptions>(configuration.GetSection(ConfigurationSectionKeys.PollyRetryOptions))
                .RegisterCoreIoCDependency()
                .RegisterCurrencyRatedIoCDependency()
                .RegistryDapperIoCDependencies();

            var serviceProvider = services.BuildServiceProvider();

            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseConnectionOptions>>().Value;
            var configSettingsProvider = serviceProvider.GetRequiredService<IConfigSettingsProvider>();
            var configSetting = await configSettingsProvider.GetDefaultSettingsAsync();
            var redisConnectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(databaseOptions.RedisConnectionString);

            return services
                .AddSingleton(_ => configSetting)
                .AddSingleton(_ => redisConnectionMultiplexer)
                .AddScoped(sp => sp.GetRequiredService<ConnectionMultiplexer>().GetDatabase());
        }
    }
}
