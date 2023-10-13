using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

using HomeBudget.Components.CurrencyRates.CQRS.Commands.Models;
using HomeBudget.Components.CurrencyRates.CQRS.Queries.Models;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Core.Models;
using HomeBudget.Rates.Api.Models;

using CurrencyRate = HomeBudget.Components.CurrencyRates.Models.CurrencyRate;

namespace HomeBudget.Rates.Api.Controllers
{
    [ApiController]
    [Route("currencyRates")]
    public class CurrencyRatesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CurrencyRatesController(
            IMediator mediator,
            IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<Result<int>> AddRatesAsync([FromBody] CurrencySaveRatesRequest request, CancellationToken token = default)
        {
            var unifiedCurrencyRates = _mapper
                .Map<IReadOnlyCollection<CurrencyRate>>(request.CurrencyRates);

            return await _mediator.Send(new SaveCurrencyRatesCommand(unifiedCurrencyRates), token);
        }

        [HttpGet("period/{startDate}/{endDate}")]
        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetRatesForPeriodAsync(
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken token = default)
            => await _mediator.Send(
                new GetCurrencyGroupedRatesForPeriodQuery
                {
                    StartDate = startDate,
                    EndDate = endDate
                },
                token);

        [HttpGet]
        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetAllRatesAsync(CancellationToken token = default)
            => await _mediator.Send(new GetAllRatesQuery(), token);

        [HttpGet("today")]
        public async Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetTodayRatesAsync(CancellationToken token = default)
            => await _mediator.Send(new GetTodayRatesQuery(), token);
    }
}
