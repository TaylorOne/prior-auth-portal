using PriorAuth.AuthEngine.Models;
using System.Text.Json;

namespace PriorAuth.AuthEngine.Services;

public class AuthEvaluationEngine
{
    public AuthDecision Evaluate(string clinicalDataJson, string ruleDefinitionJson)
    {
        var ruleDefinition = RuleDefinition.FromJson(ruleDefinitionJson)
            ?? throw new ArgumentException("Invalid rule definition JSON.");

        var clinicalData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(clinicalDataJson)
            ?? throw new ArgumentException("Invalid clinical data JSON.");

        var results = new List<RuleResult>();
        
        foreach (var rule in ruleDefinition.Rules)
        {
            if (rule.IsConditional)
            {
                results.AddRange(EvaluateConditional(rule, clinicalData));
            }
            else
            {
                results.Add(EvaluateNode(rule, clinicalData));
            }
        }

        return AuthDecision.From(results);
    }

    private RuleResult EvaluateNode(RuleNode rule, Dictionary<string, JsonElement> clinicalData)
    {
        if (!clinicalData.TryGetValue(rule.Field!, out var fieldValue))
            return new RuleResult { Field = rule.Field!, Passed = false, FailureReason = FailureReasons.MissingField };

        return rule.Operator switch
        {
            "equals" => EvaluateEquals(rule, fieldValue),
            "gte" => EvaluateGreaterThanEqual(rule, fieldValue),
            "hasValue" => EvaluateHasValue(rule, fieldValue),
            "gte_ordered" => EvaluateGreaterThanEqualOrdered(rule, fieldValue),
            _ => throw new NotImplementedException()
        };
    }

    private List<RuleResult> EvaluateConditional(RuleNode rule, Dictionary<string, JsonElement> clinicalData)
    {
        var conditionResult = EvaluateNode(rule.Condition!, clinicalData);
        var branch = conditionResult.Passed ? rule.Then! : rule.Else!;
        
        return branch.Select(r => EvaluateNode(r, clinicalData)).ToList();
    }

    private RuleResult EvaluateEquals(RuleNode rule, JsonElement fieldValue)
    {
        var passed = fieldValue.ValueKind == JsonValueKind.True && rule.Value?.GetBoolean() == true
            || fieldValue.ValueKind == JsonValueKind.False && rule.Value?.GetBoolean() == false;

        return new RuleResult
        {
            Field = rule.Field!,
            Passed = passed,
            FailureReason = passed ? null : FailureReasons.BooleanRequirementNotMet(rule.Field!)
        };
    }

    private RuleResult EvaluateGreaterThanEqual(RuleNode rule, JsonElement fieldValue)
    {
        if (fieldValue.ValueKind != JsonValueKind.Number || !rule.Value!.Value.TryGetDouble(out var ruleValue))
        {
            return new RuleResult
            {
                Field = rule.Field!,
                Passed = false,
                FailureReason = FailureReasons.InvalidDataType(rule.Field!)
            };
        }

        var passed = fieldValue.GetDouble() >= ruleValue;

        return new RuleResult
        {
            Field = rule.Field!,
            Passed = passed,
            FailureReason = passed ? null : FailureReasons.ThresholdNotMet(rule.Field!, ruleValue, fieldValue.GetDouble())
        };
    }

    private RuleResult EvaluateHasValue(RuleNode rule, JsonElement fieldValue)
    {
        var passed = fieldValue.ValueKind != JsonValueKind.Null && fieldValue.ValueKind != JsonValueKind.Undefined && !string.IsNullOrEmpty(fieldValue.GetString());

        return new RuleResult
        {
            Field = rule.Field!,
            Passed = passed,
            FailureReason = passed ? null : FailureReasons.ValueRequired(rule.Field!)
        };
    }

    private RuleResult EvaluateGreaterThanEqualOrdered(RuleNode rule, JsonElement fieldValue)
    {
        if (fieldValue.ValueKind != JsonValueKind.String || rule.Order == null)
        {
            return new RuleResult
            {
                Field = rule.Field!,
                Passed = false,
                FailureReason = FailureReasons.InvalidDataType(rule.Field!)
            };
        }

        var fieldStringValue = fieldValue.GetString();
        var ruleValue = rule.Value?.GetString();

        if (fieldStringValue == null || ruleValue == null)
        {
            return new RuleResult
            {
                Field = rule.Field!,
                Passed = false,
                FailureReason = FailureReasons.ValueRequired(rule.Field!)
            };
        }

        var fieldIndex = rule.Order.IndexOf(fieldStringValue);
        var ruleIndex = rule.Order.IndexOf(ruleValue);

        if (fieldIndex == -1 || ruleIndex == -1)
        {
            return new RuleResult
            {
                Field = rule.Field!,
                Passed = false,
                FailureReason = FailureReasons.ValueNotFound(rule.Field!)
            };
        }

        var passed = fieldIndex >= ruleIndex;

        return new RuleResult
        {
            Field = rule.Field!,
            Passed = passed,
            FailureReason = passed ? null : FailureReasons.OrderedThresholdNotMet(rule.Field!, ruleValue, fieldStringValue)
        };
    }
}