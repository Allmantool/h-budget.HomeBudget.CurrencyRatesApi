using HomeBudget.Core.Constants;

namespace HomeBudget.Components.IntegrationTests.TestSources
{
    internal static class ExchangeControllerTestCases
    {
        public static object[] WithUsdCases =>
        [
            new object[] { NationalBankCurrencyIds.Blr, NationalBankCurrencyIds.Usd, 314.71m },
            new object[] { NationalBankCurrencyIds.Usd, NationalBankCurrencyIds.Blr, 3177.5m },
            new object[] { NationalBankCurrencyIds.Usd, NationalBankCurrencyIds.Rub, 90809.07m },
            new object[] { NationalBankCurrencyIds.Rub, NationalBankCurrencyIds.Usd, 11.01m },
            new object[] { NationalBankCurrencyIds.Usd, NationalBankCurrencyIds.Eur, 898.54m },
            new object[] { NationalBankCurrencyIds.Eur, NationalBankCurrencyIds.Usd, 1112.92m },
        ];

        public static object[] WithBlrCases =>
        [
            new object[] { NationalBankCurrencyIds.Blr, NationalBankCurrencyIds.Usd, 314.71m },
            new object[] { NationalBankCurrencyIds.Usd, NationalBankCurrencyIds.Blr, 3177.5m },
            new object[] { NationalBankCurrencyIds.Blr, NationalBankCurrencyIds.Rub, 28578.78m },
            new object[] { NationalBankCurrencyIds.Rub, NationalBankCurrencyIds.Blr, 34.99m },
            new object[] { NationalBankCurrencyIds.Blr, NationalBankCurrencyIds.Eur, 282.78m },
            new object[] { NationalBankCurrencyIds.Eur, NationalBankCurrencyIds.Blr, 3536.3m },
            new object[] { NationalBankCurrencyIds.Blr, NationalBankCurrencyIds.Pln, 1227.55m },
            new object[] { NationalBankCurrencyIds.Pln, NationalBankCurrencyIds.Blr, 814.63m },
            new object[] { NationalBankCurrencyIds.Blr, NationalBankCurrencyIds.Try, 9268.7m },
            new object[] { NationalBankCurrencyIds.Try, NationalBankCurrencyIds.Blr, 107.89m },
            new object[] { NationalBankCurrencyIds.Blr, NationalBankCurrencyIds.Uan, 11826.2m },
            new object[] { NationalBankCurrencyIds.Uan, NationalBankCurrencyIds.Blr, 84.56m },
        ];
    }
}
