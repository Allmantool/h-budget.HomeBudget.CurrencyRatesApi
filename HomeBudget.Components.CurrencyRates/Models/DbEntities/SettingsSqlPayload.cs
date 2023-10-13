using HomeBudget.DataAccess;

namespace HomeBudget.Components.CurrencyRates.Models.DbEntities
{
    internal class SettingsForUpdatePayload : IDbPayload
    {
        public string Key { get; set; }
        public string Settings { get; set; }
    }
}
