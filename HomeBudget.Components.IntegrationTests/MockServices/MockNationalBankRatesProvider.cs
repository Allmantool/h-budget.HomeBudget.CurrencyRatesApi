using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.IntegrationTests.MockServices
{
    internal class MockNationalBankRatesProvider : INationalBankRatesProvider
    {
        public Task<IReadOnlyCollection<NationalBankShortCurrencyRate>> GetRatesForPeriodAsync(IEnumerable<int> currenciesIds, PeriodRange periodRange)
        {
            IReadOnlyCollection<NationalBankShortCurrencyRate> responsePayload = new List<NationalBankShortCurrencyRate>
            {
                new()
                {
                    CurrencyId = 1,
                    OfficialRate = 1.11m,
                    UpdateDate = new DateTime(2023, 11, 14)
                },
                new()
                {
                    CurrencyId = 2,
                    OfficialRate = 2m,
                    UpdateDate = new DateTime(2023, 11, 14)
                },
                new()
                {
                    CurrencyId = 3,
                    OfficialRate = 31m,
                    UpdateDate = new DateTime(2023, 11, 14)
                },
                new()
                {
                    CurrencyId = 4,
                    OfficialRate = 4m,
                    UpdateDate = new DateTime(2023, 11, 14)
                },
                new()
                {
                    CurrencyId = 5,
                    OfficialRate = 5m,
                    UpdateDate = new DateTime(2023, 11, 14)
                },                new()
                {
                    CurrencyId = 6,
                    OfficialRate = 6m,
                    UpdateDate = new DateTime(2023, 11, 14)
                }
            };

            return Task.FromResult(responsePayload);
        }

        public Task<IReadOnlyCollection<NationalBankCurrencyRate>> GetTodayActiveRatesAsync()
        {
            IReadOnlyCollection<NationalBankCurrencyRate> responsePayload = new List<NationalBankCurrencyRate>
            {
                new()
                {
                    Abbreviation = "USD",
                    Scale = 1,
                    CurrencyId = 1,
                    OfficialRate = 1.11m,
                    UpdateDate = new DateTime(2023, 11, 14)
                },
                new()
                {
                    Abbreviation = "RUB",
                    Scale = 3,
                    CurrencyId = 2,
                    OfficialRate = 2.22m,
                    UpdateDate = new DateTime(2023, 11, 14)
                },
                new()
                {
                    Abbreviation = "PLN",
                    Scale = 5,
                    CurrencyId = 3,
                    OfficialRate = 3.33m,
                    UpdateDate = new DateTime(2023, 11, 14)
                },
                new()
                {
                    Abbreviation = "EUR",
                    Scale = 10,
                    CurrencyId = 4,
                    OfficialRate = 4.44m,
                    UpdateDate = new DateTime(2023, 11, 14)
                },
                new()
                {
                    Abbreviation = "UAH",
                    Scale = 15,
                    CurrencyId = 5,
                    OfficialRate = 5.55m,
                    UpdateDate = new DateTime(2023, 11, 14)
                },
                new()
                {
                    Abbreviation = "TRY",
                    Scale = 20,
                    CurrencyId = 6,
                    OfficialRate = 6.66m,
                    UpdateDate = new DateTime(2023, 11, 14)
                }
            };

            return Task.FromResult(responsePayload);
        }
    }
}
