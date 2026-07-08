# ADR-001: Prior Authorization Determination Agent

* Status: Accepted — walking skeleton implemented (offline core, stub model)
* Date: 2026-07-08
* Deciders: Jordon
* Depends on: PriorAuth portal (`TaylorOne/prior-auth-portal`)

## 1. Context

PriorAuth is a .NET/React prior-authorization portal on Azure (Service Bus,
Azure SQL, managed identity, structured audit logging). It handles intake and
orchestration of authorization requests and already contains a **deterministic
rules engine** (`AuthEvaluationEngine`) that evaluates structured clinical
form fields against machine-readable rule definitions.

What the portal does *not* automate is the manual-review step: requests whose
auth rule sets `RequiresManualReview` land in `Status.UnderReview`, where a
human reviewer reads payer policy, checks the submitted documentation against
criteria, identifies missing evidence, and renders a determination. That step
is a real, regulated, high-volume business process — and the right insertion
point for an agent that **assists (not replaces)** the reviewer.

### Relationship to the deterministic engine

This is the design sentence that makes the whole project defensible: clear-cut
structured criteria (`bmi >= 30`, `therapyDurationWeeks >= 6`) stay with the
deterministic engine — cheap, exact, already audited. The agent targets
precisely the cases the engine routes to manual review, where criteria live in
unstructured policy text and evidence is documentation rather than form
fields. An LLM never re-adjudicates something a rules engine can decide.

### Non-goals

* **Not an autonomous approver.** The agent recommends; a human determines.
  Hard boundary, not a v2 relaxation.
* **Not a general medical-reasoning system.** Scope is matching submitted
  documentation against a specific, retrievable policy's criteria.
* **Not a replacement for intake/orchestration** — it is a reasoning layer
  invoked by the existing workflow.

## 2. Decision

Build a retrieval-grounded, tool-calling determination agent invoked as a step
in the PriorAuth workflow, fronted by a model router/gateway, with a
human-in-the-loop approval boundary and a first-class evaluation harness.

```
Incoming auth request (PriorAuth intake, Status → UnderReview)
        │
        ▼
┌─────────────────────────────────────────────┐
│  Determination Agent (orchestrator)         │
│  1. Resolve payer + service → policy id     │
│  2. Retrieve governing policy criteria      │
│  3. Fetch submitted clinical evidence       │
│     (tool calls into PriorAuth APIs)        │
│  4. Reason: criteria vs. evidence           │
│  5. Emit STRUCTURED determination draft     │
│     + per-criterion citations + gaps        │
└─────────────────────────────────────────────┘
        │
        ▼
   Human reviewer ──approve / edit / reject──► PriorAuth determination record
        │
        ▼
   Everything logged → eval + audit store
```

### 2.1 Decisions locked in by the implementation

1. **Separate deliverable from the portal.** The agent talks to PriorAuth only
   through its public API (client-credentials daemon auth). Living outside the
   portal solution makes that boundary physical and keeps the core portable.
2. **The gateway is a standalone service** (`DeterminationAgent.Gateway`), not
   middleware in the PriorAuth API. It routes by task class
   (`triage` → cheap model, `reasoning` → frontier model), logs every call to
   a central usage log (task class, provider, model, tokens, latency,
   success), and is the only component that knows provider specifics —
   swapping Azure OpenAI for Vertex is a route-table change.
3. **Grounding is enforced in code, not in the prompt.** `CitationGuard` runs
   after every model call: a `met`/`not-met` finding without a valid citation
   to an existing policy clause is downgraded to `insufficient-evidence`;
   findings about clauses that don't exist are dropped and counted as
   hallucinations; clauses the model skipped are filled in as
   `insufficient-evidence`. No citation → not asserted.
4. **The model never picks the outcome.** It assesses criteria; the
   recommendation is derived deterministically in code
   (any `not-met` → deny; else any `insufficient-evidence` → pend-for-info;
   else approve). Gaps are likewise derived from the findings.
5. **Fail closed.** Unparseable model output, unknown policy, missing case —
   every failure path lands on `pend-for-info` with low confidence, never on
   an approval or denial.
6. **Clause ids double as criterion ids.** Policies are authored as
   clause-structured markdown; this is what makes citations mechanically
   checkable rather than vibes.

### 2.2 Structured output contract

`DeterminationDraft` (camelCase, kebab-case enums on the wire):
`recommendation: approve | deny | pend-for-info`; per-criterion findings with
`status: met | not-met | insufficient-evidence`, citation, evidenceRefs,
rationale; `gaps`; `confidence: high | medium | low`;
`modelPath: triage-only | escalated`.

### 2.3 Escalation

Triage model first; anything below high confidence re-runs on the reasoning
model. Confidence calibration (what *should* force escalation) is an open
milestone — the current rule is deliberately conservative.

## 3. Evaluation harness (the differentiating artifact)

Implemented as `DeterminationAgent.Eval`, treated as a first-class deliverable:

* **Labeled test set** — curated synthetic cases with known-correct
  per-criterion determinations, aligned with the portal's seeded auth rules.
* **Offline metrics** — per-criterion accuracy; precision/recall on `not-met`
  (unjustified denials) and `insufficient-evidence` (gap detection); citation
  downgrades; hallucinated-criterion count; escalation rate.
* **Regression gating** — `--min-criterion-accuracy` fails the run in CI when
  a model or prompt change regresses the suite.
* **Baseline** — a deterministic heuristic stub model gives an honest,
  imperfect floor (see `docs/baseline-eval-report.md`) that real models must
  beat; it also makes the entire pipeline runnable offline and in CI.
* **Online signal (planned)** — reviewer edit/override rate per criterion once
  the HITL loop lands in the portal.

## 4. Consequences

* Compounds the PriorAuth investment (domain, FHIR alignment, APIs, seeded
  rules) instead of starting fresh.
* The regulated setting demands the eval rigor above; cutting it corners the
  one thing that makes the project credible.
* Router + RAG + eval is real surface area — the build plan
  (`docs/build-plan.md`) sequences it so each milestone is independently
  demonstrable.

## 5. Open questions (resolve at build time)

* Vector store: Azure AI Search vs. self-hosted (and the GCP analog).
* Whether the policy corpus grows via Synthea-generated FHIR data.
* Confidence calibration: how `confidence` should be derived and what forces
  escalation.
