using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using HomeBudget.Rates.Api.Exceptions;

namespace HomeBudget.Rates.Api.Middlewares
{
    internal class ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (BadExternalApiRequestTimeoutException exception)
            {
                logger.LogError(exception, "External Api request exception: {ErrorMessage}", exception.Message);

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status408RequestTimeout,
                    Title = "External Api request failed due to big latency"
                };

                context.Response.StatusCode =
                    StatusCodes.Status408RequestTimeout;

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception, "Exception occurred: {ErrorMessage}", exception.Message);

                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Server Error"
                };

                context.Response.StatusCode =
                    StatusCodes.Status500InternalServerError;

                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
