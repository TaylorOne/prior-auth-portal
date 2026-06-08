export const fieldLabels: Record<string, string> = {
  // Humira RA
  priorDMARDTrial: "Prior DMARD Trial",
  dmardDurationWeeks: "Duration of DMARD Trial (weeks)",

  // Humira Psoriatic Arthritis
  priorNSAIDTrial: "Prior NSAID Trial",
  nsaidDurationWeeks: "Duration of NSAID Trial (weeks)",
  dermatologyConfirmed: "Dermatology Consultation Confirmed",

  // Humira Crohn's
  // (shares priorDMARDTrial, dmardDurationWeeks)
  colonoscopyConfirmed: "Colonoscopy Confirmed",

  // Wegovy
  bmi: "Current BMI",
  comorbidity: "Weight-Related Comorbidity Present",
  priorWeightLossProgram: "Prior Supervised Weight Loss Program Completed",

  // Ozempic
  hba1c: "Most Recent HbA1c (%)",
  priorMetforminTrial: "Prior Metformin Trial Completed",
  metforminDurationWeeks: "Duration of Metformin Trial (weeks)",
  metforminContraindicated: "Metformin Contraindicated",
  diabetesEducationCompleted: "Diabetes Self-Management Education Completed",

  // Xarelto AFib
  priorWarfarinTrial: "Prior Warfarin Trial",
  warfarinDurationWeeks: "Duration of Warfarin Trial (weeks)",
  reasonForSwitch: "Reason for Switch to Xarelto",

  // Xarelto DVT
  dvtConfirmed: "DVT Confirmed by Imaging",
  imagingType: "Imaging Type",
  priorHeparinBridge: "Prior Heparin Bridge Therapy",
  estimatedTreatmentDurationWeeks: "Estimated Treatment Duration (weeks)",

  // MRI Knee
  therapyCompleted: "Physical/Occupational Therapy Completed",
  therapyDurationWeeks: "Duration of Therapy (weeks)",

  // BRCA Genetic Testing
  familyHistoryConfirmed: "First-Degree Family History Confirmed",
  priorCounselingCompleted: "Prior Genetic Counseling Completed",
};

export function getFieldLabel(fieldName: string): string {
  return fieldLabels[fieldName] ?? formatCamelCase(fieldName);
}

// Fallback: converts "priorWeightLossProgram" → "Prior Weight Loss Program"
function formatCamelCase(field: string): string {
  return field
    .replace(/([A-Z])/g, " $1")
    .replace(/^./, (c) => c.toUpperCase())
    .trim();
}