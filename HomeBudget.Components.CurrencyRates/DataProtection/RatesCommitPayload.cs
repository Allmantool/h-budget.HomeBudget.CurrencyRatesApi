using System;

using HomeBudget.DataAccess;

namespace HomeBudget.Components.CurrencyRates.DataProtection
{
    internal sealed class RatesCommitPayload : IDbPayload
    {
        public Guid ReceiptId { get; init; }

        public string LockResource { get; init; }

        public string GlobalMaintenanceLockResource { get; init; }

        public int LockTimeoutMilliseconds { get; init; }
    }
}
