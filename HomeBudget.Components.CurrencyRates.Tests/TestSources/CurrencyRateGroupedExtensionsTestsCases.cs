using HomeBudget.Core.Constants;

namespace HomeBudget.Components.CurrencyRates.Tests.TestSources
{
    internal static class CurrencyRateGroupedExtensionsTestsCases
    {
        public static object[] WithCurrencyGroups =>
        [
            new object[] { CurrencyCodes.Blr, 1m },
            new object[] { CurrencyCodes.Usd, 3.2m },
        ];
    }
}
