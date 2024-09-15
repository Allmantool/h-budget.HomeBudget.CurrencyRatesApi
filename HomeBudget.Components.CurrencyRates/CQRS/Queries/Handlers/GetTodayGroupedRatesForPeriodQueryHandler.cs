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
        ICacheService cacheService,
        ICurrencyRatesService currencyRatesService)
        : IRequestHandler<GetCurrencyGroupedRatesForPeriodQuery, Result<IReadOnlyCollection<CurrencyRateGrouped>>>
    {
        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> Handle(GetCurrencyGroupedRatesForPeriodQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = $"{nameof(ICurrencyRatesService.GetRatesForPeriodAsync)}" +
                           $"|{request.StartDate.ToString(DateFormats.NationalBankApiRequest)}-{request.EndDate.ToString(DateFormats.NationalBankApiRequest)}";

            logger.LogWithExecutionMemberName($"Method: '{nameof(ICurrencyRatesService.GetRatesForPeriodAsync)}' with key: '{cacheKey}'");

            return await cacheService.GetOrCreateAsync(
                  cacheKey,
                  () => currencyRatesService.GetRatesForPeriodAsync(request.StartDate, request.EndDate));
        }
    }
}
