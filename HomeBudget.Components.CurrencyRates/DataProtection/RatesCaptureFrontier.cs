using System;

namespace HomeBudget.Components.CurrencyRates.DataProtection
{
    public sealed record RatesCaptureFrontier(
        Guid ReceiptId,
        long CommitSequence,
        DateTimeOffset CommittedUtc)
    {
        public string DecimalSerialization { get; } = "invariant-decimal-string";

        public string SourceDecimalStorage { get; } = "legacy-sql-float-lossy";
    }
}
