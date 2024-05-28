using System.Linq;

namespace HomeBudget.Core.Constants
{
    public class NationalBankCurrencies(int id, string name)
        : BaseEnumeration(id, name)
    {
        public static readonly NationalBankCurrencies Blr = new(-1, CurrencyCodes.Blr);
        public static readonly NationalBankCurrencies Usd = new(431, CurrencyCodes.Usd);
        public static readonly NationalBankCurrencies Uan = new(449, CurrencyCodes.Uan);
        public static readonly NationalBankCurrencies Eur = new(451, CurrencyCodes.Eur);
        public static readonly NationalBankCurrencies Pln = new(452, CurrencyCodes.Pln);
        public static readonly NationalBankCurrencies Rub = new(456, CurrencyCodes.Rub);
        public static readonly NationalBankCurrencies Try = new(460, CurrencyCodes.Try);

        public static int? GetIdByAbbreviation(string abbreviation)
            => GetAll<NationalBankCurrencies>().FirstOrDefault(i => string.Equals(abbreviation, i.Name))?.Id;
    }
}
