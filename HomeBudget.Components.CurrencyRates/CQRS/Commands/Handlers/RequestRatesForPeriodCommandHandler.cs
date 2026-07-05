using System.Threading;
using System.Threading.Tasks;
using HomeBudget.Components.CurrencyRates.CQRS.Commands.Models;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Services.Interfaces;
using MediatR;

namespace HomeBudget.Components.CurrencyRates.CQRS.Commands.Handlers
{
    internal class RequestRatesForPeriodCommandHandler(
        ICacheService cacheService,
        ICurrencyRatesService currencyRatesService)
        : IRequestHandler<RequestRatesForPeriodCommand>
    {
        public async Task Handle(
            RequestRatesForPeriodCommand request,
            CancellationToken cancellationToken)
        {
            var operationResult = await currencyRatesService.GetRatesForPeriodAsync(
                request.StartDate,
                request.EndDate,
                cancellationToken);

            if (operationResult.IsSucceeded)
            {
                await cacheService.FlushAsync();

                // TODO: SignalR
            }
        }
    }
}
