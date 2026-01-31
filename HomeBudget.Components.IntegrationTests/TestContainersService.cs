using System;
using System.Threading;
using System.Threading.Tasks;

using DotNet.Testcontainers.Builders;
using EvolveDb;
using EvolveDb.Configuration;
using Microsoft.Data.SqlClient;
using Testcontainers.MsSql;
using Testcontainers.Redis;

using HomeBudget.Components.IntegrationTests.Constants;
using HomeBudget.Core;

namespace HomeBudget.Components.IntegrationTests
{
    internal class TestContainersService : IAsyncDisposable
    {
        private static readonly SemaphoreSlim _lock = new(1, 1);
        private static TestContainersService _instance;
        private bool _isDisposed;

        public bool IsReadyForUse { get; private set; }

        public static TestContainersService GetInstance { get; private set; } = _instance;
        public MsSqlContainer MsSqlDbContainer { get; private set; }
        public RedisContainer CacheContainer { get; private set; }

        protected TestContainersService()
        {
        }

        public void ApplyDbMigrations()
        {
            using var cnx = new SqlConnection(MsSqlDbContainer.GetConnectionString());
            var evolve = new Evolve(cnx)
            {
                Locations = ["db/migrations"],
                EnableClusterMode = false,
                TransactionMode = TransactionKind.CommitEach
            };

            evolve.Migrate();
        }

        public static async Task<TestContainersService> InitAsync()
        {
            if (GetInstance is not null)
            {
                return GetInstance;
            }

            await using (await SemaphoreGuard.WaitAsync(_lock))
            {
                if (_instance is null)
                {
                    _instance = new TestContainersService();
                    await _instance.UpAndRunningContainersAsync();
                }

                GetInstance = _instance;

                return GetInstance;
            }
        }

        private async Task<bool> UpAndRunningContainersAsync()
        {
            if (IsReadyForUse)
            {
                return true;
            }

            try
            {
                MsSqlDbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
                    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                    .WithName("integration-sql-server")
                    .WithPassword("Passw0rd!")
                    .WithEnvironment("ACCEPT_EULA", "Y")
                    .WithEnvironment("SA_PASSWORD", "Passw0rd!")
                    .WithPortBinding(1433, true)
                    .WithWaitStrategy(
                        Wait.ForUnixContainer()
                            .AddCustomWaitStrategy(
                                new CustomWaitStrategy(TimeSpan.FromMinutes(BaseTestContainerOptions.StopTimeoutInMinutes))
                        )
                   )
                  .Build();

                CacheContainer = new RedisBuilder("redis:7.0.7")
                    .WithImage("redis:7.0.7")
                    .WithPortBinding(6379, true)
                    .WithName("integration-redis_server")
                    .WithWaitStrategy(
                        Wait.ForUnixContainer()
                            .AddCustomWaitStrategy(
                                new CustomWaitStrategy(TimeSpan.FromMinutes(BaseTestContainerOptions.StopTimeoutInMinutes))
                        )
                    )
                    .Build();

                if (MsSqlDbContainer != null)
                {
                    await MsSqlDbContainer.StartAsync();
                }

                if (CacheContainer != null)
                {
                    await CacheContainer.StartAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(BaseTestContainerOptions.WaitUntilContainersInitInSeconds));

                ApplyDbMigrations();

                IsReadyForUse = true;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                IsReadyForUse = false;

                throw;
            }
        }

        public async Task StopAsync()
        {
            await CacheContainer.StopAsync();
            await MsSqlDbContainer.StopAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (MsSqlDbContainer != null)
            {
                await MsSqlDbContainer.DisposeAsync();
            }

            if (CacheContainer != null)
            {
                await CacheContainer.DisposeAsync();
            }
        }
    }
}
