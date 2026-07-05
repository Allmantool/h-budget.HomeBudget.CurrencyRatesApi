using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using HomeBudget.Rates.Api.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace HomeBudget.Rates.Api.Tests.Swagger
{
    [TestFixture]
    internal class SwaggerEndpointTests
    {
        [Test]
        public async Task GetSwaggerJsonWithApplicationFactoryReturnsOpenApiDefinition()
        {
            await using var factory = new WebApplicationFactory<HomeBudget.Rates.Api.Program>();
            await using var configuredFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(HostEnvironments.Integration);
                builder.ConfigureAppConfiguration((_, configuration) =>
                {
                    configuration.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        ["DatabaseOptions:ConnectionString"] = string.Empty,
                        ["DatabaseOptions:RedisConnectionString"] = string.Empty,
                        ["UiOriginsUrl:0"] = "*"
                    });
                });
            });

            using var client = configuredFactory.CreateClient();

            using var response = await client.GetAsync(new System.Uri("/swagger/v1/swagger.json", System.UriKind.Relative));
            var content = await response.Content.ReadAsStringAsync();

            Assert.Multiple(() =>
            {
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                Assert.That(content, Does.Contain("\"openapi\""));
                Assert.That(content, Does.Contain("HomeBudget_Rates_Api"));
            });
        }
    }
}
