using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HomeBudget.Components.CurrencyRates.Models.Api;
using Refit;

namespace HomeBudget.Components.CurrencyRates.Clients
{
    public interface INationalBankApiClient
    {
        [Get("/")]
        Task WarmUpAsync();

        [Get("/exrates/rates?periodicity=0")]
        Task<IEnumerable<NationalBankCurrencyRate>> GetTodayRatesAsync();

        [Get("/exrates/rates?ondate={onDate}&periodicity={periodicity}")]
        Task<IEnumerable<NationalBankCurrencyRate>> GetRatesAsync(
            [AliasAs("onDate")] string onDate,
            [AliasAs("periodicity")] int periodicity,
            CancellationToken ct = default);

        [Get("/exrates/rates/{id}?ondate={onDate}")]
        Task<NationalBankCurrencyRate> GetRateAsync(
            [AliasAs("id")] int currencyId,
            [AliasAs("onDate")] string onDate,
            CancellationToken ct = default);

        [Get("/exrates/rates/dynamics/{id}?startDate={startDate}&endDate={endDate}")]
        Task<IEnumerable<NationalBankShortCurrencyRate>> GetRatesForPeriodAsync(
            [AliasAs("id")] int currencyId,
            [AliasAs("startDate")] string startDate,
            [AliasAs("endDate")] string endDate,
            CancellationToken ct = default);

        [Get("/exrates/currencies")]
        Task<IEnumerable<NationalBankCurrency>> GetCurrenciesAsync(CancellationToken ct = default);

        [Get("/exrates/currencies/{id}")]
        Task<NationalBankCurrency> GetCurrencyAsync(
            [AliasAs("id")] int currencyId,
            CancellationToken ct = default);
    }
}
