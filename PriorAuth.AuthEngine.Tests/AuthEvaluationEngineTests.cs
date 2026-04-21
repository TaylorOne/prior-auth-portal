using PriorAuth.AuthEngine.Models;
using PriorAuth.AuthEngine.Services;

namespace PriorAuth.AuthEngine.Tests;

public class AuthEvaluationEngineTests
{
    [Fact]
    public void Evaluate_EqualsBooleanRule_PassesWhenValueMatches()
    {
        var clinicalData = """{"priorNSAIDTrial": true}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "field": "priorNSAIDTrial",
                    "operator": "equals",
                    "value": true
                }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.Approved, decision.Outcome);
        Assert.Single(decision.RuleResults);
        Assert.True(decision.RuleResults[0].Passed);
    }

    [Fact]
    public void Evaluate_EqualsBooleanRule_FailsWhenValueDoesNotMatch()
    {
        var clinicalData = """{"priorNSAIDTrial": false}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "field": "priorNSAIDTrial",
                    "operator": "equals",
                    "value": true
                }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.Denied, decision.Outcome);
        Assert.Single(decision.RuleResults);
        Assert.False(decision.RuleResults[0].Passed);
    }

    [Fact]
    public void Evaluate_GreaterThanEqualRule_PassesWhenValueIsGreater()
    {
        var clinicalData = """{"dmardDurationWeeks": 14}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "field": "dmardDurationWeeks",
                    "operator": "gte",
                    "value": 14
                }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.Approved, decision.Outcome);
        Assert.Single(decision.RuleResults);
        Assert.True(decision.RuleResults[0].Passed);
    }

    [Fact]
    public void Evaluate_GreaterThanEqualRule_FailsWhenValueIsLess()
    {
        var clinicalData = """{"bmi": 15}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "field": "bmi",
                    "operator": "gte",
                    "value": 30
                }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.Denied, decision.Outcome);
        Assert.Single(decision.RuleResults);
        Assert.False(decision.RuleResults[0].Passed);
    }

    [Fact]
    public void Evaluate_MissingField_ReturnsNeedsMoreInfo()
    {
        var clinicalData = """{}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "field": "bmi",
                    "operator": "gte",
                    "value": 30
                }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.NeedsMoreInfo, decision.Outcome);
        Assert.Single(decision.RuleResults);
    }
}
