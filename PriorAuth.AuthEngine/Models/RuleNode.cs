using System.Text.Json;
using System.Text.Json.Serialization;

namespace PriorAuth.AuthEngine.Models;

public record RuleNode
{
    /// <summary>
    /// Null for simple rules. "conditional" for branching rules.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    // -------------------------
    // Simple rule fields
    // -------------------------

    [JsonPropertyName("field")]
    public string? Field { get; init; }

    [JsonPropertyName("operator")]
    public string? Operator { get; init; }

    /// <summary>
    /// The expected value. Stored as JsonElement to handle bool, numeric,
    /// and string values without boxing or a discriminated union.
    /// </summary>
    [JsonPropertyName("value")]
    public JsonElement? Value { get; init; }

    /// <summary>
    /// Explicit ordering for gte_ordered comparisons (e.g. ["Mild", "Moderate", "Severe"]).
    /// </summary>
    [JsonPropertyName("order")]
    public List<string>? Order { get; init; }

    // -------------------------
    // Conditional rule fields
    // -------------------------

    /// <summary>
    /// The condition to evaluate. Always a simple rule node (not itself a conditional).
    /// </summary>
    [JsonPropertyName("condition")]
    public RuleNode? Condition { get; init; }

    /// <summary>
    /// Rules to evaluate when Condition passes.
    /// </summary>
    [JsonPropertyName("then")]
    public List<RuleNode>? Then { get; init; }

    /// <summary>
    /// Rules to evaluate when Condition fails. May be empty (excused path).
    /// </summary>
    [JsonPropertyName("else")]
    public List<RuleNode>? Else { get; init; }

    // -------------------------
    // Helpers
    // -------------------------

    [JsonIgnore]
    public bool IsConditional => Type == "conditional";

    [JsonIgnore]
    public bool IsSimple => !IsConditional;
}