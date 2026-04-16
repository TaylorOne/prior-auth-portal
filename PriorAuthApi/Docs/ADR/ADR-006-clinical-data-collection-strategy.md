# ADR-006: FormDefinition-Driven Clinical Data Collection vs. AI Note Parsing

**Status:** Accepted  
**Date:** 2025-04

---

## Context

PA submissions require structured clinical data that varies by drug and indication — prior treatment history, lab values, diagnostic codes, attestations. Two approaches were evaluated for how a prescriber provides this data during submission:

1. **AI note parsing** — the submission form presents a large free-text field where the clinician dictates or pastes a clinical note (SOAP format or similar). The backend sends the note to an LLM, extracts structured clinical data from the prose, and populates `ClinicalData` from the parsed output.

2. **FormDefinition-driven dynamic fields** — `AuthRule.FormDefinition` defines the field schema for a given drug/indication. The frontend reads this definition and renders structured inputs (dropdowns, checkboxes, text fields) directly. The clinician fills in the form; submitted values are persisted as-is to `ClinicalData`, keyed by field name.

---

## Decision

Use FormDefinition-driven dynamic fields. AI note parsing is explicitly deferred as a potential future UX enhancement, not the primary data collection mechanism.

The FormDefinition approach is not merely simpler — it is more architecturally representative of how the industry has converged on this problem. The Da Vinci Documentation Templates and Rules (DTR) specification addresses exactly this: a payer publishes a structured `Questionnaire` resource for a drug/indication, the EHR's DTR SMART app renders that questionnaire inside the clinical workflow (optionally pre-populating answers via CQL rules from the patient record), and a structured `QuestionnaireResponse` is returned to the payer. The payer's rules engine evaluates the structured response against its criteria.

The mapping to this project's design is direct:

| Da Vinci DTR concept | This project |
|---|---|
| `Questionnaire` resource | `AuthRule.FormDefinition` JSON column |
| DTR SMART app rendering the form | Frontend reads `FormDefinition`, renders dynamic fields |
| `QuestionnaireResponse` | `ClinicalData` JSON column, keyed by `FormDefinition` field names |
| CQL-driven payer evaluation | Rules engine reading `ClinicalData` against `AuthRule.RuleDefinition` |

AI note parsing was deferred for several reasons:
- Free-text clinical note parsing into structured PA data is an active unsolved problem in health IT, not a standardized workflow pattern. Implementing it would pull the project toward a demo novelty rather than something that models how the industry is actually moving.
- It would introduce an LLM call in the submission path, requiring error handling around extraction confidence, latency, and correction UX if the parse is wrong — significant scope for uncertain gain.
- The `FormDefinition` → `ClinicalData` contract already provides a clean extension point: a future enhancement could pre-populate form fields from parsed note text, layering AI assistance *on top of* the structured submission flow rather than replacing it.

---

## Consequences

- Clinical data arriving in `ClinicalData` is clean, typed, and immediately usable by the rules engine without a parsing or validation step.
- The `FormDefinition` field schema is the authoritative contract between form rendering and data storage. Adding a new PA scenario with new clinical fields requires no schema migration — only an updated `AuthRule` seed record.
- The design is conceptually aligned with Da Vinci DTR and the broader industry direction toward payer-published, EHR-rendered structured questionnaires.
- AI note parsing remains a viable future enhancement. Because `ClinicalData` is a flat JSON bag keyed by field name, a parsing layer could populate it by the same field name keys that the form currently populates by direct input — no downstream contract changes required.
- The submission UX is more structured than a free-text field, which is appropriate for a B2B prescriber-to-payer interface. Clinicians in real PA workflows are accustomed to structured data entry, not narrative authoring.
