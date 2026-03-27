using PriorAuthApi.Entities;

namespace PriorAuthApi.Data
{
    public static class AuthRuleSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (context.AuthRules.Any()) return;

            context.AuthRules.AddRange(

                // MRI Knee
                new AuthRule
                {
                    RequestType = RequestType.Procedure,
                    CodeSystem = "CPT",
                    Code = "73721",
                    IndicationCode = "M25.561",
                    DisplayName = "MRI Knee without Contrast",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            { "name": "diagnosisCode", "label": "Diagnosis Code", "type": "select",
                            "options": ["M25.561 - Pain in right knee", "M25.562 - Pain in left knee"] },
                            { "name": "therapyCompleted", "label": "Conservative Therapy Completed", "type": "boolean" },
                            { "name": "therapyDurationWeeks", "label": "Duration of Therapy (weeks)", "type": "number" },
                            { "name": "notes", "label": "Additional Notes", "type": "text", "required": false }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "therapyRequired": true,
                        "minTherapyWeeks": 6
                    }
                    """
                },

                // Genetic Testing
                new AuthRule
                {
                    RequestType = RequestType.Procedure,
                    CodeSystem = "CPT",
                    Code = "81162",
                    IndicationCode = "Z15.01",
                    DisplayName = "Genetic Testing - Hereditary Breast/Ovarian Cancer (BRCA1/BRCA2)",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            { "name": "diagnosisCode", "label": "Indication Code", "type": "select",
                            "options": ["Z15.01 - Genetic susceptibility to breast cancer", "Z80.3 - Family history of breast cancer"] },
                            { "name": "familyHistoryConfirmed", "label": "First-Degree Family History Confirmed", "type": "boolean" },
                            { "name": "priorCounselingCompleted", "label": "Prior Genetic Counseling Completed", "type": "boolean" },
                            { "name": "notes", "label": "Additional Notes", "type": "text", "required": false }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "familyHistoryRequired": true,
                        "priorCounselingRequired": true
                    }
                    """
                },

                // Humira
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J0135",
                    IndicationCode = "M06.9",
                    DisplayName = "Humira (adalimumab) - Rheumatoid Arthritis",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            { "name": "diagnosisCode", "label": "Diagnosis Code", "type": "select",
                            "options": ["M06.9 - Rheumatoid arthritis, unspecified", "M05.9 - Seropositive rheumatoid arthritis"] },
                            { "name": "priorDMARDTrial", "label": "Prior DMARD Trial Completed", "type": "boolean" },
                            { "name": "dmardName", "label": "DMARD Medication Name", "type": "select",
                            "options": ["Methotrexate", "Hydroxychloroquine", "Sulfasalazine", "Leflunomide"] },
                            { "name": "dmardDurationWeeks", "label": "Duration of DMARD Trial (weeks)", "type": "number" },
                            { "name": "notes", "label": "Additional Notes", "type": "text", "required": false }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "priorDMARDRequired": true,
                        "minDMARDWeeks": 12
                    }
                    """
                },

                // Wegovy
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J3490",
                    IndicationCode = "E66.9",
                    DisplayName = "Wegovy (semaglutide) - Chronic Weight Management",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            { "name": "diagnosisCode", "label": "Diagnosis Code", "type": "select",
                            "options": ["E66.9 - Obesity, unspecified", "E66.01 - Morbid obesity due to excess calories"] },
                            { "name": "bmi", "label": "Current BMI", "type": "number" },
                            { "name": "comorbidity", "label": "Weight-Related Comorbidity Present", "type": "boolean" },
                            { "name": "priorWeightLossProgram", "label": "Prior Supervised Weight Loss Program Completed", "type": "boolean" },
                            { "name": "notes", "label": "Additional Notes", "type": "text", "required": false }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "minBMIWithComorbidity": 27,
                        "minBMIWithoutComorbidity": 30,
                        "priorProgramRequired": true
                    }
                    """
                },

                // Xarelto
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "RxNorm",
                    Code = "1599538",
                    IndicationCode = "I48.91",
                    DisplayName = "Xarelto (rivaroxaban) - Atrial Fibrillation",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            { "name": "diagnosisCode", "label": "Diagnosis Code", "type": "select",
                            "options": ["I48.91 - Unspecified atrial fibrillation", "I48.19 - Other persistent atrial fibrillation"] },
                            { "name": "priorWarfarinTrial", "label": "Prior Warfarin Trial Completed", "type": "boolean" },
                            { "name": "warfarinDurationWeeks", "label": "Duration of Warfarin Trial (weeks)", "type": "number" },
                            { "name": "reasonForSwitch", "label": "Reason for Transition to Xarelto", "type": "select",
                            "options": ["Unstable INR", "Patient intolerance", "Drug interaction", "Patient preference"] },
                            { "name": "notes", "label": "Additional Notes", "type": "text", "required": false }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "priorWarfarinRequired": true,
                        "minWarfarinWeeks": 4,
                        "reasonForSwitchRequired": true
                    }
                    """
                }
            );

            context.SaveChanges();
        }
    }
}