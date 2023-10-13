using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Extensions;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.Providers
{
    internal class NationalBankRatesProvider : INationalBankRatesProvider
    {
        private readonly ConfigSettings _configSettings;
        private readonly INationalBankApiClient _nationalBankApiClient;

        public NationalBankRatesProvider(
            ConfigSettings configSettings,
            INationalBankApiClient nationalBankApiClient)
        {
            _configSettings = configSettings;
            _nationalBankApiClient = nationalBankApiClient;
        }

        public async Task<IReadOnlyCollection<NationalBankShortCurrencyRate>> GetRatesForPeriodAsync(
            IEnumerable<int> currenciesIds,
            PeriodRange periodRange)
        {
            var yearRatesRequestPayloads = GetYearPeriodRequests(currenciesIds, periodRange);

            var getRatesFromExternalApiTasks = yearRatesRequestPayloads.Select(payload => _nationalBankApiClient
                .GetRatesForPeriodAsync(
                    payload.CurrencyId,
                    payload.Period.StartDate.ToDateTime(TimeOnly.MinValue).ToString(DateFormats.NationalBankExternalApi),
                    payload.Period.EndDate.ToDateTime(TimeOnly.MinValue).ToString(DateFormats.NationalBankExternalApi)));

            var ratesFromExternalApiByChunks = await Task.WhenAll(getRatesFromExternalApiTasks);

            return ratesFromExternalApiByChunks
                .SelectMany(i => i)
                .ToList();
        }

        public async Task<IReadOnlyCollection<NationalBankCurrencyRate>> GetTodayActiveRatesAsync()
        {
            var activeCurrencyAbbreviations = _configSettings
                .ActiveNationalBankCurrencies
                .Select(i => i.Abbreviation);

            var todayRatesFromApi = await _nationalBankApiClient.GetTodayRatesAsync();

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
                new()
                {
                    StartDate = period.StartDate,
                    EndDate = period.IsWithinTheSameYear() ? period.EndDate : period.StartDate.LastDateOfYear(),
                },
                new()
                {
                    StartDate = period.IsWithinTheSameYear() ? period.StartDate : period.EndDate.FirstDateOfYear(),
                    EndDate = period.EndDate,
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
