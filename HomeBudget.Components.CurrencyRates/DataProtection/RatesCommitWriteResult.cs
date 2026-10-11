using System;

namespace HomeBudget.Components.CurrencyRates.DataProtection
{
    internal sealed class RatesCommitWriteResult
    {
        public Guid ReceiptId { get; set; }

        public long CommitSequence { get; set; }

        public int AffectedRows { get; set; }

        public DateTime CommittedUtc { get; set; }
    }
}
