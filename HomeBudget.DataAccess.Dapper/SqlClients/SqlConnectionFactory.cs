using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using HomeBudget.Core.Extensions;
using HomeBudget.Core.Options;
using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.DataAccess.Dapper.SqlClients
{
    internal class SqlConnectionFactory(
        ILogger<SqlConnectionFactory> logger,
        IOptions<DatabaseConnectionOptions> options)
        : ISqlConnectionFactory
    {
        private readonly DatabaseConnectionOptions _databaseConnectionOptions = options.Value;

        public IDbConnection Create()
        {
            try
            {
                return new SqlConnection(_databaseConnectionOptions.ConnectionString);
            }
            catch (Exception ex)
            {
                logger.LogWithExecutionMemberName(
                    $"Failed connect to database with connection string: '{_databaseConnectionOptions.ConnectionString}'. " +
                    $"Error message: '{ex.Message}'",
                    LogLevel.Critical);

                throw;
            }
        }
    }
}
