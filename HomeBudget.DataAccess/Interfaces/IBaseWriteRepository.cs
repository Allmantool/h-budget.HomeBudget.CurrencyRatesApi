using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace HomeBudget.DataAccess.Interfaces
{
    public interface IBaseWriteRepository
    {
        string Database { get; set; }

        Task<int> ExecuteAsync<T>(string sqlQuery, T parameters, IDbTransaction dbTransaction = null)
            where T : IDbEntity;

        Task<int> ExecuteAsync<T>(string sqlQuery, T[] parameters, IDbTransaction dbTransaction = null)
            where T : IDbEntity;

        Task<int> ExecuteAsync(string sqlQuery, DataTable dt, string mapToDbType, IDbTransaction dbTransaction = null);

        Task<TResult> QuerySingleAsync<TResult, T>(
            string sqlQuery,
            T parameters,
            CancellationToken cancellationToken = default)
            where T : IDbEntity;

        Task<TResult> QuerySingleAsync<TResult>(
            string sqlQuery,
            DataTable dt,
            string mapToDbType,
            object parameters,
            CancellationToken cancellationToken = default);
    }
}
