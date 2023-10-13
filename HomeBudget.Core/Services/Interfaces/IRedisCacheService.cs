using System;
using System.Threading.Tasks;

using HomeBudget.Core.Models;

namespace HomeBudget.Core.Services.Interfaces
{
    public interface IRedisCacheService
    {
        Task<bool> KeyExistsAsync(string cacheKey);
        Task<T> GetAsync<T>(string cacheKey);
        Task<bool> AddAsync<T>(string cacheKey, T cacheValue);
        Task<Result<T>> CacheWrappedMethodAsync<T>(string cacheKey, Func<Task<Result<T>>> wrappedMethod);

        Task FlushDatabaseAsync();
    }
}
