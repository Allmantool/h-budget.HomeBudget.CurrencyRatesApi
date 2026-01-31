namespace HomeBudget.Core.Options
{
    public record HttpClientOptions
    {
        public int MaxConcurrentRequests { get; init; } = 10;
        public int RateLimiterPermitLimit { get; init; } = 3;
        public int TimeoutInSeconds { get; init; } = 180;
        public int HandlerLifetimeInMinutes { get; init; } = 15;
        public int MaxRequestsPerSecond { get; init; } = 5;
        public int RateLimiterQueueLimit { get; init; } = 120;
        public double RateLimiterWindow { get; init; } = 1;
        public double PooledConnectionIdleTimeoutInMinutes { get; init; } = 2;
        public int RateLimiterBurstLimit { get; init; } = 10;
        public int RateLimiterTokensPerPeriod { get; init; } = 5;
        public double RateLimiterReplenishmentSeconds { get; init; } = 1;
    }
}
