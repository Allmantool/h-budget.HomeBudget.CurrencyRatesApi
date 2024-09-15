using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Clients;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Extensions;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.Providers
{
    internal class NationalBankRatesProvider(
        ConfigSettings configSettings,
        INationalBankApiClient nationalBankApiClient)
        : INationalBankRatesProvider
    {
        public async Task<IReadOnlyCollection<NationalBankShortCurrencyRate>> GetRatesForPeriodAsync(
            IEnumerable<int> currenciesIds,
            PeriodRange periodRange)
        {
            var yearRatesRequestPayloads = GetYearPeriodRequests(currenciesIds, periodRange);

            var getRatesFromExternalApiTasks = yearRatesRequestPayloads.Select(payload => nationalBankApiClient
                .GetRatesForPeriodAsync(
                    payload.CurrencyId,
                    payload.Period.StartDate.ToString(DateFormats.NationalBankApiRequest),
                    payload.Period.EndDate.ToString(DateFormats.NationalBankApiRequest)));

            var ratesFromExternalApiByChunks = await Task.WhenAll(getRatesFromExternalApiTasks);

            return ratesFromExternalApiByChunks
                .SelectMany(i => i)
                .ToList();
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
            var ratesForYearPeriods = new List<PeriodRange>(period.GetFullYearsDateRangesForPeriod())
            {
                period with
                {
                    EndDate = period.IsWithinTheSameYear()
                        ? period.EndDate
                        : period.StartDate.LastDateOfYear()
                },
                period with
                {
                    StartDate = period.IsWithinTheSameYear()
                        ? period.StartDate
                        : period.EndDate.FirstDateOfYear()
                }
            }.DistinctBy(i => new { i.StartDate, i.EndDate });

            return currenciesIds.SelectMany(
                _ => ratesForYearPeriods,
                (id, rangePeriod) => new YearRatesRequestPayload
                {
                    CurrencyId = id,
                    Period = rangePeriod
                }
            ).ToList();
        }
    }
}
