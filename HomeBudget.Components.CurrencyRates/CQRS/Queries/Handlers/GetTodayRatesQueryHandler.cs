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
    internal class GetTodayRatesQueryHandler
        : IRequestHandler<GetTodayRatesQuery, Result<IReadOnlyCollection<CurrencyRateGrouped>>>
    {
        private const string CacheKeyPrefix = nameof(GetTodayRatesQueryHandler);

        private readonly IRedisCacheService _redisCacheService;
        private readonly ICurrencyRatesService _currencyRatesService;

        public GetTodayRatesQueryHandler(
            IRedisCacheService redisCacheService,
            ICurrencyRatesService currencyRatesService)
        {
            _redisCacheService = redisCacheService;
            _currencyRatesService = currencyRatesService;
        }

        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> Handle(
            GetTodayRatesQuery request,
            CancellationToken cancellationToken)
        {
            return await _redisCacheService.CacheWrappedMethodAsync(
                $"{CacheKeyPrefix}|{nameof(ICurrencyRatesService.GetTodayRatesAsync)}|{DateTime.Today}",
                () => _currencyRatesService.GetTodayRatesAsync());
        }
    }
}
