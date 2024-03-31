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
                    CurrencyId = NationalBankCurrencies.Usd,
                    OfficialRate = 3.1775m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencies.Rub,
                    OfficialRate = 3.4991m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencies.Pln,
                    OfficialRate = 8.1463m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencies.Eur,
                    OfficialRate = 3.5363m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencies.Uan,
                    OfficialRate = 8.4558m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencies.Try,
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
                    Abbreviation = nameof(NationalBankCurrencies.Usd),
                    Scale = 1,
                    CurrencyId = NationalBankCurrencies.Usd,
                    OfficialRate = 3.2085m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencies.Rub),
                    Scale = 100,
                    CurrencyId = NationalBankCurrencies.Rub,
                    OfficialRate = 3.5519m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencies.Pln),
                    Scale = 10,
                    CurrencyId = NationalBankCurrencies.Pln,
                    OfficialRate = 8.0973m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencies.Eur),
                    Scale = 1,
                    CurrencyId = NationalBankCurrencies.Eur,
                    OfficialRate = 3.4894m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencies.Uan),
                    Scale = 100,
                    CurrencyId = NationalBankCurrencies.Uan,
                    OfficialRate = 8.3503m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencies.Try),
                    Scale = 10,
                    CurrencyId = NationalBankCurrencies.Try,
                    OfficialRate = 1.0113m,
                    UpdateDate = today
                }
            };

            return Task.FromResult(responsePayload);
        }
    }
}
