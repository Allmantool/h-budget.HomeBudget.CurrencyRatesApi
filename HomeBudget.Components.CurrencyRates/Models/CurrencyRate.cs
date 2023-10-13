using System;

namespace HomeBudget.Components.CurrencyRates.Models
{
    public class CurrencyRate
    {
        public int CurrencyId { get; set; }

        public string Name { get; set; }

        public string Abbreviation { get; set; }

        public int Scale { get; set; }

        public decimal OfficialRate { get; set; }

        public decimal RatePerUnit { get; set; }

        public DateOnly UpdateDate { get; set; }
    }
}
