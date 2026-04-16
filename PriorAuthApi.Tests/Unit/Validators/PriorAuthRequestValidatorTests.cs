using PriorAuthApi.Entities;
using PriorAuthApi.DTOs;
using PriorAuthApi.Validators;
using System.Text.Json;
using FluentAssertions;


namespace PriorAuthApi.Tests
{
    public class PriorAuthRequestValidatorTests
    {
        private readonly AuthRule _humiraRaRule;

        public PriorAuthRequestValidatorTests()
        {
            _humiraRaRule = new AuthRule
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
                            "validation": { "required": true, "allowedValues": ["Methotrexate", "Hydroxychloroquine", "Sulfasalazine", "Leflunomide"] }
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
            };
        }

        private static SubmitPriorAuthDto ValidDto(Dictionary<string, JsonElement>? clinicalData = null) =>
            new(
                Priority: "routine",
                Code: new CodeableConceptDto("HCPCS", "J0135", "Adalimumab injection"),
                PatientId: 1,
                PractitionerId: 1,
                ReasonCode: ["M05.79"],
                ClinicalData: clinicalData ?? BuildValidClinicalData(),
                MedicationRequest: null
            );

        private static Dictionary<string, JsonElement> BuildValidClinicalData() =>
            new()
            {
                ["priorDMARDTrial"] = JsonDocument.Parse("true").RootElement,
                ["dmardName"] = JsonDocument.Parse("\"Methotrexate\"").RootElement,
                ["dmardDurationWeeks"] = JsonDocument.Parse("16").RootElement,
                ["notes"] = JsonDocument.Parse("\"Patient tolerated poorly.\"").RootElement
            };

        [Fact]
        public void Should_Pass_With_Valid_ClinicalData()
        {
            var validator = new PriorAuthRequestValidator(_humiraRaRule);
            var result = validator.Validate(ValidDto());
            result.IsValid.Should().BeTrue();
        }

        [Fact]
        public void Should_Fail_When_Required_Boolean_Field_Missing()
        {
            var data = BuildValidClinicalData();
            data.Remove("priorDMARDTrial");

            var result = new PriorAuthRequestValidator(_humiraRaRule).Validate(ValidDto(data));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("priorDMARDTrial"));
        }

        [Fact]
        public void Should_Fail_When_Select_Value_Not_In_AllowedValues()
        {
            var data = BuildValidClinicalData();
            data["dmardName"] = JsonDocument.Parse("\"Aspirin\"").RootElement;

            var result = new PriorAuthRequestValidator(_humiraRaRule).Validate(ValidDto(data));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("dmardName"));
        }

        [Fact]
        public void Should_Fail_When_Number_Exceeds_Max()
        {
            var data = BuildValidClinicalData();
            data["dmardDurationWeeks"] = JsonDocument.Parse("999").RootElement;

            var result = new PriorAuthRequestValidator(_humiraRaRule).Validate(ValidDto(data));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("dmardDurationWeeks"));
        }

        [Fact]
        public void Should_Pass_When_Optional_Field_Is_Absent()
        {
            var data = BuildValidClinicalData();
            data.Remove("notes");

            var result = new PriorAuthRequestValidator(_humiraRaRule).Validate(ValidDto(data));

            result.IsValid.Should().BeTrue();
        }
    }
}