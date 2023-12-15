using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using HomeBudget.Core.Constants;

namespace HomeBudget.Rates.Api.Middlewares
{
    internal class CorrelationIdMiddleware(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var requestHeaders = context.Request.Headers;

            _ = requestHeaders.TryGetValue(HttpHeaderKeys.CorrelationIdHeaderKey, out var correlationId);

            var responseHeaders = context.Response.Headers;

            responseHeaders.TryAdd(HttpHeaderKeys.CorrelationIdHeaderKey, correlationId);

            await next.Invoke(context);
        }
    }
}
