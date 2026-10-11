using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.CQRS.Commands.Models;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.Services.Interfaces
{
    public interface ICurrencyRatesService
    {
        Task<Result<int>> SaveWithRewriteAsync(
            SaveCurrencyRatesCommand saveRatesCommand,
            CancellationToken cancellationToken = default);

        Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetRatesAsync();

        Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetRatesForPeriodAsync(
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken ct = default);

        Task<Result<IReadOnlyCollection<CurrencyRateGrouped>>> GetTodayRatesAsync();
    }
}
