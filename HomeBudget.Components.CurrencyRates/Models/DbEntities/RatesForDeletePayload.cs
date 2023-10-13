using System;
using System.Collections.Generic;

using HomeBudget.DataAccess;

namespace HomeBudget.Components.CurrencyRates.Models.DbEntities
{
    internal class RatesForDeletePayload : IDbPayload
    {
        public IEnumerable<int> CurrencyIds { get; set; }
        public IEnumerable<DateTime> UpdateDates { get; set; }
    }
}
