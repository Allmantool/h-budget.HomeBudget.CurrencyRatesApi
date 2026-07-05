using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;

using HomeBudget.Components.CurrencyRates.CQRS.Commands.Models;
using HomeBudget.Components.CurrencyRates.Extensions;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
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
            DateOnly endDate,
            CancellationToken ct = default)
        {
            var activeCurrencies = await nationalBankRatesProvider.GetActiveCurrenciesAsync(endDate, ct);

            var shortRates = await nationalBankRatesProvider.GetRatesForPeriodAsync(
                activeCurrencies.Select(r => r.CurrencyId),
                new PeriodRange
                {
                    StartDate = startDate,
                    EndDate = endDate
                },
                ct);

            var ratesFromApiCall = mapper.Map<IReadOnlyCollection<CurrencyRate>>(shortRates);

            foreach (var rate in ratesFromApiCall)
            {
                var currencyInfo = activeCurrencies
                    .SingleOrDefault(i => i.CurrencyId == rate.CurrencyId);

                if (currencyInfo == null)
                {
                    continue;
                }

                rate.EnrichWithRateGroupInfo(currencyInfo);
            }

            await SaveWithRewriteAsync(new SaveCurrencyRatesCommand(ratesFromApiCall));

            var ratesForPeriodFromDatabase = await currencyRatesReadProvider.GetRatesForPeriodAsync(startDate, endDate);

            var currencyGroups = ratesForPeriodFromDatabase.MapToCurrencyRateGrouped(mapper);

            // TODO: SignalR or .net 10 SSE
            return Result<IReadOnlyCollection<CurrencyRateGrouped>>.Succeeded(currencyGroups);
        }

        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetTodayRatesAsync()
        {
            var activeCurrencyRates = await nationalBankRatesProvider.GetTodayActiveRatesAsync();

            var ratesFromApiCall = mapper.Map<IReadOnlyCollection<CurrencyRate>>(activeCurrencyRates);

            await SaveWithRewriteAsync(new SaveCurrencyRatesCommand(ratesFromApiCall));

            var todayRatesFromDatabase = await currencyRatesReadProvider.GetTodayRatesAsync();

            return Result<IReadOnlyCollection<CurrencyRateGrouped>>.Succeeded(todayRatesFromDatabase.MapToCurrencyRateGrouped(mapper));
        }

        public async Task<Result<int>> SaveWithRewriteAsync(SaveCurrencyRatesCommand saveRatesCommand)
        {
            var ratesFromApiCall = saveRatesCommand.RatesFromApiCall ?? Enumerable.Empty<CurrencyRate>().ToList();

            if (ratesFromApiCall.Count == 0)
            {
                return Result<int>.Succeeded(default);
            }

            var amountOfAffectedRows = await currencyRatesWriteProvider.UpsertRatesWithSaveAsync(ratesFromApiCall);

            return Result<int>.Succeeded(amountOfAffectedRows);
        }
    }
}
