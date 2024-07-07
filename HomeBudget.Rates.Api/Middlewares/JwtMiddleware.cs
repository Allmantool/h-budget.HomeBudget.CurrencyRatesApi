using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace HomeBudget.Rates.Api.Middlewares
{
    internal class JwtMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Get the token from the Authorization header
            var bearer = context.Request.Headers["Authorization"].ToString();
            var token = bearer.Replace("Bearer ", string.Empty);

            if (!string.IsNullOrEmpty(token))
            {
                // Verify the token using the IJwtBuilder / call to identity api
                var userId = "validationResponse";

                if (Guid.TryParse(userId, out _))
                {
                    // Store the userId in the HttpContext items for later use
                    context.Items["userId"] = userId;
                }
                else
                {
                    // If token or userId are invalid, send 401 Unauthorized status
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                }
            }

            // Continue processing the request
            await next(context);
        }
    }
}
