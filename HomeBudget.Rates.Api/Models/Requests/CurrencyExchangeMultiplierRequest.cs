using System;

namespace HomeBudget.Rates.Api.Models.Requests
{
    public record CurrencyExchangeMultiplierRequest
    {
        public string OriginCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public DateOnly OperationDate { get; set; }
    }
}
