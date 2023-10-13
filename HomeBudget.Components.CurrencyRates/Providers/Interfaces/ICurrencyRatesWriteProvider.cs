using System.Collections.Generic;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Models;

namespace HomeBudget.Components.CurrencyRates.Providers.Interfaces
{
    internal interface ICurrencyRatesWriteProvider
    {
        Task<int> UpsertRatesWithSaveAsync(IReadOnlyCollection<CurrencyRate> rates);
    }
}
