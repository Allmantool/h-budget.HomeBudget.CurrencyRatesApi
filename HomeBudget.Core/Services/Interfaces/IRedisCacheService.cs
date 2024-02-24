using System;
using System.Threading.Tasks;

using HomeBudget.Core.Models;

namespace HomeBudget.Core.Services.Interfaces
{
    public interface IRedisCacheService
    {
        Task<Result<T>> AddOrGetExistingAsync<T>(string key, Func<Task<Result<T>>> callback);
        Task FlushDatabaseAsync();
    }
}
