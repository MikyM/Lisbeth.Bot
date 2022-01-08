using System.Text.Json;
using System.Text.Json.Serialization;
using MikyM.Common.Utilities.Extensions;

namespace MikyM.Common.Utilities.Json;

/// <summary>
/// Creates instances of <see cref="NullableConverter{TValue}"/>.
/// </summary>
public class NullableConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsNullable();
    }
    
    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converter = (JsonConverter?)Activator.CreateInstance
        (
            typeof(NullableConverter<>).MakeGenericType(typeToConvert.GetGenericArguments())
        );

        if (converter is null)
        {
            throw new InvalidOperationException();
        }

        return converter;
    }
}