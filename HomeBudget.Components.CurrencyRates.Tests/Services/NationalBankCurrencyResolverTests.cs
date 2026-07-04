using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using HomeBudget.Components.CurrencyRates.Clients;
using HomeBudget.Components.CurrencyRates.Exceptions;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Services;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace HomeBudget.Components.CurrencyRates.Tests.Services
{
    [TestFixture]
    internal class NationalBankCurrencyResolverTests
    {
        private static readonly DateOnly RequestedDate = new(2026, 7, 4);

        private Mock<INationalBankApiClient> nationalBankApiClientMock;

        [SetUp]
        public void SetUp()
        {
            nationalBankApiClientMock = new Mock<INationalBankApiClient>();
        }

        [Test]
        public async Task ResolveActiveCurrenciesAsync_WhenThbConfiguredById_PreservesCatalogMetadata()
        {
            var sut = CreateSut(
                new ConfigCurrency
                {
                    Id = 468,
                    Abbreviation = "THB",
                    Name = "Baht",
                    Scale = 100,
                    Periodicity = 1
                },
                ThbCatalogCurrency());

            var result = await sut.ResolveActiveCurrenciesAsync(RequestedDate);

            var thb = result.Should().ContainSingle().Subject;

            Assert.Multiple(() =>
            {
                thb.CurrencyId.Should().Be(468);
                thb.Abbreviation.Should().Be("THB");
                thb.Scale.Should().Be(100);
                thb.Periodicity.Should().Be(1);
                thb.ParentId.Should().Be(132);
            });
        }

        [Test]
        public async Task ResolveActiveCurrenciesAsync_WhenThbConfiguredByAbbreviation_ResolvesCurrencyId()
        {
            var sut = CreateSut(
                new ConfigCurrency
                {
                    Abbreviation = "THB"
                },
                ThbCatalogCurrency());

            var result = await sut.ResolveActiveCurrenciesAsync(RequestedDate);

            result.Should().ContainSingle().Subject.CurrencyId.Should().Be(468);
        }

        [Test]
        public async Task ResolveActiveCurrenciesAsync_WhenDailyCurrenciesConfigured_StillResolve()
        {
            var sut = CreateSut(
                new[]
                {
                    new ConfigCurrency { Abbreviation = "USD" },
                    new ConfigCurrency { Abbreviation = "EUR" },
                    new ConfigCurrency { Abbreviation = "RUB" }
                },
                new[]
                {
                    CatalogCurrency(431, "USD", "840", "US Dollar", 1, 0),
                    CatalogCurrency(451, "EUR", "978", "Euro", 1, 0),
                    CatalogCurrency(456, "RUB", "643", "Russian Ruble", 100, 0)
                });

            var result = await sut.ResolveActiveCurrenciesAsync(RequestedDate);

            result.Should().HaveCount(3);
            result.Should().Contain(c => c.Abbreviation == "USD" && c.CurrencyId == 431);
            result.Should().Contain(c => c.Abbreviation == "EUR" && c.CurrencyId == 451);
            result.Should().Contain(c => c.Abbreviation == "RUB" && c.CurrencyId == 456);
        }

        [Test]
        public async Task ResolveActiveCurrenciesAsync_WhenDuplicateConfiguredCurrency_Throws()
        {
            var sut = CreateSut(
                new[]
                {
                    new ConfigCurrency { Abbreviation = "THB" },
                    new ConfigCurrency { Abbreviation = "THB" }
                },
                new[] { ThbCatalogCurrency() });

            var act = async () => await sut.ResolveActiveCurrenciesAsync(RequestedDate);

            await act.Should().ThrowAsync<NationalBankCurrencyResolutionException>()
                .WithMessage("*duplicate abbreviations*");
        }

        [Test]
        public async Task ResolveActiveCurrenciesAsync_WhenCurrencyInactiveForRequestedDate_Throws()
        {
            var sut = CreateSut(
                new ConfigCurrency
                {
                    Abbreviation = "THB"
                },
                ThbCatalogCurrency(dateEnd: new DateTime(2022, 1, 1)));

            var act = async () => await sut.ResolveActiveCurrenciesAsync(RequestedDate);

            await act.Should().ThrowAsync<NationalBankCurrencyResolutionException>()
                .WithMessage("*inactive*");
        }

        [Test]
        public async Task ResolveActiveCurrenciesAsync_WhenAbbreviationHasOverlappingActiveRecords_Throws()
        {
            var sut = CreateSut(
                new ConfigCurrency
                {
                    Abbreviation = "THB"
                },
                ThbCatalogCurrency(),
                CatalogCurrency(999, "THB", "764", "Baht duplicate", 100, 1));

            var act = async () => await sut.ResolveActiveCurrenciesAsync(RequestedDate);

            await act.Should().ThrowAsync<NationalBankCurrencyResolutionException>()
                .WithMessage("*ambiguous*");
        }

        [Test]
        public async Task ResolveActiveCurrenciesAsync_WhenConfiguredIdBelongsToDifferentAbbreviation_Throws()
        {
            var sut = CreateSut(
                new ConfigCurrency
                {
                    Id = 468,
                    Abbreviation = "USD"
                },
                ThbCatalogCurrency());

            var act = async () => await sut.ResolveActiveCurrenciesAsync(RequestedDate);

            await act.Should().ThrowAsync<NationalBankCurrencyResolutionException>()
                .WithMessage("*belongs to 'THB'*");
        }

        private NationalBankCurrencyResolver CreateSut(
            ConfigCurrency configuredCurrency,
            params NationalBankCurrency[] catalog)
            => CreateSut(new[] { configuredCurrency }, catalog);

        private NationalBankCurrencyResolver CreateSut(
            IReadOnlyCollection<ConfigCurrency> configuredCurrencies,
            IReadOnlyCollection<NationalBankCurrency> catalog)
        {
            nationalBankApiClientMock
                .Setup(c => c.GetCurrenciesAsync(default))
                .ReturnsAsync(catalog);

            return new NationalBankCurrencyResolver(
                new ConfigSettings
                {
                    ActiveNationalBankCurrencies = configuredCurrencies
                },
                nationalBankApiClientMock.Object,
                Mock.Of<ILogger<NationalBankCurrencyResolver>>());
        }

        private static NationalBankCurrency ThbCatalogCurrency(DateTime? dateEnd = null)
            => CatalogCurrency(468, "THB", "764", "Baht", 100, 1, 132, dateEnd);

        private static NationalBankCurrency CatalogCurrency(
            int currencyId,
            string abbreviation,
            string code,
            string englishName,
            int scale,
            int periodicity,
            int? parentId = null,
            DateTime? dateEnd = null)
            => new()
            {
                CurrencyId = currencyId,
                ParentId = parentId,
                Code = code,
                Abbreviation = abbreviation,
                Name = englishName,
                EnglishName = englishName,
                Scale = scale,
                Periodicity = periodicity,
                DateStart = new DateTime(2021, 7, 9),
                DateEnd = dateEnd ?? new DateTime(2050, 1, 1)
            };
    }
}
