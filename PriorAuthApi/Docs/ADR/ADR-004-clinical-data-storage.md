# ADR-004: ClinicalData Storage — JSON Column Keyed by FormDefinition Field Names

**Status:** Accepted  
**Date:** 2025-04

---

## Context

Each PA request requires clinical data that varies by drug and indication. A request for Humira for rheumatoid arthritis needs different fields than one for Ozempic for Type 2 diabetes — prior treatment history, lab values, diagnostic codes, and attestations differ entirely. The system needed a way to capture this variable clinical data without making every new PA scenario a schema change.

Three approaches were considered:

1. **Normalized columns** — a fixed set of nullable columns (e.g., `PriorTherapyTried`, `LabValue`, `DiagnosisCode`) on `PriorAuthRequest`. Simple but brittle — new scenarios require migrations, and many columns sit null for any given request.
2. **Separate `ClinicalData` table with EAV (Entity-Attribute-Value)** — fully flexible, but EAV schemas are notoriously difficult to query, validate, and reason about. The overhead wasn't justified.
3. **JSON column on `PriorAuthRequest`** — stores clinical data as a key-value blob, keyed by `FormDefinition` field names, deserialized from the submitted form payload.

---

## Decision

Store clinical data as a JSON column (`ClinicalData`) on `PriorAuthRequest`, keyed by the field names defined in the corresponding `AuthRule.FormDefinition`.

The contract is: whatever field names `FormDefinition` defines, those same names are the keys in `ClinicalData`. The frontend form is generated from `FormDefinition`; the submitted values are persisted as-is in `ClinicalData`. The rules engine reads `ClinicalData` using the same field name keys defined in `RuleDefinition`.

This replaces what was briefly considered as a separate `ParsedClinicalData` entity. That entity was scoped out — the added complexity of a separate table with its own FK and mapping logic wasn't justified when a JSON column accomplishes the same thing with less ceremony.

---

## Consequences

- Adding a new PA scenario with new clinical fields requires no schema migration — only a new `AuthRule` with an updated `FormDefinition` and corresponding `RuleDefinition`.
- The `FormDefinition` → `ClinicalData` → `RuleDefinition` field name contract is implicit. It's enforced by convention and seeder data, not by a foreign key or schema constraint. This is a known tradeoff — the system trusts that the seeded `AuthRule` records are internally consistent.
- `ClinicalData` is not individually queryable by field (e.g., "find all requests where HbA1c > 9") without JSON path queries or application-side filtering. For the current scope (reviewer reads a single request's clinical data), this is acceptable.
- The design is conceptually aligned with enterprise DTR (Documentation Templates and Rules) / CQL-driven questionnaire patterns, where form structure and evaluation logic are externalizable configuration rather than hardcoded application behavior.
