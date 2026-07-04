using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace HomeBudget.Rates.Api.Extensions
{
    internal static class SwaggerExtensions
    {
        private const string DocumentName = "v1";
        private const string ApiTitle = "HomeBudget_Rates_Api";
        private const string BearerSchemeName = "Bearer";

        public static IServiceCollection SetupSwaggerGen(this IServiceCollection services)
        {
            return services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    DocumentName,
                    new OpenApiInfo
                    {
                        Title = ApiTitle,
                        Version = DocumentName
                    });

                options.CustomSchemaIds(type => type.FullName ?? type.Name);

                options.AddSecurityDefinition(
                    BearerSchemeName,
                    new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Description = "JWT Authorization header using the Bearer scheme.",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    });

                options.AddSecurityRequirement(
                    document => new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference(BearerSchemeName, document)] = []
                    });
            });
        }

        public static IApplicationBuilder SetUpSwaggerUi(this IApplicationBuilder app)
        {
            return app
                .UseSwagger(options =>
                {
                    options.RouteTemplate = "swagger/{documentName}/swagger.json";
                })
                .UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{DocumentName}/swagger.json",
                        $"{ApiTitle} {DocumentName}");

                    options.RoutePrefix = "swagger";
                });
        }
    }
}