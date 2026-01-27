using System;
using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using HomeBudget.Core.Options;
using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.DataAccess.Dapper.SqlClients
{
    internal class SqlConnectionFactory(
        ILogger<SqlConnectionFactory> logger,
        IOptions<DatabaseConnectionOptions> options)
    : ISqlConnectionFactory
    {
        private static readonly Action<ILogger, string, Exception> _logConnectionFailure =
            LoggerMessage.Define<string>(
                LogLevel.Critical,
                new EventId(1001, nameof(SqlConnectionFactory)),
                "Failed to connect to database with connection string: '{ConnectionString}'");

        private readonly DatabaseConnectionOptions _databaseConnectionOptions = options.Value;

        public IDbConnection Create(string databasename)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(databasename))
                {
                    var builder = new SqlConnectionStringBuilder(_databaseConnectionOptions.ConnectionString)
                    {
                        InitialCatalog = databasename
                    };

                    return new SqlConnection(builder.ConnectionString);
                }

                return new SqlConnection(_databaseConnectionOptions.ConnectionString);
            }
            catch (Exception ex)
            {
                _logConnectionFailure(logger, _databaseConnectionOptions.ConnectionString, ex);
                throw;
            }
        }
    }
}
