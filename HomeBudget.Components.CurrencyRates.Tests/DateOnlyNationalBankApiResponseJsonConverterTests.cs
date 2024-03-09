using System;
using System.IO;

using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

using HomeBudget.Components.CurrencyRates.Configuration;
using Newtonsoft.Json.Linq;

namespace HomeBudget.Components.CurrencyRates.Tests
{
    [TestFixture]
    public class DateOnlyNationalBankApiResponseJsonConverterTests
    {
        [TestCase("17.03.2024 00:00:00", "17.03.2024")]
        [TestCase("14/09/2024 00:00:00", "14.09.2024")]
        public void ReadJson_WithNationBankApiResponseDateTimeFormat_ReturnsExpectedDateOnly(string apiDateTimeResponse, string expectedDayOnlyAsString)
        {
            var sut = new DateOnlyNationalBankApiResponseJsonConverter();

            var payload = JObject.FromObject(new { dateValue = apiDateTimeResponse });

            using var stringReader = new StringReader(payload.ToString());
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

            result.ToString().Should().Be(expectedDayOnlyAsString);
        }
    }
}
