using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using HomeBudget.Core.Constants;

namespace HomeBudget.Rates.Api.Middlewares
{
    internal class CorrelationIdMiddleware(ILogger<CorrelationIdMiddleware> logger, RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var requestHeaders = context.Request.Headers;

            if (requestHeaders.TryGetValue(HttpHeaderKeys.CorrelationId, out var correlationId))
            {
                logger.LogInformation(
                    "Propagate header '{headerName}' to response: {correlationId}",
                    nameof(HttpHeaderKeys.CorrelationId),
                    correlationId);

                var responseHeaders = context.Response.Headers;

                responseHeaders.TryAdd(HttpHeaderKeys.CorrelationId, correlationId);
            }
            else
            {
                logger.LogWarning(
                    "The '{correlationId}' has not been provided",
                    nameof(HttpHeaderKeys.CorrelationId));
            }

            await next.Invoke(context);
        }
    }
}
