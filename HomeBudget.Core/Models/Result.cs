namespace HomeBudget.Core.Models
{
    public class Result<T>(
        T payload,
        string errorMessage,
        bool isSucceeded)
    {
        public T Payload { get; private set; } = payload;
        public bool IsSucceeded { get; private set; } = isSucceeded;
        public string ErrorMessage { get; private set; } = errorMessage;
    }

    public static class Result
    {
        public static Result<T> Succeeded<T>(T payload) => new(payload, null, true);
        public static Result<T> Failure<T>(string errorMessage = default) => new(default, errorMessage, false);
    }
}
