# ADR-002: FHIR Adherence Strategy — Relational Storage, FHIR-Shaped DTOs at the Boundary

**Status:** Accepted  
**Date:** 2025-04

---

## Context

Once `ServiceRequest` and `MedicationRequest` were chosen as the core resources (see ADR-001), a follow-on decision was needed: how closely should the implementation track FHIR at the persistence and API layers?

Two approaches were evaluated:

1. **Use the Firely SDK** — serialize and deserialize FHIR resources directly, store as FHIR JSON or use a FHIR server (e.g., Azure Health Data Services), and expose true FHIR-compliant REST endpoints (`GET /ServiceRequest/{id}` returning a conformant resource).
2. **Model FHIR shapes as DTOs** — store data relationally in SQL Server via EF Core, and shape API responses to mirror FHIR field names and structures without using SDK machinery.

---

## Decision

Store relationally, speak FHIR at the boundary.

EF Core entities are standard relational models (normalized tables, FK relationships, no FHIR serialization concerns). At the API layer, response DTOs are shaped after FHIR resources — field names, nesting structure, and code values follow FHIR conventions — but are plain C# records with no dependency on Firely or any FHIR SDK.

The Firely SDK was evaluated and rejected for this project. It adds significant complexity (resource validation pipelines, canonical URL management, bundle construction) that is disproportionate to the goal. The project is demonstrating understanding of FHIR concepts and how they map to real PA workflows — not building a FHIR server.

---

## Consequences

- **API responses** use FHIR-informed field names (e.g., `status`, `subject`, `requester`, `authoredOn`) and code values (e.g., `draft`, `active`, `completed`) but are not validated against FHIR StructureDefinitions.
- **DTO mapping** uses a `FromEntity` static factory pattern co-located with each DTO. This keeps mapping logic close to the shape being produced and avoids a separate mapping layer.
- **No SDK dependency** means no Firely NuGet packages, no conformance validation, and no FHIR server infrastructure. This is appropriate for a portfolio/demo context.
- The approach is honest about what it is. Documentation (README, ADRs) makes the "FHIR-informed, not FHIR-compliant" stance explicit so a reviewer understands the tradeoff rather than assuming an accidental gap.
- Terminology choices (code systems, value set codes) still follow FHIR conventions where practical — the intent is FHIR literacy, not FHIR theater.
