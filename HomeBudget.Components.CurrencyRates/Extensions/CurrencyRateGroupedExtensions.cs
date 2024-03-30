using System.Collections.Generic;
using System.Linq;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Extensions;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.Extensions
{
    public static class CurrencyRateGroupedExtensions
    {
        public static Result<decimal> GetSingleRateValue(
            this IEnumerable<CurrencyRateGrouped> rateGrouped,
            int currencyId)
        {
            if (currencyId == NationalBankCurrencyIds.Blr)
            {
                return Result<decimal>.Succeeded(1m);
            }

            var originCurrencyForPeriod = rateGrouped.Where(x => x.CurrencyId == currencyId).ToArray();

            if (originCurrencyForPeriod.IsNullOrEmpty() || originCurrencyForPeriod.Length > 1)
            {
                return Result<decimal>.Failure($"'{nameof(CurrencyRateGrouped)}' hasn't been found or has more then one entry for target currency id '{currencyId}'");
            }

            var currencyGroup = originCurrencyForPeriod.Single();
            var rateValues = currencyGroup.RateValues.ToArray();

            if (rateValues.IsNullOrEmpty() || rateValues.Length > 1)
            {
                return Result<decimal>.Failure($"'{nameof(CurrencyRateGrouped.RateValues)}' hasn't been found or has more then one entry with currency group '{currencyGroup.Abbreviation}'");
            }

            var rate = rateValues.Single();

            return Result<decimal>.Succeeded(rate.RatePerUnit);
        }
    }
}
