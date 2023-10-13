using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.Models.Api
{
    internal class YearRatesRequestPayload
    {
        public int CurrencyId { get; set; }
        public PeriodRange Period { get; set; }
    }
}
