using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.IntegrationTests.MockServices
{
    internal class MockNationalBankRatesProvider : INationalBankRatesProvider
    {
        public Task<IReadOnlyCollection<NationalBankShortCurrencyRate>> GetRatesForPeriodAsync(
            IEnumerable<int> currenciesIds,
            PeriodRange periodRange)
        {
            IReadOnlyCollection<NationalBankShortCurrencyRate> responsePayload = new List<NationalBankShortCurrencyRate>
            {
                new()
                {
                    CurrencyId = NationalBankCurrencyIds.USD,
                    OfficialRate = 3.1775m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencyIds.RUB,
                    OfficialRate = 3.4991m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencyIds.PLN,
                    OfficialRate = 8.1463m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencyIds.EUR,
                    OfficialRate = 3.5363m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencyIds.UAN,
                    OfficialRate = 8.4558m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencyIds.TRY,
                    OfficialRate = 1.0789m,
                    UpdateDate = periodRange.StartDate
                }
            };

            return Task.FromResult(responsePayload);
        }

        public Task<IReadOnlyCollection<NationalBankCurrencyRate>> GetTodayActiveRatesAsync()
        {
            var today = new DateTime(2023, 11, 14);

            IReadOnlyCollection<NationalBankCurrencyRate> responsePayload = new List<NationalBankCurrencyRate>
            {
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencyIds.USD),
                    Scale = 1,
                    CurrencyId = NationalBankCurrencyIds.USD,
                    OfficialRate = 3.2085m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencyIds.RUB),
                    Scale = 100,
                    CurrencyId = NationalBankCurrencyIds.RUB,
                    OfficialRate = 3.5519m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencyIds.PLN),
                    Scale = 10,
                    CurrencyId = NationalBankCurrencyIds.PLN,
                    OfficialRate = 8.0973m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencyIds.EUR),
                    Scale = 1,
                    CurrencyId = NationalBankCurrencyIds.EUR,
                    OfficialRate = 3.4894m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencyIds.UAN),
                    Scale = 100,
                    CurrencyId = NationalBankCurrencyIds.UAN,
                    OfficialRate = 8.3503m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencyIds.TRY),
                    Scale = 10,
                    CurrencyId = NationalBankCurrencyIds.TRY,
                    OfficialRate = 1.0113m,
                    UpdateDate = today
                }
            };

            return Task.FromResult(responsePayload);
        }
    }
}
