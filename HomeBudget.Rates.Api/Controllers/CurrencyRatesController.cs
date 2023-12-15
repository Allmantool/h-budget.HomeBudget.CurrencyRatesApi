using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using HomeBudget.Rates.Api.Constants;
using HomeBudget.Components.CurrencyRates.CQRS.Commands.Models;
using HomeBudget.Components.CurrencyRates.CQRS.Queries.Models;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Core.Models;
using HomeBudget.Rates.Api.Models;

using CurrencyRate = HomeBudget.Components.CurrencyRates.Models.CurrencyRate;

namespace HomeBudget.Rates.Api.Controllers
{
    [ApiController]
    [Route(Endpoints.RatesApi, Name = Endpoints.RatesApi)]
    public class CurrencyRatesController(
        ISender mediator,
        IMapper mapper) : ControllerBase
    {
        [HttpPost]
        public async Task<Result<int>> AddRatesAsync([FromBody] CurrencySaveRatesRequest request, CancellationToken token = default)
        {
            var unifiedCurrencyRates = mapper
                .Map<IReadOnlyCollection<CurrencyRate>>(request.CurrencyRates);

            return await mediator.Send(new SaveCurrencyRatesCommand(unifiedCurrencyRates), token);
        }

        [HttpGet("period/{startDate}/{endDate}")]
        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetRatesForPeriodAsync(
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken token = default)
            => await mediator.Send(
                new GetCurrencyGroupedRatesForPeriodQuery
                {
                    StartDate = startDate,
                    EndDate = endDate
                },
                token);

        [HttpGet]
        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetAllRatesAsync(CancellationToken token = default)
            => await mediator.Send(new GetAllRatesQuery(), token);

        [HttpGet("today")]
        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetTodayRatesAsync(CancellationToken token = default)
            => await mediator.Send(new GetTodayRatesQuery(), token);
    }
}
