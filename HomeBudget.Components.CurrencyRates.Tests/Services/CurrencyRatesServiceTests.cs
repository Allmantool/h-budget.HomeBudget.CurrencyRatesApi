using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using HomeBudget.Components.CurrencyRates.Clients;
using HomeBudget.Components.CurrencyRates.CQRS.Commands.Models;
using HomeBudget.Components.CurrencyRates.Extensions;
using HomeBudget.Components.CurrencyRates.MapperProfileConfigurations;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Providers;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Components.CurrencyRates.Services;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Limiters;
using HomeBudget.Core.Models;
using HomeBudget.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace HomeBudget.Components.CurrencyRates.Tests.Services
{
    [TestFixture]
    internal class CurrencyRatesServiceTests
    {
        private readonly Mock<INationalBankApiClient> _nationalBankApiClientMock = new();
        private readonly Mock<ICurrencyRatesReadProvider> _currencyRatesReadProviderMock = new();
        private readonly Mock<ICurrencyRatesWriteProvider> _currencyRatesWriteProviderMock = new();
        private readonly Mock<INationalBankRatesProvider> _nationalBankRatesProviderMock = new();
        private readonly Mock<IHttpClientRateLimiter> _httpClientRateLimiterMock = new();

        private CurrencyRatesService _sut;

        [SetUp]
        public void Setup()
        {
            _nationalBankApiClientMock
                .Setup(i => i.GetTodayRatesAsync())
                .ReturnsAsync(new[]
                {
                    new NationalBankCurrencyRate
                    {
                        CurrencyId = 1,
                        Abbreviation = "Abb-A",
                        Name = "Name-A",
                        OfficialRate = 1.2m,
                        Scale = 1
                    },
                    new NationalBankCurrencyRate
                    {
                        CurrencyId = 2,
                        Abbreviation = "Abb-B",
                        Name = "Name-B",
                        OfficialRate = 1.6m,
                        Scale = 1
                    },
                });

            _currencyRatesWriteProviderMock
                .Setup(i => i.UpsertRatesWithSaveAsync(It.IsAny<IReadOnlyCollection<CurrencyRate>>()))
                .ReturnsAsync(0);

            _sut = new CurrencyRatesService(
                GetDefaultMapper(),
                _currencyRatesReadProviderMock.Object,
                _currencyRatesWriteProviderMock.Object,
                _nationalBankRatesProviderMock.Object);

            _currencyRatesWriteProviderMock.Invocations.Clear();
        }

        [Test]
        public async Task GetRatesForPeriodAsync_WhenPerformSeveralApiCallsForCurrencies_ResultExpectedRatesCount()
        {
            const int expectedRatesCount = 4;

            var testStartDate = new DateOnly(2021, 3, 2);
            var testEndDate = new DateOnly(2021, 8, 2);

            using var lease = new TestRateLimitLease(isAcquired: true);

            _httpClientRateLimiterMock
                .Setup(x => x.AcquireAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lease);

            var currencyResolverMock = new Mock<INationalBankCurrencyResolver>();

            currencyResolverMock
                .Setup(r => r.ResolveActiveCurrenciesAsync(testEndDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new NationalBankCurrencyDefinition(
                        1,
                        null,
                        "001",
                        "Abb-A",
                        "Name-A",
                        "Name-A",
                        1,
                        0,
                        new DateOnly(2000, 1, 1),
                        new DateOnly(2050, 1, 1)),
                    new NationalBankCurrencyDefinition(
                        2,
                        null,
                        "002",
                        "Abb-B",
                        "Name-B",
                        "Name-B",
                        1,
                        0,
                        new DateOnly(2000, 1, 1),
                        new DateOnly(2050, 1, 1))
                });

            _nationalBankApiClientMock
                .Setup(i => i.GetRatesForPeriodAsync(
                    1,
                    testStartDate.ToString(DateFormats.NationalBankApiRequest, CultureInfo.InvariantCulture),
                    testEndDate.ToString(DateFormats.NationalBankApiRequest, CultureInfo.InvariantCulture),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NationalBankShortCurrencyRate>
                {
                    new ()
                    {
                        CurrencyId = 1,
                        OfficialRate = 2.1m
                    },
                    new ()
                    {
                        CurrencyId = 1,
                        OfficialRate = 4.1m
                    },
                });

            _nationalBankApiClientMock
                .Setup(i => i.GetRatesForPeriodAsync(
                    2,
                    testStartDate.ToString(DateFormats.NationalBankApiRequest, CultureInfo.InvariantCulture),
                    testEndDate.ToString(DateFormats.NationalBankApiRequest, CultureInfo.InvariantCulture),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NationalBankShortCurrencyRate>
                {
                    new ()
                    {
                        CurrencyId = 2,
                        OfficialRate = 2.1m
                    },
                    new ()
                    {
                        CurrencyId = 2,
                        OfficialRate = 4.1m
                    },
                });

            _currencyRatesReadProviderMock
                .Setup(i => i.GetRatesForPeriodAsync(testStartDate, testEndDate))
                .ReturnsAsync(new List<CurrencyRate>
                {
                    CreateRate(1, "Abb-A", "Name-A", 1, 2.1m, testStartDate),
                    CreateRate(1, "Abb-A", "Name-A", 1, 4.1m, testEndDate),
                    CreateRate(2, "Abb-B", "Name-B", 1, 2.1m, testStartDate),
                    CreateRate(2, "Abb-B", "Name-B", 1, 4.1m, testEndDate),
                });

            _sut = new CurrencyRatesService(
                GetDefaultMapper(),
                _currencyRatesReadProviderMock.Object,
                _currencyRatesWriteProviderMock.Object,
                new NationalBankRatesProvider(
                    Mock.Of<ILogger<NationalBankRatesProvider>>(),
                    Options.Create(new HttpClientOptions()),
                    _nationalBankApiClientMock.Object,
                    _httpClientRateLimiterMock.Object,
                    currencyResolverMock.Object));

            var rates = await _sut.GetRatesForPeriodAsync(testStartDate, testEndDate);

            expectedRatesCount.Should().Be(rates.Payload.SelectMany(i => i.RateValues).Count());
        }

        [TestCase(462, "CNY", "Yuan Renminbi", "Китайский юань")]
        [TestCase(468, "THB", "Baht", "Таиландский бат")]
        public async Task GetRatesForPeriodAsync_WhenUpstreamNameDiffersFromDatabaseName_ReturnsDatabaseName(
            int currencyId,
            string abbreviation,
            string upstreamName,
            string databaseName)
        {
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 2);

            _nationalBankRatesProviderMock
                .Setup(i => i.GetActiveCurrenciesAsync(endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    CreateDefinition(currencyId, abbreviation, upstreamName, 10)
                });

            _nationalBankRatesProviderMock
                .Setup(i => i.GetRatesForPeriodAsync(
                    It.Is<IEnumerable<int>>(ids => ids.Single() == currencyId),
                    It.Is<PeriodRange>(period => period.StartDate == startDate && period.EndDate == endDate),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new NationalBankShortCurrencyRate
                    {
                        CurrencyId = currencyId,
                        OfficialRate = 4.2m,
                        UpdateDate = startDate
                    }
                });

            _currencyRatesReadProviderMock
                .Setup(i => i.GetRatesForPeriodAsync(startDate, endDate))
                .ReturnsAsync(new[]
                {
                    CreateRate(currencyId, abbreviation, databaseName, 10, 4.2m, startDate)
                });

            var result = await _sut.GetRatesForPeriodAsync(startDate, endDate);

            result.Payload.Should().ContainSingle()
                .Which.Name.Should().Be(databaseName);
            result.Payload.Should().NotContain(rate => rate.Name == upstreamName);
        }

        [Test]
        public async Task GetRatesForPeriodAsync_WhenConfiguredCurrenciesIncludeKnownCatalog_ReturnsDatabaseNames()
        {
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 1);
            var expectedNames = new Dictionary<string, string>
            {
                ["USD"] = "Доллар США",
                ["RUB"] = "Российских рублей",
                ["EUR"] = "Евро",
                ["UAH"] = "Гривен",
                ["PLN"] = "Злотых",
                ["TRY"] = "Турецких лир",
                ["CNY"] = "Китайский юань",
                ["THB"] = "Таиландский бат"
            };

            var definitions = expectedNames
                .Select((entry, index) => CreateDefinition(431 + index, entry.Key, $"Upstream {entry.Key}", 1))
                .ToList();

            _nationalBankRatesProviderMock
                .Setup(i => i.GetActiveCurrenciesAsync(endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(definitions);

            _nationalBankRatesProviderMock
                .Setup(i => i.GetRatesForPeriodAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<PeriodRange>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(definitions.Select(definition => new NationalBankShortCurrencyRate
                {
                    CurrencyId = definition.CurrencyId,
                    OfficialRate = 3.1m,
                    UpdateDate = startDate
                }).ToList());

            _currencyRatesReadProviderMock
                .Setup(i => i.GetRatesForPeriodAsync(startDate, endDate))
                .ReturnsAsync(definitions.Select(definition =>
                    CreateRate(
                        definition.CurrencyId,
                        definition.Abbreviation,
                        expectedNames[definition.Abbreviation],
                        definition.Scale,
                        3.1m,
                        startDate)).ToList());

            var result = await _sut.GetRatesForPeriodAsync(startDate, endDate);

            result.Payload.Should().HaveCount(expectedNames.Count);
            result.Payload.Should().OnlyContain(rate => rate.Name == expectedNames[rate.Abbreviation]);
        }

        [Test]
        public async Task SaveWithRewriteAsync_WhenDatabaseCountMatchesButKeysDiffer_StillUpsertsFetchedRates()
        {
            var fetchedRate = CreateRate(462, "CNY", "Yuan Renminbi", 10, 4.2m, new DateOnly(2024, 1, 1));
            var existingDifferentRate = CreateRate(468, "THB", "Baht", 10, 9.7m, new DateOnly(2024, 1, 2));

            await _sut.SaveWithRewriteAsync(
                new SaveCurrencyRatesCommand(
                    new[] { fetchedRate },
                    new[] { existingDifferentRate }));

            _currencyRatesWriteProviderMock.Verify(
                i => i.UpsertRatesWithSaveAsync(It.Is<IReadOnlyCollection<CurrencyRate>>(rates =>
                    rates.Single().CurrencyId == fetchedRate.CurrencyId
                    && rates.Single().UpdateDate == fetchedRate.UpdateDate)),
                Times.Once);
        }

        [Test]
        public async Task GetRatesForPeriodAsync_WhenProviderReturnsStartAndEndDateRates_PersistsBothBoundaries()
        {
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 3);

            _nationalBankRatesProviderMock
                .Setup(i => i.GetActiveCurrenciesAsync(endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { CreateDefinition(431, "USD", "US Dollar", 1) });

            _nationalBankRatesProviderMock
                .Setup(i => i.GetRatesForPeriodAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<PeriodRange>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new NationalBankShortCurrencyRate { CurrencyId = 431, OfficialRate = 3.1m, UpdateDate = startDate },
                    new NationalBankShortCurrencyRate { CurrencyId = 431, OfficialRate = 3.2m, UpdateDate = endDate }
                });

            _currencyRatesReadProviderMock
                .Setup(i => i.GetRatesForPeriodAsync(startDate, endDate))
                .ReturnsAsync(new[]
                {
                    CreateRate(431, "USD", "Доллар США", 1, 3.1m, startDate),
                    CreateRate(431, "USD", "Доллар США", 1, 3.2m, endDate)
                });

            await _sut.GetRatesForPeriodAsync(startDate, endDate);

            _currencyRatesWriteProviderMock.Verify(
                i => i.UpsertRatesWithSaveAsync(It.Is<IReadOnlyCollection<CurrencyRate>>(rates =>
                    rates.Any(rate => rate.UpdateDate == startDate)
                    && rates.Any(rate => rate.UpdateDate == endDate))),
                Times.Once);
        }

        [Test]
        public async Task GetRatesForPeriodAsync_WhenProviderOmitsOneCurrency_PersistsAvailableRows()
        {
            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 1);

            _nationalBankRatesProviderMock
                .Setup(i => i.GetActiveCurrenciesAsync(endDate, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    CreateDefinition(431, "USD", "US Dollar", 1),
                    CreateDefinition(451, "EUR", "Euro", 1)
                });

            _nationalBankRatesProviderMock
                .Setup(i => i.GetRatesForPeriodAsync(
                    It.IsAny<IEnumerable<int>>(),
                    It.IsAny<PeriodRange>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new NationalBankShortCurrencyRate
                    {
                        CurrencyId = 431,
                        OfficialRate = 3.1m,
                        UpdateDate = startDate
                    }
                });

            _currencyRatesReadProviderMock
                .Setup(i => i.GetRatesForPeriodAsync(startDate, endDate))
                .ReturnsAsync(new[]
                {
                    CreateRate(431, "USD", "Доллар США", 1, 3.1m, startDate)
                });

            var result = await _sut.GetRatesForPeriodAsync(startDate, endDate);

            result.Payload.Should().ContainSingle(rate => rate.Abbreviation == "USD");
            _currencyRatesWriteProviderMock.Verify(
                i => i.UpsertRatesWithSaveAsync(It.Is<IReadOnlyCollection<CurrencyRate>>(rates =>
                    rates.Count == 1 && rates.Single().CurrencyId == 431)),
                Times.Once);
        }

        [Test]
        public void CurrencyRatesGroup_WhenMapToCurrencyRateGroupedFromCurrencyRates_ReturnsExpectedRatesGroupCount()
        {
            var rates = new[]
            {
                new CurrencyRate
                {
                    CurrencyId = 1,
                    Scale = 1,
                    Name = "Name_A",
                    Abbreviation = "Currency_Code_A",
                    OfficialRate = 1.1m,
                    RatePerUnit = 1.1m,
                    UpdateDate = new DateOnly(2022, 7, 1)
                },
                new CurrencyRate
                {
                    CurrencyId = 1,
                    Scale = 1,
                    Name = "Name_A",
                    Abbreviation = "Currency_Code_A",
                    OfficialRate = 1.3m,
                    RatePerUnit = 1.3m,
                    UpdateDate = new DateOnly(2022, 7, 2)
                },
                new CurrencyRate
                {
                    CurrencyId = 2,
                    Scale = 1,
                    Name = "Name_B",
                    Abbreviation = "Currency_Code_B",
                    OfficialRate = 2.1m,
                    RatePerUnit = 2.1m,
                    UpdateDate = new DateOnly(2022, 7, 1)
                },
                new CurrencyRate
                {
                    CurrencyId = 2,
                    Scale = 1,
                    Name = "Name_B",
                    Abbreviation = "Currency_Code_B",
                    OfficialRate = 2.2m,
                    RatePerUnit = 3.1m,
                    UpdateDate = new DateOnly(2022, 7, 2)
                },
            };

            var mapper = GetDefaultMapper();

            var currencyRatesGroups = rates.MapToCurrencyRateGrouped(mapper);

            currencyRatesGroups.Count.Should().Be(2);
        }

        private static IMapper GetDefaultMapper()
        {
            var configurationExpression = new MapperConfigurationExpression();
            configurationExpression.AddProfile<CurrencyRateMappingProfiler>();
            configurationExpression.AddProfile<CurrencyRateGroupedMappingProfile>();

            var mapperConfiguration = new MapperConfiguration(configurationExpression, NullLoggerFactory.Instance);

            return new Mapper(mapperConfiguration);
        }

        private static NationalBankCurrencyDefinition CreateDefinition(
            int currencyId,
            string abbreviation,
            string name,
            int scale)
            => new(
                currencyId,
                null,
                currencyId.ToString(CultureInfo.InvariantCulture),
                abbreviation,
                name,
                name,
                scale,
                0,
                new DateOnly(2000, 1, 1),
                new DateOnly(2050, 1, 1));

        private static CurrencyRate CreateRate(
            int currencyId,
            string abbreviation,
            string name,
            int scale,
            decimal officialRate,
            DateOnly updateDate)
            => new()
            {
                CurrencyId = currencyId,
                Abbreviation = abbreviation,
                Name = name,
                Scale = scale,
                OfficialRate = officialRate,
                RatePerUnit = officialRate / scale,
                UpdateDate = updateDate
            };
    }
}
