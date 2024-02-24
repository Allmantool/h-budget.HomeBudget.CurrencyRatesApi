using Microsoft.Extensions.DependencyInjection;
using HomeBudget.Core.Services;
using HomeBudget.Core.Services.Interfaces;

namespace HomeBudget.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterCoreIoCDependency(this IServiceCollection services)
        {
            return services
                .AddScoped<ICacheService, RedisCacheService>();
        }
    }
}
