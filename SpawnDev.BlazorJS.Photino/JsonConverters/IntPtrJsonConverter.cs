using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpawnDev.BlazorJS.Photino.JsonConverters
{
    internal class IntPtrJsonConverter : JsonConverter<nint>
    {
        public override nint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default(nint);
            }
            var value = JsonSerializer.Deserialize<long>(ref reader, options);
            return new nint(value);
        }
        public override void Write(Utf8JsonWriter writer, nint value, JsonSerializerOptions options)
        {
            var sValue = value.ToInt64();
            JsonSerializer.Serialize(writer, sValue, options);
        }
    }
}
