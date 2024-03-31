using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;

using Moq;
using NUnit.Framework;

using HomeBudget.Components.CurrencyRates.Clients;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Providers;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Extensions;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.CurrencyRates.Tests.Providers
{
    [TestFixture]
    public class NationalBankRatesProviderTests
    {
        private const int CurrencyTypeBId = 272;

        private PeriodRange _defaultPeriod;

        private NationalBankRatesProvider _sut;

        private Mock<INationalBankApiClient> _mockNationalBankApiClient;

        [SetUp]
        public void SetUp()
        {
            _defaultPeriod = new PeriodRange
            {
                StartDate = new DateOnly(2020, 04, 01),
                EndDate = new DateOnly(2023, 08, 01)
            };

            _mockNationalBankApiClient = new Mock<INationalBankApiClient>();

            _mockNationalBankApiClient
                .Setup(cl => cl.GetRatesForPeriodAsync(
                    NationalBankCurrencies.Usd,
                    _defaultPeriod.StartDate.ToString(DateFormats.NationalBankApiRequest),
                    _defaultPeriod.StartDate.LastDateOfYear().ToString(DateFormats.NationalBankApiRequest)))
                .ReturnsAsync(() => new List<NationalBankShortCurrencyRate>
                {
                    new()
                    {
                        CurrencyId = NationalBankCurrencies.Usd,
                        OfficialRate = 1
                    },
                });

            _mockNationalBankApiClient
                .Setup(cl => cl.GetRatesForPeriodAsync(
                    CurrencyTypeBId,
                    _defaultPeriod.EndDate.FirstDateOfYear().ToString(DateFormats.NationalBankApiRequest),
                    _defaultPeriod.EndDate.ToString(DateFormats.NationalBankApiRequest)))
                .ReturnsAsync(() => new List<NationalBankShortCurrencyRate>
                {
                    new()
                    {
                        CurrencyId = CurrencyTypeBId,
                        OfficialRate = 6
                    },
                    new()
                    {
                        CurrencyId = CurrencyTypeBId,
                        OfficialRate = 4
                    },
                });
        }

        [TestCase]
        public async Task GetRatesForPeriodAsync_WhenRequestDataForRangeMoreThenYear_ReturnsExpectedAmountOfRates()
        {
            var configSettings = new ConfigSettings
            {
                ActiveNationalBankCurrencies = new[]
                {
                    new ConfigCurrency
                    {
                        Abbreviation = "test-abbreviation",
                        Id = 2,
                        Name = "test-abbreviation",
                        Scale = 32
                    }
                }
            };

            _sut = new NationalBankRatesProvider(configSettings, _mockNationalBankApiClient.Object);

            var result = await _sut.GetRatesForPeriodAsync(
                new[] { NationalBankCurrencies.Usd, CurrencyTypeBId },
                _defaultPeriod);

            result.Count.Should().Be(3);
        }
    }
}
