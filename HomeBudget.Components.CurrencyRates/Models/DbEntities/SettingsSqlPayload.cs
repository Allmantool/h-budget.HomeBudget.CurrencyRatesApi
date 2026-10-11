using System;

using HomeBudget.DataAccess;

namespace HomeBudget.Components.CurrencyRates.Models.DbEntities
{
    internal class SettingsForUpdatePayload : IDbPayload
    {
        public string Key { get; set; }

        public string Settings { get; set; }

        public Guid ReceiptId { get; set; }

        public string LockResource { get; set; }

        public string GlobalMaintenanceLockResource { get; set; }

        public int LockTimeoutMilliseconds { get; set; }
    }
}
