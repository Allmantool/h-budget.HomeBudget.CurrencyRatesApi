using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AutoMapper;

using HomeBudget.Components.CurrencyRates.CQRS.Commands.Models;
using HomeBudget.Components.CurrencyRates.Extensions;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Extensions;
using HomeBudget.Core.Models;
using HomeBudget.Core.Services;

namespace HomeBudget.Components.CurrencyRates.Services
{
    internal class CurrencyRatesService : BaseService, ICurrencyRatesService
    {
        private readonly IMapper _mapper;
        private readonly ICurrencyRatesReadProvider _currencyRatesReadProvider;
        private readonly ICurrencyRatesWriteProvider _currencyRatesWriteProvider;
        private readonly INationalBankRatesProvider _nationalBankRatesProvider;

        public CurrencyRatesService(
            IMapper mapper,
            ICurrencyRatesReadProvider currencyRatesReadProvider,
            ICurrencyRatesWriteProvider currencyRatesWriteProvider,
            INationalBankRatesProvider nationalBankRatesProvider)
        {
            _mapper = mapper;
            _currencyRatesReadProvider = currencyRatesReadProvider;
            _currencyRatesWriteProvider = currencyRatesWriteProvider;
            _nationalBankRatesProvider = nationalBankRatesProvider;
        }

        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetRatesAsync()
        {
            var rates = await _currencyRatesReadProvider.GetRatesAsync();

            var groupedCurrencyRates = rates.MapToCurrencyRateGrouped(_mapper);

            return Succeeded(groupedCurrencyRates);
        }

        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetRatesForPeriodAsync(DateOnly startDate, DateOnly endDate)
        {
            var todayRatesResponse = await GetTodayRatesAsync();

            var shortRates = await _nationalBankRatesProvider.GetRatesForPeriodAsync(
                todayRatesResponse.Payload.Select(r => r.CurrencyId),
                new PeriodRange
                {
                    StartDate = startDate,
                    EndDate = endDate
                });

            var ratesFromApiCall = _mapper.Map<IReadOnlyCollection<CurrencyRate>>(shortRates);

            foreach (var rate in ratesFromApiCall)
            {
                var configInfo = todayRatesResponse
                    .Payload
                    .SingleOrDefault(i => i.CurrencyId == rate.CurrencyId);

                if (configInfo == null)
                {
                    continue;
                }

                rate.EnrichWithRateGroupInfo(configInfo);
            }

            var ratesForPeriodFromDatabase = await _currencyRatesReadProvider.GetRatesForPeriodAsync(startDate, endDate);

            await SaveWithRewriteAsync(new SaveCurrencyRatesCommand(ratesFromApiCall, ratesForPeriodFromDatabase));

            return Succeeded(ratesFromApiCall.MapToCurrencyRateGrouped(_mapper));
        }

        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetTodayRatesAsync()
        {
            var activeCurrencyRates = await _nationalBankRatesProvider.GetTodayActiveRatesAsync();

            var ratesFromApiCall = _mapper.Map<IReadOnlyCollection<CurrencyRate>>(activeCurrencyRates);

            var todayRatesFromDatabase = await _currencyRatesReadProvider.GetTodayRatesAsync();

            await SaveWithRewriteAsync(new SaveCurrencyRatesCommand(ratesFromApiCall, todayRatesFromDatabase));

            return Succeeded(ratesFromApiCall.MapToCurrencyRateGrouped(_mapper));
        }

        public async Task<Result<int>> SaveWithRewriteAsync(SaveCurrencyRatesCommand saveRatesCommand)
        {
            var ratesFromApiCall = saveRatesCommand.RatesFromApiCall ?? Enumerable.Empty<CurrencyRate>().ToList();

            var amountOfAffectedRows = saveRatesCommand.RatesFromDatabase.IsNullOrEmpty()
                                       || saveRatesCommand.RatesFromDatabase.Count != ratesFromApiCall.Count
                ? await _currencyRatesWriteProvider.UpsertRatesWithSaveAsync(ratesFromApiCall)
                : default;

            return Succeeded(amountOfAffectedRows);
        }
    }
}
