namespace HomeBudget.Core.Options
{
    public record HttpClientOptions
    {
        public int MaxConcurrentRequests { get; set; } = 10;
        public int TimeoutInSeconds { get; set; } = 180;
        public int HandlerLifetimeInMinutes { get; set; } = 15;
    }
}
