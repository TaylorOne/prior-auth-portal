# Prior Authorization Rules — Reference

9 rules across 6 drugs/procedures. Each rule is keyed by **code + indication** — the same drug can have different approval criteria depending on why it's being prescribed.

---

## Procedures

### MRI Knee without Contrast
| Field | Value |
|---|---|
| Code | CPT 73721 |
| Indication | Knee pain (ICD-10: M25.561) |

**Approval requires all of:**
- Conservative therapy completed: **yes**
- Duration of therapy: **≥ 6 weeks**

---

### Genetic Testing — Hereditary Breast/Ovarian Cancer
| Field | Value |
|---|---|
| Code | CPT 81162 |
| Indication | BRCA1/BRCA2 risk (ICD-10: Z15.01) |

**Approval requires all of:**
- First-degree family history confirmed: **yes**
- Prior genetic counseling completed: **yes**

---

## Medications

### Humira (adalimumab) — Rheumatoid Arthritis
| Field | Value |
|---|---|
| Code | HCPCS J0135 |
| Indication | Rheumatoid Arthritis (ICD-10: M06.9) |
| Default dosage | 40mg subcutaneous injection every other week |

**Approval requires all of:**
- Prior DMARD trial completed: **yes**
- DMARD must be one of: Methotrexate, Hydroxychloroquine, Sulfasalazine, Leflunomide
- Duration of DMARD trial: **≥ 12 weeks**

---

### Humira (adalimumab) — Psoriatic Arthritis
| Field | Value |
|---|---|
| Code | HCPCS J0135 |
| Indication | Psoriatic Arthritis (ICD-10: L40.50) |
| Default dosage | 40mg subcutaneous injection every other week |

**Approval requires all of:**
- Prior NSAID trial completed: **yes**
- Duration of NSAID trial: **≥ 4 weeks**
- Diagnosis confirmed by rheumatologist or dermatologist: **yes**

---

### Humira (adalimumab) — Crohn's Disease
| Field | Value |
|---|---|
| Code | HCPCS J0135 |
| Indication | Crohn's Disease (ICD-10: K50.90) |
| Default dosage | 160mg at week 0, 80mg at week 2, then 40mg every other week |

**Approval requires all of:**
- Prior corticosteroid trial completed: **yes**
- Prior immunomodulator trial completed: **yes**
- Immunomodulator must be one of: Azathioprine, 6-Mercaptopurine, Methotrexate
- Disease classification: **Moderate or Severe**

---

### Wegovy (semaglutide) — Chronic Weight Management
| Field | Value |
|---|---|
| Code | HCPCS J3490 |
| Indication | Chronic Weight Management / Obesity (ICD-10: E66.9) |
| Default dosage | 0.25mg subcutaneous injection once weekly (starting dose) |

**Approval requires all of:**
- Prior supervised weight loss program completed: **yes**
- BMI threshold *(conditional)*:
  - If weight-related comorbidity present → BMI **≥ 27**
  - If no comorbidity → BMI **≥ 30**

---

### Ozempic (semaglutide) — Type 2 Diabetes
| Field | Value |
|---|---|
| Code | HCPCS J3101 |
| Indication | Type 2 Diabetes (ICD-10: E11.9) |
| Default dosage | 0.5mg subcutaneous injection once weekly |

**Approval requires all of:**
- Most recent HbA1c: **≥ 7.0%**
- Metformin step therapy *(conditional)*:
  - If metformin is **not** contraindicated → prior metformin trial (**yes**) + duration **≥ 12 weeks**
  - If metformin **is** contraindicated → step therapy waived
- Diabetes self-management education completed: **yes**

---

### Xarelto (rivaroxaban) — Atrial Fibrillation
| Field | Value |
|---|---|
| Code | RxNorm 1599538 |
| Indication | Atrial Fibrillation (ICD-10: I48.91) |
| Default dosage | 20mg orally once daily with evening meal |

**Approval requires all of:**
- Prior warfarin trial completed: **yes**
- Duration of warfarin trial: **≥ 4 weeks**
- Reason for transition to Xarelto must be one of: Unstable INR, Patient intolerance, Drug interaction, Patient preference

---

### Xarelto (rivaroxaban) — Deep Vein Thrombosis
| Field | Value |
|---|---|
| Code | RxNorm 1599538 |
| Indication | Deep Vein Thrombosis (ICD-10: I82.401) |
| Default dosage | 15mg twice daily for 21 days, then 20mg once daily |

**Approval requires all of:**
- DVT confirmed by imaging: **yes**
- Confirmatory imaging type must be one of: Duplex Ultrasound, CT Venography, MR Venography
- Prior heparin bridge therapy completed: **yes**
- Estimated treatment duration: **≥ 12 weeks**

---

## Rule Logic Patterns

Three patterns appear across these rules — worth knowing since they map directly to how `AuthEvaluationEngine` works:

| Pattern | Example |
|---|---|
| **Simple boolean** | `therapyCompleted == true` |
| **Numeric threshold** | `therapyDurationWeeks >= 6` |
| **Conditional (step therapy)** | Wegovy BMI cutoff changes based on comorbidity; Ozempic metformin requirement waived if contraindicated |
