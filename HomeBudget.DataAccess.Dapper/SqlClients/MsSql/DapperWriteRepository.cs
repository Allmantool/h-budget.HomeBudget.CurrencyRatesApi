using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using HomeBudget.Core.Options;
using HomeBudget.DataAccess.Interfaces;
using Microsoft.Extensions.Options;

namespace HomeBudget.DataAccess.Dapper.SqlClients.MsSql
{
    public class DapperWriteRepository(ISqlConnectionFactory sqlConnectionFactory, IOptions<DatabaseConnectionOptions> sqlOptions) : IBaseWriteRepository
    {
        public string Database { get; set; }

        public async Task<int> ExecuteAsync<T>(string sqlQuery, T parameters, IDbTransaction dbTransaction = null)
            where T : IDbEntity
        {
            using var db = sqlConnectionFactory.Create(Database);

            return dbTransaction == null
                ? await db.ExecuteAsync(sqlQuery, parameters)
                : await db.ExecuteAsync(sqlQuery, parameters, dbTransaction);
        }

        public async Task<int> ExecuteAsync<T>(string sqlQuery, T[] parameters, IDbTransaction dbTransaction = null)
            where T : IDbEntity
        {
            using var db = sqlConnectionFactory.Create(Database);

            return dbTransaction == null
                ? await db.ExecuteAsync(sqlQuery, parameters, commandTimeout: sqlOptions.Value.SqlWriteCommandTimeoutSeconds)
                : await db.ExecuteAsync(sqlQuery, parameters, dbTransaction, commandTimeout: sqlOptions.Value.SqlWriteCommandTimeoutSeconds);
        }

        public Task<int> ExecuteAsync(
            string sqlQuery,
            DataTable dt,
            string mapToDbType,
            IDbTransaction dbTransaction = null)
        {
            ArgumentNullException.ThrowIfNull(dt);

            return ExecuteDataTableAsync(sqlQuery, dt, mapToDbType, dbTransaction);
        }

        private async Task<int> ExecuteDataTableAsync(
            string sqlQuery,
            DataTable dt,
            string mapToDbType,
            IDbTransaction dbTransaction)
        {
            using var db = sqlConnectionFactory.Create(Database);

            var parameters = new DynamicParameters();
            parameters.Add($"@{dt.TableName}", dt.AsTableValuedParameter(mapToDbType));

            return dbTransaction == null
                ? await db.ExecuteAsync(sqlQuery, parameters, commandTimeout: sqlOptions.Value.SqlWriteCommandTimeoutSeconds)
                : await db.ExecuteAsync(sqlQuery, parameters, dbTransaction, commandTimeout: sqlOptions.Value.SqlWriteCommandTimeoutSeconds);
        }

        public async Task<TResult> QuerySingleAsync<TResult, T>(
            string sqlQuery,
            T parameters,
            CancellationToken cancellationToken = default)
            where T : IDbEntity
        {
            using var db = sqlConnectionFactory.Create(Database);
            var command = new CommandDefinition(
                sqlQuery,
                parameters,
                commandTimeout: sqlOptions.Value.SqlWriteCommandTimeoutSeconds,
                cancellationToken: cancellationToken);

            return await db.QuerySingleAsync<TResult>(command);
        }

        public Task<TResult> QuerySingleAsync<TResult>(
            string sqlQuery,
            DataTable dt,
            string mapToDbType,
            object parameters,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(dt);

            return QuerySingleDataTableAsync<TResult>(
                sqlQuery,
                dt,
                mapToDbType,
                parameters,
                cancellationToken);
        }

        private async Task<TResult> QuerySingleDataTableAsync<TResult>(
            string sqlQuery,
            DataTable dt,
            string mapToDbType,
            object parameters,
            CancellationToken cancellationToken)
        {
            using var db = sqlConnectionFactory.Create(Database);
            var dynamicParameters = new DynamicParameters(parameters);
            dynamicParameters.Add($"@{dt.TableName}", dt.AsTableValuedParameter(mapToDbType));
            var command = new CommandDefinition(
                sqlQuery,
                dynamicParameters,
                commandTimeout: sqlOptions.Value.SqlWriteCommandTimeoutSeconds,
                cancellationToken: cancellationToken);

            return await db.QuerySingleAsync<TResult>(command);
        }
    }
}
