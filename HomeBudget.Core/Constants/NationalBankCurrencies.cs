using System.Collections.Generic;

namespace HomeBudget.Core.Constants
{
    public static class NationalBankCurrencies
    {
        private static readonly Dictionary<string, int> CurrencyHandbook;

        static NationalBankCurrencies()
        {
            CurrencyHandbook = new()
            {
                { CurrencyCodes.Blr, Blr },
                { CurrencyCodes.Usd, Usd },
                { CurrencyCodes.Rub, Rub },
                { CurrencyCodes.Pln, Pln },
                { CurrencyCodes.Eur, Eur },
                { CurrencyCodes.Uan, Uan },
                { CurrencyCodes.Try, Try },
            };
        }

        public static readonly int Blr = -1;

        public static readonly int Usd = 431;

        public static readonly int Rub = 456;

        public static readonly int Pln = 452;

        public static readonly int Eur = 451;

        public static readonly int Uan = 449;

        public static readonly int Try = 460;

        public static int? GetIdByAbbreviation(string abbreviation) => CurrencyHandbook.GetValueOrDefault(abbreviation);
    }
}
