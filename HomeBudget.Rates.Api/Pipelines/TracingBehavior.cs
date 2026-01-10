using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using MediatR;
using OpenTelemetry.Trace;

using HomeBudget.Core;
using HomeBudget.Core.Constants;

namespace HomeBudget.Rates.Api.Pipelines
{
    internal sealed class TracingBehavior<TRequest, TResponse>
        : IPipelineBehavior<TRequest, TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;

            using var activity = Observability.ActivitySource.StartActivity(
                $"MediatR {requestName}",
                ActivityKind.Internal);

            activity?.SetTag(ActivityTags.MediatorRequest, requestName);
            activity?.SetTag(ActivityTags.MediatorRequestType, typeof(TRequest).FullName);

            try
            {
                var response = await next();

                activity?.SetStatus(ActivityStatusCode.Ok);
                return response;
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
        }
    }
}
