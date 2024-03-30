using System.Reflection;

namespace HomeBudget.Components.Exchange.MapperProfileConfigurations
{
    public static class ExchangeMappingProfiles
    {
        public static Assembly GetExecutingAssembly() => typeof(ExchangeMappingProfiles).Assembly;
    }
}
