using System.Reflection;

namespace HomeBudget.Rates.Api.MapperProfileConfigurations
{
    internal static class ApiCallMappingProfiles
    {
        public static Assembly GetExecutingAssembly() => typeof(ApiCallMappingProfiles).Assembly;
    }
}
