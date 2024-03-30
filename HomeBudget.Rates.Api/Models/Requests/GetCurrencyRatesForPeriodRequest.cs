using System;

namespace HomeBudget.Rates.Api.Models.Requests
{
    internal record GetCurrencyRatesForPeriodRequest
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
