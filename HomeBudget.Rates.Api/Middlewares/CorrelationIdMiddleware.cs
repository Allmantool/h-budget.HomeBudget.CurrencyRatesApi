using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using HomeBudget.Core.Constants;

namespace HomeBudget.Rates.Api.Middlewares
{
    internal class CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger, RequestDelegate next)
    {
        private static readonly EventId CorrelationIdPropagatedEventId =
            new(2001, "CorrelationIdPropagated");

        private static readonly EventId CorrelationIdMissingEventId =
            new(2002, "CorrelationIdMissing");

        private static readonly Action<ILogger, string, string, Exception> _logCorrelationIdPropagated =
            LoggerMessage.Define<string, string>(
                LogLevel.Information,
                CorrelationIdPropagatedEventId,
                "Propagate header '{HeaderName}' to response: {CorrelationId}");

        private static readonly Action<ILogger, string, Exception> _logCorrelationIdMissing =
            LoggerMessage.Define<string>(
                LogLevel.Warning,
                CorrelationIdMissingEventId,
                "The '{HeaderName}' has not been provided");

        public async Task InvokeAsync(HttpContext context)
        {
            var requestPath = context.Request.Path.Value;

            var isHealthCheckRequest = string.Equals(requestPath, "/health", StringComparison.OrdinalIgnoreCase);

            var requestHeaders = context.Request.Headers;

            if (requestHeaders.TryGetValue(HttpHeaderKeys.CorrelationId, out var correlationId))
            {
                context.Response.Headers.TryAdd(HttpHeaderKeys.CorrelationId, correlationId);

                _logCorrelationIdPropagated(logger, nameof(HttpHeaderKeys.CorrelationId), correlationId, null);
            }
            else
            {
                if (!isHealthCheckRequest)
                {
                    _logCorrelationIdMissing(logger, nameof(HttpHeaderKeys.CorrelationId), null);
                }
            }

            await next.Invoke(context);
        }
    }
}
