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
        : BaseService, ICacheService
    {
        private readonly CacheStoreOptions _cacheOptions = cacheOptions.Value;

        public Task FlushAsync() => GetCurrentServer().FlushDatabaseAsync();

        public async Task<Result<T>> GetOrCreateAsync<T>(string key, Func<Task<Result<T>>> callback)
        {
            if (await DoesKeyExistAsync(key))
            {
                return Succeeded(await GetAsync<T>(key));
            }

            if (callback is null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var cacheValue = await callback.Invoke();
            await AddAsync(key, cacheValue.Payload);

            return Succeeded(await GetAsync<T>(key));
        }

        private async Task<T> GetAsync<T>(string key)
        {
            var value = await redisDatabase.StringGetAsync(key);

            if (value.IsNullOrEmpty)
            {
                return await Task.FromResult<T>(default);
            }

            return JsonSerializer.Deserialize<T>(value);
        }

        private Task<bool> DoesKeyExistAsync(string key)
        {
            return redisDatabase.KeyExistsAsync(key);
        }

        private Task<bool> AddAsync<T>(string key, T value)
        {
            return Equals(value, default(T))
                ? Task.FromResult(false)
                : redisDatabase.StringSetAsync(
                    key,
                    JsonSerializer.Serialize(value),
                    TimeSpan.FromMinutes(_cacheOptions.ExpirationInMinutes));
        }

        private IServer GetCurrentServer()
        {
            var servers = redisDatabase.Multiplexer.GetServers();

            return servers.FirstOrDefault();
        }
    }
}
