using System.Collections.Generic;

namespace HomeBudget.Components.CurrencyRates.Models
{
    public class CurrencyRateGrouped
    {
        public int CurrencyId { get; set; }

        public string Name { get; set; }

        public string Abbreviation { get; set; }

        public int Scale { get; set; }

        public IEnumerable<CurrencyRateValue> RateValues { get; set; }
    }
}
