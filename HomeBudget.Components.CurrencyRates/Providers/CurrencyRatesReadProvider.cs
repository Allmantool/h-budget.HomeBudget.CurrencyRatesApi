using System;
using System.Collections.Generic;
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
            _ratesAbbreviationPredicate = $"[Abbreviation] IN ({abbreviations})";

            _readRepository.Database = DatabaseName;
        }

        public async Task<IReadOnlyCollection<CurrencyRate>> GetRatesForPeriodAsync(DateOnly startDate, DateOnly endDate)
        {
            var query = "SELECT * " +
                          $"FROM dbo.[CurrencyRates] WITH (NOLOCK) " +
                         "WHERE [UpdateDate] BETWEEN @StartDate AND @EndDate " +
                          $"AND {_ratesAbbreviationPredicate};";

            var response = await _readRepository.GetAsync<CurrencyRateEntity>(
                query,
                new
                {
                    StartDate = startDate.ToString(DateFormats.MsSqlDayOnlyFormat),
                    EndDate = endDate.ToString(DateFormats.MsSqlDayOnlyFormat)
                });

            return _mapper.Map<IReadOnlyCollection<CurrencyRate>>(response);
        }

        public async Task<IReadOnlyCollection<CurrencyRate>> GetRatesAsync()
        {
            var query = "SELECT * " +
                          $"FROM dbo.[CurrencyRates] WITH (NOLOCK) " +
                        $"WHERE {_ratesAbbreviationPredicate};";

            var response = await _readRepository.GetAsync<CurrencyRateEntity>(query);

            return _mapper.Map<IReadOnlyCollection<CurrencyRate>>(response);
        }

        public async Task<IReadOnlyCollection<CurrencyRate>> GetTodayRatesAsync()
        {
            var query = "SELECT * " +
                          $"FROM dbo.[CurrencyRates] WITH (NOLOCK) " +
                         "WHERE [UpdateDate] = @Today " +
                          $"AND {_ratesAbbreviationPredicate};";

            var response = await _readRepository.GetAsync<CurrencyRateEntity>(
                query,
                new
                {
                    Today = DateOnly.FromDateTime(DateTime.Now).ToString(DateFormats.MsSqlDayOnlyFormat)
                });

            return _mapper.Map<IReadOnlyCollection<CurrencyRate>>(response);
        }
    }
}
