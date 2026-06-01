using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ecommerce.Application.Common;

public sealed record OptionFeatureSnapshot(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("value")] string Value,
    [property: JsonPropertyName("description")] string? Description);

public static class OptionFeatureJson
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static string Serialize(IEnumerable<OptionFeatureSnapshot> features) =>
        JsonSerializer.Serialize(features, JsonOptions);

    public static List<OptionFeatureSnapshot> Deserialize(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        return JsonSerializer.Deserialize<List<OptionFeatureSnapshot>>(json, JsonOptions) ?? [];
    }
}
