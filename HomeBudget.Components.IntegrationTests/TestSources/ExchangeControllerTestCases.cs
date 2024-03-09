using HomeBudget.Core.Constants;

namespace HomeBudget.Components.IntegrationTests.TestSources
{
    internal static class ExchangeControllerTestCases
    {
        public static object[] WithUsdCases =>
        [
            new object[] { NationalBankCurrencyIds.BLR, NationalBankCurrencyIds.USD, 314.71m },
            new object[] { NationalBankCurrencyIds.USD, NationalBankCurrencyIds.BLR, 3177.5m },
            new object[] { NationalBankCurrencyIds.USD, NationalBankCurrencyIds.RUB, 90809.07m },
            new object[] { NationalBankCurrencyIds.RUB, NationalBankCurrencyIds.USD, 11.01m },
            new object[] { NationalBankCurrencyIds.USD, NationalBankCurrencyIds.EUR, 898.54m },
            new object[] { NationalBankCurrencyIds.EUR, NationalBankCurrencyIds.USD, 1112.92m },
        ];

        public static object[] WithBlrCases =>
        [
            new object[] { NationalBankCurrencyIds.BLR, NationalBankCurrencyIds.USD, 314.71m },
            new object[] { NationalBankCurrencyIds.USD, NationalBankCurrencyIds.BLR, 3177.5m },
            new object[] { NationalBankCurrencyIds.BLR, NationalBankCurrencyIds.RUB, 28578.78m },
            new object[] { NationalBankCurrencyIds.RUB, NationalBankCurrencyIds.BLR, 34.99m },
            new object[] { NationalBankCurrencyIds.BLR, NationalBankCurrencyIds.EUR, 282.78m },
            new object[] { NationalBankCurrencyIds.EUR, NationalBankCurrencyIds.BLR, 3536.3m },
            new object[] { NationalBankCurrencyIds.BLR, NationalBankCurrencyIds.PLN, 1227.55m },
            new object[] { NationalBankCurrencyIds.PLN, NationalBankCurrencyIds.BLR, 814.63m },
            new object[] { NationalBankCurrencyIds.BLR, NationalBankCurrencyIds.TRY, 9268.7m },
            new object[] { NationalBankCurrencyIds.TRY, NationalBankCurrencyIds.BLR, 107.89m },
            new object[] { NationalBankCurrencyIds.BLR, NationalBankCurrencyIds.UAN, 11826.2m },
            new object[] { NationalBankCurrencyIds.UAN, NationalBankCurrencyIds.BLR, 84.56m },
        ];
    }
}
