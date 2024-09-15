using System;

namespace HomeBudget.Rates.Api.Models.Requests
{
    public record CurrencyForPeriodRequest
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
