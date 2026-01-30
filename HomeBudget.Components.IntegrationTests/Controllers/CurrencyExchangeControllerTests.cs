using System;
using System.Net;
using System.Threading;
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
    [TestFixture]
    [Category(TestTypes.Integration)]
    [NonParallelizable]
    [Order(IntegrationTestOrderIndex.CurrencyExchangeControllerTests)]
    public class CurrencyExchangeControllerTests : BaseIntegrationTests
    {
        private static int _counter;
        private readonly int _id = Interlocked.Increment(ref _counter);

        private CurrencyRatesTestWebApp _sut;
        private RestClient _restClient;

        [OneTimeSetUp]
        public override async Task SetupAsync()
        {
            TestContext.Progress.WriteLine($"[WebApp {_id}] OneTimeSetUp START ({GetType().Name})");

            _sut = new CurrencyRatesTestWebApp();

            await base.SetupAsync();

            _restClient = _sut.RestHttpClient;

            TestContext.Progress.WriteLine($"[WebApp {_id}] OneTimeSetUp END");
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            TestContext.Progress.WriteLine(
                $"[WebApp {_id}] OneTimeTearDown START");

            await this.TerminateAsync();

            TestContext.Progress.WriteLine(
                $"[WebApp {_id}] OneTimeTearDown END");
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

            var response = await _restClient!.ExecuteAsync<Result<decimal>>(currencyExchangeRequest);
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

            var response = await _restClient!.ExecuteAsync<Result<decimal>>(currencyExchangeRequest);
            var payload = response.Data;

            Assert.Multiple(() =>
            {
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                payload.IsSucceeded.Should().BeTrue();
                payload.Payload.Should().Be(0.31471M);
            });
        }
    }
}
