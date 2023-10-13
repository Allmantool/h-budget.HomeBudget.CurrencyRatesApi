using System.Collections.Generic;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.Providers.Interfaces
{
    internal interface INationalBankRatesProvider
    {
        Task<IReadOnlyCollection<NationalBankShortCurrencyRate>> GetRatesForPeriodAsync(
            IEnumerable<int> currenciesIds,
            PeriodRange periodRange);

        Task<IReadOnlyCollection<NationalBankCurrencyRate>> GetTodayActiveRatesAsync();
    }
}