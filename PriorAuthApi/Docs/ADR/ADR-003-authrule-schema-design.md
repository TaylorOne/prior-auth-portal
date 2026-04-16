# ADR-003: AuthRule Schema Design — IndicationCode as Load-Bearing Field, JSON Columns for Rule and Form Definitions

**Status:** Accepted  
**Date:** 2025-04

---

## Context

`AuthRule` is the core configuration entity that drives PA evaluation. It defines, for a given drug or procedure, what clinical criteria must be met for authorization. Several design questions came up during modeling:

1. **How to key an AuthRule?** A drug like Humira (adalimumab) is used for rheumatoid arthritis, plaque psoriasis, Crohn's disease, and more. The PA criteria — required clinical data, step therapy requirements, documentation — are entirely different per indication. A single `Code` (e.g., HCPCS J0135) is not a sufficient key.
2. **How to represent the form structure that drives clinical data collection?** The questions asked during a PA submission vary by drug and indication. Hardcoding columns would make every new scenario a schema migration.
3. **How to represent evaluation rules?** Rules like "patient must have tried and failed methotrexate for at least 3 months" are structured data, not simple booleans.

---

## Decision

**On keying:** `AuthRule` has a unique constraint on `(Code, IndicationCode)`. `IndicationCode` is a load-bearing field, not a label. The same molecule with a different HCPCS billing code (e.g., Ozempic J3101 vs. Wegovy J3490) gets a separate `AuthRule` record — same active ingredient, different payer adjudication path.

`AuthRule` also carries two display-oriented fields that keep the API readable without requiring joins:
- `DisplayName` — the drug or service name (e.g., "Humira (adalimumab)")
- `IndicationDisplayName` — the indication label (e.g., "Rheumatoid Arthritis")

**On form structure:** A `FormDefinition` JSON column stores the field schema that drives clinical data collection on the submission form. It is deserialized to `JsonElement` at the API layer so it returns as a proper JSON object rather than an escaped string. The field names in `FormDefinition` serve as the keys in `ClinicalData` (see ADR-004).

**On evaluation rules:** A `RuleDefinition` JSON column stores the structured criteria used during PA evaluation — step therapy requirements, quantity limits, diagnostic thresholds. Same `JsonElement` deserialization pattern as `FormDefinition`.

---

## Consequences

- Adding a new PA scenario (new drug/indication pair) requires no schema migration — only a new `AuthRule` seed record with a `FormDefinition` and `RuleDefinition` appropriate to that scenario.
- The `(Code, IndicationCode)` unique constraint enforces data integrity at the database level and makes the intent explicit: the combination, not the code alone, identifies a rule.
- `FormDefinition` and `RuleDefinition` as JSON columns sacrifice queryability inside those structures in exchange for flexibility. For the current scope (5 seeded scenarios, no dynamic rule authoring UI), this is the right tradeoff.
- The `JsonElement` deserialization pattern requires care at the EF Core / serialization boundary — `HasColumnType("nvarchar(max)")` with `JsonStringEnumConverter` registered globally handles this cleanly.
- Gold carding is not implemented. It was considered (auto-approval for high-performing prescribers) but scoped out as a future enhancement.
