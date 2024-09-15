using System;

using MediatR;

using HomeBudget.Core.CQRS;

namespace HomeBudget.Components.CurrencyRates.CQRS.Commands.Models
{
    public class RequestRatesForPeriodCommand(DateOnly startDate, DateOnly endDate)
        : BaseCommand, IRequest
    {
        public DateOnly StartDate { get; } = startDate;
        public DateOnly EndDate { get; } = endDate;
    }
}
