using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeBudget.Core.Extensions;
using HomeBudget.Rates.Api.Extensions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;

namespace HomeBudget.Rates.Api.Exceptions.Handlers
{
    internal sealed class GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment environment) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(
                exception,
                "Global exception. " +
                "Message: {ExceptionMessage}." +
                "Stack: {ErrorStack}",
                string.Join(',', exception.GetInnerExceptions().Select(ex => ex.Message)),
                exception.StackTrace);

            var maintenanceClosed = exception is SqlException sqlException
                && sqlException.Errors.Cast<SqlError>().Any(error => error.Number is 51030 or 51031);
            var problemDetails = new ProblemDetails
            {
                Status = maintenanceClosed
                    ? StatusCodes.Status503ServiceUnavailable
                    : StatusCodes.Status500InternalServerError,
                Title = maintenanceClosed
                    ? "Rates mutations are temporarily closed for maintenance"
                    : environment.IsUnderDevelopment()
                    ? $"Global exception. Message: {exception.Message}"
                    : "Server Error"
            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response
                .WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
