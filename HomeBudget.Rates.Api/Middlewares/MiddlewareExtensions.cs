using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBudget.Rates.Api.Middlewares
{
    internal static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseNationalBankClientWarmUpMiddleware(
            this IApplicationBuilder builder, IServiceCollection services)
        {
            return builder.UseMiddleware<NationalBankClientWarmUpMiddleware>(services);
        }
    }
}
