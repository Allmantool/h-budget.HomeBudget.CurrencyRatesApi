using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using StackExchange.Redis;

using HomeBudget.Core.Models;
using HomeBudget.Core.Services.Interfaces;

namespace HomeBudget.Core.Services
{
    internal class RedisCacheService(IDatabase redisDatabase, IOptions<CacheStoreOptions> cacheOptions)
        : BaseService, IRedisCacheService
    {
        private readonly CacheStoreOptions _cacheOptions = cacheOptions.Value;

        public Task<bool> KeyExistsAsync(string cacheKey)
        {
            return redisDatabase.KeyExistsAsync(cacheKey);
        }

        public async Task<T> GetAsync<T>(string cacheKey)
        {
            var cacheValue = await redisDatabase.StringGetAsync(cacheKey);

            if (cacheValue.IsNullOrEmpty)
            {
                return await Task.FromResult<T>(default);
            }

            return JsonSerializer.Deserialize<T>(cacheValue);
        }

        public Task<bool> AddAsync<T>(string cacheKey, T cacheValue)
        {
            return Equals(cacheValue, default(T))
                ? Task.FromResult(false)
                : redisDatabase.StringSetAsync(
                    cacheKey,
                    JsonSerializer.Serialize(cacheValue),
                    TimeSpan.FromMinutes(_cacheOptions.ExpirationInMinutes));
        }

        public Task FlushDatabaseAsync() => GetCurrentServer().FlushDatabaseAsync();

        public async Task<Result<T>> CacheWrappedMethodAsync<T>(string cacheKey, Func<Task<Result<T>>> wrappedMethod)
        {
            if (await KeyExistsAsync(cacheKey))
            {
                return Succeeded(await GetAsync<T>(cacheKey));
            }

            if (wrappedMethod is null)
            {
                throw new ArgumentNullException(nameof(wrappedMethod));
            }

            var cacheValue = await wrappedMethod.Invoke();
            await AddAsync(cacheKey, cacheValue.Payload);

            return Succeeded(await GetAsync<T>(cacheKey));
        }

        private IServer GetCurrentServer()
        {
            var servers = redisDatabase.Multiplexer.GetServers();

            return servers.FirstOrDefault();
        }
    }
}
