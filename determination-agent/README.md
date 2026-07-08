# Prior Authorization Determination Agent

A retrieval-grounded, tool-calling agent that drafts prior-authorization
determination recommendations for a human reviewer: given a request, it
resolves the governing payer policy, checks each policy clause against the
submitted clinical evidence, and emits a **structured draft** with
per-criterion citations and evidence gaps. A human approves — always.

This is the companion project to the [PriorAuth portal](../README.md): the
portal's deterministic rules engine handles structured criteria; this agent
targets the cases routed to manual review. Design record:
[ADR-001](docs/adr/ADR-001-determination-agent.md).

**Everything here runs offline** — the model gateway ships with a
deterministic heuristic baseline, so the full pipeline (prompt → parse →
citation guard → aggregation → eval) works with zero cloud dependencies.
Real models slot in behind the gateway (see [build plan](docs/build-plan.md)).

## Layout

```
src/DeterminationAgent.Contracts   structured draft schema (the wire contract)
src/DeterminationAgent.Core        orchestrator, policy store, citation guard, stub model
src/DeterminationAgent.Gateway     model router service: task-class routing + usage log
eval/DeterminationAgent.Eval       eval harness: metrics, report, regression gate
eval/data/policies                 synthetic payer policy corpus (clause-structured markdown)
eval/data/cases                    labeled eval cases with ground-truth determinations
tests/                             unit tests for the governance-critical paths
docs/                              ADR, build plan, failure-mode catalog, baseline report
```

## Quickstart

```bash
dotnet test                                        # 16 tests, no cloud needed
dotnet run --project eval/DeterminationAgent.Eval  # run the eval suite, print report

# run the gateway (stub routes) and eval through it over HTTP:
dotnet run --project src/DeterminationAgent.Gateway &
dotnet run --project eval/DeterminationAgent.Eval -- --gateway http://localhost:5000
```

Baseline results (heuristic stub): see
[docs/baseline-eval-report.md](docs/baseline-eval-report.md) — 97% criterion
accuracy with two deliberate failures (`WGV-002`, `WGV-003`) that a real model
must beat. The [failure-mode catalog](docs/failure-modes.md) explains why the
baseline's mistakes are load-bearing.

## Governance invariants (enforced in code, tested)

1. **No citation → not asserted.** `CitationGuard` downgrades any met/not-met
   finding lacking a valid clause citation and drops findings about
   nonexistent clauses.
2. **The model never picks the outcome.** The recommendation is derived
   deterministically from per-criterion findings.
3. **Fail closed.** Unparseable output, unknown policy, missing evidence —
   every failure path pends for a human; none approve or deny.
4. **Every model call is logged** through the gateway (task class, provider,
   model, tokens, latency, success).

## Extracting to its own repository

This directory is self-contained (own solution, no references into the
portal). To split it out:

```bash
mkdir ../pa-determination-agent && cp -r determination-agent/. ../pa-determination-agent/
cd ../pa-determination-agent && git init -b main && git add -A && git commit -m "Import determination agent"
```

Then delete `determination-agent/` from the portal repo and move
`.github/workflows/determination-agent.yml` across (dropping the `paths`
filter and the working-directory prefix).
