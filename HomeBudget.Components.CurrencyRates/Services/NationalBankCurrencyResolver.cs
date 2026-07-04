using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeBudget.Components.CurrencyRates.Clients;
using HomeBudget.Components.CurrencyRates.Exceptions;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Extensions;
using Microsoft.Extensions.Logging;
using Refit;

namespace HomeBudget.Components.CurrencyRates.Services
{
    internal sealed class NationalBankCurrencyResolver(
        ConfigSettings configSettings,
        INationalBankApiClient nationalBankApiClient,
        ILogger<NationalBankCurrencyResolver> logger)
        : INationalBankCurrencyResolver
    {
        private Task<IReadOnlyCollection<NationalBankCurrency>> catalogTask;

        public async Task<IReadOnlyCollection<NationalBankCurrencyDefinition>> ResolveActiveCurrenciesAsync(
            DateOnly requestedDate,
            CancellationToken ct = default)
        {
            var configuredCurrencies = configSettings.ActiveNationalBankCurrencies?.ToArray()
                ?? Array.Empty<ConfigCurrency>();

            ValidateDuplicates(configuredCurrencies);

            var catalog = await GetCatalogAsync(ct);
            var resolved = new List<NationalBankCurrencyDefinition>();

            foreach (var configuredCurrency in configuredCurrencies)
            {
                resolved.Add(Resolve(configuredCurrency, catalog, requestedDate));
            }

            return resolved;
        }

        private async Task<IReadOnlyCollection<NationalBankCurrency>> GetCatalogAsync(CancellationToken ct)
        {
            catalogTask ??= LoadCatalogAsync(ct);

            try
            {
                return await catalogTask;
            }
            catch
            {
                catalogTask = null;
                throw;
            }
        }

        private async Task<IReadOnlyCollection<NationalBankCurrency>> LoadCatalogAsync(CancellationToken ct)
        {
            try
            {
                var currencies = await nationalBankApiClient.GetCurrenciesAsync(ct);

                return currencies.ToList();
            }
            catch (ApiException ex)
            {
                throw new NationalBankCurrencyResolutionException("NBRB currency catalog request failed.", ex);
            }
        }

        private NationalBankCurrencyDefinition Resolve(
            ConfigCurrency configuredCurrency,
            IReadOnlyCollection<NationalBankCurrency> catalog,
            DateOnly requestedDate)
        {
            if (configuredCurrency.Id > 0)
            {
                return ResolveById(configuredCurrency, catalog, requestedDate);
            }

            if (string.IsNullOrWhiteSpace(configuredCurrency.Abbreviation))
            {
                throw new NationalBankCurrencyResolutionException("Configured currency must provide either Id or Abbreviation.");
            }

            return ResolveByAbbreviation(configuredCurrency, catalog, requestedDate);
        }

        private NationalBankCurrencyDefinition ResolveById(
            ConfigCurrency configuredCurrency,
            IReadOnlyCollection<NationalBankCurrency> catalog,
            DateOnly requestedDate)
        {
            var currency = catalog.SingleOrDefault(c => c.CurrencyId == configuredCurrency.Id);

            if (currency == null)
            {
                throw new NationalBankCurrencyResolutionException(
                    $"Currency with NBRB id '{configuredCurrency.Id}' was not found in catalog.");
            }

            EnsureConfiguredAbbreviationMatches(configuredCurrency, currency);
            EnsureActive(currency, requestedDate);
            LogConfigMismatches(configuredCurrency, currency);

            return ToDefinition(currency);
        }

        private NationalBankCurrencyDefinition ResolveByAbbreviation(
            ConfigCurrency configuredCurrency,
            IReadOnlyCollection<NationalBankCurrency> catalog,
            DateOnly requestedDate)
        {
            var matchingCurrencies = catalog
                .Where(c => string.Equals(c.Abbreviation, configuredCurrency.Abbreviation, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (matchingCurrencies.IsNullOrEmpty())
            {
                throw new NationalBankCurrencyResolutionException(
                    $"Currency '{configuredCurrency.Abbreviation}' was not found in NBRB catalog.");
            }

            var activeCurrencies = matchingCurrencies
                .Where(c => IsActive(c, requestedDate))
                .ToArray();

            if (activeCurrencies.IsNullOrEmpty())
            {
                throw new NationalBankCurrencyResolutionException(
                    $"Currency '{configuredCurrency.Abbreviation}' is inactive for '{requestedDate:yyyy-MM-dd}'.");
            }

            if (activeCurrencies.Length > 1)
            {
                throw new NationalBankCurrencyResolutionException(
                    $"Currency abbreviation '{configuredCurrency.Abbreviation}' is ambiguous for '{requestedDate:yyyy-MM-dd}'.");
            }

            var currency = activeCurrencies.Single();

            LogConfigMismatches(configuredCurrency, currency);

            return ToDefinition(currency);
        }

        private static void ValidateDuplicates(IReadOnlyCollection<ConfigCurrency> configuredCurrencies)
        {
            var duplicateIds = configuredCurrencies
                .Where(c => c.Id > 0)
                .GroupBy(c => c.Id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            if (!duplicateIds.IsNullOrEmpty())
            {
                throw new NationalBankCurrencyResolutionException(
                    $"Configured currencies contain duplicate NBRB ids: {string.Join(',', duplicateIds)}.");
            }

            var duplicateAbbreviations = configuredCurrencies
                .Where(c => !string.IsNullOrWhiteSpace(c.Abbreviation))
                .GroupBy(c => c.Abbreviation, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToArray();

            if (!duplicateAbbreviations.IsNullOrEmpty())
            {
                throw new NationalBankCurrencyResolutionException(
                    $"Configured currencies contain duplicate abbreviations: {string.Join(',', duplicateAbbreviations)}.");
            }
        }

        private static void EnsureConfiguredAbbreviationMatches(
            ConfigCurrency configuredCurrency,
            NationalBankCurrency currency)
        {
            if (string.IsNullOrWhiteSpace(configuredCurrency.Abbreviation)
                || string.Equals(configuredCurrency.Abbreviation, currency.Abbreviation, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            throw new NationalBankCurrencyResolutionException(
                $"Configured currency id '{configuredCurrency.Id}' belongs to '{currency.Abbreviation}', not '{configuredCurrency.Abbreviation}'.");
        }

        private static void EnsureActive(NationalBankCurrency currency, DateOnly requestedDate)
        {
            if (IsActive(currency, requestedDate))
            {
                return;
            }

            throw new NationalBankCurrencyResolutionException(
                $"Currency '{currency.Abbreviation}' with NBRB id '{currency.CurrencyId}' is inactive for '{requestedDate:yyyy-MM-dd}'.");
        }

        private static bool IsActive(NationalBankCurrency currency, DateOnly requestedDate)
        {
            var start = DateOnly.FromDateTime(currency.DateStart);
            var end = DateOnly.FromDateTime(currency.DateEnd);

            return start <= requestedDate && requestedDate < end;
        }

        private void LogConfigMismatches(ConfigCurrency configuredCurrency, NationalBankCurrency currency)
        {
            if (configuredCurrency.Scale > 0 && configuredCurrency.Scale != currency.Scale)
            {
                logger.LogWarning(
                    "Configured scale {ConfiguredScale} for currency {Abbreviation} ({CurrencyId}) does not match NBRB scale {CatalogScale}. NBRB metadata is used.",
                    configuredCurrency.Scale,
                    currency.Abbreviation,
                    currency.CurrencyId,
                    currency.Scale);
            }

            if (configuredCurrency.Periodicity.HasValue && configuredCurrency.Periodicity.Value != currency.Periodicity)
            {
                logger.LogWarning(
                    "Configured periodicity {ConfiguredPeriodicity} for currency {Abbreviation} ({CurrencyId}) does not match NBRB periodicity {CatalogPeriodicity}. NBRB metadata is used.",
                    configuredCurrency.Periodicity.Value,
                    currency.Abbreviation,
                    currency.CurrencyId,
                    currency.Periodicity);
            }
        }

        private static NationalBankCurrencyDefinition ToDefinition(NationalBankCurrency currency)
            => new(
                currency.CurrencyId,
                currency.ParentId,
                currency.Code,
                currency.Abbreviation,
                currency.EnglishName ?? currency.Name,
                currency.EnglishName,
                currency.Scale,
                currency.Periodicity,
                DateOnly.FromDateTime(currency.DateStart),
                DateOnly.FromDateTime(currency.DateEnd));
    }
}
