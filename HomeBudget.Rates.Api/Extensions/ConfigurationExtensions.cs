using Microsoft.Extensions.Configuration;

using HomeBudget.Rates.Api.Constants;

namespace HomeBudget.Rates.Api.Extensions
{
    internal static class ConfigurationExtensions
    {
        public static string GetHealthCheckEndpoint(this IConfiguration configuration, string hostUrls)
        {
            var hostFromConfiguration = configuration.GetRequiredSection("HealthCheckOptions:Host").Value;

            var healthCheckHost = string.IsNullOrWhiteSpace(hostFromConfiguration) ? hostUrls : hostFromConfiguration;

            return $"{healthCheckHost}{Endpoints.HealthCheckSource}";
        }
    }
}
