using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.DbEntities;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.Components.CurrencyRates.Providers
{
    internal class CurrencyRatesReadProvider : ICurrencyRatesReadProvider
    {
        private const string DatabaseName = "HomeBudget.CurrencyRates";
        private const string CurrencyRateSelect = @"
            SELECT
                rates.[CurrencyId],
                COALESCE(localNames.[Name], rates.[Name]) AS [Name],
                rates.[Abbreviation],
                rates.[Scale],
                rates.[OfficialRate],
                rates.[RatePerUnit],
                rates.[UpdateDate]
            FROM dbo.[CurrencyRates] rates WITH (NOLOCK)
            OUTER APPLY
            (
                SELECT TOP (1) abbreviations.[Name]
                FROM dbo.[CurrencyAbbreviations] abbreviations WITH (NOLOCK)
                WHERE abbreviations.[CurrencyId] = rates.[CurrencyId]
                ORDER BY
                    CASE
                        WHEN abbreviations.[Abbreviation] = rates.[Abbreviation] THEN 0
                        ELSE 1
                    END
            ) localNames";

        private readonly string _ratesAbbreviationPredicate;
        private readonly IMapper _mapper;
        private readonly IBaseReadRepository _readRepository;

        public CurrencyRatesReadProvider(
            ConfigSettings configSettings,
            IMapper mapper,
            IBaseReadRepository readRepository)
        {
            var abbreviations = string.Join(',', configSettings.ActiveNationalBankCurrencies
                .Select(i => $"'{i.Abbreviation}'"));

            _mapper = mapper;
            _readRepository = readRepository;
            _ratesAbbreviationPredicate = $"rates.[Abbreviation] IN ({abbreviations})";

            _readRepository.Database = DatabaseName;
        }

        public async Task<IReadOnlyCollection<CurrencyRate>> GetRatesForPeriodAsync(DateOnly startDate, DateOnly endDate)
        {
            var startDateParameter = startDate.ToString(DateFormats.MsSqlDayOnlyFormat, CultureInfo.InvariantCulture);
            var endDateParameter = endDate.ToString(DateFormats.MsSqlDayOnlyFormat, CultureInfo.InvariantCulture);
            var query = CurrencyRateSelect +
                         " WHERE rates.[UpdateDate] BETWEEN @StartDate AND @EndDate " +
                         $"AND {_ratesAbbreviationPredicate};";

            var response = await _readRepository.GetAsync<CurrencyRateEntity>(
                query,
                new
                {
                    StartDate = startDateParameter,
                    EndDate = endDateParameter
                });

            return _mapper.Map<IReadOnlyCollection<CurrencyRate>>(response);
        }

        public async Task<IReadOnlyCollection<CurrencyRate>> GetRatesAsync()
        {
            var query = CurrencyRateSelect +
                        $" WHERE {_ratesAbbreviationPredicate};";

            var response = await _readRepository.GetAsync<CurrencyRateEntity>(query);

            return _mapper.Map<IReadOnlyCollection<CurrencyRate>>(response);
        }

        public async Task<IReadOnlyCollection<CurrencyRate>> GetTodayRatesAsync()
        {
            var todayParameter = DateOnly.FromDateTime(DateTime.Now).ToString(
                DateFormats.MsSqlDayOnlyFormat,
                CultureInfo.InvariantCulture);
            var query = CurrencyRateSelect +
                         " WHERE rates.[UpdateDate] = @Today " +
                         $"AND {_ratesAbbreviationPredicate};";

            var response = await _readRepository.GetAsync<CurrencyRateEntity>(
                query,
                new
                {
                    Today = todayParameter
                });

            return _mapper.Map<IReadOnlyCollection<CurrencyRate>>(response);
        }
    }
}
