using System;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using Microsoft.AspNetCore.Mvc;

using HomeBudget.Components.CurrencyRates.CQRS.Queries.Models;
using HomeBudget.Components.CurrencyRates.Extensions;
using HomeBudget.Core.Models;
using HomeBudget.Rates.Api.Constants;
using HomeBudget.Rates.Api.Models.Requests;

namespace HomeBudget.Rates.Api.Controllers
{
    [ApiController]
    [Route(Endpoints.CurrencyExchangeApi, Name = Endpoints.CurrencyExchangeApi)]
    public class CurrencyExchangeController(ISender mediator) : ControllerBase
    {
        [HttpPost]
        public async Task<Result<decimal>> GetExchangeAsync([FromBody] CurrencyExchangeRequest request, CancellationToken token = default)
        {
            var currenciesForPeriodResult = await mediator.Send(
                new GetCurrencyGroupedRatesForPeriodQuery
                {
                    StartDate = request.OperationDate,
                    EndDate = request.OperationDate,
                },
                token);

            var currenciesForPeriod = currenciesForPeriodResult.Payload;

            var originRateValueResult = currenciesForPeriod.GetSingleRateValue(request.OriginCurrencyId);
            var targetRateValueResult = currenciesForPeriod.GetSingleRateValue(request.TargetCurrencyId);

            if (originRateValueResult.IsSucceeded
                && targetRateValueResult.IsSucceeded
                && targetRateValueResult.Payload != 0)
            {
                var exchangeMultiplier = Math.Round(originRateValueResult.Payload / targetRateValueResult.Payload, 5);

                return Result<decimal>.Succeeded(request.Amount * exchangeMultiplier);
            }

            return Result<decimal>.Failure(string.Join(',', [
                originRateValueResult.StatusMessage,
                targetRateValueResult.StatusMessage,
                "Original rate value can not be equal to 0"]));
        }
    }
}
