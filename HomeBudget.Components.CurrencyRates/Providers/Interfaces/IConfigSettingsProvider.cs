using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Models;

namespace HomeBudget.Components.CurrencyRates.Providers.Interfaces
{
    public interface IConfigSettingsProvider
    {
        Task<ConfigSettings> GetDefaultSettingsAsync();
        Task<int> SaveDefaultSettingsAsync(ConfigSettings settings);
    }
}
