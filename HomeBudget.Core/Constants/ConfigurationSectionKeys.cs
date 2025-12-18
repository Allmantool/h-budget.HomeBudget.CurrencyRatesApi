namespace HomeBudget.Core.Constants
{
    public static class ConfigurationSectionKeys
    {
        public static readonly string DatabaseOptions = nameof(DatabaseOptions);
        public static readonly string ExternalResourceUrls = nameof(ExternalResourceUrls);
        public static readonly string PollyRetryOptions = nameof(PollyRetryOptions);
        public static readonly string HttpClientOptions = nameof(HttpClientOptions);
        public static readonly string CacheStoreOptions = nameof(CacheStoreOptions);
        public static readonly string SeqOptions = nameof(SeqOptions);
        public static readonly string ElasticSearchOptions = nameof(ElasticSearchOptions);
    }
}
