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
    internal class GetAllRatesQueryHandler(
        IRedisCacheService redisCacheService,
        ICurrencyRatesService currencyRatesService)
        : IRequestHandler<GetAllRatesQuery, Result<IReadOnlyCollection<CurrencyRateGrouped>>>
    {
        private const string CacheKeyPrefix = nameof(GetAllRatesQueryHandler);

        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> Handle(
            GetAllRatesQuery request,
            CancellationToken cancellationToken)
        {
            return await redisCacheService.AddOrGetExistingAsync(
                $"{CacheKeyPrefix}|{nameof(ICurrencyRatesService.GetRatesAsync)}|{DateTime.Today}",
                currencyRatesService.GetRatesAsync);
        }
    }
}
