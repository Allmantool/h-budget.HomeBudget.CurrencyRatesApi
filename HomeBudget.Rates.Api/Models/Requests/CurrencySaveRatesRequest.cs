using System.Collections.Generic;

namespace HomeBudget.Rates.Api.Models.Requests
{
    public record CurrencySaveRatesRequest
    {
        public IReadOnlyCollection<CurrencyRate> CurrencyRates { get; set; }
    }
}
