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

namespace HomeBudget.Components.CurrencyRates.Services
{
    internal class CurrencyRatesService(
        IMapper mapper,
        ICurrencyRatesReadProvider currencyRatesReadProvider,
        ICurrencyRatesWriteProvider currencyRatesWriteProvider,
        INationalBankRatesProvider nationalBankRatesProvider)
        : ICurrencyRatesService
    {
        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetRatesAsync()
        {
            var rates = await currencyRatesReadProvider.GetRatesAsync();

            var groupedCurrencyRates = rates.MapToCurrencyRateGrouped(mapper);

            return Result<IReadOnlyCollection<CurrencyRateGrouped>>.Succeeded(groupedCurrencyRates);
        }

        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetRatesForPeriodAsync(
            DateOnly startDate,
            DateOnly endDate)
        {
            var todayRatesResponse = await GetTodayRatesAsync();

            var shortRates = await nationalBankRatesProvider.GetRatesForPeriodAsync(
                todayRatesResponse.Payload.Select(r => r.CurrencyId),
                new PeriodRange
                {
                    StartDate = startDate,
                    EndDate = endDate
                });

            var ratesFromApiCall = mapper.Map<IReadOnlyCollection<CurrencyRate>>(shortRates);

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

            var ratesForPeriodFromDatabase = await currencyRatesReadProvider.GetRatesForPeriodAsync(startDate, endDate);

            await SaveWithRewriteAsync(new SaveCurrencyRatesCommand(ratesFromApiCall, ratesForPeriodFromDatabase));

            var currencyGroups = ratesFromApiCall.MapToCurrencyRateGrouped(mapper);

            // TODO: SignalR or .net 10 SSE
            return Result<IReadOnlyCollection<CurrencyRateGrouped>>.Succeeded(currencyGroups);
        }

        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetTodayRatesAsync()
        {
            var activeCurrencyRates = await nationalBankRatesProvider.GetTodayActiveRatesAsync();

            var ratesFromApiCall = mapper.Map<IReadOnlyCollection<CurrencyRate>>(activeCurrencyRates);

            var todayRatesFromDatabase = await currencyRatesReadProvider.GetTodayRatesAsync();

            await SaveWithRewriteAsync(new SaveCurrencyRatesCommand(ratesFromApiCall, todayRatesFromDatabase));

            return Result<IReadOnlyCollection<CurrencyRateGrouped>>.Succeeded(ratesFromApiCall.MapToCurrencyRateGrouped(mapper));
        }

        public async Task<Result<int>> SaveWithRewriteAsync(SaveCurrencyRatesCommand saveRatesCommand)
        {
            var ratesFromApiCall = saveRatesCommand.RatesFromApiCall ?? Enumerable.Empty<CurrencyRate>().ToList();

            var amountOfAffectedRows = saveRatesCommand.RatesFromDatabase.IsNullOrEmpty()
                                       || saveRatesCommand.RatesFromDatabase.Count != ratesFromApiCall.Count
                ? await currencyRatesWriteProvider.UpsertRatesWithSaveAsync(ratesFromApiCall)
                : default;

            return Result<int>.Succeeded(amountOfAffectedRows);
        }
    }
}
