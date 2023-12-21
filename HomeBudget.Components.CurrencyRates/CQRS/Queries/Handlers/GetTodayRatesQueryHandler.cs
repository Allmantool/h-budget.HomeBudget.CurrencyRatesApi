using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

using MediatR;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Core.Models;
using HomeBudget.Components.CurrencyRates.CQRS.Queries.Models;
using HomeBudget.Core.Services.Interfaces;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;

namespace HomeBudget.Components.CurrencyRates.CQRS.Queries.Handlers
{
    internal class GetTodayRatesQueryHandler(
        IRedisCacheService redisCacheService,
        ICurrencyRatesService currencyRatesService)
        : IRequestHandler<GetTodayRatesQuery, Result<IReadOnlyCollection<CurrencyRateGrouped>>>
    {
        private const string CacheKeyPrefix = nameof(GetTodayRatesQueryHandler);

        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> Handle(
            GetTodayRatesQuery request,
            CancellationToken cancellationToken)
        {
            return await redisCacheService.CacheWrappedMethodAsync(
                $"{CacheKeyPrefix}|{nameof(ICurrencyRatesService.GetTodayRatesAsync)}|{DateTime.Today}",
                () => currencyRatesService.GetTodayRatesAsync());
        }
    }
}
