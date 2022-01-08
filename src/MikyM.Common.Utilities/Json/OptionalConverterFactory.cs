using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using MikyM.Common.Utilities.Optionals;

namespace MikyM.Common.Utilities.Json;

/// <summary>
/// Creates converters for <see cref="Optional{TValue}"/>.
/// </summary>
public class OptionalConverterFactory : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        var typeInfo = typeToConvert.GetTypeInfo();
        if (!typeInfo.IsGenericType || typeInfo.IsGenericTypeDefinition)
        {
            return false;
        }

        var genericType = typeInfo.GetGenericTypeDefinition();
        return genericType == typeof(Optional<>);
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var typeInfo = typeToConvert.GetTypeInfo();

        var optionalType = typeof(OptionalConverter<>).MakeGenericType(typeInfo.GenericTypeArguments);

        if (Activator.CreateInstance(optionalType) is not JsonConverter createdConverter)
        {
            throw new JsonException();
        }

        return createdConverter;
    }
}