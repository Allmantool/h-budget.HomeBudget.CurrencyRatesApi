using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using HomeBudget.Components.CurrencyRates.Services.Interfaces;

namespace HomeBudget.Rates.Api.Middlewares
{
    internal class NationalBankClientWarmUpMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceCollection _services;

        public NationalBankClientWarmUpMiddleware(RequestDelegate next, IServiceCollection services)
        {
            _next = next;
            _services = services;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            var httpClient = _services.BuildServiceProvider().GetService<INationalBankApiClient>();

            if (httpClient != null)
            {
                await httpClient.WarmUpAsync();
                await httpClient.GetTodayRatesAsync();
            }

            await _next(httpContext);
        }
    }
}
