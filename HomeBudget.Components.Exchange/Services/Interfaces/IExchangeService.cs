using System.Threading;
using System.Threading.Tasks;

using HomeBudget.Components.Exchange.Models;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.Exchange.Services.Interfaces
{
    public interface IExchangeService
    {
        public Task<Result<decimal>> GetCurrencyConversionMultiplierAsync(ExchangeMultiplierQuery query, CancellationToken token);
    }
}
