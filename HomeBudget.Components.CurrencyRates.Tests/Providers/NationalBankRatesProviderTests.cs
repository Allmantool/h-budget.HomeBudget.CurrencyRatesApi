using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using HomeBudget.Components.CurrencyRates.Clients;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Providers;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Limiters;
using HomeBudget.Core.Models;
using HomeBudget.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Refit;

namespace HomeBudget.Components.CurrencyRates.Tests.Providers
{
    [TestFixture]
    internal sealed class NationalBankRatesProviderTests : IDisposable
    {
        private readonly Mock<IHttpClientRateLimiter> httpClientRateLimiterMock = new();

        private NationalBankRatesProvider sut;
        private TestRateLimitLease rateLimitLease;
        private bool disposed;

        private Mock<INationalBankApiClient> nationalBankApiClientMock;
        private Mock<INationalBankCurrencyResolver> currencyResolverMock;

        [SetUp]
        public void SetUp()
        {
            rateLimitLease?.Dispose();

            nationalBankApiClientMock = new Mock<INationalBankApiClient>();
            currencyResolverMock = new Mock<INationalBankCurrencyResolver>();

            rateLimitLease = new TestRateLimitLease(isAcquired: true);

            httpClientRateLimiterMock
                .Setup(x => x.AcquireAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(rateLimitLease);

            sut = new NationalBankRatesProvider(
                Mock.Of<ILogger<NationalBankRatesProvider>>(),
                Options.Create(new HttpClientOptions()),
                nationalBankApiClientMock.Object,
                httpClientRateLimiterMock.Object,
                currencyResolverMock.Object);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                rateLimitLease?.Dispose();
            }

            disposed = true;
        }

        [Test]
        public async Task GetRatesForPeriodAsync_WhenRangeExceedsYear_SplitsRequestsInto365DayChunks()
        {
            var requestedRanges = new List<PeriodRange>();
            var period = new PeriodRange
            {
                StartDate = new DateOnly(2020, 1, 1),
                EndDate = new DateOnly(2021, 1, 1)
            };

            nationalBankApiClientMock
                .Setup(cl => cl.GetRatesForPeriodAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Callback<int, string, string, CancellationToken>((_, startDate, endDate, _) =>
                {
                    requestedRanges.Add(new PeriodRange
                    {
                        StartDate = DateOnly.ParseExact(startDate, DateFormats.NationalBankApiRequest, CultureInfo.InvariantCulture),
                        EndDate = DateOnly.ParseExact(endDate, DateFormats.NationalBankApiRequest, CultureInfo.InvariantCulture)
                    });
                })
                .ReturnsAsync((int currencyId, string _, string _, CancellationToken _) => new List<NationalBankShortCurrencyRate>
                {
                    new()
                    {
                        CurrencyId = currencyId,
                        OfficialRate = 1m
                    }
                });

            var result = await sut.GetRatesForPeriodAsync([NationalBankCurrencies.Usd.Id], period);

            result.Count.Should().Be(2);
            requestedRanges.Should().OnlyContain(r => r.EndDate.DayNumber - r.StartDate.DayNumber + 1 <= 365);
        }

        [Test]
        public async Task GetTodayActiveRatesAsync_WhenThbConfiguredByCatalogDefinition_FetchesRateByCurrencyId()
        {
            const int thbCurrencyId = 468;

            currencyResolverMock
                .Setup(r => r.ResolveActiveCurrenciesAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new NationalBankCurrencyDefinition(
                        thbCurrencyId,
                        132,
                        "764",
                        "THB",
                        "Baht",
                        "Baht",
                        100,
                        1,
                        new DateOnly(2021, 7, 9),
                        new DateOnly(2050, 1, 1))
                });

            nationalBankApiClientMock
                .Setup(cl => cl.GetRateAsync(
                    thbCurrencyId,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NationalBankCurrencyRate
                {
                    CurrencyId = thbCurrencyId,
                    Abbreviation = "THB",
                    Name = "Батов",
                    Scale = 100,
                    OfficialRate = 8.7425m,
                    UpdateDate = new DateTime(2026, 7, 4)
                });

            var result = await sut.GetTodayActiveRatesAsync();

            var thbRate = result.Single();

            Assert.Multiple(() =>
            {
                thbRate.CurrencyId.Should().Be(thbCurrencyId);
                thbRate.Abbreviation.Should().Be("THB");
                thbRate.Scale.Should().Be(100);
                thbRate.OfficialRate.Should().Be(8.7425m);
            });
        }

        [Test]
        public async Task GetTodayActiveRatesAsync_WhenRateIsUnavailable_DoesNotReturnZeroRate()
        {
            currencyResolverMock
                .Setup(r => r.ResolveActiveCurrenciesAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new NationalBankCurrencyDefinition(
                        468,
                        132,
                        "764",
                        "THB",
                        "Baht",
                        "Baht",
                        100,
                        1,
                        new DateOnly(2021, 7, 9),
                        new DateOnly(2050, 1, 1))
                });

            nationalBankApiClientMock
                .Setup(cl => cl.GetRateAsync(
                    468,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new NationalBankCurrencyRate
                {
                    CurrencyId = 468,
                    Abbreviation = "THB",
                    Scale = 100,
                    OfficialRate = null
                });

            var result = await sut.GetTodayActiveRatesAsync();

            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetTodayActiveRatesAsync_WhenNationalBankReturnsNotFound_DoesNotReturnDefaultRate()
        {
            currencyResolverMock
                .Setup(r => r.ResolveActiveCurrenciesAsync(It.IsAny<DateOnly>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new NationalBankCurrencyDefinition(
                        468,
                        132,
                        "764",
                        "THB",
                        "Baht",
                        "Baht",
                        100,
                        1,
                        new DateOnly(2021, 7, 9),
                        new DateOnly(2050, 1, 1))
                });

            var apiException = await CreateApiExceptionAsync(HttpStatusCode.NotFound);

            nationalBankApiClientMock
                .Setup(cl => cl.GetRateAsync(
                    468,
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(apiException);

            var result = await sut.GetTodayActiveRatesAsync();

            result.Should().BeEmpty();
        }

        private static async Task<ApiException> CreateApiExceptionAsync(HttpStatusCode statusCode)
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                new Uri("https://api.nbrb.by/exrates/rates/468"));
            using var response = new HttpResponseMessage(statusCode)
            {
                RequestMessage = request
            };

            return await ApiException.Create(
                request,
                HttpMethod.Get,
                response,
                new RefitSettings());
        }
    }
}
