﻿using System.Text.Json;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.DbEntities;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.Components.CurrencyRates.Providers
{
    internal class ConfigSettingsProvider(
        IBaseWriteRepository baseWriteRepository,
        IBaseReadRepository readRepository)
        : IConfigSettingsProvider
    {
        private const string GeneralConfigSettingsKey = "General";

        public async Task<ConfigSettings> GetDefaultSettingsAsync()
        {
            const string query = "SELECT [Settings] FROM [ConfigSettings] WITH (NOLOCK) WHERE [Key] = @Key";

            var configAsJson = await readRepository.SingleAsync<string>(
                query,
                new
                {
                    Key = GeneralConfigSettingsKey
                });

            return JsonSerializer.Deserialize<ConfigSettings>(configAsJson);
        }

        public async Task<int> SaveDefaultSettingsAsync(ConfigSettings settings)
        {
            var settingsAsJson = JsonSerializer.Serialize(settings);

            const string query = "UPDATE [ConfigSettings]" +
                                    "SET Settings = @Settings" +
                                  "WHERE [Key] = @Key";

            return await baseWriteRepository.ExecuteAsync(
                query,
                new SettingsForUpdatePayload
                {
                    Key = GeneralConfigSettingsKey,
                    Settings = settingsAsJson
                });
        }
    }
}
