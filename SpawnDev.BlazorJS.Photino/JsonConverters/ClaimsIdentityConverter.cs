using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpawnDev.BlazorJS.Photino.JsonConverters
{
    internal class ClaimsIdentityConverter : JsonConverter<ClaimsIdentity>
    {
        public override ClaimsIdentity Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = JsonSerializer.Deserialize<string>(ref reader, options);
            return string.IsNullOrWhiteSpace(value) ? default! : Base64ToClaimsIdentity(value);
        }
        public override void Write(Utf8JsonWriter writer, ClaimsIdentity value, JsonSerializerOptions options)
        {
            var sValue = ToBase64(value);
            JsonSerializer.Serialize(writer, sValue, options);
        }
        static string ToBase64(ClaimsIdentity claimsIdentity)
        {
            using var buffer = new MemoryStream();
            using var writer = new BinaryWriter(buffer);
            claimsIdentity.WriteTo(writer);
            var data = buffer.ToArray();
            return Convert.ToBase64String(data);
        }
        static ClaimsIdentity Base64ToClaimsIdentity(string claimsIdentity)
        {
            var data = Convert.FromBase64String(claimsIdentity);
            using var buffer = new MemoryStream(data);
            using var reader = new BinaryReader(buffer);
            return new ClaimsIdentity(reader);
        }
    }
}
