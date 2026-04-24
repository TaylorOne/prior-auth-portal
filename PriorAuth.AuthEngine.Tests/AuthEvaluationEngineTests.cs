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

    [Fact]
    public void Evaluate_HasValueRule_PassesWhenValuePresent()
    {
        var clinicalData = """{"reasonForSwitch": "Drug interaction"}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "field": "reasonForSwitch",
                    "operator": "hasValue"
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
    public void Evaluate_HasValue_FailsWhenValueEmpty()
    {
        var clinicalData = """{"reasonForSwitch": ""}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "field": "reasonForSwitch",
                    "operator": "hasValue"
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
    public void Evaluate_HasValue_FailsWhenValueNull()
    {
        var clinicalData = """{"reasonForSwitch": null}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "field": "reasonForSwitch",
                    "operator": "hasValue"
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
    public void Evaluate_GreaterThanEqual_OrderedRule_Passes()
    {
        var clinicalData = """{"diseaseClassification": "Moderate"}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "field": "diseaseClassification",
                    "operator": "gte_ordered",
                    "value": "Moderate",
                    "order": [
                        "Mild",
                        "Moderate",
                        "Severe"
                    ]
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
    public void Evaluate_GreaterThanEqual_OrderedRule_Fails()
    {
        var clinicalData = """{"diseaseClassification": "Mild"}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "field": "diseaseClassification",
                    "operator": "gte_ordered",
                    "value": "Moderate",
                    "order": [
                        "Mild",
                        "Moderate",
                        "Severe"
                    ]
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
    public void Evaluate_ConditionalRuleTrue_ThenPasses()
    {
        var clinicalData = """{"comorbidity": true, "bmi": 28}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "type": "conditional",
                    "condition": {
                        "field": "comorbidity",
                        "operator": "equals",
                        "value": true
                    },
                    "then": [
                        {
                            "field": "bmi",
                            "operator": "gte",
                            "value": 27
                        }
                    ],
                    "else": [
                        {
                            "field": "bmi",
                            "operator": "gte",
                            "value": 30
                        }
                    ]
                }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.Approved, decision.Outcome);
        Assert.True(decision.RuleResults[0].Passed);
    }

    [Fact]
    public void Evaluate_ConditionalRuleTrue_ThenFails()
    {
        var clinicalData = """{"comorbidity": true, "bmi": 25}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "type": "conditional",
                    "condition": {
                        "field": "comorbidity",
                        "operator": "equals",
                        "value": true
                    },
                    "then": [
                        {
                            "field": "bmi",
                            "operator": "gte",
                            "value": 27
                        }
                    ],
                    "else": [
                        {
                            "field": "bmi",
                            "operator": "gte",
                            "value": 30
                        }
                    ]
                }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.Denied, decision.Outcome);
        Assert.False(decision.RuleResults[0].Passed);
    }

    [Fact]
    public void Evaluate_ConditionalRuleFalse_ElsePasses()
    {
        var clinicalData = """{"comorbidity": false, "bmi": 32}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "type": "conditional",
                    "condition": {
                        "field": "comorbidity",
                        "operator": "equals",
                        "value": true
                    },
                    "then": [
                        {
                            "field": "bmi",
                            "operator": "gte",
                            "value": 27
                        }
                    ],
                    "else": [
                        {
                            "field": "bmi",
                            "operator": "gte",
                            "value": 30
                        }
                    ]
                }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.Approved, decision.Outcome);
        Assert.True(decision.RuleResults[0].Passed);
    }

    [Fact]
    public void Evaluate_ConditionalRuleFalse_ElseFails()
    {
        var clinicalData = """{"comorbidity": false, "bmi": 28}""";

        var ruleDefinition = """
        {
            "rules": [
                {
                    "type": "conditional",
                    "condition": {
                        "field": "comorbidity",
                        "operator": "equals",
                        "value": true
                    },
                    "then": [
                        {
                            "field": "bmi",
                            "operator": "gte",
                            "value": 27
                        }
                    ],
                    "else": [
                        {
                            "field": "bmi",
                            "operator": "gte",
                            "value": 30
                        }
                    ]
                }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.Denied, decision.Outcome);
        Assert.False(decision.RuleResults[0].Passed);
    }

    [Fact]
    public void Evaluate_HumiraRA_AllRulesPass_Approved()
    {
        var clinicalData = """
        {
            "priorDMARDTrial": true,
            "dmardName": "Methotrexate",
            "dmardDurationWeeks": 14,
            "notes": "Patient tolerated well"
        }
        """;

        var ruleDefinition = """
        {
            "rules": [
                { "field": "priorDMARDTrial", "operator": "equals", "value": true },
                { "field": "dmardDurationWeeks", "operator": "gte", "value": 12 }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.Approved, decision.Outcome);
        Assert.All(decision.RuleResults, r => Assert.True(r.Passed));
    }

    [Fact]
    public void Evaluate_HumiraRA_DMARDweeksInsufficient_Denied()
    {
        var clinicalData = """
        {
            "priorDMARDTrial": true,
            "dmardName": "Methotrexate",
            "dmardDurationWeeks": 8,
            "notes": "Patient tolerated well"
        }
        """;

        var ruleDefinition = """
        {
            "rules": [
                { "field": "priorDMARDTrial", "operator": "equals", "value": true },
                { "field": "dmardDurationWeeks", "operator": "gte", "value": 12 }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.Denied, decision.Outcome);
        Assert.Equal(2, decision.RuleResults.Count);
        Assert.True(decision.RuleResults[0].Passed);   // priorDMARDTrial = true, passes
        Assert.False(decision.RuleResults[1].Passed);  // dmardDurationWeeks 8 < 12, fails
        Assert.NotNull(decision.RuleResults[1].FailureReason);
    }

    [Fact]
    public void Evaluate_Ozempic_Contraindicated_EmptyElse_Approved()
    {
        var clinicalData = """
        {
            "hba1c": 9.5,
            "metforminContraindicated": true,
            "diabetesEducationCompleted": true
        }
        """;

        var ruleDefinition = """
        {
            "rules": [
                { "field": "hba1c", "operator": "gte", "value": 7.0 },
                {
                    "type": "conditional",
                    "condition": { "field": "metforminContraindicated", "operator": "equals", "value": false },
                    "then": [
                        { "field": "priorMetforminTrial", "operator": "equals", "value": true },
                        { "field": "metforminDurationWeeks", "operator": "gte", "value": 12 }
                    ],
                "else": []
                },
                { "field": "diabetesEducationCompleted", "operator": "equals", "value": true }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.Approved, decision.Outcome);
        Assert.Equal(2, decision.RuleResults.Count); // hba1c + diabetesEducation, conditional contributes nothing
        Assert.All(decision.RuleResults, r => Assert.True(r.Passed));
    }

    [Fact]
    public void Evaluate_Wegovy_ComorbidityTrue_BMIThresholdLower_Approved()
    {
        var clinicalData = """
        {
            "bmi": 28,
            "comorbidity": true,
            "priorWeightLossProgram": true
        }
        """;

        var ruleDefinition = """
        {
            "rules": [
                {
                    "type": "conditional",
                    "condition": { "field": "comorbidity", "operator": "equals", "value": true },
                    "then": [
                        { "field": "bmi", "operator": "gte", "value": 27 }
                    ],
                    "else": [
                        { "field": "bmi", "operator": "gte", "value": 30 }
                    ]
                },
                { "field": "priorWeightLossProgram", "operator": "equals", "value": true }
            ]
        }
        """;

        var engine = new AuthEvaluationEngine();
        var decision = engine.Evaluate(clinicalData, ruleDefinition);

        Assert.Equal(AuthOutcome.Approved, decision.Outcome);
        Assert.Equal(2, decision.RuleResults.Count);
        Assert.All(decision.RuleResults, r => Assert.True(r.Passed));
    }
}
