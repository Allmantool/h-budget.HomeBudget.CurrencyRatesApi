﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using FluentAssertions;
using NUnit.Framework;
using RestSharp;

using HomeBudget.Core.Constants;
using HomeBudget.Core.Models;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Rates.Api.Models;
using HomeBudget.Components.IntegrationTests.Constants;
using HomeBudget.Components.IntegrationTests.WebApps;
using CurrencyRate = HomeBudget.Rates.Api.Models.CurrencyRate;

namespace HomeBudget.Components.IntegrationTests.Controllers
{
    // [Ignore("Intend to be used only for local testing. Not appropriate infrastructure has been setup")]
    [Category(TestTypes.Integration)]
    [TestFixture]
    public class CurrencyRatesControllerTests : IAsyncDisposable
    {
        private readonly CurrencyRatesTestWebApp _sut = new();

        [OneTimeTearDown]
        public async Task StopAsync() => await _sut.StopAsync();

        [Test]
        public async Task GetRatesForPeriodAsync_WhenExecuteTheCallToEnquireRatesForPeriodOfTime_ThenIsSuccessStatusCode()
        {
            var startDay = new DateTime(2022, 10, 25).ToString(DateFormats.RatesApiRequestFormat);
            var endDate = new DateTime(2022, 12, 25).ToString(DateFormats.RatesApiRequestFormat);

            var getCurrencyRatesForPeriodRequest = new RestRequest($"/currencyRates/period/{startDay}/{endDate}");

            var response = await _sut.RestHttpClient!.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(getCurrencyRatesForPeriodRequest);

            Assert.IsTrue(response.IsSuccessful);
        }

        [Test]
        public async Task GetRatesForPeriodAsync_WhenExecuteTheCallToEnquireRatesForPeriodOfTime_ThenReturnsExpectedAmountOfCurrencyGroupsInResponse()
        {
            // "2023-07-30"
            var startDay = new DateTime(2022, 10, 25).ToString(DateFormats.RatesApiRequestFormat);
            var endDate = new DateTime(2022, 12, 25).ToString(DateFormats.RatesApiRequestFormat);

            var getCurrencyRatesForPeriodRequest = new RestRequest($"/currencyRates/period/{startDay}/{endDate}");

            var response = await _sut.RestHttpClient!.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(getCurrencyRatesForPeriodRequest);
            var payload = response.Data;
            var currencyGroupAmount = payload?.Payload.Count;

            currencyGroupAmount.Should().Be(6);
        }

        [Test]
        public async Task GetAllRatesAsync_WhenTodayCurrencyHasNotBeenSaved_ThenNotFound()
        {
            var getRatesRequest = new RestRequest("/currencyRates");

            var response = await _sut.RestHttpClient!.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(getRatesRequest);

            // TODO: think over what to test
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetTodayRatesAsync_WhenEnquireAndSaveTodayRates_ThenOkAsStatus()
        {
            var getTodayRatesRequest = new RestRequest("/currencyRates/today");

            var response = await _sut.RestHttpClient!.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(getTodayRatesRequest);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
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
                        Abbreviation = "USD",
                        CurrencyId = 431,
                        Name = "Доллар США",
                        OfficialRate = 2.7286m,
                        RatePerUnit = 2.7286m,
                        UpdateDate = new DateOnly(2023, 1, 8)
                    }
                }
            };

            var currencySaveRatesRequest = new RestRequest("/currencyRates", Method.Post)
                .AddJsonBody(requestBody);

            var response = await _sut.RestHttpClient!.ExecuteAsync<Result<int>>(currencySaveRatesRequest);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        public async ValueTask DisposeAsync()
        {
            if (_sut != null)
            {
                await _sut.DisposeAsync();
            }
        }
    }
}