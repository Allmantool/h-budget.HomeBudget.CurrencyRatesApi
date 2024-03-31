using HomeBudget.Core.Constants;

namespace HomeBudget.Components.IntegrationTests.TestSources
{
    internal static class ExchangeControllerTestCases
    {
        public static object[] WithUsdCases =>
        [
            new object[] { CurrencyCodes.Blr, CurrencyCodes.Usd, 314.71m },
            new object[] { CurrencyCodes.Usd, CurrencyCodes.Blr, 3177.5m },
            new object[] { CurrencyCodes.Usd, CurrencyCodes.Rub, 90809.07m },
            new object[] { CurrencyCodes.Rub, CurrencyCodes.Usd, 11.01m },
            new object[] { CurrencyCodes.Usd, CurrencyCodes.Eur, 898.54m },
            new object[] { CurrencyCodes.Eur, CurrencyCodes.Usd, 1112.92m },
        ];

        public static object[] WithBlrCases =>
        [
            new object[] { CurrencyCodes.Blr, CurrencyCodes.Usd, 314.71m },
            new object[] { CurrencyCodes.Usd, CurrencyCodes.Blr, 3177.5m },
            new object[] { CurrencyCodes.Blr, CurrencyCodes.Rub, 28578.78m },
            new object[] { CurrencyCodes.Rub, CurrencyCodes.Blr, 34.99m },
            new object[] { CurrencyCodes.Blr, CurrencyCodes.Eur, 282.78m },
            new object[] { CurrencyCodes.Eur, CurrencyCodes.Blr, 3536.3m },
            new object[] { CurrencyCodes.Blr, CurrencyCodes.Pln, 1227.55m },
            new object[] { CurrencyCodes.Pln, CurrencyCodes.Blr, 814.63m },
            new object[] { CurrencyCodes.Blr, CurrencyCodes.Try, 9268.7m },
            new object[] { CurrencyCodes.Try, CurrencyCodes.Blr, 107.89m },
            new object[] { CurrencyCodes.Blr, CurrencyCodes.Uan, 11826.2m },
            new object[] { CurrencyCodes.Uan, CurrencyCodes.Blr, 84.56m },
        ];
    }
}
