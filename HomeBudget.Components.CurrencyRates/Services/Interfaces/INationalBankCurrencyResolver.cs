using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Models;

namespace HomeBudget.Components.CurrencyRates.Services.Interfaces
{
    internal interface INationalBankCurrencyResolver
    {
        Task<IReadOnlyCollection<NationalBankCurrencyDefinition>> ResolveActiveCurrenciesAsync(
            DateOnly requestedDate,
            CancellationToken ct = default);
    }
}
