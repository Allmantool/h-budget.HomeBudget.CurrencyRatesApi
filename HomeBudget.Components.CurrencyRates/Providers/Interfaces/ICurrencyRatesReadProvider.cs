using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Models;

namespace HomeBudget.Components.CurrencyRates.Providers.Interfaces
{
    internal interface ICurrencyRatesReadProvider
    {
        Task<IReadOnlyCollection<CurrencyRate>> GetTodayRatesAsync();

        Task<IReadOnlyCollection<CurrencyRate>> GetRatesForPeriodAsync(DateOnly startDate, DateOnly endDate);

        Task<IReadOnlyCollection<CurrencyRate>> GetRatesAsync();
    }
}
