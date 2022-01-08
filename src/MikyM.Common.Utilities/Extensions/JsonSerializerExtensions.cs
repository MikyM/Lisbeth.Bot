using System.Text.Json;
using System.Text.Json.Serialization;

namespace MikyM.Common.Utilities.Extensions;

public static class JsonSerializerExtensions
{
    /// <summary>
    /// Adds a JSON converter to the given json options.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <typeparam name="TConverter">The converter type.</typeparam>
    /// <returns>The added converter.</returns>
    public static JsonSerializerOptions AddConverter<TConverter>(this JsonSerializerOptions options)
        where TConverter : JsonConverter, new()
    {
        options.Converters.Insert(0, new TConverter());

        return options;
    }
}
