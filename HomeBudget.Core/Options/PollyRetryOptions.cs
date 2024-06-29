namespace HomeBudget.Core.Options
{
    public record PollyRetryOptions
    {
        public int RetryCount { get; set; }
        public int SleepDurationInSeconds { get; set; }
    }
}
