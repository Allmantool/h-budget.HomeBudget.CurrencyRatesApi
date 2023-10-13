using System.Collections.Generic;

namespace HomeBudget.Rates.Api.Models
{
    public class CurrencySaveRatesRequest
    {
        public IReadOnlyCollection<CurrencyRate> CurrencyRates { get; set; }
    }
}
