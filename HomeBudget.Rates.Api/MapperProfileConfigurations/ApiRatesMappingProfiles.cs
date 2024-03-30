using System.Reflection;

namespace HomeBudget.Rates.Api.MapperProfileConfigurations
{
    internal static class ApiRatesMappingProfiles
    {
        public static Assembly GetExecutingAssembly() => typeof(ApiRatesMappingProfiles).Assembly;
    }
}
