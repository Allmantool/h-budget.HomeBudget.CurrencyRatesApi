using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using HomeBudget.Components.CurrencyRates.Clients;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Extensions;
using HomeBudget.Core.Models;
using HomeBudget.Core.Options;

namespace HomeBudget.Components.CurrencyRates.Providers
{
    internal class NationalBankRatesProvider(
        ConfigSettings configSettings,
        IOptions<HttpClientOptions> httpClientOptions,
        INationalBankApiClient nationalBankApiClient)
        : INationalBankRatesProvider
    {
        public async Task<IReadOnlyCollection<NationalBankShortCurrencyRate>> GetRatesForPeriodAsync(
            IEnumerable<int> currenciesIds,
            PeriodRange periodRange)
        {
            var yearRatesRequestPayloads = GetYearPeriodRequests(currenciesIds, periodRange);

            using var semaphore = new SemaphoreSlim(httpClientOptions.Value.MaxConcurrentRequests);

            var tasks = yearRatesRequestPayloads.Select(async payload =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await nationalBankApiClient.GetRatesForPeriodAsync(
                        payload.CurrencyId,
                        payload.Period.StartDate.ToString(DateFormats.NationalBankApiRequest),
                        payload.Period.EndDate.ToString(DateFormats.NationalBankApiRequest));
                }
                finally
                {
                    semaphore.Release();
                }
            });

            return (await Task.WhenAll(tasks)).SelectMany(x => x).ToList();
        }

        public async Task<IReadOnlyCollection<NationalBankCurrencyRate>> GetTodayActiveRatesAsync()
        {
            var activeCurrencyAbbreviations = configSettings
                .ActiveNationalBankCurrencies
                .Select(i => i.Abbreviation);

            var todayRatesFromApi = await nationalBankApiClient.GetTodayRatesAsync();

            return todayRatesFromApi
                .Where(r => activeCurrencyAbbreviations.Contains(r.Abbreviation, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        private static IReadOnlyCollection<YearRatesRequestPayload> GetYearPeriodRequests(
            IEnumerable<int> currenciesIds,
            PeriodRange period)
        {
            var yearRanges = period.SplitByYear();

            return currenciesIds
                .SelectMany(
                    id => yearRanges,
                    (id, range) => new YearRatesRequestPayload
                    {
                        CurrencyId = id,
                        Period = range
                    })
                .ToList();
        }
    }
}
