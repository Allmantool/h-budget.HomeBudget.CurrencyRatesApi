using System.Collections.Generic;

namespace HomeBudget.Components.CurrencyRates.Models
{
    public class ConfigSettings
    {
        public IEnumerable<ConfigCurrency> ActiveNationalBankCurrencies { get; set; }
    }
}
