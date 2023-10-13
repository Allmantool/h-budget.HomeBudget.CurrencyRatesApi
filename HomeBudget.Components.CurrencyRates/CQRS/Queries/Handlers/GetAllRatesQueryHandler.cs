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
    internal class GetAllRatesQueryHandler
        : IRequestHandler<GetAllRatesQuery, Result<IReadOnlyCollection<CurrencyRateGrouped>>>
    {
        private const string CacheKeyPrefix = nameof(GetAllRatesQueryHandler);

        private readonly IRedisCacheService _redisCacheService;
        private readonly ICurrencyRatesService _currencyRatesService;

        public GetAllRatesQueryHandler(
            IRedisCacheService redisCacheService,
            ICurrencyRatesService currencyRatesService)
        {
            _redisCacheService = redisCacheService;
            _currencyRatesService = currencyRatesService;
        }

        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> Handle(
            GetAllRatesQuery request,
            CancellationToken cancellationToken)
        {
            return await _redisCacheService.CacheWrappedMethodAsync(
                $"{CacheKeyPrefix}|{nameof(ICurrencyRatesService.GetRatesAsync)}|{DateTime.Today}",
                () => _currencyRatesService.GetRatesAsync());
        }
    }
}
