using HomeBudget.Core.Models;

namespace HomeBudget.Core.Services
{
    public abstract class BaseService
    {
        public Result<T> Failed<T>()
        {
            return new Result<T>(isSucceeded: false);
        }

        public Result<T> Succeeded<T>(T payload)
        {
            return new Result<T>(payload: payload, isSucceeded: true);
        }

        public Result<T> NotFound<T>()
        {
            return new Result<T>(isSucceeded: false);
        }
    }
}
