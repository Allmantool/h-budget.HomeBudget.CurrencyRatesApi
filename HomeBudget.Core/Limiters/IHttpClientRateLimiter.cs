using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace HomeBudget.Core.Limiters
{
    public interface IHttpClientRateLimiter
    {
        ValueTask<RateLimitLease> AcquireAsync(int permitCount, CancellationToken cancellationToken = default);
    }
}
