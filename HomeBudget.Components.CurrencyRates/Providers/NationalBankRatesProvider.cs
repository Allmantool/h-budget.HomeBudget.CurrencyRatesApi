using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using HomeBudget.Components.CurrencyRates.Clients;
using HomeBudget.Components.CurrencyRates.Logs;
using HomeBudget.Components.CurrencyRates.Models;
using HomeBudget.Components.CurrencyRates.Models.Api;
using HomeBudget.Components.CurrencyRates.Providers.Interfaces;
using HomeBudget.Core.Constants;
using HomeBudget.Core.Extensions;
using HomeBudget.Core.Limiters;
using HomeBudget.Core.Models;
using HomeBudget.Core.Options;

namespace HomeBudget.Components.CurrencyRates.Providers
{
    internal class NationalBankRatesProvider : INationalBankRatesProvider
    {
        private readonly ILogger<NationalBankRatesProvider> logger;
        private readonly HttpClientOptions httpClientOptions;
        private readonly IHttpClientRateLimiter nationalBankHttpRateLimiter;
        private readonly ConfigSettings configSettings;
        private readonly INationalBankApiClient nationalBankApiClient;

        public NationalBankRatesProvider(
            ConfigSettings configSettings,
            ILogger<NationalBankRatesProvider> logger,
            IOptions<HttpClientOptions> httpOptions,
            INationalBankApiClient nationalBankApiClient,
            IHttpClientRateLimiter nationalBankHttpClientRateLimiter)
        {
            this.httpClientOptions = httpOptions.Value;
            this.configSettings = configSettings;
            this.nationalBankApiClient = nationalBankApiClient;
            this.nationalBankHttpRateLimiter = nationalBankHttpClientRateLimiter;
            this.logger = logger;
        }

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

                    foreach (var rate in rates)
                    {
                        ratesForPeriod.Add(rate);
                    }
                });

            return ratesForPeriod.ToList();
        }

        public async Task<IReadOnlyCollection<NationalBankCurrencyRate>> GetTodayActiveRatesAsync(CancellationToken ct = default)
        {
            var activeCurrencyAbbreviations = configSettings
                .ActiveNationalBankCurrencies
                .Select(i => i.Abbreviation);

            var todayRatesFromApi = await nationalBankApiClient.GetTodayRatesAsync();

            return todayRatesFromApi
                .Where(r => activeCurrencyAbbreviations.Contains(r.Abbreviation, StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        private static IReadOnlyCollection<YearRatesRequestPayload> GetYearPeriodRequests(
            IEnumerable<int> currenciesIds,
            PeriodRange period)
        {
            var yearRanges = period.SplitByYear();

            return currenciesIds
                .SelectMany(
                    id => yearRanges,
                    (id, range) => new YearRatesRequestPayload
                    {
                        CurrencyId = id,
                        Period = range
                    })
                .ToList();
        }
    }
}
