using System.Data;
using System.Threading.Tasks;

using Dapper;

using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.DataAccess.Dapper.SqlClients.MsSql
{
    public class DapperWriteRepository : IBaseWriteRepository
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public DapperWriteRepository(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<int> ExecuteAsync<T>(string sqlQuery, T parameters, IDbTransaction dbTransaction = null)
            where T : IDbEntity
        {
            using var db = _sqlConnectionFactory.Create();

            return dbTransaction == null
                ? await db.ExecuteAsync(sqlQuery, parameters)
                : await db.ExecuteAsync(sqlQuery, parameters, dbTransaction);
        }

        public async Task<int> ExecuteAsync<T>(string sqlQuery, T[] parameters, IDbTransaction dbTransaction = null)
            where T : IDbEntity
        {
            using var db = _sqlConnectionFactory.Create();

            return dbTransaction == null
                ? await db.ExecuteAsync(sqlQuery, parameters)
                : await db.ExecuteAsync(sqlQuery, parameters, dbTransaction);
        }
    }
}
