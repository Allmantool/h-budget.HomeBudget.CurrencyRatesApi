using System;

namespace HomeBudget.Rates.Api.Exceptions
{
    internal sealed class BadExternalApiRequestTimeoutException(string message = "External Api response timeout")
        : Exception(message)
    {
    }
}
