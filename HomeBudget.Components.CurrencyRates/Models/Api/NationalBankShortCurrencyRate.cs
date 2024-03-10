using System;

using Newtonsoft.Json;

using HomeBudget.Components.CurrencyRates.Configuration;

namespace HomeBudget.Components.CurrencyRates.Models.Api
{
    public class NationalBankShortCurrencyRate
    {
        [JsonProperty(PropertyName = "Cur_ID")]
        public int CurrencyId { get; set; }

        [JsonProperty(PropertyName = "Date")]
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly UpdateDate { get; set; }

        [JsonProperty(PropertyName = "Cur_OfficialRate")]
        public decimal? OfficialRate { get; set; }
    }
}
