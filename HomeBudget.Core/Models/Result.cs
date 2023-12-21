namespace HomeBudget.Core.Models
{
    public class Result<T>(T payload = default, string message = default, bool isSucceeded = default)
    {
        public T Payload { get; } = payload;
        public bool IsSucceeded { get; } = isSucceeded;
        public string Message { get; } = message;
    }
}
