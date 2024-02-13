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
                    "Propagate header 'correlation id' to response: {correlationId}",
                    correlationId);
            }

            var responseHeaders = context.Response.Headers;

            responseHeaders.TryAdd(HttpHeaderKeys.CorrelationId, correlationId);

            await next.Invoke(context);
        }
    }
}
