using System;
using System.Globalization;

using Newtonsoft.Json;

using HomeBudget.Core.Constants;

namespace HomeBudget.Components.CurrencyRates.Configuration
{
    internal class DateOnlyNationalBankApiResponseJsonConverter : JsonConverter<DateOnly>
    {
        public override DateOnly ReadJson(
            JsonReader reader,
            Type objectType,
            DateOnly existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var originalValueAsString = reader.Value == null ? string.Empty : reader.Value.ToString();

            if (DateOnly.TryParseExact(originalValueAsString, DateFormats.NationalBankApiResponse, out var valueAsDateOnly))
            {
                return valueAsDateOnly;
            }

            throw new ArgumentException($"Invalid '{nameof(DateFormats.NationalBankApiRequest)}' date format payload. Invalid value: '{originalValueAsString}'");
        }

        public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer) =>
            writer.WriteValue(value.ToString(DateFormats.NationalBankApiRequest, CultureInfo.InvariantCulture));
    }
}
