using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.Services.Interfaces
{
    public interface IConfigSettingsServices
    {
        Task<Result<IReadOnlyCollection<Currency>>> GetAvailableCurrenciesAsync();

        Task<Result<int>> SaveSettingsAsync(
            ConfigSettings settings,
            CancellationToken cancellationToken = default);

        Task<Result<ConfigSettings>> GetSettingsAsync();
    }
}
