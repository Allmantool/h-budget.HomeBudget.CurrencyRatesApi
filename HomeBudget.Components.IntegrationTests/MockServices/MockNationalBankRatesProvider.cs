using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Models;

namespace HomeBudget.Components.IntegrationTests.MockServices
{
    internal class MockNationalBankRatesProvider : INationalBankRatesProvider
    {
        public Task<IReadOnlyCollection<NationalBankCurrencyDefinition>> GetActiveCurrenciesAsync(
            DateOnly requestedDate,
            CancellationToken ct = default)
        {
            IReadOnlyCollection<NationalBankCurrencyDefinition> responsePayload = new List<NationalBankCurrencyDefinition>
            {
                CreateDefinition(NationalBankCurrencies.Usd.Id, NationalBankCurrencies.Usd.Name, "US Dollar", 1),
                CreateDefinition(NationalBankCurrencies.Rub.Id, NationalBankCurrencies.Rub.Name, "Russian Ruble", 100),
                CreateDefinition(NationalBankCurrencies.Pln.Id, NationalBankCurrencies.Pln.Name, "Polish Zloty", 10),
                CreateDefinition(NationalBankCurrencies.Eur.Id, NationalBankCurrencies.Eur.Name, "Euro", 1),
                CreateDefinition(NationalBankCurrencies.Uan.Id, NationalBankCurrencies.Uan.Name, "Hryvnia", 100),
                CreateDefinition(NationalBankCurrencies.Try.Id, NationalBankCurrencies.Try.Name, "Turkish Lira", 10)
            };

            return Task.FromResult(responsePayload);
        }

        public Task<IReadOnlyCollection<NationalBankShortCurrencyRate>> GetRatesForPeriodAsync(
            IEnumerable<int> currenciesIds,
            PeriodRange periodRange,
            CancellationToken ct = default)
        {
            IReadOnlyCollection<NationalBankShortCurrencyRate> responsePayload = new List<NationalBankShortCurrencyRate>
            {
                new()
                {
                    CurrencyId = NationalBankCurrencies.Usd.Id,
                    OfficialRate = 3.1775m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencies.Rub.Id,
                    OfficialRate = 3.4991m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencies.Pln.Id,
                    OfficialRate = 8.1463m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencies.Eur.Id,
                    OfficialRate = 3.5363m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencies.Uan.Id,
                    OfficialRate = 8.4558m,
                    UpdateDate = periodRange.StartDate
                },
                new()
                {
                    CurrencyId = NationalBankCurrencies.Try.Id,
                    OfficialRate = 1.0789m,
                    UpdateDate = periodRange.StartDate
                }
            };

            return Task.FromResult(responsePayload);
        }

        public Task<IReadOnlyCollection<NationalBankCurrencyRate>> GetTodayActiveRatesAsync(CancellationToken ct = default)
        {
            var today = new DateTime(2023, 11, 14);

            IReadOnlyCollection<NationalBankCurrencyRate> responsePayload = new List<NationalBankCurrencyRate>
            {
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencies.Usd),
                    Scale = 1,
                    CurrencyId = NationalBankCurrencies.Usd.Id,
                    OfficialRate = 3.2085m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencies.Rub),
                    Scale = 100,
                    CurrencyId = NationalBankCurrencies.Rub.Id,
                    OfficialRate = 3.5519m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencies.Pln),
                    Scale = 10,
                    CurrencyId = NationalBankCurrencies.Pln.Id,
                    OfficialRate = 8.0973m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencies.Eur),
                    Scale = 1,
                    CurrencyId = NationalBankCurrencies.Eur.Id,
                    OfficialRate = 3.4894m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencies.Uan),
                    Scale = 100,
                    CurrencyId = NationalBankCurrencies.Uan.Id,
                    OfficialRate = 8.3503m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = nameof(NationalBankCurrencies.Try),
                    Scale = 10,
                    CurrencyId = NationalBankCurrencies.Try.Id,
                    OfficialRate = 1.0113m,
                    UpdateDate = today
                }
            };

            return Task.FromResult(responsePayload);
        }

        private static NationalBankCurrencyDefinition CreateDefinition(
            int currencyId,
            string abbreviation,
            string name,
            int scale)
            => new(
                currencyId,
                null,
                string.Empty,
                abbreviation,
                name,
                name,
                scale,
                0,
                new DateOnly(2000, 1, 1),
                new DateOnly(2050, 1, 1));
    }
}
