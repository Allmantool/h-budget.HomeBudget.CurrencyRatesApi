namespace HomeBudget.Core.Models
{
    public class PollyRetryOptions
    {
        public int RetryCount { get; set; }
        public int SleepDurationInSeconds { get; set; }
    }
}
