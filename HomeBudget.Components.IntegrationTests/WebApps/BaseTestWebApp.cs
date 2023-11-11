using System;
using System.Threading.Tasks;

using NUnit.Framework;
using RestSharp;
using Testcontainers.MsSql;
using Testcontainers.Redis;

using HomeBudget.Components.IntegrationTests.Constants;

namespace HomeBudget.Components.IntegrationTests.WebApps
{
    public abstract class BaseTestWebApp<TEntryPoint> : IDisposable, IAsyncDisposable
        where TEntryPoint : class
    {
        private bool _disposed;

        private IntegrationTestWebApplicationFactory<TEntryPoint> WebFactory { get; }
        internal RestClient RestHttpClient { get; private set; }
        protected MsSqlContainer DbContainer { get; private set; }
        protected RedisContainer CacheContainer { get; private set; }

        protected BaseTestWebApp()
        {
            var testProperties = TestContext.CurrentContext.Test.Properties;
            var testCategory = testProperties.Get("Category") as string;

            if (TestTypes.Integration.Equals(testCategory, StringComparison.OrdinalIgnoreCase))
            {
                WebFactory = new IntegrationTestWebApplicationFactory<TEntryPoint>(SetUpDockerContainers);

                RestHttpClient = new RestClient(
                    WebFactory.CreateClient(),
                    new RestClientOptions(new Uri("http://localhost:6064")));
            }
        }

        public async Task StartAsync()
        {
            if (DbContainer != null)
            {
                await DbContainer.StartAsync();
            }

            if (CacheContainer != null)
            {
                await CacheContainer.StartAsync();
            }
        }

        public async Task StopAsync()
        {
            await CacheContainer.StopAsync();
            await DbContainer.StopAsync();
        }

        private void SetUpDockerContainers()
        {
            var configuration = WebFactory?.Configuration;

            var databaseConnection = configuration?.GetSection("DatabaseOptions:ConnectionString");
            var redisConnection = configuration?.GetSection("DatabaseOptions:RedisConnectionString");

            DbContainer = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithName("integration-sql-server")
                .WithPassword("Strong_password_123!")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("SA_PASSWORD", "Strong_password_123!")
                .WithPortBinding(1533, 1433)
                .Build();

            CacheContainer = new RedisBuilder()
                .WithImage("redis:7.0.7")
                .WithPortBinding(6479, 6379)
                .WithName("integration-redis_server")
                .Build();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCoreAsync();

            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _ = DisposeAsyncCoreAsync().AsTask();
            }

            _disposed = true;
        }

        protected virtual async ValueTask DisposeAsyncCoreAsync()
        {
            if (DbContainer != null)
            {
                await DbContainer.DisposeAsync();
            }

            if (CacheContainer != null)
            {
                await CacheContainer.DisposeAsync();
            }

            if (WebFactory != null)
            {
                await WebFactory.DisposeAsync();
            }

            RestHttpClient?.Dispose();
        }
    }
}
