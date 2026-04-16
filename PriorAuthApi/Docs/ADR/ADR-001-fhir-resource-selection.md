# ADR-001: FHIR Resource Selection for the Prior Authorization Envelope

**Status:** Accepted  
**Date:** 2025-04

---

## Context

A prior authorization request needs a primary FHIR resource to act as the envelope — the root resource that represents "a clinician is requesting authorization for a service or medication." The Da Vinci Prior Authorization Support (PAS) Implementation Guide v2.2.1 uses `Claim` with `$submit` as its primary operation, which mirrors how production payer systems (including X12 278 workflows) represent PA requests. That was the first candidate evaluated.

The system also needs to handle two meaningfully different PA subtypes:
- **Drug PAs**, which require medication-specific data (drug code, dosage, quantity, days supply)
- **Procedure/diagnostic PAs**, which require only a service code and clinical justification

---

## Decision

Use `ServiceRequest` as the universal PA envelope, with an optional child `MedicationRequest` for drug PAs.

- A `ServiceRequest` with a null `MedicationRequest` FK represents a procedure or diagnostic PA.
- A `ServiceRequest` with a populated `MedicationRequest` FK represents a drug PA.
- The request type (`Drug` vs `Procedure`) is therefore derivable from the presence of the child record — no separate `RequestType` column is needed.

`Claim` / `ClaimResponse` were considered and rejected. `Claim` is the correct resource in a full Da Vinci PAS implementation targeting X12 278 interoperability, but it carries significant overhead: it requires `Coverage`, `ClaimResponse`, and a `$submit` operation that doesn't fit a simple REST API. For a portfolio project modeling the PA workflow without targeting certification compliance, that overhead doesn't pay off.

The Da Vinci PAS IG is used as a design north star — the resource shape, field semantics, and terminology choices are informed by it — but full conformance is not a goal.

---

## Consequences

- The API surface and DTO shapes are FHIR-informed but not FHIR-compliant. This is intentional and documented.
- `Coverage` is out of scope. Insurance context is implied by the payer organization relationship rather than modeled as a discrete FHIR resource.
- X12 278 is acknowledged in architecture documentation but not implemented.
- If this system were ever extended toward real Da Vinci PAS conformance, `ServiceRequest` would need to be replaced or wrapped with `Claim/$submit`. That's a known and accepted tradeoff.
- The `ServiceRequest` + optional `MedicationRequest` pattern is a clean, queryable relational design. It avoids a wide single-table approach while keeping the two PA subtypes in a coherent hierarchy.
