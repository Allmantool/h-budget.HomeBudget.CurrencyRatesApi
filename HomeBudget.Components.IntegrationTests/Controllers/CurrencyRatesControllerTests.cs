using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using FluentAssertions;
using NUnit.Framework;
using RestSharp;

using HomeBudget.Rates.Api.Constants;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.IntegrationTests.Constants;
using HomeBudget.Components.IntegrationTests.WebApps;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Models;
using HomeBudget.Rates.Api.Models.Requests;
using CurrencyRate = HomeBudget.Rates.Api.Models.CurrencyRate;

namespace HomeBudget.Components.IntegrationTests.Controllers
{
    [TestFixture]
    [Category(TestTypes.Integration)]
    [NonParallelizable]
    [Order(IntegrationTestOrderIndex.CurrencyRatesControllerTests)]
    public class CurrencyRatesControllerTests : BaseIntegrationTests
    {
        private readonly CurrencyRatesTestWebApp _sut = new();

        [OneTimeSetUp]
        public override async Task SetupAsync()
        {
            await _sut.InitAsync();
            await base.SetupAsync();
        }

        [OneTimeTearDown]
        public async Task CleanupAsync()
        {
            await _sut.DisposeAsync();
        }

        [Test]
        public async Task GetRatesForPeriodAsync_WhenExecuteTheCallToEnquireRatesForPeriodOfTime_ThenIsSuccessStatusCode()
        {
            var startDay = new DateTime(2022, 10, 25).ToString(DateFormats.RatesApiRequestFormat);
            var endDate = new DateTime(2022, 12, 25).ToString(DateFormats.RatesApiRequestFormat);

            var getCurrencyRatesForPeriodRequest = new RestRequest($"/{Endpoints.RatesApi}/period/{startDay}/{endDate}");

            var response = await _sut.RestHttpClient!.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(getCurrencyRatesForPeriodRequest);

            response.IsSuccessful.Should().BeTrue();
        }

        [Test]
        public async Task GetRatesForPeriodAsync_WhenExecuteTheCallToEnquireRatesForPeriodOfTime_ThenReturnsExpectedAmountOfCurrencyGroupsInResponse()
        {
            // "2023-07-30"
            var startDay = new DateTime(2022, 10, 25).ToString(DateFormats.RatesApiRequestFormat);
            var endDate = new DateTime(2022, 12, 25).ToString(DateFormats.RatesApiRequestFormat);

            var getCurrencyRatesForPeriodRequest = new RestRequest($"/{Endpoints.RatesApi}/period/{startDay}/{endDate}");

            var response = await _sut.RestHttpClient!.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(getCurrencyRatesForPeriodRequest);
            var payload = response.Data;
            var currencyGroupAmount = payload?.Payload.Count;

            currencyGroupAmount.Should().Be(6);
        }

        [Test]
        public async Task GetAllRatesAsync_WhenTodayCurrencyHasNotBeenSaved_ThenNotFound()
        {
            var getRatesRequest = new RestRequest($"/{Endpoints.RatesApi}");

            var response = await _sut.RestHttpClient!.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(getRatesRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetTodayRatesAsync_WhenEnquireAndSaveTodayRates_ThenOkAsStatus()
        {
            var getTodayRatesRequest = new RestRequest($"/{Endpoints.RatesApi}/today");

            var response = await _sut.RestHttpClient!.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(getTodayRatesRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task AddRatesAsync_WhenTryToSaveAlreadyExistedValue_ThenOk()
        {
            var requestBody = new CurrencySaveRatesRequest
            {
                CurrencyRates = new List<CurrencyRate>
                {
                    new()
                    {
                        Scale = 1,
                        Abbreviation = CurrencyCodes.Usd,
                        CurrencyId = 431,
                        Name = "Доллар США",
                        OfficialRate = 2.7286m,
                        RatePerUnit = 2.7286m,
                        UpdateDate = new DateOnly(2023, 1, 8)
                    }
                }
            };

            var currencySaveRatesRequest = new RestRequest($"/{Endpoints.RatesApi}", Method.Post)
                .AddJsonBody(requestBody);

            var response = await _sut.RestHttpClient!.ExecuteAsync<Result<int>>(currencySaveRatesRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
