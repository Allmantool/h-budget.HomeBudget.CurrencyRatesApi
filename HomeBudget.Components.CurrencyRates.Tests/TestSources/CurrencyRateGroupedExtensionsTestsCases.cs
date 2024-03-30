using HomeBudget.Core.Constants;

namespace HomeBudget.Components.CurrencyRates.Tests.TestSources
{
    internal static class CurrencyRateGroupedExtensionsTestsCases
    {
        public static object[] WithCurrencyGroups =>
        [
            new object[] { NationalBankCurrencyIds.Blr, 1m },
            new object[] { NationalBankCurrencyIds.Usd, 3.2m },
        ];
    }
}
