using System.Collections.Generic;

using FluentAssertions;
using NUnit.Framework;

using HomeBudget.Components.CurrencyRates.Extensions;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Tests.TestSources;
using HomeBudget.Core.Constants;

namespace HomeBudget.Components.CurrencyRates.Tests.Extensions
{
    [TestFixture]
    public class CurrencyRateGroupedExtensionsTests
    {
        [TestCaseSource(typeof(CurrencyRateGroupedExtensionsTestsCases), nameof(CurrencyRateGroupedExtensionsTestsCases.WithCurrencyGroups))]
        public void GetSingleRateValue_WithCurrencyId_ThenExpectedRate(int currencyId, decimal expectedRate)
        {
            var currencyGroups = new List<CurrencyRateGrouped>
            {
                new()
                {
                    Name = "A",
                    CurrencyId = NationalBankCurrencyIds.Usd,
                    RateValues = new List<CurrencyRateValue>
                    {
                         new()
                         {
                             RatePerUnit = expectedRate
                         }
                    }
                }
            };

            var result = currencyGroups.GetSingleRateValue(currencyId);

            result.Payload.Should().Be(expectedRate);
        }
    }
}
