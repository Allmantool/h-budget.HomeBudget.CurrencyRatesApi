using System;

namespace HomeBudget.Components.Exchange.Models
{
    public class ExchangeMultiplierQuery
    {
        public int OriginCurrencyId { get; set; }
        public int TargetCurrencyId { get; set; }
        public DateOnly OperationDate { get; set; }
    }
}
