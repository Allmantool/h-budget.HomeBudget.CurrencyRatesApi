using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;

namespace HomeBudget.Components.IntegrationTests.Extensions
{
    internal static class ConfigurationExtensions
    {
        private static readonly Regex PortRegex = new("\\d+");

        public static int? GetTestSqlConnectionPort(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                return default;
            }

            var sqlConnectionBuilder = new SqlConnectionStringBuilder
            {
                ConnectionString = configuration.GetSection("DatabaseOptions:ConnectionString")?.Value
            };

            var sqlDbPortAsString = PortRegex.Matches(sqlConnectionBuilder.DataSource)[0].Value;

            return int.TryParse(sqlDbPortAsString, out var sqlTestPort) ? sqlTestPort : default;
        }

        public static string GetTestSqlPassword(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                return default;
            }

            var sqlConnectionBuilder = new SqlConnectionStringBuilder
            {
                ConnectionString = configuration.GetSection("DatabaseOptions:ConnectionString")?.Value
            };

            return sqlConnectionBuilder.Password;
        }

        public static int? GetTestRedisPort(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                return default;
            }

            var redisConnection = configuration.GetSection("DatabaseOptions:RedisConnectionString")?.Value;

            var redistTestPort = PortRegex.Matches(redisConnection)[0].Value;

            return int.TryParse(redistTestPort, out var redisTestPort) ? redisTestPort : default;
        }
    }
}
