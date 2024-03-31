using System;

namespace HomeBudget.Rates.Api.Models.Requests
{
    public record CurrencyExchangeRequest
    {
        public string OriginCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public DateOnly OperationDate { get; set; }
        public decimal Amount { get; set; }
    }
}
