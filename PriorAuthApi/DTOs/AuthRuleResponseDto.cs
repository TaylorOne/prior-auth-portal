using System.Text.Json;
using PriorAuthApi.Entities;

namespace PriorAuthApi.DTOs
{
    public record AuthRuleResponseDto
    {
        public int Id { get; init; }
        public RequestType RequestType { get; init; }
        public string CodeSystem { get; init; } = default!;
        public string Code { get; init; } = default!;
        public string? IndicationCode { get; init; }
        public string DisplayName { get; init; } = default!;
        public JsonElement FormDefinition { get; init; }
        public JsonElement RuleDefinition { get; init; }
        public bool IsActive { get; init; }
        public DateOnly EffectiveDate { get; init; }
        public DateOnly CreatedAt { get; init; }

        public static AuthRuleResponseDto FromEntity(AuthRule rule) => new()
        {
            Id = rule.Id,
            RequestType = rule.RequestType,
            CodeSystem = rule.CodeSystem,
            Code = rule.Code,
            IndicationCode = rule.IndicationCode,
            DisplayName = rule.DisplayName,
            FormDefinition = JsonSerializer.Deserialize<JsonElement>(rule.FormDefinition),
            RuleDefinition = JsonSerializer.Deserialize<JsonElement>(rule.RuleDefinition),
            IsActive = rule.IsActive,
            EffectiveDate = rule.EffectiveDate,
            CreatedAt = rule.CreatedAt
        };
    }
}
