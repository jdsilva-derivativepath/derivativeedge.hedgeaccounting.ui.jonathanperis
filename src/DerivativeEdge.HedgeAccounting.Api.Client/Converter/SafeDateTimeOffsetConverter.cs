using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerivativeEdge.HedgeAccounting.Api.Client.Converter
{
    public class SafeDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
    {
        public override void WriteJson(JsonWriter writer, DateTimeOffset? value, JsonSerializer serializer)
        {
            if (value.HasValue && value.Value != DateTimeOffset.MinValue)
            {
                writer.WriteValue(value.Value);
            }
            else
            {
                writer.WriteNull();
            }
        }

        public override DateTimeOffset? ReadJson(JsonReader reader, Type objectType, DateTimeOffset? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            var stringValue = reader.Value.ToString();

            // Handle common invalid date patterns
            if (string.IsNullOrWhiteSpace(stringValue) ||
                stringValue == "0001-01-01" ||
                stringValue.StartsWith("0001-01-01"))
            {
                return null;
            }

            if (DateTimeOffset.TryParse(stringValue, out var result))
            {
                return result;
            }

            return null;
        }
    }
}
