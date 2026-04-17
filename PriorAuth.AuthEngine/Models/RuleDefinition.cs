using System.Text.Json;
using System.Text.Json.Serialization;

namespace PriorAuth.AuthEngine.Models;

public record RuleDefinition
{
    [JsonPropertyName("rules")]
    public List<RuleNode> Rules { get; init; } = [];

    /// <summary>
    /// Deserializes a RuleDefinition from the JSON column value stored on AuthRule.
    /// Returns null if the input is null or empty.
    /// </summary>
    public static RuleDefinition? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        return JsonSerializer.Deserialize<RuleDefinition>(json, JsonOptions);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
}