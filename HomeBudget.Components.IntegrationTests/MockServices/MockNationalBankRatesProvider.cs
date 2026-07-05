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
        private const string UkrainianHryvniaAbbreviation = "UAH";
        private const string ChineseYuanAbbreviation = "CNY";
        private const string ThaiBahtAbbreviation = "THB";

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
                CreateDefinition(NationalBankCurrencies.Uan.Id, UkrainianHryvniaAbbreviation, "Hryvnia", 100),
                CreateDefinition(NationalBankCurrencies.Try.Id, NationalBankCurrencies.Try.Name, "Turkish Lira", 10),
                CreateDefinition(462, ChineseYuanAbbreviation, "Yuan Renminbi", 10),
                CreateDefinition(468, ThaiBahtAbbreviation, "Baht", 10)
            };

            return Task.FromResult(responsePayload);
        }

        public Task<IReadOnlyCollection<NationalBankShortCurrencyRate>> GetRatesForPeriodAsync(
            IEnumerable<int> currenciesIds,
            PeriodRange periodRange,
            CancellationToken ct = default)
        {
            var responsePayload = new List<NationalBankShortCurrencyRate>();

            for (var date = periodRange.StartDate; date <= periodRange.EndDate; date = date.AddDays(1))
            {
                responsePayload.AddRange(
                    new List<NationalBankShortCurrencyRate>
                    {
                        CreateShortRate(NationalBankCurrencies.Usd.Id, 3.1775m, date),
                        CreateShortRate(NationalBankCurrencies.Rub.Id, 3.4991m, date),
                        CreateShortRate(NationalBankCurrencies.Pln.Id, 8.1463m, date),
                        CreateShortRate(NationalBankCurrencies.Eur.Id, 3.5363m, date),
                        CreateShortRate(NationalBankCurrencies.Uan.Id, 8.4558m, date),
                        CreateShortRate(NationalBankCurrencies.Try.Id, 1.0789m, date),
                        CreateShortRate(462, 4.4255m, date),
                        CreateShortRate(468, 9.7281m, date)
                    });
            }

            return Task.FromResult<IReadOnlyCollection<NationalBankShortCurrencyRate>>(responsePayload);
        }

        public Task<IReadOnlyCollection<NationalBankCurrencyRate>> GetTodayActiveRatesAsync(CancellationToken ct = default)
        {
            var today = new DateTime(2023, 11, 14);

            IReadOnlyCollection<NationalBankCurrencyRate> responsePayload = new List<NationalBankCurrencyRate>
            {
                new()
                {
                    Abbreviation = NationalBankCurrencies.Usd.Name,
                    Scale = 1,
                    CurrencyId = NationalBankCurrencies.Usd.Id,
                    OfficialRate = 3.2085m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = NationalBankCurrencies.Rub.Name,
                    Scale = 100,
                    CurrencyId = NationalBankCurrencies.Rub.Id,
                    OfficialRate = 3.5519m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = NationalBankCurrencies.Pln.Name,
                    Scale = 10,
                    CurrencyId = NationalBankCurrencies.Pln.Id,
                    OfficialRate = 8.0973m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = NationalBankCurrencies.Eur.Name,
                    Scale = 1,
                    CurrencyId = NationalBankCurrencies.Eur.Id,
                    OfficialRate = 3.4894m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = UkrainianHryvniaAbbreviation,
                    Scale = 100,
                    CurrencyId = NationalBankCurrencies.Uan.Id,
                    OfficialRate = 8.3503m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = NationalBankCurrencies.Try.Name,
                    Scale = 10,
                    CurrencyId = NationalBankCurrencies.Try.Id,
                    OfficialRate = 1.0113m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = ChineseYuanAbbreviation,
                    Scale = 10,
                    CurrencyId = 462,
                    OfficialRate = 4.4255m,
                    UpdateDate = today
                },
                new()
                {
                    Abbreviation = ThaiBahtAbbreviation,
                    Scale = 10,
                    CurrencyId = 468,
                    OfficialRate = 9.7281m,
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

        private static NationalBankShortCurrencyRate CreateShortRate(
            int currencyId,
            decimal officialRate,
            DateOnly updateDate)
            => new()
            {
                CurrencyId = currencyId,
                OfficialRate = officialRate,
                UpdateDate = updateDate
            };
    }
}
