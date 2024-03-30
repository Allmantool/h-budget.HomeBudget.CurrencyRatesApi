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
                    CurrencyId = NationalBankCurrencyIds.Usd,
                    OfficialRate = 3.1775m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencyIds.Rub,
                    OfficialRate = 3.4991m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencyIds.Pln,
                    OfficialRate = 8.1463m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencyIds.Eur,
                    OfficialRate = 3.5363m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencyIds.Uan,
                    OfficialRate = 8.4558m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencyIds.Try,
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
                    Abbreviation = nameof(NationalBankCurrencyIds.Usd),
                    Scale = 1,
                    CurrencyId = NationalBankCurrencyIds.Usd,
                    OfficialRate = 3.2085m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencyIds.Rub),
                    Scale = 100,
                    CurrencyId = NationalBankCurrencyIds.Rub,
                    OfficialRate = 3.5519m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencyIds.Pln),
                    Scale = 10,
                    CurrencyId = NationalBankCurrencyIds.Pln,
                    OfficialRate = 8.0973m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencyIds.Eur),
                    Scale = 1,
                    CurrencyId = NationalBankCurrencyIds.Eur,
                    OfficialRate = 3.4894m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencyIds.Uan),
                    Scale = 100,
                    CurrencyId = NationalBankCurrencyIds.Uan,
                    OfficialRate = 8.3503m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencyIds.Try),
                    Scale = 10,
                    CurrencyId = NationalBankCurrencyIds.Try,
                    OfficialRate = 1.0113m,
                    UpdateDate = today
                }
            };

            return Task.FromResult(responsePayload);
        }
    }
}
