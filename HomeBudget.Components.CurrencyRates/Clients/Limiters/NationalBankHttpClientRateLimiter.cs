using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

using HomeBudget.Core.Limiters;

namespace HomeBudget.Components.CurrencyRates.Clients.Limiters
{
    internal class NationalBankHttpClientRateLimiter(TokenBucketRateLimiter limiter) : IHttpClientRateLimiter
    {
        public ValueTask<RateLimitLease> AcquireAsync(int permitCount, CancellationToken cancellationToken = default)
        {
            return limiter.AcquireAsync(permitCount, cancellationToken);
        }
    }
}
