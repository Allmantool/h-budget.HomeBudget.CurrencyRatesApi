using HealthChecks.UI.Client;
using HomeBudget.Rates.Api.Constants;
using HomeBudget.Rates.Api.Extensions;
using HomeBudget.Rates.Api.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HomeBudget.Rates.Api.Configuration
{
    internal static class HealthCheckConfiguration
    {
        private static readonly string[] CustomTags = new[] { "custom" };
        private static readonly string[] RedisTags = new[] { "redis" };
        private static readonly string[] SqlServerTags = new[] { "sqlServer" };

        public static IServiceCollection SetUpHealthCheck(this IServiceCollection services, IConfiguration configuration, string hostUrls)
        {
            var msSqlConnectionString = configuration.GetRequiredSection("DatabaseOptions:ConnectionString").Value;
            var redisConnectionString = configuration.GetRequiredSection("DatabaseOptions:RedisConnectionString").Value;

            if (string.IsNullOrWhiteSpace(msSqlConnectionString) || string.IsNullOrWhiteSpace(redisConnectionString))
            {
                return services;
            }

            services
                .AddHealthChecks()
                .AddCheck("heartbeat", () => HealthCheckResult.Healthy())
                .AddCheck<CustomLogicHealthCheck>(nameof(CustomLogicHealthCheck), tags: CustomTags)
                .AddSqlServer(msSqlConnectionString, tags: SqlServerTags)
                .AddRedis(redisConnectionString, tags: RedisTags);

            services.AddHealthChecksUI(setupSettings: setup =>
                {
                    setup.AddHealthCheckEndpoint("[Currency rates endpoint]", configuration.GetHealthCheckEndpoint(hostUrls));
                })
                .AddInMemoryStorage();

            return services;
        }

        public static IEndpointRouteBuilder SetUpHealthCheckEndpoints(this IEndpointRouteBuilder builder)
        {
            builder.MapHealthChecks(Endpoints.HealthCheckSource, new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });

            builder.MapHealthChecksUI(options =>
            {
                options.UIPath = "/show-health-ui";
                options.ApiPath = "/health-ui-api";
            });

            return builder;
        }
    }
}
