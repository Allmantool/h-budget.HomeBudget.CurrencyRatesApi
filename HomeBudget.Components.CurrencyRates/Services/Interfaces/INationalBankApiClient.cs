using System.Collections.Generic;
using System.Threading.Tasks;

using Refit;

using HomeBudget.Components.CurrencyRates.Models.Api;

namespace HomeBudget.Components.CurrencyRates.Services.Interfaces
{
    public interface INationalBankApiClient
    {
        [Get("/")]
        Task WarmUpAsync();

        [Get("/api/exrates/rates?periodicity=0")]
        Task<IEnumerable<NationalBankCurrencyRate>> GetTodayRatesAsync();

        [Get("/api/exrates/rates/dynamics/{id}?startDate={startDate}&endDate={endDate}")]
        Task<IEnumerable<NationalBankShortCurrencyRate>> GetRatesForPeriodAsync(
            [AliasAs("id")] int currencyId,
            [AliasAs("startDate")] string startDate,
            [AliasAs("endDate")] string endDate);

        [Get("/api/exrates/currencies")]
        Task<IEnumerable<NationalBankCurrency>> GetCurrenciesAsync();
    }
}
