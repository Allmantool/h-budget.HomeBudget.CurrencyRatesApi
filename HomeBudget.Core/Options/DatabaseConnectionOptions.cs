namespace HomeBudget.Core.Options
{
    public record DatabaseConnectionOptions
    {
        public string ConnectionString { get; set; }
        public string RedisConnectionString { get; set; }
    }
}
