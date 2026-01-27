using System.Data;

namespace HomeBudget.DataAccess.Interfaces
{
    public interface ISqlConnectionFactory
    {
        IDbConnection Create(string databasename);
    }
}
