using System;

using Newtonsoft.Json;

namespace HomeBudget.Components.CurrencyRates.Models.Api
{
    public class NationalBankCurrency
    {
        [JsonProperty(PropertyName = "Cur_ID")]
        public int CurrencyId { get; set; }

        [JsonProperty(PropertyName = "Cur_Code")]
        public string Code { get; set; }

        [JsonProperty(PropertyName = "Cur_Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Cur_Name_Eng")]
        public string EnglishName { get; set; }

        [JsonProperty(PropertyName = "Cur_Scale")]
        public int Scale { get; set; }

        [JsonProperty(PropertyName = "Cur_DateStart")]
        public DateTime DateStart { get; set; }

        [JsonProperty(PropertyName = "Cur_DateEnd")]
        public DateTime DateEnd { get; set; }
    }
}
