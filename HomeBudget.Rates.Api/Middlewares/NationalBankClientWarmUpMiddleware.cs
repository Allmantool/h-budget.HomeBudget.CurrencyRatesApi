using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using HomeBudget.Components.CurrencyRates.Services.Interfaces;

namespace HomeBudget.Rates.Api.Middlewares
{
    internal class NationalBankClientWarmUpMiddleware(RequestDelegate next, IServiceCollection services)
    {
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var httpClient = services.BuildServiceProvider().GetService<INationalBankApiClient>();

            if (httpClient != null)
            {
                await httpClient.WarmUpAsync();
                await httpClient.GetTodayRatesAsync();
            }

            await next(httpContext);
        }
    }
}
