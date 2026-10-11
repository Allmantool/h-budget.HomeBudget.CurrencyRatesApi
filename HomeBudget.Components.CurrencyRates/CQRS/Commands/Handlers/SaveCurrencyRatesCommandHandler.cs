using System.Threading;
using System.Threading.Tasks;
using HomeBudget.Components.CurrencyRates.CQRS.Commands.Models;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Models;
using HomeBudget.Core.Services.Interfaces;
using MediatR;

namespace HomeBudget.Components.CurrencyRates.CQRS.Commands.Handlers
{
    internal class SaveCurrencyRatesCommandHandler(
        ICurrencyRatesService currencyRatesService,
        ICacheService cacheService)
        : IRequestHandler<SaveCurrencyRatesCommand, Result<int>>
    {
        public async Task<Result<int>> Handle(SaveCurrencyRatesCommand request, CancellationToken cancellationToken)
        {
            var result = await currencyRatesService.SaveWithRewriteAsync(request, cancellationToken);

            await cacheService.FlushAsync();

            return result;
        }
    }
}
