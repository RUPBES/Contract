using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MvcLayer.Models.JSONSerializer
{
    public class DateFormatConverter : JsonConverter<DateTime>
    {
        private const string Format = "dd.MM.yyyy";

        public override DateTime Read(ref Utf8JsonReader reader,
            Type typeToConvert, JsonSerializerOptions options)
            => DateTime.ParseExact(reader.GetString()!, Format,
                CultureInfo.InvariantCulture);

        public override void Write(Utf8JsonWriter writer,
            DateTime value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString(Format, CultureInfo.InvariantCulture));
    }

    public class NullableDateFormatConverter : JsonConverter<DateTime?>
    {
        private const string Format = "dd.MM.yyyy";

        public override DateTime? Read(ref Utf8JsonReader reader,
            Type typeToConvert, JsonSerializerOptions options)
        {
            var str = reader.GetString();
            if (string.IsNullOrEmpty(str)) return null;
            return DateTime.ParseExact(str, Format, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer,
            DateTime? value, JsonSerializerOptions options)
        {
            if (value is null) writer.WriteNullValue();
            else writer.WriteStringValue(value.Value.ToString(Format, CultureInfo.InvariantCulture));
        }
    }
}
