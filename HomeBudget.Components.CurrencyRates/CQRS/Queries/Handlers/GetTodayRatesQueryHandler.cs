using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using HomeBudget.Components.CurrencyRates.CQRS.Queries.Models;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Models;
using HomeBudget.Core.Services.Interfaces;

namespace HomeBudget.Components.CurrencyRates.CQRS.Queries.Handlers
{
    internal class GetTodayRatesQueryHandler(
        ICacheService cacheService,
        ICurrencyRatesService currencyRatesService)
        : IRequestHandler<GetTodayRatesQuery, Result<IReadOnlyCollection<CurrencyRateGrouped>>>
    {
        private const string CacheKeyPrefix = nameof(GetTodayRatesQueryHandler);

        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> Handle(
            GetTodayRatesQuery request,
            CancellationToken cancellationToken)
        {
            return await cacheService.GetOrCreateAsync(
                $"{CacheKeyPrefix}|{nameof(ICurrencyRatesService.GetTodayRatesAsync)}|{DateTime.Today}",
                currencyRatesService.GetTodayRatesAsync);
        }
    }
}
