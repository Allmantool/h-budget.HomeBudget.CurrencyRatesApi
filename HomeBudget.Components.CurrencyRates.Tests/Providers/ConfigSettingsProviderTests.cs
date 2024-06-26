﻿using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;
using Moq;
using NUnit.Framework;

using HomeBudget.Components.CurrencyRates.Providers;
using HomeBudget.Core.Constants;
using HomeBudget.DataAccess.Interfaces;

namespace HomeBudget.Components.CurrencyRates.Tests.Providers
{
    [TestFixture]
    public class ConfigSettingsProviderTests
    {
        private ConfigSettingsProvider _sut;

        private Mock<IBaseReadRepository> _mockBaseReadRepository;

        [SetUp]
        public void SetUp()
        {
            _mockBaseReadRepository = new Mock<IBaseReadRepository>();
        }

        [Test]
        public async Task GetDefaultSettingsAsync_WhenExpectedConfigAsJson_ThenParseWithoutError()
        {
            const string configAsJson = "{" +
                                            "\"ActiveNationalBankCurrencies\": " +
                                            "[" +
                                                "{" +
                                                    "\"Abbreviation\": \"USD\"," +
                                                    "\"Id\": 431," +
                                                    "\"Name\": \"US Dollar\"," +
                                                    "\"Scale\": 1" +
                                                "}" +
                                            "]" +
                                        "}";

            _mockBaseReadRepository
                .Setup(r => r.SingleAsync<string>(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(configAsJson);

            _sut = new ConfigSettingsProvider(null, _mockBaseReadRepository.Object);

            var result = await _sut.GetDefaultSettingsAsync();

            var targetCurrency = result.ActiveNationalBankCurrencies.First();

            Assert.Multiple(() =>
            {
                targetCurrency.Abbreviation.Should().BeEquivalentTo(nameof(NationalBankCurrencies.Usd));
                targetCurrency.Id.Should().Be(NationalBankCurrencies.Usd.Id);
                targetCurrency.Name.Should().Be("US Dollar");
                targetCurrency.Scale.Should().Be(1);
            });
        }
    }
}
