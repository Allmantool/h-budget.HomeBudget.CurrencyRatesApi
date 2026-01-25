using System;
using System.Net;
using System.Threading.Tasks;

using FluentAssertions;
using NUnit.Framework;
using RestSharp;

using HomeBudget.Components.IntegrationTests.Constants;
using HomeBudget.Components.IntegrationTests.TestSources;
using HomeBudget.Components.IntegrationTests.WebApps;
using HomeBudget.Core.Models;
using HomeBudget.Rates.Api.Constants;
using HomeBudget.Rates.Api.Models.Requests;

namespace HomeBudget.Components.IntegrationTests.Controllers
{
    [Category(TestTypes.Integration)]
    public class CurrencyExchangeControllerTests : BaseIntegrationTests
    {
        private readonly CurrencyExchangeTestWebApp _sut = new();

        [OneTimeSetUp]
        public override async Task SetupAsync()
        {
            await _sut.InitAsync();
            await base.SetupAsync();
        }

        [TestCaseSource(typeof(ExchangeControllerTestCases), nameof(ExchangeControllerTestCases.WithUsdCases))]
        [TestCaseSource(typeof(ExchangeControllerTestCases), nameof(ExchangeControllerTestCases.WithBlrCases))]
        public async Task GetExchangeAsync_WhenFromBlrToUsd_ThenExpectedExchangeOutput(
            string originCurrency,
            string targetCurrency,
            decimal exchangeResult)
        {
            var requestBody = new CurrencyExchangeRequest
            {
                TargetCurrency = targetCurrency,
                OriginCurrency = originCurrency,
                Amount = 1000,
                OperationDate = new DateOnly(2024, 1, 15)
            };

            var currencyExchangeRequest = new RestRequest($"/{Endpoints.CurrencyExchangeApi}", Method.Post)
                .AddJsonBody(requestBody);

            var response = await _sut.RestHttpClient!.ExecuteAsync<Result<decimal>>(currencyExchangeRequest);
            var payload = response.Data;

            Assert.Multiple(() =>
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                payload.IsSucceeded.Should().BeTrue();
                payload.Payload.Should().Be(exchangeResult);
            });
        }

        [Test]
        public async Task GetExchangeMultiplierAsync_WhenFromBlrToUsdInFuture_ThenReturnMostUpToDateCurrency()
        {
            const string requestBodyAsJson = "{" +
                 "\"originCurrency\": \"BYN\"," +
                 "\"targetCurrency\": \"USD\"," +
                 "\"operationDate\": \"3024-04-28\"" +
                 "}";

            var currencyExchangeRequest = new RestRequest($"/{Endpoints.CurrencyExchangeApi}/multiplier", Method.Post)
                .AddJsonBody(requestBodyAsJson);

            var response = await _sut.RestHttpClient!.ExecuteAsync<Result<decimal>>(currencyExchangeRequest);
            var payload = response.Data;

            Assert.Multiple(() =>
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                payload.IsSucceeded.Should().BeTrue();
                payload.Payload.Should().Be(0.31471M);
            });
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
