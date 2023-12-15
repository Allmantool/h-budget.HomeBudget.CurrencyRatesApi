using System.Threading;
using System.Threading.Tasks;

using MediatR;

using HomeBudget.Components.CurrencyRates.CQRS.Commands.Models;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Models;
using HomeBudget.Core.Services.Interfaces;

namespace HomeBudget.Components.CurrencyRates.CQRS.Commands.Handlers
{
    internal class SaveCurrencyRatesCommandHandler(
        ICurrencyRatesService currencyRatesService,
        IRedisCacheService redisCacheService)
        : IRequestHandler<SaveCurrencyRatesCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(SaveCurrencyRatesCommand request, CancellationToken cancellationToken)
        {
            var result = await currencyRatesService.SaveWithRewriteAsync(request);

            await redisCacheService.FlushDatabaseAsync();

            return result;
        }
    }
}
