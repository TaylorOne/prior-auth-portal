using System.Text.Json;
using FluentAssertions;
using PriorAuth.Data.Entities;
using PriorAuthApi.DTOs;
using PriorAuthApi.Validators;

namespace PriorAuthApi.Tests.Unit.Validators
{
    public class PriorAuthRequestValidatorTests
    {
        [Fact]
        public void Should_Pass_With_Valid_Humira_Ra_ClinicalData()
        {
            var scenario = HumiraRaScenario();

            var result = Validate(scenario);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Pass_With_Valid_Mri_Knee_ClinicalData()
        {
            var scenario = MriKneeScenario();

            var result = Validate(scenario);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Pass_With_Valid_Wegovy_ClinicalData()
        {
            var scenario = WegovyScenario();

            var result = Validate(scenario);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Pass_With_Wegovy_When_No_Comorbidity_And_Bmi_At_Threshold()
        {
            var scenario = WegovyScenario();
            var data = CopyClinicalData(scenario.ValidClinicalData);
            data["comorbidity"] = Json("false");
            data["bmi"] = Json("30.0");

            var result = Validate(scenario, data);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Pass_With_Wegovy_When_Notes_Absent()
        {
            var scenario = WegovyScenario();
            var data = CopyClinicalData(scenario.ValidClinicalData);
            data.Remove("notes");

            var result = Validate(scenario, data);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Fail_When_Wegovy_Bmi_Is_Missing()
        {
            var scenario = WegovyScenario();
            var data = CopyClinicalData(scenario.ValidClinicalData);
            data.Remove("bmi");

            var result = Validate(scenario, data);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("bmi"));
        }

        [Fact]
        public void Should_Fail_When_Wegovy_Bmi_Exceeds_Max()
        {
            var scenario = WegovyScenario();
            var data = CopyClinicalData(scenario.ValidClinicalData);
            data["bmi"] = Json("81");

            var result = Validate(scenario, data);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("bmi"));
        }

        [Fact]
        public void Should_Fail_When_Wegovy_Bmi_Below_Min()
        {
            var scenario = WegovyScenario();
            var data = CopyClinicalData(scenario.ValidClinicalData);
            data["bmi"] = Json("9");

            var result = Validate(scenario, data);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("bmi"));
        }

        [Fact]
        public void Should_Fail_When_Wegovy_Comorbidity_Is_Missing()
        {
            var scenario = WegovyScenario();
            var data = CopyClinicalData(scenario.ValidClinicalData);
            data.Remove("comorbidity");

            var result = Validate(scenario, data);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("comorbidity"));
        }

        [Fact]
        public void Should_Fail_When_Wegovy_PriorWeightLossProgram_Is_Missing()
        {
            var scenario = WegovyScenario();
            var data = CopyClinicalData(scenario.ValidClinicalData);
            data.Remove("priorWeightLossProgram");

            var result = Validate(scenario, data);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("priorWeightLossProgram"));
        }

        [Fact]
        public void Should_Pass_With_Valid_Ozempic_ClinicalData()
        {
            var scenario = OzempicScenario();

            var result = Validate(scenario);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Pass_With_Ozempic_When_Optional_Metformin_Duration_Absent()
        {
            var scenario = OzempicScenario();
            var data = CopyClinicalData(scenario.ValidClinicalData);
            data["priorMetforminTrial"] = Json("false");
            data["metforminContraindicated"] = Json("true");
            data.Remove("metforminDurationWeeks");

            var result = Validate(scenario, data);

            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Fail_When_Required_Boolean_Field_Missing()
        {
            var scenario = HumiraRaScenario();
            var data = CopyClinicalData(scenario.ValidClinicalData);
            data.Remove("priorDMARDTrial");

            var result = Validate(scenario, data);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("priorDMARDTrial"));
        }

        [Fact]
        public void Should_Fail_When_Select_Value_Not_In_AllowedValues()
        {
            var scenario = HumiraRaScenario();
            var data = CopyClinicalData(scenario.ValidClinicalData);
            data["dmardName"] = Json("\"Aspirin\"");

            var result = Validate(scenario, data);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("dmardName"));
        }

        [Fact]
        public void Should_Fail_When_Number_Exceeds_Max()
        {
            var scenario = HumiraRaScenario();
            var data = CopyClinicalData(scenario.ValidClinicalData);
            data["dmardDurationWeeks"] = Json("999");

            var result = Validate(scenario, data);

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("dmardDurationWeeks"));
        }

        [Fact]
        public void Should_Pass_When_Optional_Field_Is_Absent()
        {
            var scenario = HumiraRaScenario();
            var data = CopyClinicalData(scenario.ValidClinicalData);
            data.Remove("notes");

            var result = Validate(scenario, data);

            result.IsValid.Should().BeTrue();
        }

        private static FluentValidation.Results.ValidationResult Validate(
            ValidatorScenario scenario,
            Dictionary<string, JsonElement>? clinicalData = null)
        {
            var validator = new PriorAuthRequestValidator(scenario.AuthRule);
            return validator.Validate(ValidDto(scenario, clinicalData));
        }

        private static SubmitPriorAuthDto ValidDto(
            ValidatorScenario scenario,
            Dictionary<string, JsonElement>? clinicalData = null) =>
            new(
                Priority: "routine",
                Code: new CodeableConceptDto(
                    scenario.AuthRule.CodeSystem,
                    scenario.AuthRule.Code,
                    scenario.AuthRule.DisplayName),
                PatientId: 1,
                PractitionerId: 1,
                ReasonCode: [scenario.AuthRule.IndicationCode],
                ClinicalData: clinicalData ?? CopyClinicalData(scenario.ValidClinicalData),
                MedicationRequest: null
            );

        private static ValidatorScenario HumiraRaScenario() =>
            new(
                AuthRule: new AuthRule
                {
                    Code = "J0135",
                    CodeSystem = "HCPCS",
                    DisplayName = "Adalimumab injection",
                    IndicationCode = "M06.9",
                    FormDefinition = """
                    {
                        "fields": [
                            {
                                "name": "priorDMARDTrial",
                                "type": "boolean",
                                "validation": { "required": true }
                            },
                            {
                                "name": "dmardName",
                                "type": "select",
                                "validation": {
                                    "required": true,
                                    "allowedValues": ["Methotrexate", "Hydroxychloroquine", "Sulfasalazine", "Leflunomide"]
                                }
                            },
                            {
                                "name": "dmardDurationWeeks",
                                "type": "number",
                                "validation": { "required": true, "min": 0, "max": 104, "integer": true }
                            },
                            {
                                "name": "notes",
                                "type": "text",
                                "validation": { "required": false, "maxLength": 1000 }
                            }
                        ],
                        "medicationFields": []
                    }
                    """,
                    RuleDefinition = """
                    {
                        "priorDMARDRequired": true,
                        "minDMARDWeeks": 12
                    }
                    """,
                },
                ValidClinicalData: new Dictionary<string, JsonElement>
                {
                    ["priorDMARDTrial"] = Json("true"),
                    ["dmardName"] = Json("\"Methotrexate\""),
                    ["dmardDurationWeeks"] = Json("16"),
                    ["notes"] = Json("\"Patient tolerated poorly.\"")
                });

        private static ValidatorScenario MriKneeScenario() =>
            new(
                AuthRule: new AuthRule
                {
                    Code = "73721",
                    CodeSystem = "CPT",
                    DisplayName = "MRI Knee without Contrast",
                    IndicationCode = "M25.561",
                    FormDefinition = """
                    {
                        "fields": [
                            {
                                "name": "therapyCompleted",
                                "type": "boolean",
                                "validation": { "required": true }
                            },
                            {
                                "name": "therapyDurationWeeks",
                                "type": "number",
                                "validation": { "required": true, "min": 0, "max": 52, "integer": true }
                            },
                            {
                                "name": "notes",
                                "type": "text",
                                "validation": { "required": false, "maxLength": 1000 }
                            }
                        ],
                        "medicationFields": []
                    }
                    """,
                    RuleDefinition = """
                    {
                        "rules": [
                            { "field": "therapyCompleted", "operator": "equals", "value": true },
                            { "field": "therapyDurationWeeks", "operator": "gte", "value": 6 }
                        ]
                    }
                    """,
                },
                ValidClinicalData: new Dictionary<string, JsonElement>
                {
                    ["therapyCompleted"] = Json("true"),
                    ["therapyDurationWeeks"] = Json("8"),
                    ["notes"] = Json("\"Symptoms persist despite conservative therapy.\"")
                });

        private static ValidatorScenario OzempicScenario() =>
            new(
                AuthRule: new AuthRule
                {
                    Code = "J3101",
                    CodeSystem = "HCPCS",
                    DisplayName = "Ozempic (semaglutide)",
                    IndicationCode = "E11.9",
                    FormDefinition = """
                    {
                        "fields": [
                            {
                                "name": "hba1c",
                                "type": "number",
                                "validation": { "required": true, "min": 4.0, "max": 20.0, "integer": false }
                            },
                            {
                                "name": "priorMetforminTrial",
                                "type": "boolean",
                                "validation": { "required": true }
                            },
                            {
                                "name": "metforminDurationWeeks",
                                "type": "number",
                                "validation": { "required": false, "min": 0, "max": 104, "integer": true }
                            },
                            {
                                "name": "metforminContraindicated",
                                "type": "boolean",
                                "validation": { "required": true }
                            },
                            {
                                "name": "diabetesEducationCompleted",
                                "type": "boolean",
                                "validation": { "required": true }
                            }
                        ],
                        "medicationFields": []
                    }
                    """,
                    RuleDefinition = """
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
                    """,
                },
                ValidClinicalData: new Dictionary<string, JsonElement>
                {
                    ["hba1c"] = Json("8.2"),
                    ["priorMetforminTrial"] = Json("true"),
                    ["metforminDurationWeeks"] = Json("12"),
                    ["metforminContraindicated"] = Json("false"),
                    ["diabetesEducationCompleted"] = Json("true")
                });

        private static ValidatorScenario WegovyScenario() =>
            new(
                AuthRule: new AuthRule
                {
                    Code = "J3490",
                    CodeSystem = "HCPCS",
                    DisplayName = "Wegovy (semaglutide)",
                    IndicationCode = "E66.9",
                    FormDefinition = """
                    {
                        "fields": [
                            {
                                "name": "bmi",
                                "type": "number",
                                "validation": { "required": true, "min": 10, "max": 80, "integer": false }
                            },
                            {
                                "name": "comorbidity",
                                "type": "boolean",
                                "validation": { "required": true }
                            },
                            {
                                "name": "priorWeightLossProgram",
                                "type": "boolean",
                                "validation": { "required": true }
                            },
                            {
                                "name": "notes",
                                "type": "text",
                                "validation": { "required": false, "maxLength": 1000 }
                            }
                        ],
                        "medicationFields": []
                    }
                    """,
                    RuleDefinition = """
                    {
                        "rules": [
                            {
                                "type": "conditional",
                                "condition": { "field": "comorbidity", "operator": "equals", "value": true },
                                "then": [{ "field": "bmi", "operator": "gte", "value": 27 }],
                                "else": [{ "field": "bmi", "operator": "gte", "value": 30 }]
                            },
                            { "field": "priorWeightLossProgram", "operator": "equals", "value": true }
                        ]
                    }
                    """,
                },
                ValidClinicalData: new Dictionary<string, JsonElement>
                {
                    ["bmi"] = Json("32.5"),
                    ["comorbidity"] = Json("true"),
                    ["priorWeightLossProgram"] = Json("true"),
                    ["notes"] = Json("\"Patient participated in 6-month supervised program.\"")
                });

        private static Dictionary<string, JsonElement> CopyClinicalData(Dictionary<string, JsonElement> clinicalData) =>
            clinicalData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone());

        private static JsonElement Json(string json)
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }

        private sealed record ValidatorScenario(
            AuthRule AuthRule,
            Dictionary<string, JsonElement> ValidClinicalData);
    }
}
