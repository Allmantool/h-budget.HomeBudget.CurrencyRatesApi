using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using HomeBudget.Core.Constants;
using HomeBudget.Core.Extensions;

namespace HomeBudget.Rates.Api.Exceptions.Handlers
{
    public class HttpLoggingHandler(ILogger<HttpLoggingHandler> logger) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            var correlationHeader = request.Headers
                .SingleOrDefault(h => h.Key.Contains(HttpHeaderKeys.CorrelationId, StringComparison.OrdinalIgnoreCase));

            var requestId = Guid.NewGuid();

            if (!correlationHeader.Value.IsNullOrEmpty())
            {
                var correlationId = correlationHeader.Value.SingleOrDefault();

                logger.LogInformation(
                    "[requestId: {requestId}] [CorrelationId: {correlationId}] Request: {request}",
                    requestId,
                    correlationId,
                    request);

                logger.LogInformation(
                    "[RequestId: {requestId}] [CorrelationId: {correlationId}] Response: {response}",
                    requestId,
                    correlationId,
                    response);
            }

            return response;
        }
    }
}
