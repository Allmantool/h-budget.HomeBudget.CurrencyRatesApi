using System;

using Newtonsoft.Json;

namespace HomeBudget.Components.CurrencyRates.Models.Api
{
    public class NationalBankShortCurrencyRate
    {
        [JsonProperty(PropertyName = "Cur_ID")]
        public int CurrencyId { get; set; }

        [JsonProperty(PropertyName = "Date")]
        public DateOnly UpdateDate { get; set; }

        [JsonProperty(PropertyName = "Cur_OfficialRate")]
        public decimal? OfficialRate { get; set; }
    }
}
