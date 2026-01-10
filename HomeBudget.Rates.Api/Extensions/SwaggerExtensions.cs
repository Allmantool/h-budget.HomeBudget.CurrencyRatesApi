using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace HomeBudget.Rates.Api.Extensions
{
    internal static class SwaggerExtensions
    {
        public static IServiceCollection SetupSwaggerGen(this IServiceCollection services)
        {
            return services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new OpenApiInfo
                    {
                        Title = "HomeBudget_Rates_Api",
                        Version = "v1"
                    });

                options.CustomSchemaIds(type => type.ToString());

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT token with the prefix Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("bearer", document)] = []
                });
            });
        }

        public static IApplicationBuilder SetUpSwaggerUi(this IApplicationBuilder app)
        {
            return app
                .UseSwagger(options =>
                {
                    // Optional configuration
                    options.RouteTemplate = "swagger/{documentName}/swagger.json";
                })
                .UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "HomeBudget_Rates_Api v1");
                    options.RoutePrefix = "swagger"; // Optional: set custom route
                });
        }
    }
}