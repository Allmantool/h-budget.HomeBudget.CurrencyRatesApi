using System;

namespace HomeBudget.Rates.Api.Models.Requests
{
    public class CurrencyExchangeRequest
    {
        public int OriginCurrencyId { get; set; }
        public int TargetCurrencyId { get; set; }
        public DateOnly OperationDate { get; set; }
        public decimal Amount { get; set; }
    }
}
