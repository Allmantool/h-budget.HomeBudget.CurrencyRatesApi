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
    public class CurrencyExchangeControllerTests : IAsyncDisposable
    {
        private readonly CurrencyExchangeTestWebApp _sut = new();

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

        public async ValueTask DisposeAsync()
        {
            if (_sut != null)
            {
                await _sut.DisposeAsync();
            }
        }
    }
}
