using System;

using Newtonsoft.Json;

namespace HomeBudget.Components.CurrencyRates.Models.Api
{
    public class NationalBankCurrencyRate
    {
        [JsonProperty(PropertyName = "Cur_ID")]
        public int CurrencyId { get; set; }

        [JsonProperty(PropertyName = "Date")]
        public DateTime UpdateDate { get; set; }

        [JsonProperty(PropertyName = "Cur_Abbreviation")]
        public string Abbreviation { get; set; }

        [JsonProperty(PropertyName = "Cur_Scale")]
        public int Scale { get; set; }

        [JsonProperty(PropertyName = "Cur_Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Cur_OfficialRate")]
        public decimal? OfficialRate { get; set; }
    }
}
