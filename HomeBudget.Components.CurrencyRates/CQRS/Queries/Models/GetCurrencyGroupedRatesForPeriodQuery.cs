using System;
using System.Collections.Generic;

using MediatR;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.CQRS.Queries.Models
{
    public class GetCurrencyGroupedRatesForPeriodQuery : IRequest<Result<IReadOnlyCollection<CurrencyRateGrouped>>>
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
