using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using EvolveDb;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using Testcontainers.MsSql;
using Testcontainers.Redis;

namespace HomeBudget.Components.IntegrationTests
{
    internal class TestContainersService : IAsyncDisposable
    {
        private readonly IConfiguration _configuration;

        protected MsSqlContainer DbContainer { get; private set; }
        protected RedisContainer CacheContainer { get; private set; }

        public TestContainersService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ApplyDbMigrations()
        {
            var cnx = new SqlConnection(DbContainer.GetConnectionString());
            var evolve = new Evolve(cnx)
            {
                Locations = new[] { "db/migrations" },
                EnableClusterMode = false
            };

            evolve.Migrate();
        }

        public async Task UpAndRunningContainersAsync()
        {
            if (_configuration == null)
            {
                return;
            }

            var redisConfiguration = ConfigurationOptions.Parse(_configuration.GetSection("DatabaseOptions:RedisConnectionString")?.Value);

            var redistTestPort = redisConfiguration.EndPoints.Single();

            var sqlConnectionBuilder = new SqlConnectionStringBuilder
            {
                ConnectionString = _configuration.GetSection("DatabaseOptions:ConnectionString")?.Value
            };

            var sqlDbPort = new Regex("\\d+").Matches(sqlConnectionBuilder.DataSource)[0].Value;

            DbContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithName("integration-sql-server")
                .WithPassword(sqlConnectionBuilder.Password)
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("SA_PASSWORD", sqlConnectionBuilder.Password)
                .WithPortBinding(int.Parse(sqlDbPort), 1433)
                .Build();

            CacheContainer = new RedisBuilder()
                .WithImage("redis:7.0.7")
                .WithPortBinding(6479, 6379)
                .WithName("integration-redis_server")
                .Build();

            if (DbContainer != null)
            {
                await DbContainer.StartAsync();
            }

            if (CacheContainer != null)
            {
                await CacheContainer.StartAsync();
            }

            ApplyDbMigrations();
        }

        public async Task StopAsync()
        {
            await CacheContainer.StopAsync();
            await DbContainer.StopAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (DbContainer != null)
            {
                await DbContainer.DisposeAsync();
            }

            if (CacheContainer != null)
            {
                await CacheContainer.DisposeAsync();
            }
        }
    }
}
