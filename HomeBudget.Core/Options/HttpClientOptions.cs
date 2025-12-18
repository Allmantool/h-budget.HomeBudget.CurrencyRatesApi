namespace HomeBudget.Core.Options
{
    public record HttpClientOptions
    {
        public int MaxConcurrentRequests { get; set; } = 10;
    }
}
