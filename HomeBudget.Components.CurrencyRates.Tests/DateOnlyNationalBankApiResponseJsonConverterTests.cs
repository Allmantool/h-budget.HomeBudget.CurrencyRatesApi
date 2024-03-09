using System;
using System.IO;

using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

using HomeBudget.Components.CurrencyRates.Configuration;

namespace HomeBudget.Components.CurrencyRates.Tests
{
    [TestFixture]
    public class DateOnlyNationalBankApiResponseJsonConverterTests
    {
        [Test]
        public void ReadJson_WithNationBankApiResponseDateTimeFormat_ReturnsExpectedDateOnly()
        {
            var sut = new DateOnlyNationalBankApiResponseJsonConverter();

            using var stringReader = new StringReader("{ 'dateValue': '01.03.2024 00:00:00' }");
            using var jsonReader = new JsonTextReader(stringReader);

            do
            {
                jsonReader.Read();
            }
            while (jsonReader.TokenType == JsonToken.EndObject || !(jsonReader.Path.Equals("dateValue", StringComparison.OrdinalIgnoreCase) && jsonReader.TokenType == JsonToken.String));

            var result = sut.ReadJson(
                jsonReader,
                typeof(string),
                DateOnly.MinValue,
                true,
                new JsonSerializer());

            result.Should().Be(new DateOnly(2024, 3, 1));
        }
    }
}
