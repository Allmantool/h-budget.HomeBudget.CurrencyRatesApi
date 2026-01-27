using System;
using System.Collections.Generic;
using System.Threading.RateLimiting;

namespace HomeBudget.Components.CurrencyRates.Tests
{
    internal sealed class TestRateLimitLease : RateLimitLease
    {
        private readonly bool _isAcquired;

        public TestRateLimitLease(bool isAcquired)
        {
            _isAcquired = isAcquired;
        }

        public override bool IsAcquired => _isAcquired;

        public override IEnumerable<string> MetadataNames => Array.Empty<string>();

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            metadata = null;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            // nothing to dispose
        }
    }
}
