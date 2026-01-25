using System.Text.Json;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.DbEntities;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.Components.CurrencyRates.Providers
{
    internal class ConfigSettingsProvider : IConfigSettingsProvider
    {
        private const string GeneralConfigSettingsKey = "General";
        private const string DatabaseName = "HomeBudget.CurrencyRates";

        private readonly IBaseWriteRepository _writeRepository;
        private readonly IBaseReadRepository _readRepository;

        public ConfigSettingsProvider(
            IBaseWriteRepository baseWriteRepository,
            IBaseReadRepository readRepository)
        {
            _writeRepository = baseWriteRepository;
            _readRepository = readRepository;

            _readRepository.Database = DatabaseName;
            _writeRepository.Database = DatabaseName;
        }

        public async Task<ConfigSettings> GetDefaultSettingsAsync()
        {
            const string query = $"SELECT [Settings] FROM [dbo].[ConfigSettings] WITH (NOLOCK) WHERE [Key] = @Key";

            var configAsJson = await _readRepository.SingleAsync<string>(
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

            const string query = $"UPDATE [dbo].[ConfigSettings] " +
                                    "SET Settings = @Settings " +
                                  "WHERE [Key] = @Key";

            return await _writeRepository.ExecuteAsync(
                query,
                new SettingsForUpdatePayload
                {
                    Key = GeneralConfigSettingsKey,
                    Settings = settingsAsJson
                });
        }
    }
}
