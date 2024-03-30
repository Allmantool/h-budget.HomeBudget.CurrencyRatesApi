using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;

using HomeBudget.Components.CurrencyRates.CQRS.Queries.Models;
using HomeBudget.Components.CurrencyRates.Extensions;
using HomeBudget.Components.Exchange.Models;
using HomeBudget.Components.Exchange.Services.Interfaces;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.Exchange.Services
{
    internal class ExchangeService(ISender mediator) : IExchangeService
    {
        public async Task<Result<decimal>> GetCurrencyConversionMultiplierAsync(ExchangeMultiplierQuery query, CancellationToken token)
        {
            var currenciesForPeriodResult = await mediator.Send(
                 new GetCurrencyGroupedRatesForPeriodQuery
                 {
                     StartDate = query.OperationDate,
                     EndDate = query.OperationDate,
                 },
                 token);

            var currenciesForPeriod = currenciesForPeriodResult.Payload;

            var originRateValueResult = currenciesForPeriod.GetSingleRateValue(query.OriginCurrencyId);
            var targetRateValueResult = currenciesForPeriod.GetSingleRateValue(query.TargetCurrencyId);

            if (originRateValueResult.IsSucceeded
                && targetRateValueResult.IsSucceeded
                && targetRateValueResult.Payload != 0)
            {
                var exchangeMultiplier = Math.Round(originRateValueResult.Payload / targetRateValueResult.Payload, 5);

                return Result<decimal>.Succeeded(exchangeMultiplier);
            }

            return Result<decimal>.Failure(
                string.Join(
                    ',',
                    [
                        originRateValueResult.StatusMessage,
                        targetRateValueResult.StatusMessage,
                        "Original rate value can not be equal to 0"
                    ]));
        }
    }
}
