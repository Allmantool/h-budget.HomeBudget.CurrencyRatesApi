﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Dapper;

using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.DataAccess.Dapper.SqlClients.MsSql
{
    internal class DapperReadRepository(ISqlConnectionFactory sqlConnectionFactory) : IBaseReadRepository
    {
        public async Task<IReadOnlyCollection<T>> GetAsync<T>(string sqlQuery, object parameters = null)
        {
            using var db = sqlConnectionFactory.Create();
            var result = parameters == null
                ? await db.QueryAsync<T>(sqlQuery)
                : await db.QueryAsync<T>(sqlQuery, parameters);

            return result.ToList();
        }

        public async Task<T> SingleAsync<T>(string sqlQuery, object parameters)
        {
            using var db = sqlConnectionFactory.Create();

            return await db.QuerySingleAsync<T>(sqlQuery, parameters);
        }
    }
}
