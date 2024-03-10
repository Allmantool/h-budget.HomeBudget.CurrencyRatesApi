using System;
using System.IO;

using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

using HomeBudget.Components.CurrencyRates.Configuration;
using HomeBudget.Components.CurrencyRates.Tests.TestSources;

namespace HomeBudget.Components.CurrencyRates.Tests
{
    [TestFixture]
    public class DateOnlyJsonConverterTests
    {
        [TestCaseSource(typeof(DateOnlyJsonConverterTestCases), nameof(DateOnlyJsonConverterTestCases.WithNationalBankApi))]
        public void ReadJson_WithNationBankApiResponseDateTimeFormat_ReturnsExpectedDateOnly(string apiDateTimeResponse, DateOnly expectedDayOnly)
        {
            var sut = new DateOnlyJsonConverter();

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

            result.Should().Be(expectedDayOnly);
        }
    }
}
