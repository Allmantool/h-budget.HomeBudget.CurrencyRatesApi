using System.Threading;
using System.Threading.Tasks;

using MediatR;

using HomeBudget.Components.CurrencyRates.CQRS.Commands.Models;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Services.Interfaces;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.CQRS.Commands.Handlers
{
    internal class SaveCurrencyRatesCommandHandler : IRequestHandler<SaveCurrencyRatesCommand, Result<int>>
    {
        private readonly ICurrencyRatesService _currencyRatesService;
        private readonly IRedisCacheService _redisCacheService;

        public SaveCurrencyRatesCommandHandler(ICurrencyRatesService currencyRatesService, IRedisCacheService redisCacheService)
        {
            _currencyRatesService = currencyRatesService;
            _redisCacheService = redisCacheService;
        }

        public async Task<Result<int>> Handle(SaveCurrencyRatesCommand request, CancellationToken cancellationToken)
        {
            var result = await _currencyRatesService.SaveWithRewriteAsync(request);

            await _redisCacheService.FlushDatabaseAsync();

            return result;
        }
    }
}
