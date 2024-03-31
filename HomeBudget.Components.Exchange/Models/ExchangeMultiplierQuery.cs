using System;

namespace HomeBudget.Components.Exchange.Models
{
    public class ExchangeMultiplierQuery
    {
        public string OriginCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public DateOnly OperationDate { get; set; }
    }
}
