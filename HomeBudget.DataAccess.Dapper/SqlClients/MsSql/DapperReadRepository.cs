using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Dapper;
using Microsoft.Extensions.Options;

using HomeBudget.Core.Options;
using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.DataAccess.Dapper.SqlClients.MsSql
{
    internal class DapperReadRepository(ISqlConnectionFactory sqlConnectionFactory, IOptions<DatabaseConnectionOptions> sqlOptions)
        : IBaseReadRepository
    {
        public string Database { get; set; }

        public async Task<IReadOnlyCollection<T>> GetAsync<T>(string sqlQuery, object parameters = null)
        {
            using var db = sqlConnectionFactory.Create(Database);
            var result = parameters == null
                ? await db.QueryAsync<T>(sqlQuery, commandTimeout: sqlOptions.Value.SqlReadCommandTimeoutSeconds)
                : await db.QueryAsync<T>(sqlQuery, parameters, commandTimeout: sqlOptions.Value.SqlReadCommandTimeoutSeconds);

            return result.ToList();
        }

        public async Task<T> SingleAsync<T>(string sqlQuery, object parameters)
        {
            using var db = sqlConnectionFactory.Create(Database);

            return await db.QuerySingleAsync<T>(sqlQuery, parameters, commandTimeout: sqlOptions.Value.SqlReadCommandTimeoutSeconds);
        }
    }
}
