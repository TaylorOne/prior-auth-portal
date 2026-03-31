using System.Text.Json;

namespace PriorAuthApi.Entities
{
    public class AuthRuleResponse
    {
        public int Id { get; set; }
        public RequestType RequestType { get; set; }
        public string CodeSystem { get; set; } = default!;
        public string Code { get; set; } = default!;
        public string? IndicationCode { get; set; }
        public string DisplayName { get; set; } = default!;
        public JsonElement FormDefinition { get; set; }
        public JsonElement RuleDefinition { get; set; }
        public bool IsActive { get; set; }
        public DateOnly EffectiveDate { get; set; }
        public DateOnly CreatedAt { get; set; }

        public static AuthRuleResponse FromEntity(AuthRule rule) => new()
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
