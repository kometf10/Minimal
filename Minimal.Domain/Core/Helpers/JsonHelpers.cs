using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minimal.Domain.Core.Helpers
{
    internal class JsonHelpers
    {
    }

    public class DateOnlyJsonConverter : JsonConverter<DateOnly?>
    {
        private const string Format = "MM-dd-yyyy";

        public override DateOnly? ReadJson(JsonReader reader, Type objectType, DateOnly? existingDate, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var val = (string)reader.Value!;
            if (string.IsNullOrEmpty(val))
                return null;
            return DateOnly.ParseExact((string)reader.Value!, Format, CultureInfo.InvariantCulture);
        }

        public override void WriteJson(JsonWriter writer, DateOnly? date, JsonSerializer serializer)
        {
            writer.WriteValue(date != null && date.HasValue ? date.Value.ToString(Format, CultureInfo.InvariantCulture) : "");
        }
    }
    public class TimeOnlyJsonConverter : JsonConverter<TimeOnly?>
    {

        public override TimeOnly? ReadJson(JsonReader reader, Type objectType, TimeOnly? existingDate, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            var val = (string)reader.Value!;
            if (string.IsNullOrEmpty(val))
                return null;
            return TimeOnly.Parse(val);
        }

        public override void WriteJson(JsonWriter writer, TimeOnly? date, JsonSerializer serializer)
        {
            writer.WriteValue(date != null && date.HasValue ? date.Value.ToString() : "");
        }
    }
}
