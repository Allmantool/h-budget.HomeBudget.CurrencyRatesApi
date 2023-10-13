using System;

namespace HomeBudget.Components.CurrencyRates.Models
{
    public class Currency
    {
        public int CurrencyId { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public int Scale { get; set; }

        public DateOnly DateStart { get; set; }

        public DateOnly DateEnd { get; set; }
    }
}
