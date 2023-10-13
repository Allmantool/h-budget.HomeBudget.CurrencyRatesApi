using Microsoft.Extensions.DependencyInjection;

using HomeBudget.DataAccess.Dapper.SqlClients.MsSql;
using HomeBudget.DataAccess.Interfaces;
using HomeBudget.DataAccess.Dapper.SqlClients;

namespace HomeBudget.DataAccess.Dapper.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegistryDapperIoCDependencies(this IServiceCollection services)
            => services
                .AddScoped<ISqlConnectionFactory, SqlConnectionFactory>()
                .AddScoped<IBaseWriteRepository, DapperWriteRepository>()
                .AddScoped<IBaseReadRepository, DapperReadRepository>();
    }
}
