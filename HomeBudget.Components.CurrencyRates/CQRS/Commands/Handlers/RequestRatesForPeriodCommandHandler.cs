using System.Threading;
using System.Threading.Tasks;

using MediatR;

using HomeBudget.Components.CurrencyRates.CQRS.Commands.Models;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Services.Interfaces;

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
            var cacheKey = $"{nameof(ICurrencyRatesService.GetRatesForPeriodAsync)}" +
                           $"|{request.StartDate.ToString(DateFormats.NationalBankApiRequest)}-{request.EndDate.ToString(DateFormats.NationalBankApiRequest)}";

            var operationResult = await cacheService.GetOrCreateAsync(
                 cacheKey,
                 () => currencyRatesService.GetRatesForPeriodAsync(request.StartDate, request.EndDate));

            if (operationResult.IsSucceeded)
            {
                // TODO: SignalR
            }
        }
    }
}
