using System.Collections.Generic;

using MediatR;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.CQRS.Commands.Models
{
    public class SaveCurrencyRatesCommand(
        IReadOnlyCollection<CurrencyRate> ratesFromApiCall,
        IReadOnlyCollection<CurrencyRate> ratesFromDatabase)
        : IRequest<Result<int>>
    {
        public IReadOnlyCollection<CurrencyRate> RatesFromDatabase { get; } = ratesFromDatabase;
        public IReadOnlyCollection<CurrencyRate> RatesFromApiCall { get; } = ratesFromApiCall;

        public SaveCurrencyRatesCommand(
            IReadOnlyCollection<CurrencyRate> ratesFromApiCall) : this(ratesFromApiCall, null)
        {
        }
    }
}
