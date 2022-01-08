using System.Text.Json;
using System.Text.Json.Serialization;
using MikyM.Common.Utilities.Optionals;

namespace MikyM.Common.Utilities.Json;

/// <summary>
/// Converts optional fields to their JSON representation.
/// </summary>
/// <typeparam name="TValue">The underlying type.</typeparam>
public class OptionalConverter<TValue> : JsonConverter<Optional<TValue?>>
{
    /// <inheritdoc />
    public override Optional<TValue?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new(JsonSerializer.Deserialize<TValue>(ref reader, options));
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Optional<TValue?> value, JsonSerializerOptions options)
    {
        if (value.Value is null)
        {
            writer.WriteNullValue();
            return;
        }

        JsonSerializer.Serialize(writer, value.Value, options);
    }
}
