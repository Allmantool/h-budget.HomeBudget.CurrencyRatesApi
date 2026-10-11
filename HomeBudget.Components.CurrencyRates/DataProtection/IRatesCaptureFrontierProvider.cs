using System.Threading;
using System.Threading.Tasks;

namespace HomeBudget.Components.CurrencyRates.DataProtection
{
    public interface IRatesCaptureFrontierProvider
    {
        Task<RatesCaptureFrontier> AcquireAsync(CancellationToken cancellationToken = default);
    }
}
