using System.Collections.Generic;

using MediatR;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.CQRS.Commands.Models
{
    public class SaveCurrencyRatesCommand : IRequest<Result<int>>
    {
        public IReadOnlyCollection<CurrencyRate> RatesFromDatabase { get; }
        public IReadOnlyCollection<CurrencyRate> RatesFromApiCall { get; }

        public SaveCurrencyRatesCommand(
            IReadOnlyCollection<CurrencyRate> ratesFromApiCall,
            IReadOnlyCollection<CurrencyRate> ratesFromDatabase)
        {
            RatesFromApiCall = ratesFromApiCall;
            RatesFromDatabase = ratesFromDatabase;
        }

        public SaveCurrencyRatesCommand(
            IReadOnlyCollection<CurrencyRate> ratesFromApiCall)
        {
            RatesFromApiCall = ratesFromApiCall;
        }
    }
}
