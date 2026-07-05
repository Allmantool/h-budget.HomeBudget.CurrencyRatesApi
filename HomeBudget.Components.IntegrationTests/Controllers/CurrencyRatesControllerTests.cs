using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.IntegrationTests.Constants;
using HomeBudget.Components.IntegrationTests.WebApps;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Models;
using HomeBudget.Rates.Api.Constants;
using HomeBudget.Rates.Api.Models.Requests;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using RestSharp;
using CurrencyRate = HomeBudget.Rates.Api.Models.CurrencyRate;

namespace HomeBudget.Components.IntegrationTests.Controllers
{
    [TestFixture]
    [Category(TestTypes.Integration)]
    [NonParallelizable]
    [Order(IntegrationTestOrderIndex.CurrencyRatesControllerTests)]
    internal class CurrencyRatesControllerTests : BaseIntegrationTests
    {
        private readonly CurrencyRatesTestWebApp _sut;
        private RestClient _httpClient;

        public CurrencyRatesControllerTests()
        {
            _sut = new CurrencyRatesTestWebApp();
        }

        [SetUp]
        public override async Task SetupAsync()
        {
            await _sut.InitAsync();

            _httpClient = _sut.RestHttpClient;
        }

        public override async Task TerminateAsync()
        {
            await _sut.DisposeAsync();
        }

        [Test]
        public async Task GetRatesForPeriodAsync_WhenExecuteTheCallToEnquireRatesForPeriodOfTime_ThenIsSuccessStatusCode()
        {
            var startDay = new DateTime(2022, 10, 25).ToString(DateFormats.RatesApiRequestFormat, CultureInfo.InvariantCulture);
            var endDate = new DateTime(2022, 12, 25).ToString(DateFormats.RatesApiRequestFormat, CultureInfo.InvariantCulture);

            var getCurrencyRatesForPeriodRequest = new RestRequest($"/{Endpoints.RatesApi}/period/{startDay}/{endDate}");

            var response = await _httpClient.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(getCurrencyRatesForPeriodRequest);

            response.IsSuccessful.Should().BeTrue();
        }

        [Test]
        public async Task GetRatesForPeriodAsync_WhenExecuteTheCallToEnquireRatesForPeriodOfTime_ThenReturnsExpectedAmountOfCurrencyGroupsInResponse()
        {
            // "2023-07-30"
            var startDay = new DateTime(2022, 10, 25).ToString(DateFormats.RatesApiRequestFormat, CultureInfo.InvariantCulture);
            var endDate = new DateTime(2022, 12, 25).ToString(DateFormats.RatesApiRequestFormat, CultureInfo.InvariantCulture);

            var getCurrencyRatesForPeriodRequest = new RestRequest($"/{Endpoints.RatesApi}/period/{startDay}/{endDate}");

            var response = await _httpClient.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(getCurrencyRatesForPeriodRequest);
            var payload = response.Data;
            var currencyGroupAmount = payload?.Payload.Count;

            currencyGroupAmount.Should().Be(8);
        }

        [Test]
        public async Task RequestRatesForPeriodAsync_WhenExecutedTwice_PersistsMultipleDatesAndCurrenciesWithoutDuplicates()
        {
            var startDate = new DateOnly(2026, 1, 1);
            var endDate = new DateOnly(2026, 1, 3);
            var startDay = startDate.ToString(DateFormats.RatesApiRequestFormat, CultureInfo.InvariantCulture);
            var endDay = endDate.ToString(DateFormats.RatesApiRequestFormat, CultureInfo.InvariantCulture);
            var requestBody = new CurrencyForPeriodRequest
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var firstRequest = new RestRequest($"/{Endpoints.RatesApi}/period/{startDay}/{endDay}", Method.Post)
                .AddJsonBody(requestBody);
            var secondRequest = new RestRequest($"/{Endpoints.RatesApi}/period/{startDay}/{endDay}", Method.Post)
                .AddJsonBody(requestBody);

            var firstResponse = await _httpClient.ExecuteAsync(firstRequest);
            var secondResponse = await _httpClient.ExecuteAsync(secondRequest);
            var persistedRows = await CountPersistedRatesAsync(startDate, endDate);

            firstResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            secondResponse.StatusCode.Should().Be(HttpStatusCode.Accepted);
            persistedRows.Should().Be(24);
        }

        [Test]
        public async Task GetRatesForPeriodAsync_WhenProviderNamesAreEnglish_ReturnsCurrencyAbbreviationNames()
        {
            var date = new DateOnly(2026, 2, 1);
            var requestDate = date.ToString(DateFormats.RatesApiRequestFormat, CultureInfo.InvariantCulture);
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

            var request = new RestRequest($"/{Endpoints.RatesApi}/period/{requestDate}/{requestDate}");

            var response = await _httpClient.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Data?.Payload.Should().HaveCount(expectedNames.Count);
            response.Data?.Payload.Should().OnlyContain(rate => rate.Name == expectedNames[rate.Abbreviation]);
        }

        [Test]
        public async Task GetAllRatesAsync_WhenTodayCurrencyHasNotBeenSaved_ThenNotFound()
        {
            var getRatesRequest = new RestRequest($"/{Endpoints.RatesApi}");

            var response = await _httpClient.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(getRatesRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public async Task GetTodayRatesAsync_WhenEnquireAndSaveTodayRates_ThenOkAsStatus()
        {
            var getTodayRatesRequest = new RestRequest($"/{Endpoints.RatesApi}/today");

            var response = await _httpClient.ExecuteAsync<Result<IReadOnlyCollection<CurrencyRateGrouped>>>(getTodayRatesRequest);

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

            var response = await _httpClient.ExecuteAsync<Result<int>>(currencySaveRatesRequest);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private async Task<int> CountPersistedRatesAsync(DateOnly startDate, DateOnly endDate)
        {
            const string query = @"
                SELECT COUNT(*)
                FROM [HomeBudget.CurrencyRates].[dbo].[CurrencyRates]
                WHERE [UpdateDate] BETWEEN @StartDate AND @EndDate
                  AND [Abbreviation] IN ('USD', 'RUB', 'EUR', 'UAH', 'PLN', 'TRY', 'CNY', 'THB');";

            await using var connection = new SqlConnection(TestContainers.MsSqlDbContainer.GetConnectionString());
            await using var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@StartDate", startDate.ToDateTime(TimeOnly.MinValue));
            command.Parameters.AddWithValue("@EndDate", endDate.ToDateTime(TimeOnly.MinValue));

            await connection.OpenAsync();

            return (int)await command.ExecuteScalarAsync();
        }
    }
}
