using System;
using System.Globalization;

using Newtonsoft.Json;

using HomeBudget.Core.Constants;

namespace HomeBudget.Components.CurrencyRates.Configuration
{
    internal class DateOnlyJsonConverter : JsonConverter<DateOnly>
    {
        public override DateOnly ReadJson(
            JsonReader reader,
            Type objectType,
            DateOnly existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            var originalValueAsString = reader.Value == null ? string.Empty : reader.Value.ToString();

            if (DateTime.TryParse(originalValueAsString, out var responseAsDateTime))
            {
                return DateOnly.FromDateTime(responseAsDateTime);
            }

            if (DateOnly.TryParse(originalValueAsString, out var valueAsDateOnly))
            {
                return valueAsDateOnly;
            }

            if (DateOnly.TryParseExact(
                    originalValueAsString,
                    DateFormats.NationalBankApiResponseWithDot,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var responseWithDotAsDateOnly))
            {
                return responseWithDotAsDateOnly;
            }

            if (DateOnly.TryParseExact(
                    originalValueAsString,
                    DateFormats.NationalBankApiResponseWithSlash,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var responseWithSlashAsDateOnly))
            {
                return responseWithSlashAsDateOnly;
            }

            throw new ArgumentException($"Invalid '{nameof(DateFormats.NationalBankApiRequest)}' date format payload. Invalid value: '{originalValueAsString}'");
        }

        public override void WriteJson(JsonWriter writer, DateOnly value, JsonSerializer serializer) =>
            writer.WriteValue(value.ToString(DateFormats.NationalBankApiRequest, CultureInfo.InvariantCulture));
    }
}
