using System;

namespace HomeBudget.Rates.Api.Models.Requests
{
    public record CurrencyExchangeMultiplierRequest
    {
        public int OriginCurrencyId { get; set; }
        public int TargetCurrencyId { get; set; }
        public DateOnly OperationDate { get; set; }
    }
}
