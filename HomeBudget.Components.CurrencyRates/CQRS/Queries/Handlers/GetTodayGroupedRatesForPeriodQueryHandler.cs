using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.Extensions.Logging;

using HomeBudget.Components.CurrencyRates.CQRS.Queries.Models;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Extensions;
using HomeBudget.Core.Models;
using HomeBudget.Core.Services.Interfaces;

namespace HomeBudget.Components.CurrencyRates.CQRS.Queries.Handlers
{
    internal class GetTodayGroupedRatesForPeriodQueryHandler(
        ILogger<GetTodayGroupedRatesForPeriodQueryHandler> logger,
        IRedisCacheService redisCacheService,
        ICurrencyRatesService currencyRatesService)
        : IRequestHandler<GetCurrencyGroupedRatesForPeriodQuery, Result<IReadOnlyCollection<CurrencyRateGrouped>>>
    {
        private const string CacheKeyPrefix = nameof(GetTodayGroupedRatesForPeriodQueryHandler);

        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> Handle(GetCurrencyGroupedRatesForPeriodQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"{CacheKeyPrefix}" +
                                $"|{nameof(ICurrencyRatesService.GetRatesForPeriodAsync)}" +
                                $"|{request.StartDate.ToString(DateFormats.NationalBankExternalApi)}-{request.EndDate.ToString(DateFormats.NationalBankExternalApi)}";

            logger.LogWithExecutionMemberName($"Method: '{nameof(ICurrencyRatesService.GetRatesForPeriodAsync)}' with key: '{cacheKey}'");

            return await redisCacheService.AddOrGetExistingAsync(
                  cacheKey,
                  () => currencyRatesService.GetRatesForPeriodAsync(request.StartDate, request.EndDate));
        }
    }
}
