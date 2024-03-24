using System.Collections.Generic;
using System.Linq;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.Extensions
{
    public static class CurrencyRateGroupedExtensions
    {
        public static Result<decimal> GetSingleRateValue(
            this IEnumerable<CurrencyRateGrouped> rateGrouped,
            int currencyId)
        {
            if (currencyId == 0)
            {
                return Result<decimal>.Succeeded(1m);
            }

            var originCurrencyForPeriod = rateGrouped.SingleOrDefault(x => x.CurrencyId == currencyId);

            if (originCurrencyForPeriod == null)
            {
                return Result<decimal>.Failure($"'{nameof(CurrencyRateGrouped)}' is null or has more then single value");
            }

            var rateValue = originCurrencyForPeriod.RateValues.SingleOrDefault();

            if (rateValue == null)
            {
                return Result<decimal>.Failure($"'{nameof(CurrencyRateGrouped.RateValues)}' is null or has more then single value");
            }

            return Result<decimal>.Succeeded(rateValue.RatePerUnit);
        }
    }
}
