using System;

namespace HomeBudget.Components.CurrencyRates.Tests.TestSources
{
    internal static class DateOnlyJsonConverterTestCases
    {
        public static object[] WithNationalBankApi =>
        [
            new object[] { "17.03.2024 00:00:00", new DateOnly(2024, 3, 17) },
            new object[] { "14/09/2024 00:00:00", new DateOnly(2024, 9, 14) },
            new object[] { "08/26/2024 00:00:00", new DateOnly(2024, 8, 26) },
        ];
    }
}
