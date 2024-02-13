using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
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
using HomeBudget.Rates.Api.Constants;

namespace HomeBudget.Rates.Api.Configuration
{
    internal static class DependencyRegistrations
    {
        public static async Task<IServiceCollection> SetUpDiAsync(
            this IServiceCollection services,
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
        {
            services
                .Configure<DatabaseConnectionOptions>(configuration.GetSection(ConfigurationSectionKeys.DatabaseOptions))
                .Configure<CacheStoreOptions>(configuration.GetSection(ConfigurationSectionKeys.CacheStoreOptions))
                .Configure<ExternalResourceUrls>(configuration.GetSection(ConfigurationSectionKeys.ExternalResourceUrls))
                .Configure<PollyRetryOptions>(configuration.GetSection(ConfigurationSectionKeys.PollyRetryOptions))
                .RegisterCoreIoCDependency()
                .RegisterCurrencyRatesIoCDependency()
                .RegistryDapperIoCDependencies();

            if (!HostEnvironments.Integration.Equals(webHostEnvironment.EnvironmentName, StringComparison.OrdinalIgnoreCase))
            {
                await services.SetDiForConnectionsAsync();
            }

            services.SetDiForHttpClient();

            return services;
        }

        public static IServiceCollection SetDiForHttpClient(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            return services
                .AddScoped<HttpLoggingHandler>()
                .RegisterNationalApiHttpClient(serviceProvider);
        }

        public static async Task<IServiceCollection> SetDiForConnectionsAsync(this IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseConnectionOptions>>().Value;
            var configSettingsProvider = serviceProvider.GetRequiredService<IConfigSettingsProvider>();

            var redisConnectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(databaseOptions.RedisConnectionString);
            var configSetting = await configSettingsProvider.GetDefaultSettingsAsync();

            return services
                 .AddSingleton(_ => configSetting)
                 .AddSingleton(_ => redisConnectionMultiplexer)
                 .AddScoped(sp => sp.GetRequiredService<ConnectionMultiplexer>().GetDatabase());
        }
    }
}
