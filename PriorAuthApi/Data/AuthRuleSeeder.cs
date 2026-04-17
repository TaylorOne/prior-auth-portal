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
                    DisplayName = "MRI Knee without Contrast",
                    IndicationCode = "M25.561",
                    IndicationDisplayName = "Knee pain",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "therapyCompleted",
                            "label": "Conservative Therapy Completed",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "therapyDurationWeeks",
                            "label": "Duration of Therapy (weeks)",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 52, "integer": true }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "type": "text",
                            "validation": { "required": false, "maxLength": 1000 }
                            }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "rules": [
                            { "field": "therapyCompleted", "operator": "equals", "value": true },
                            { "field": "therapyDurationWeeks", "operator": "gte", "value": 6 }
                        ]
                    }
                    """
                },

                // Genetic Testing - Hereditary Breast/Ovarian Cancer (BRCA1/BRCA2)
                new AuthRule
                {
                    RequestType = RequestType.Procedure,
                    CodeSystem = "CPT",
                    Code = "81162",
                    DisplayName = "Genetic Testing",
                    IndicationCode = "Z15.01",
                    IndicationDisplayName = "Hereditary Breast/Ovarian Cancer (BRCA1/BRCA2)",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "familyHistoryConfirmed",
                            "label": "First-Degree Family History Confirmed",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "priorCounselingCompleted",
                            "label": "Prior Genetic Counseling Completed",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "type": "text",
                            "validation": { "required": false, "maxLength": 1000 }
                            }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "rules": [
                            { "field": "familyHistoryConfirmed", "operator": "equals", "value": true },
                            { "field": "priorCounselingCompleted", "operator": "equals", "value": true }
                        ]
                    }
                    """
                },

                // Humira - Rheumatoid Arthritis
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J0135",
                    DisplayName = "Humira (adalimumab)",
                    IndicationCode = "M06.9",
                    IndicationDisplayName = "Rheumatoid Arthritis",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            {
                                "name": "priorDMARDTrial",
                                "label": "Prior DMARD Trial Completed",
                                "type": "boolean",
                                "validation": { "required": true }
                            },
                            {
                                "name": "dmardName",
                                "label": "DMARD Medication Name",
                                "type": "select",
                                "options": ["Methotrexate", "Hydroxychloroquine", "Sulfasalazine", "Leflunomide"],
                                "validation": {
                                    "required": true,
                                    "allowedValues": ["Methotrexate", "Hydroxychloroquine", "Sulfasalazine", "Leflunomide"]
                                }
                            },
                            {
                                "name": "dmardDurationWeeks",
                                "label": "Duration of DMARD Trial (weeks)",
                                "type": "number",
                                "validation": { "required": true, "min": 0, "max": 104, "integer": true }
                            },
                            {
                                "name": "notes",
                                "label": "Additional Notes",
                                "type": "text", 
                                "validation": { "required": false, "maxLength": 1000 }
                            }
                        ],
                        "medicationFields": [
                            {
                            "name": "quantityUnit",
                            "label": "Quantity Unit",
                            "type": "select",
                            "options": ["vial", "syringe", "pen"],
                            "validation": { "required": true, "allowedValues": ["vial", "syringe", "pen"] },
                            "defaultValue": "vial",
                            "editable": false
                            },
                            {
                            "name": "quantityValue",
                            "label": "Quantity",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 10, "integer": true },
                            "defaultValue": 2,
                            "editable": true
                            },
                            {
                            "name": "dosageInstructionText",
                            "label": "Dosage Instructions",
                            "type": "text",
                            "validation": { "required": true, "maxLength": 500 },
                            "defaultValue": "40mg subcutaneous injection every other week",
                            "editable": true
                            },
                            {
                            "name": "numberOfRepeatsAllowed",
                            "label": "Refills Authorized",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 12, "integer": true },
                            "defaultValue": 5,
                            "editable": true
                            },
                            {
                            "name": "expectedSupplyDurationDays",
                            "label": "Days Supply",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 90, "integer": true },
                            "defaultValue": 30,
                            "editable": true
                            }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "rules": [
                            { "field": "priorDMARDTrial", "operator": "equals", "value": true },
                            { "field": "dmardDurationWeeks", "operator": "gte", "value": 12 }
                        ]
                    }
                    """
                },

                // Wegovy - Chronic Weight Management
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J3490",
                    DisplayName = "Wegovy (semaglutide)",
                    IndicationCode = "E66.9",
                    IndicationDisplayName = "Chronic Weight Management",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "bmi",
                            "label": "Current BMI",
                            "type": "number",
                            "validation": { "required": true, "min": 10, "max": 80, "integer": false }
                            },
                            {
                            "name": "comorbidity",
                            "label": "Weight-Related Comorbidity Present",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "priorWeightLossProgram",
                            "label": "Prior Supervised Weight Loss Program Completed",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "type": "text",
                            "validation": { "required": false, "maxLength": 1000 }
                            }
                        ],
                        "medicationFields": [
                            {
                            "name": "quantityUnit",
                            "label": "Quantity Unit",
                            "type": "select",
                            "options": ["pen"],
                            "validation": { "required": true, "allowedValues": ["pen"] },
                            "defaultValue": "pen",
                            "editable": false
                            },
                            {
                            "name": "quantityValue",
                            "label": "Quantity",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 4, "integer": true },
                            "defaultValue": 1,
                            "editable": true
                            },
                            {
                            "name": "dosageInstructionText",
                            "label": "Dosage Instructions",
                            "type": "text",
                            "validation": { "required": true, "maxLength": 500 },
                            "placeholder": "e.g. 0.25mg subcutaneous injection once weekly (starting dose)",
                            "editable": true
                            },
                            {
                            "name": "numberOfRepeatsAllowed",
                            "label": "Refills Authorized",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 12, "integer": true },
                            "defaultValue": 3,
                            "editable": true
                            },
                            {
                            "name": "expectedSupplyDurationDays",
                            "label": "Days Supply",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 90, "integer": true },
                            "defaultValue": 28,
                            "editable": true
                            }
                        ]
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
                    """
                },

                // Xarelto - Atrial Fibrillation
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "RxNorm",
                    Code = "1599538",
                    DisplayName = "Xarelto (rivaroxaban)",
                    IndicationCode = "I48.91",
                    IndicationDisplayName = "Atrial Fibrillation",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "priorWarfarinTrial",
                            "label": "Prior Warfarin Trial Completed",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "warfarinDurationWeeks",
                            "label": "Duration of Warfarin Trial (weeks)",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 52, "integer": true }
                            },
                            {
                            "name": "reasonForSwitch",
                            "label": "Reason for Transition to Xarelto",
                            "type": "select",
                            "options": ["Unstable INR", "Patient intolerance", "Drug interaction", "Patient preference"],
                            "validation": { "required": true, "allowedValues": ["Unstable INR", "Patient intolerance", "Drug interaction", "Patient preference"] }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "type": "text",
                            "validation": { "required": false, "maxLength": 1000 }
                            }
                        ],
                        "medicationFields": [
                            {
                            "name": "quantityUnit",
                            "label": "Quantity Unit",
                            "type": "select",
                            "options": ["tablet"],
                            "validation": { "required": true, "allowedValues": ["tablet"] },
                            "defaultValue": "tablet",
                            "editable": false
                            },
                            {
                            "name": "quantityValue",
                            "label": "Quantity",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 90, "integer": true },
                            "defaultValue": 30,
                            "editable": true
                            },
                            {
                            "name": "dosageInstructionText",
                            "label": "Dosage Instructions",
                            "type": "text",
                            "validation": { "required": true, "maxLength": 500 },
                            "defaultValue": "20mg orally once daily with evening meal",
                            "editable": true
                            },
                            {
                            "name": "numberOfRepeatsAllowed",
                            "label": "Refills Authorized",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 12, "integer": true },
                            "defaultValue": 5,
                            "editable": true
                            },
                            {
                            "name": "expectedSupplyDurationDays",
                            "label": "Days Supply",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 90, "integer": true },
                            "defaultValue": 30,
                            "editable": true
                            }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "rules": [
                            { "field": "priorWarfarinTrial", "operator": "equals", "value": true },
                            { "field": "warfarinDurationWeeks", "operator": "gte", "value": 4 },
                            { "field": "reasonForSwitch", "operator": "hasValue" }
                        ]
                    }
                    """
                },

                // Humira - Psoriatic Arthritis
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J0135",
                    DisplayName = "Humira (adalimumab)",
                    IndicationCode = "L40.50",
                    IndicationDisplayName = "Psoriatic Arthritis",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "priorNSAIDTrial",
                            "label": "Prior NSAID Trial Completed",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "nsaidDurationWeeks",
                            "label": "Duration of NSAID Trial (weeks)",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 52, "integer": true }
                            },
                            {
                            "name": "jointsAffected",
                            "label": "Number of Affected Joints",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 100, "integer": true }
                            },
                            {
                            "name": "dermatologyConfirmed",
                            "label": "Diagnosis Confirmed by Rheumatologist or Dermatologist",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "type": "text",
                            "validation": { "required": false, "maxLength": 1000 }
                            }
                        ],
                        "medicationFields": [
                            {
                            "name": "quantityUnit",
                            "label": "Quantity Unit",
                            "type": "select",
                            "options": ["vial", "syringe", "pen"],
                            "validation": { "required": true, "allowedValues": ["vial", "syringe", "pen"] },
                            "defaultValue": "vial",
                            "editable": false
                            },
                            {
                            "name": "quantityValue",
                            "label": "Quantity",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 10, "integer": true },
                            "defaultValue": 2,
                            "editable": true
                            },
                            {
                            "name": "dosageInstructionText",
                            "label": "Dosage Instructions",
                            "type": "text",
                            "validation": { "required": true, "maxLength": 500 },
                            "defaultValue": "40mg subcutaneous injection every other week",
                            "editable": true
                            },
                            {
                            "name": "numberOfRepeatsAllowed",
                            "label": "Refills Authorized",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 12, "integer": true },
                            "defaultValue": 5,
                            "editable": true
                            },
                            {
                            "name": "expectedSupplyDurationDays",
                            "label": "Days Supply",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 90, "integer": true },
                            "defaultValue": 30,
                            "editable": true
                            }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "rules": [
                            { "field": "priorNSAIDTrial", "operator": "equals", "value": true },
                            { "field": "nsaidDurationWeeks", "operator": "gte", "value": 4 },
                            { "field": "dermatologyConfirmed", "operator": "equals", "value": true }
                        ]
                    }
                    """
                },

                // Humira - Crohn's Disease
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J0135",
                    DisplayName = "Humira (adalimumab)",
                    IndicationCode = "K50.90",
                    IndicationDisplayName = "Crohn's Disease",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "priorCorticosteroidTrial",
                            "label": "Prior Corticosteroid Trial Completed",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "priorImmunomodulatorTrial",
                            "label": "Prior Immunomodulator Trial Completed",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "immunomodulatorName",
                            "label": "Immunomodulator Medication Name",
                            "type": "select",
                            "options": ["Azathioprine", "6-Mercaptopurine", "Methotrexate"],
                            "validation": { "required": true, "allowedValues": ["Azathioprine", "6-Mercaptopurine", "Methotrexate"] }
                            },
                            {
                            "name": "diseaseClassification",
                            "label": "Disease Classification",
                            "type": "select",
                            "options": ["Mild", "Moderate", "Severe"],
                            "validation": { "required": true, "allowedValues": ["Mild", "Moderate", "Severe"] }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "type": "text",
                            "validation": { "required": false, "maxLength": 1000 }
                            }
                        ],
                        "medicationFields": [
                            {
                            "name": "quantityUnit",
                            "label": "Quantity Unit",
                            "type": "select",
                            "options": ["vial", "syringe", "pen"],
                            "validation": { "required": true, "allowedValues": ["vial", "syringe", "pen"] },
                            "defaultValue": "vial",
                            "editable": false
                            },
                            {
                            "name": "quantityValue",
                            "label": "Quantity",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 10, "integer": true },
                            "defaultValue": 2,
                            "editable": true
                            },
                            {
                            "name": "dosageInstructionText",
                            "label": "Dosage Instructions",
                            "type": "text",
                            "validation": { "required": true, "maxLength": 500 },
                            "defaultValue": "160mg subcutaneous injection at week 0, 80mg at week 2, then 40mg every other week",
                            "editable": true
                            },
                            {
                            "name": "numberOfRepeatsAllowed",
                            "label": "Refills Authorized",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 12, "integer": true },
                            "defaultValue": 5,
                            "editable": true
                            },
                            {
                            "name": "expectedSupplyDurationDays",
                            "label": "Days Supply",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 90, "integer": true },
                            "defaultValue": 30,
                            "editable": true
                            }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "rules": [
                            { "field": "priorCorticosteroidTrial", "operator": "equals", "value": true },
                            { "field": "priorImmunomodulatorTrial", "operator": "equals", "value": true },
                            { 
                                "field": "diseaseClassification", 
                                "operator": "gte_ordered", 
                                "value": "Moderate",
                                "order": ["Mild", "Moderate", "Severe"]
                            }
                        ]
                    }
                    """
                },

                // Xarelto - DVT
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "RxNorm",
                    Code = "1599538",
                    DisplayName = "Xarelto (rivaroxaban)",
                    IndicationCode = "I82.401",
                    IndicationDisplayName = "Deep Vein Thrombosis",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "dvtConfirmed",
                            "label": "DVT Confirmed by Imaging",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "imagingType",
                            "label": "Confirmatory Imaging Type",
                            "type": "select",
                            "options": ["Duplex Ultrasound", "CT Venography", "MR Venography"],
                            "validation": { "required": true, "allowedValues": ["Duplex Ultrasound", "CT Venography", "MR Venography"] }
                            },
                            {
                            "name": "priorHeparinBridge",
                            "label": "Prior Heparin Bridge Therapy Completed",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "estimatedTreatmentDurationWeeks",
                            "label": "Estimated Treatment Duration (weeks)",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 52, "integer": true }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "type": "text",
                            "validation": { "required": false, "maxLength": 1000 }
                            }
                        ],
                        "medicationFields": [
                            {
                            "name": "quantityUnit",
                            "label": "Quantity Unit",
                            "type": "select",
                            "options": ["tablet"],
                            "validation": { "required": true, "allowedValues": ["tablet"] },
                            "defaultValue": "tablet",
                            "editable": false
                            },
                            {
                            "name": "quantityValue",
                            "label": "Quantity",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 90, "integer": true },
                            "defaultValue": 30,
                            "editable": true
                            },
                            {
                            "name": "dosageInstructionText",
                            "label": "Dosage Instructions",
                            "type": "text",
                            "validation": { "required": true, "maxLength": 500 },
                            "placeholder": "e.g. 15mg orally twice daily with food for 21 days, then 20mg once daily",
                            "editable": true
                            },
                            {
                            "name": "numberOfRepeatsAllowed",
                            "label": "Refills Authorized",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 12, "integer": true },
                            "defaultValue": 3,
                            "editable": true
                            },
                            {
                            "name": "expectedSupplyDurationDays",
                            "label": "Days Supply",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 90, "integer": true },
                            "defaultValue": 30,
                            "editable": true
                            }
                        ]
                    }
                    """,
                    RuleDefinition = """
                    {
                        "rules": [
                            { "field": "dvtConfirmed", "operator": "equals", "value": true },
                            { "field": "imagingType", "operator": "hasValue" },
                            { "field": "priorHeparinBridge", "operator": "equals", "value": true },
                            { "field": "estimatedTreatmentDurationWeeks", "operator": "gte", "value": 12 }
                        ]
                    }
                    """
                },

                // Ozempic - Type 2 Diabetes
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J3101",
                    DisplayName = "Ozempic (semaglutide)",
                    IndicationCode = "E11.9",
                    IndicationDisplayName = "Type 2 Diabetes",
                    IsActive = true,
                    EffectiveDate = new DateOnly(2024, 1, 1),
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "hba1c",
                            "label": "Most Recent HbA1c (%)",
                            "type": "number",
                            "validation": { "required": true, "min": 4.0, "max": 20.0, "integer": false }
                            },
                            {
                            "name": "priorMetforminTrial",
                            "label": "Prior Metformin Trial Completed",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "metforminDurationWeeks",
                            "label": "Duration of Metformin Trial (weeks)",
                            "type": "number",
                            "validation": { "required": false, "min": 0, "max": 104, "integer": true }
                            },
                            {
                            "name": "metforminContraindicated",
                            "label": "Metformin Contraindicated",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "diabetesEducationCompleted",
                            "label": "Diabetes Self-Management Education Completed",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "type": "text",
                            "validation": { "required": false, "maxLength": 1000 }
                            }
                        ],
                        "medicationFields": [
                            {
                            "name": "quantityUnit",
                            "label": "Quantity Unit",
                            "type": "select",
                            "options": ["pen"],
                            "validation": { "required": true, "allowedValues": ["pen"] },
                            "defaultValue": "pen",
                            "editable": false
                            },
                            {
                            "name": "quantityValue",
                            "label": "Quantity",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 4, "integer": true },
                            "defaultValue": 1,
                            "editable": true
                            },
                            {
                            "name": "dosageInstructionText",
                            "label": "Dosage Instructions",
                            "type": "text",
                            "validation": { "required": true, "maxLength": 500 },
                            "placeholder": "e.g. 0.5mg subcutaneous injection once weekly",
                            "editable": true
                            },
                            {
                            "name": "numberOfRepeatsAllowed",
                            "label": "Refills Authorized",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 12, "integer": true },
                            "defaultValue": 3,
                            "editable": true
                            },
                            {
                            "name": "expectedSupplyDurationDays",
                            "label": "Days Supply",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 90, "integer": true },
                            "defaultValue": 28,
                            "editable": true
                            }
                        ]
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