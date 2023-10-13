using System.Collections.Generic;
using System.Linq;

using AutoMapper;

using HomeBudget.Components.CurrencyRates.Models;

namespace HomeBudget.Components.CurrencyRates.Extensions
{
    internal static class CurrencyRateExtensions
    {
        public static IReadOnlyCollection<CurrencyRateGrouped> MapToCurrencyRateGrouped(
            this IEnumerable<CurrencyRate> rates, IMapper mapper)
        {
            return rates
                .OrderBy(r => r.UpdateDate)
                .GroupBy(rate => new
                {
                    rate.CurrencyId,
                    rate.Abbreviation,
                    rate.Scale,
                    rate.Name
                })
                .Select(mapper.Map<IEnumerable<CurrencyRate>, CurrencyRateGrouped>)
                .ToList();
        }

        public static void EnrichWithRateGroupInfo(this CurrencyRate rate, CurrencyRateGrouped configInfo)
        {
            rate.Abbreviation = configInfo.Abbreviation;
            rate.Name = configInfo.Name;
            rate.Scale = configInfo.Scale;
            rate.RatePerUnit = rate.OfficialRate / configInfo.Scale;
        }
    }
}
