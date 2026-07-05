using System.Collections.Generic;
using System.Text.RegularExpressions;

using HomeBudget.Rates.Api.Extensions;

namespace HomeBudget.Rates.Api.Tests.Extensions
{
    [TestFixture]
    internal class SwaggerExtensionsTests
    {
        [Test]
        public void CreateSchemaIdWithNestedTypeReturnsOpenApiSafeNamespaceQualifiedName()
        {
            var schemaId = SwaggerExtensions.CreateSchemaId(typeof(NestedResponse));

            Assert.That(schemaId, Does.Contain(nameof(SwaggerExtensionsTests)));
            Assert.That(schemaId, Does.Contain(nameof(NestedResponse)));
            Assert.That(schemaId, Does.Not.Contain("+"));
            Assert.That(IsOpenApiSafe(schemaId), Is.True);
        }

        [Test]
        public void CreateSchemaIdWithGenericTypeReturnsDeterministicNameWithGenericArguments()
        {
            var schemaId = SwaggerExtensions.CreateSchemaId(typeof(Dictionary<string, List<NestedResponse>>));
            var repeatedSchemaId = SwaggerExtensions.CreateSchemaId(typeof(Dictionary<string, List<NestedResponse>>));

            Assert.That(schemaId, Is.EqualTo(repeatedSchemaId));
            Assert.That(schemaId, Does.Contain("Dictionary"));
            Assert.That(schemaId, Does.Contain("String"));
            Assert.That(schemaId, Does.Contain("List"));
            Assert.That(schemaId, Does.Contain(nameof(NestedResponse)));
            Assert.That(schemaId, Does.Not.Contain("`"));
            Assert.That(IsOpenApiSafe(schemaId), Is.True);
        }

        private static bool IsOpenApiSafe(string schemaId)
            => Regex.IsMatch(schemaId, "^[A-Za-z0-9._-]+$");

        private sealed class NestedResponse
        {
        }
    }
}
