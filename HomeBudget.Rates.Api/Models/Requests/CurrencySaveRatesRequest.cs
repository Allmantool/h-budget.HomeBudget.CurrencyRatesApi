using System.Collections.Generic;

namespace HomeBudget.Rates.Api.Models.Requests
{
    public class CurrencySaveRatesRequest
    {
        public IReadOnlyCollection<CurrencyRate> CurrencyRates { get; set; }
    }
}
