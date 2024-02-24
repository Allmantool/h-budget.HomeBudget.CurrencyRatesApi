using System;
using System.Threading.Tasks;

using HomeBudget.Core.Models;

namespace HomeBudget.Core.Services.Interfaces
{
    public interface ICacheService
    {
        Task<Result<T>> GetOrCreateAsync<T>(string key, Func<Task<Result<T>>> callback);
        Task FlushAsync();
    }
}
