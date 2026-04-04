using Microsoft.EntityFrameworkCore;
using PriorAuthApi.Entities;

namespace PriorAuthApi.Data
{
    public static class AuthRuleSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var rules = new List<AuthRule> {

                // MRI Knee
                new AuthRule
                {
                    RequestType = RequestType.Procedure,
                    CodeSystem = "CPT",
                    Code = "73721",
                    IndicationCode = "M25.561",
                    DisplayName = "MRI Knee without Contrast",
                    IndicationDisplayName = "Knee pain",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
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

                // Genetic Testing - Hereditary Breast/Ovarian Cancer (BRCA1/BRCA2)
                new AuthRule
                {
                    RequestType = RequestType.Procedure,
                    CodeSystem = "CPT",
                    Code = "81162",
                    IndicationCode = "Z15.01",
                    DisplayName = "Genetic Testing",
                    IndicationDisplayName = "Hereditary Breast/Ovarian Cancer (BRCA1/BRCA2)",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
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

                // Humira - Rheumatoid Arthritis
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J0135",
                    IndicationCode = "M06.9",
                    DisplayName = "Humira (adalimumab)",
                    IndicationDisplayName = "Rheumatoid Arthritis",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
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

                // Wegovy - Chronic Weight Management
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J3490",
                    IndicationCode = "E66.9",
                    DisplayName = "Wegovy (semaglutide)",
                    IndicationDisplayName = "Chronic Weight Management",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
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

                // Xarelto - Atrial Fibrillation
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "RxNorm",
                    Code = "1599538",
                    IndicationCode = "I48.91",
                    DisplayName = "Xarelto (rivaroxaban)",
                    IndicationDisplayName = "Atrial Fibrillation",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
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
                },

                // Humira - Psoriatic Arthritis
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J0135",
                    IndicationCode = "L40.50",
                    DisplayName = "Humira (adalimumab)",
                    IndicationDisplayName = "Psoriatic Arthritis",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            { "name": "priorNSAIDTrial", "label": "Prior NSAID Trial Completed", "type": "boolean" },
                            { "name": "nsaidDurationWeeks", "label": "Duration of NSAID Trial (weeks)", "type": "number" },
                            { "name": "jointsAffected", "label": "Number of Affected Joints", "type": "number" },
                            { "name": "dermatologyConfirmed", "label": "Diagnosis Confirmed by Rheumatologist or Dermatologist", "type": "boolean" },
                            { "name": "notes", "label": "Additional Notes", "type": "text", "required": false }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "priorNSAIDRequired": true,
                        "minNSAIDWeeks": 4,
                        "specialistConfirmationRequired": true
                    }
                    """
                },

                // Humira - Crohn's Disease
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J0135",
                    IndicationCode = "K50.90",
                    DisplayName = "Humira (adalimumab)",
                    IndicationDisplayName = "Crohn's Disease",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            { "name": "priorCorticosteroidTrial", "label": "Prior Corticosteroid Trial Completed", "type": "boolean" },
                            { "name": "priorImmunomodulatorTrial", "label": "Prior Immunomodulator Trial Completed", "type": "boolean" },
                            { "name": "immunomodulatorName", "label": "Immunomodulator Medication Name", "type": "select",
                            "options": ["Azathioprine", "6-Mercaptopurine", "Methotrexate"] },
                            { "name": "diseaseClassification", "label": "Disease Classification", "type": "select",
                            "options": ["Mild", "Moderate", "Severe"] },
                            { "name": "notes", "label": "Additional Notes", "type": "text", "required": false }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "priorCorticosteroidRequired": true,
                        "priorImmunomodulatorRequired": true,
                        "minimumDiseaseClassification": "Moderate"
                    }
                    """
                },

                // Xarelto - DVT
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "RxNorm",
                    Code = "1599538",
                    IndicationCode = "I82.401",
                    DisplayName = "Xarelto (rivaroxaban)",
                    IndicationDisplayName = "Deep Vein Thrombosis",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            { "name": "dvtConfirmed", "label": "DVT Confirmed by Imaging", "type": "boolean" },
                            { "name": "imagingType", "label": "Confirmatory Imaging Type", "type": "select",
                            "options": ["Duplex Ultrasound", "CT Venography", "MR Venography"] },
                            { "name": "priorHeparinBridge", "label": "Prior Heparin Bridge Therapy Completed", "type": "boolean" },
                            { "name": "estimatedTreatmentDurationWeeks", "label": "Estimated Treatment Duration (weeks)", "type": "number" },
                            { "name": "notes", "label": "Additional Notes", "type": "text", "required": false }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "imagingConfirmationRequired": true,
                        "priorHeparinRequired": true,
                        "minTreatmentWeeks": 12
                    }
                    """
                },

                // Ozempic - Type 2 Diabetes
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J3101",
                    IndicationCode = "E11.9",
                    DisplayName = "Ozempic (semaglutide)",
                    IndicationDisplayName = "Type 2 Diabetes",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            { "name": "hba1c", "label": "Most Recent HbA1c (%)", "type": "number" },
                            { "name": "priorMetforminTrial", "label": "Prior Metformin Trial Completed", "type": "boolean" },
                            { "name": "metforminDurationWeeks", "label": "Duration of Metformin Trial (weeks)", "type": "number" },
                            { "name": "metforminContraindicated", "label": "Metformin Contraindicated", "type": "boolean" },
                            { "name": "diabetesEducationCompleted", "label": "Diabetes Self-Management Education Completed", "type": "boolean" },
                            { "name": "notes", "label": "Additional Notes", "type": "text", "required": false }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "minHba1c": 7.0,
                        "priorMetforminRequired": true,
                        "metforminContraindicationExcused": true,
                        "minMetforminWeeks": 12
                    }
                    """
                }
            };

            foreach (var rule in rules)
            {
                var exists = await context.AuthRules
                    .AnyAsync(r => r.Code == rule.Code && r.IndicationCode == rule.IndicationCode);
                
                if (!exists)
                    context.AuthRules.Add(rule);
            }

            await context.SaveChangesAsync();
        }
    }
}