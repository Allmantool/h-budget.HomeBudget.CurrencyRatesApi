using Microsoft.Extensions.Logging;

namespace HomeBudget.Components.CurrencyRates.Logs
{
    internal static partial class NationalBankRatesProviderLogs
    {
        [LoggerMessage(
            EventId = 1001,
            Level = LogLevel.Warning,
            Message = "Rate limit exceeded for {Client}. Reason: {Reason}")]
        public static partial void RateLimiterExceed(
            this ILogger logger,
            string client,
            string reason);
    }
}
