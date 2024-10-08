﻿using System;
using System.Threading.Tasks;

using EvolveDb;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;
using Testcontainers.Redis;

using HomeBudget.Components.IntegrationTests.Extensions;

namespace HomeBudget.Components.IntegrationTests
{
    internal class TestContainersService(IConfiguration configuration) : IAsyncDisposable
    {
        protected MsSqlContainer DbContainer { get; private set; }
        protected RedisContainer CacheContainer { get; private set; }

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
            if (configuration == null)
            {
                return;
            }

            var sqlConnectionPort = configuration.GetTestSqlConnectionPort();

            if (sqlConnectionPort.HasValue)
            {
                DbContainer = new MsSqlBuilder()
                    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                    .WithName("integration-sql-server")
                    .WithPassword(configuration.GetTestSqlPassword())
                    .WithEnvironment("ACCEPT_EULA", "Y")
                    .WithEnvironment("SA_PASSWORD", configuration.GetTestSqlPassword())
                    .WithPortBinding(sqlConnectionPort.Value, 1433)
                    .Build();
            }

            var redistPort = configuration.GetTestRedisPort();

            if (redistPort.HasValue)
            {
                CacheContainer = new RedisBuilder()
                    .WithImage("redis:7.0.7")
                    .WithPortBinding(redistPort.Value, 6379)
                    .WithName("integration-redis_server")
                    .Build();
            }

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
