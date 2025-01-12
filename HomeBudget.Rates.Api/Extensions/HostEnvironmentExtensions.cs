using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

using HomeBudget.Rates.Api.Constants;

namespace HomeBudget.Rates.Api.Extensions
{
    internal static class HostEnvironmentExtensions
    {
        public static bool IsUnderDevelopment(this IWebHostEnvironment environment)
            => environment.IsDevelopment() || environment.IsEnvironment(HostEnvironments.Docker);

        public static bool IsUnderDevelopment(this IHostEnvironment environment)
            => environment.IsDevelopment() || environment.IsEnvironment(HostEnvironments.Docker);
    }
}
