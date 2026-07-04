using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using HomeBudget.Components.CurrencyRates.Clients;
using HomeBudget.Components.CurrencyRates.Logs;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Components.CurrencyRates.Services.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Extensions;
using HomeBudget.Core.Limiters;
using HomeBudget.Core.Models;
using HomeBudget.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;

namespace HomeBudget.Components.CurrencyRates.Providers
{
    internal class NationalBankRatesProvider : INationalBankRatesProvider
    {
        private readonly ILogger<NationalBankRatesProvider> logger;
        private readonly HttpClientOptions httpClientOptions;
        private readonly IHttpClientRateLimiter nationalBankHttpRateLimiter;
        private readonly INationalBankApiClient nationalBankApiClient;
        private readonly INationalBankCurrencyResolver currencyResolver;

        public NationalBankRatesProvider(
            ILogger<NationalBankRatesProvider> logger,
            IOptions<HttpClientOptions> httpOptions,
            INationalBankApiClient nationalBankApiClient,
            IHttpClientRateLimiter nationalBankHttpClientRateLimiter,
            INationalBankCurrencyResolver currencyResolver)
        {
            this.httpClientOptions = httpOptions.Value;
            this.nationalBankApiClient = nationalBankApiClient;
            this.nationalBankHttpRateLimiter = nationalBankHttpClientRateLimiter;
            this.currencyResolver = currencyResolver;
            this.logger = logger;
        }

        public Task<IReadOnlyCollection<NationalBankCurrencyDefinition>> GetActiveCurrenciesAsync(
            DateOnly requestedDate,
            CancellationToken ct = default)
            => currencyResolver.ResolveActiveCurrenciesAsync(requestedDate, ct);

        public async Task<IReadOnlyCollection<NationalBankShortCurrencyRate>> GetRatesForPeriodAsync(
             IEnumerable<int> currenciesIds,
             PeriodRange periodRange,
             CancellationToken ct = default)
        {
            var yearRatesRequestPayloads = GetYearPeriodRequests(currenciesIds, periodRange);
            var ratesForPeriod = new ConcurrentBag<NationalBankShortCurrencyRate>();

            await Parallel.ForEachAsync(
                yearRatesRequestPayloads,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Min(
                        httpClientOptions.MaxConcurrentRequests,
                        httpClientOptions.RateLimiterBurstLimit),
                    CancellationToken = ct
                },
                async (payload, loopCt) =>
                {
                    using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

                    requestCts.CancelAfter(TimeSpan.FromSeconds(httpClientOptions.TimeoutInSeconds));
                    using var lease = await nationalBankHttpRateLimiter.AcquireAsync(1, requestCts.Token);

                    if (!lease.IsAcquired)
                    {
                        var warningReason = lease.TryGetMetadata(MetadataName.ReasonPhrase, out var reason)
                                ? reason
                                : "unknown";

                        logger.RateLimiterExceed(nameof(INationalBankApiClient), warningReason);

                        return;
                    }

                    var period = payload.Period;

                    var rates = await nationalBankApiClient.GetRatesForPeriodAsync(
                        payload.CurrencyId,
                        period.StartDate.ToString(DateFormats.NationalBankApiRequest, CultureInfo.InvariantCulture),
                        period.EndDate.ToString(DateFormats.NationalBankApiRequest, CultureInfo.InvariantCulture),
                        requestCts.Token);

                    foreach (var rate in rates.Where(r => r.OfficialRate.HasValue))
                    {
                        ratesForPeriod.Add(rate);
                    }
                });

            return ratesForPeriod.ToList();
        }

        public async Task<IReadOnlyCollection<NationalBankCurrencyRate>> GetTodayActiveRatesAsync(CancellationToken ct = default)
        {
            var requestedDate = DateOnly.FromDateTime(DateTime.Today);
            var activeCurrencies = await currencyResolver.ResolveActiveCurrenciesAsync(requestedDate, ct);
            var activeRates = new ConcurrentBag<NationalBankCurrencyRate>();

            await Parallel.ForEachAsync(
                activeCurrencies,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Min(
                        httpClientOptions.MaxConcurrentRequests,
                        httpClientOptions.RateLimiterBurstLimit),
                    CancellationToken = ct
                },
                async (currency, loopCt) =>
                {
                    var rate = await GetRateByCurrencyIdAsync(currency, requestedDate, loopCt);

                    if (rate != null)
                    {
                        activeRates.Add(rate);
                    }
                });

            return activeRates.ToList();
        }

        private static IReadOnlyCollection<YearRatesRequestPayload> GetYearPeriodRequests(
            IEnumerable<int> currenciesIds,
            PeriodRange period)
        {
            const int MaxNationalBankDynamicsDays = 365;

            var ranges = period.SplitByMaxDays(MaxNationalBankDynamicsDays);

            return currenciesIds
                .SelectMany(
                    id => ranges,
                    (id, range) => new YearRatesRequestPayload
                    {
                        CurrencyId = id,
                        Period = range
                    })
                .ToList();
        }

        private async Task<NationalBankCurrencyRate> GetRateByCurrencyIdAsync(
            NationalBankCurrencyDefinition currency,
            DateOnly requestedDate,
            CancellationToken ct)
        {
            using var requestCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            requestCts.CancelAfter(TimeSpan.FromSeconds(httpClientOptions.TimeoutInSeconds));
            using var lease = await nationalBankHttpRateLimiter.AcquireAsync(1, requestCts.Token);

            if (!lease.IsAcquired)
            {
                var warningReason = lease.TryGetMetadata(MetadataName.ReasonPhrase, out var reason)
                        ? reason
                        : "unknown";

                logger.RateLimiterExceed(nameof(INationalBankApiClient), warningReason);

                return null;
            }

            try
            {
                var rate = await nationalBankApiClient.GetRateAsync(
                    currency.CurrencyId,
                    requestedDate.ToString(DateFormats.NationalBankApiRequest, CultureInfo.InvariantCulture),
                    requestCts.Token);

                if (rate == null || !rate.OfficialRate.HasValue)
                {
                    logger.LogWarning(
                        "NBRB rate is unavailable for currency {Abbreviation} ({CurrencyId}) on {RequestedDate}.",
                        currency.Abbreviation,
                        currency.CurrencyId,
                        requestedDate);

                    return null;
                }

                rate.CurrencyId = currency.CurrencyId;
                rate.Abbreviation = currency.Abbreviation;
                rate.Name = currency.Name;
                rate.Scale = currency.Scale;

                return rate;
            }
            catch (ApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                logger.LogWarning(
                    ex,
                    "NBRB returned 404 for currency {Abbreviation} ({CurrencyId}) on {RequestedDate}.",
                    currency.Abbreviation,
                    currency.CurrencyId,
                    requestedDate);

                return null;
            }
        }
    }
}
