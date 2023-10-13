using System;

using HomeBudget.DataAccess;

namespace HomeBudget.Components.CurrencyRates.Models.DbEntities
{
    internal class CurrencyRateEntity : IDbEntity
    {
        public int CurrencyId { get; set; }

        public string Name { get; set; }

        public string Abbreviation { get; set; }

        public int Scale { get; set; }

        public decimal OfficialRate { get; set; }

        public decimal RatePerUnit { get; set; }

        public DateTime UpdateDate { get; set; }
    }
}
