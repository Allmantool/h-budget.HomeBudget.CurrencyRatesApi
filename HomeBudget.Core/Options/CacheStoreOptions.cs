namespace HomeBudget.Core.Options
{
    public record CacheStoreOptions
    {
        public int ExpirationInMinutes { get; set; }
    }
}
