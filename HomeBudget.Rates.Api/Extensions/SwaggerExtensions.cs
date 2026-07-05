using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
        private static readonly Regex InvalidSchemaIdCharacters = new("[^A-Za-z0-9._-]", RegexOptions.Compiled);

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

                options.CustomSchemaIds(CreateSchemaId);

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
                        [new OpenApiSecuritySchemeReference(BearerSchemeName, document)] = new List<string>()
                    });
            });
        }

        internal static string CreateSchemaId(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);

            return InvalidSchemaIdCharacters.Replace(CreateRawSchemaId(type), "_");
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

        private static string CreateRawSchemaId(Type type)
        {
            if (type.IsArray)
            {
                return $"{CreateRawSchemaId(type.GetElementType())}_Array";
            }

            if (!type.IsGenericType)
            {
                return RemoveGenericArity((type.FullName ?? type.Name).Replace('+', '.'));
            }

            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var genericTypeName = RemoveGenericArity((genericTypeDefinition.FullName ?? genericTypeDefinition.Name).Replace('+', '.'));
            var genericArgumentNames = string.Join("_And_", type.GetGenericArguments().Select(CreateRawSchemaId));

            return $"{genericTypeName}_Of_{genericArgumentNames}";
        }

        private static string RemoveGenericArity(string typeName)
        {
            var arityMarkerIndex = typeName.IndexOf('`', StringComparison.Ordinal);

            return arityMarkerIndex < 0
                ? typeName
                : typeName[..arityMarkerIndex];
        }
    }
}
