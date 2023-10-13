using System;

namespace HomeBudget.Rates.Api.Models
{
    public class GetCurrencyRatesForPeriodRequest
    {
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
    }
}
