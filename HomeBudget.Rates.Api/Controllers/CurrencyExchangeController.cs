using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using Microsoft.AspNetCore.Mvc;

using HomeBudget.Components.Exchange.Models;
using HomeBudget.Components.Exchange.Services.Interfaces;
using HomeBudget.Core.Models;
using HomeBudget.Rates.Api.Constants;
using HomeBudget.Rates.Api.Models.Requests;

namespace HomeBudget.Rates.Api.Controllers
{
    [ApiController]
    [Route(Endpoints.CurrencyExchangeApi, Name = Endpoints.CurrencyExchangeApi)]
    public class CurrencyExchangeController(IMapper mapper, IExchangeService exchangeService) : ControllerBase
    {
        [HttpPost]
        public async Task<Result<decimal>> GetExchangeAsync(
            [FromBody] CurrencyExchangeRequest request,
            CancellationToken token = default)
        {
            var exchangeMultiplierResult = await exchangeService
                .GetCurrencyConversionMultiplierAsync(mapper.Map<ExchangeMultiplierQuery>(request), token);

            return exchangeMultiplierResult.IsSucceeded
                ? Result<decimal>.Succeeded(request.Amount * exchangeMultiplierResult.Payload)
                : Result<decimal>.Failure(exchangeMultiplierResult.StatusMessage);
        }

        [HttpPost("multiplier")]
        public async Task<Result<decimal>> GetExchangeMultiplierAsync(
            [FromBody] CurrencyExchangeMultiplierRequest request,
            CancellationToken token = default) => await exchangeService
                .GetCurrencyConversionMultiplierAsync(mapper.Map<ExchangeMultiplierQuery>(request), token);
    }
}
