using System.Collections.Generic;
using FluentAssertions;
using HomeBudget.Components.CurrencyRates.Extensions;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Tests.TestSources;
using HomeBudget.Core.Constants;
using NUnit.Framework;

namespace HomeBudget.Components.CurrencyRates.Tests.Extensions
{
    [TestFixture]
    internal class CurrencyRateGroupedExtensionsTests
    {
        [TestCaseSource(typeof(CurrencyRateGroupedExtensionsTestsCases), nameof(CurrencyRateGroupedExtensionsTestsCases.WithCurrencyGroups))]
        public void GetSingleRateValue_WithCurrencyId_ThenExpectedRate(string currency, decimal expectedRate)
        {
            var currencyGroups = new List<CurrencyRateGrouped>
            {
                new()
                {
                    Name = "A",
                    Abbreviation = CurrencyCodes.Usd,
                    CurrencyId = NationalBankCurrencies.Usd.Id,
                    RateValues = new List<CurrencyRateValue>
                    {
                         new()
                         {
                             RatePerUnit = expectedRate
                         }
                    }
                }
            };

            var result = currencyGroups.GetSingleRateValue(currency);

            result.Payload.Should().Be(expectedRate);
        }

        [Test]
        public void GetSingleRateValue_WithDynamicCurrencyAbbreviation_ThenExpectedRate()
        {
            var currencyGroups = new List<CurrencyRateGrouped>
            {
                new()
                {
                    Name = "Baht",
                    Abbreviation = "THB",
                    CurrencyId = 468,
                    RateValues = new List<CurrencyRateValue>
                    {
                        new()
                        {
                            RatePerUnit = 0.087425m
                        }
                    }
                }
            };

            var result = currencyGroups.GetSingleRateValue("THB");

            result.Payload.Should().Be(0.087425m);
        }
    }
}
