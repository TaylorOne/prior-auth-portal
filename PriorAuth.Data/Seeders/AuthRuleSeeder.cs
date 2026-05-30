using Microsoft.EntityFrameworkCore;
using PriorAuth.Data.Entities;

namespace PriorAuth.Data
{
    public static class AuthRuleSeeder
    {
        public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
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
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "therapyCompleted",
                            "label": "Conservative Therapy Completed",
                            "description": "Confirm the patient has completed a course of conservative therapy prior to advanced imaging. Most payers require documented physical therapy, rest, NSAIDs, or corticosteroid injection before authorizing MRI for knee pain.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "therapyDurationWeeks",
                            "label": "Duration of Therapy (weeks)",
                            "description": "Enter the duration of conservative therapy in weeks. Most payers require a minimum 4–6 week trial. MRI is typically authorized when symptoms persist or worsen despite adequate conservative management.",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 52, "integer": true }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "description": "Include relevant clinical context such as specific therapies attempted, response to treatment, suspected diagnosis (meniscal tear, ACL injury, cartilage damage), or prior X-ray findings.",
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
                    RequiresManualReview = true,
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "familyHistoryConfirmed",
                            "label": "First-Degree Family History Confirmed",
                            "description": "Confirm the patient has a first-degree relative (parent, sibling, or child) with a documented BRCA1 or BRCA2 mutation, or a personal or family history of breast, ovarian, fallopian tube, or peritoneal cancer. Family history is the primary clinical criterion for BRCA testing authorization.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "priorCounselingCompleted",
                            "label": "Prior Genetic Counseling Completed",
                            "description": "Confirm the patient has completed pre-test genetic counseling with a certified genetic counselor. Most payers require documented counseling prior to authorizing BRCA testing to ensure informed consent and appropriate clinical context.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "description": "Include relevant clinical context such as specific family history details, personal cancer history, ethnicity (Ashkenazi Jewish ancestry is a recognized risk factor), or the name of the genetic counselor seen.",
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

                // Wegovy - Chronic Weight Management
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J3490",
                    DisplayName = "Wegovy (semaglutide)",
                    IndicationCode = "E66.9",
                    IndicationDisplayName = "Chronic Weight Management",
                    RequiresManualReview = true,
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "bmi",
                            "label": "Current BMI",
                            "description": "Enter the patient's most recently documented BMI. Wegovy is indicated for BMI ≥ 30 (obesity), or BMI ≥ 27 with at least one weight-related comorbidity such as hypertension, type 2 diabetes, or dyslipidemia.",
                            "type": "number",
                            "validation": { "required": true, "min": 10, "max": 80, "integer": false }
                            },
                            {
                            "name": "comorbidity",
                            "label": "Weight-Related Comorbidity Present",
                            "description": "Confirm whether the patient has at least one weight-related comorbidity. Common qualifying comorbidities include hypertension, type 2 diabetes, hyperlipidemia, obstructive sleep apnea, and cardiovascular disease. Required for patients with BMI 27–29.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "priorWeightLossProgram",
                            "label": "Prior Supervised Weight Loss Program Completed",
                            "description": "Confirm the patient has participated in a supervised weight loss program prior to initiating pharmacotherapy. Most payers require documented participation in a structured behavioral or dietary intervention, typically for 3–6 months.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "description": "Include relevant clinical context such as the name and duration of the prior weight loss program, specific comorbidities present, or contraindications to other weight loss agents.",
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
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "priorWarfarinTrial",
                            "label": "Prior Warfarin Trial Completed",
                            "description": "Confirm the patient has been trialed on warfarin prior to initiating a DOAC. Many payers require documented warfarin use before approving Xarelto for non-valvular atrial fibrillation, though intolerance or instability are accepted exceptions.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "warfarinDurationWeeks",
                            "label": "Duration of Warfarin Trial (weeks)",
                            "description": "Enter the total duration of warfarin therapy in weeks. Payers typically look for a meaningful trial period; however, a short or failed trial due to unstable INR or intolerance is clinically acceptable and should be documented in the reason for switch.",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 52, "integer": true }
                            },
                            {
                            "name": "reasonForSwitch",
                            "label": "Reason for Transition to Xarelto",
                            "description": "Select the primary clinical reason for transitioning from warfarin to Xarelto. Unstable INR and drug interactions are the strongest clinical justifications. Patient preference alone may require additional documentation depending on the payer.",
                            "type": "select",
                            "options": ["Unstable INR", "Patient intolerance", "Drug interaction", "Patient preference"],
                            "validation": { "required": true, "allowedValues": ["Unstable INR", "Patient intolerance", "Drug interaction", "Patient preference"] }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "description": "Include supporting context such as INR history, specific drug or dietary interactions, or documented adverse reactions to warfarin.",
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

                // Humira - Rheumatoid Arthritis
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J0135",
                    DisplayName = "Humira (adalimumab)",
                    IndicationCode = "M06.9",
                    IndicationDisplayName = "Rheumatoid Arthritis",
                    RequiresManualReview = true,
                    FormDefinition = """
                    {
                        "fields": [
                            {
                                "name": "priorDMARDTrial",
                                "label": "Prior DMARD Trial Completed",
                                "description": "Confirm the patient has completed an adequate trial of at least one conventional DMARD prior to initiating biologic therapy. Most payers require documented failure, intolerance, or contraindication to a conventional DMARD.",
                                "type": "boolean",
                                "validation": { "required": true }
                            },
                            {
                                "name": "dmardName",
                                "label": "DMARD Medication Name",
                                "description": "Select the conventional DMARD trialed. Methotrexate is the ACR-preferred first-line agent for RA and is typically required unless contraindicated.",
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
                                "description": "Enter the total duration in weeks. Most payers require a minimum 12-week adequate trial. 24 weeks is common for step therapy requirements.",
                                "type": "number",
                                "validation": { "required": true, "min": 0, "max": 104, "integer": true }
                            },
                            {
                                "name": "notes",
                                "label": "Additional Notes",
                                "description": "Include relevant clinical context such as adverse reactions, contraindications, or reasons the trial was considered inadequate.",
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

                // Humira - Psoriatic Arthritis
                new AuthRule
                {
                    RequestType = RequestType.Medication,
                    CodeSystem = "HCPCS",
                    Code = "J0135",
                    DisplayName = "Humira (adalimumab)",
                    IndicationCode = "L40.50",
                    IndicationDisplayName = "Psoriatic Arthritis",
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "priorNSAIDTrial",
                            "label": "Prior NSAID Trial Completed",
                            "description": "Confirm the patient has completed an adequate trial of at least one NSAID prior to initiating biologic therapy. NSAIDs are the required first-line therapy for Psoriatic Arthritis before biologics will be considered.",
                            "validation": { "required": true }
                            },
                            {
                            "name": "nsaidDurationWeeks",
                            "label": "Duration of NSAID Trial (weeks)",
                            "description": "Enter the total duration in weeks. Most payers require a minimum 4-week NSAID trial, though 12 weeks is common for step therapy requirements.",
                            "type": "number",
                            "validation": { "required": true, "min": 0, "max": 52, "integer": true }
                            },
                            {
                            "name": "jointsAffected",
                            "label": "Number of Affected Joints",
                            "description": "Enter the number of actively inflamed or tender joints. Most payers require involvement of 3 or more joints to establish moderate-to-severe disease warranting biologic therapy.",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 100, "integer": true }
                            },
                            {
                            "name": "dermatologyConfirmed",
                            "label": "Diagnosis Confirmed by Rheumatologist or Dermatologist",
                            "description": "Confirm that the PsA diagnosis has been formally evaluated by a rheumatologist or dermatologist. Specialist confirmation is typically required given the overlap between PsA and other inflammatory arthropathies.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "description": "Include relevant clinical context such as NSAID intolerances, contraindications, or comorbid psoriasis severity.",
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
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "priorCorticosteroidTrial",
                            "label": "Prior Corticosteroid Trial Completed",
                            "description": "Confirm the patient has been treated with corticosteroids (e.g. prednisone, budesonide) for active disease. Corticosteroid use establishes disease activity and is typically required prior to biologic authorization.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "priorImmunomodulatorTrial",
                            "label": "Prior Immunomodulator Trial Completed",
                            "description": "Confirm the patient has trialed at least one immunomodulator. Azathioprine, 6-Mercaptopurine, and Methotrexate are the standard options. Most payers require documented failure or intolerance before approving a biologic.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "immunomodulatorName",
                            "label": "Immunomodulator Medication Name",
                            "description": "Select the immunomodulator trialed. Azathioprine and 6-Mercaptopurine are thiopurines and are first-line; Methotrexate is typically used when thiopurines are not tolerated.",
                            "type": "select",
                            "options": ["Azathioprine", "6-Mercaptopurine", "Methotrexate"],
                            "validation": { "required": true, "allowedValues": ["Azathioprine", "6-Mercaptopurine", "Methotrexate"] }
                            },
                            {
                            "name": "diseaseClassification",
                            "label": "Disease Classification",
                            "description": "Select the patient's current disease severity. Moderate-to-severe classification is typically required for biologic authorization. Mild disease is generally managed with aminosalicylates or corticosteroids alone.",
                            "type": "select",
                            "options": ["Mild", "Moderate", "Severe"],
                            "validation": { "required": true, "allowedValues": ["Mild", "Moderate", "Severe"] }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "description": "Include relevant clinical context such as reasons for immunomodulator failure, contraindications, or prior surgical history related to Crohn's disease.",
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
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "dvtConfirmed",
                            "label": "DVT Confirmed by Imaging",
                            "description": "Confirm that deep vein thrombosis has been objectively confirmed via diagnostic imaging. Imaging confirmation is required — clinical suspicion alone is insufficient for authorization.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "imagingType",
                            "label": "Confirmatory Imaging Type",
                            "description": "Select the imaging modality used to confirm DVT. Duplex ultrasound is the standard first-line confirmatory study. CT or MR venography is typically used for suspected iliocaval involvement or when ultrasound is inconclusive.",
                            "type": "select",
                            "options": ["Duplex Ultrasound", "CT Venography", "MR Venography"],
                            "validation": { "required": true, "allowedValues": ["Duplex Ultrasound", "CT Venography", "MR Venography"] }
                            },
                            {
                            "name": "priorHeparinBridge",
                            "label": "Prior Heparin Bridge Therapy Completed",
                            "description": "Confirm whether the patient received initial heparin bridge therapy. Parenteral anticoagulation is standard initial treatment for acute DVT; documentation of bridging supports the transition to oral anticoagulation with Xarelto.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "estimatedTreatmentDurationWeeks",
                            "label": "Estimated Treatment Duration (weeks)",
                            "description": "Enter the anticipated duration of anticoagulation therapy. Standard treatment for provoked DVT is 3 months (13 weeks); unprovoked DVT or recurrent disease may warrant extended or indefinite therapy.",
                            "type": "number",
                            "validation": { "required": true, "min": 1, "max": 52, "integer": true }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "description": "Include relevant clinical context such as provoked versus unprovoked DVT, presence of thrombophilia, prior VTE history, or bleeding risk factors.",
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
                    FormDefinition = """
                    {
                        "fields": [
                            {
                            "name": "hba1c",
                            "label": "Most Recent HbA1c (%)",
                            "description": "Enter the patient's most recently documented HbA1c percentage. Most payers require HbA1c ≥ 7.5–8.0% to establish inadequate glycemic control warranting GLP-1 therapy. Results should be from within the past 3–6 months.",
                            "type": "number",
                            "validation": { "required": true, "min": 4.0, "max": 20.0, "integer": false }
                            },
                            {
                            "name": "priorMetforminTrial",
                            "label": "Prior Metformin Trial Completed",
                            "description": "Confirm the patient has been trialed on metformin. Metformin is the universally required first-line agent for type 2 diabetes and must be trialed or contraindicated before most payers will authorize a GLP-1 receptor agonist.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "metforminDurationWeeks",
                            "label": "Duration of Metformin Trial (weeks)",
                            "description": "Enter the duration of metformin therapy in weeks. Most payers require a minimum 12-week adequate trial at therapeutic dose. Leave blank if metformin is contraindicated.",
                            "type": "number",
                            "validation": { "required": false, "min": 0, "max": 104, "integer": true }
                            },
                            {
                            "name": "metforminContraindicated",
                            "label": "Metformin Contraindicated",
                            "description": "Confirm if metformin is contraindicated for this patient. Common contraindications include eGFR < 30, active hepatic disease, or prior serious adverse reaction. If contraindicated, a prior trial is not required.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "diabetesEducationCompleted",
                            "label": "Diabetes Self-Management Education Completed",
                            "description": "Confirm the patient has completed or is enrolled in a diabetes self-management education program. Many payers require documented DSME participation as a condition of GLP-1 authorization.",
                            "type": "boolean",
                            "validation": { "required": true }
                            },
                            {
                            "name": "notes",
                            "label": "Additional Notes",
                            "description": "Include relevant clinical context such as specific metformin contraindications, other agents previously trialed, cardiovascular comorbidities, or documented DSME program details.",
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

            await context.SaveChangesAsync(cancellationToken);
        }
    }
}