# Build Plan

What exists today (all offline, all runnable with `dotnet test` / `dotnet run`):
contracts, orchestrator with citation guard and deterministic aggregation,
markdown policy store, heuristic stub model, model gateway with routing and
usage logging, synthetic policy corpus (6 policies), labeled eval set
(18 cases), eval harness with regression gating.

The milestones below are the parts *you* build, in order. Each one is
independently demonstrable.

## M0 — Extract to its own repository

```bash
mkdir ../pa-determination-agent && cp -r determination-agent/. ../pa-determination-agent/
cd ../pa-determination-agent && git init -b main && git add -A && git commit -m "Import determination agent"
```

Then delete `determination-agent/` from the portal repo. Add a CI workflow
(build, test, eval with `--min-criterion-accuracy` as the regression gate —
see the workflow in `.github/workflows/determination-agent.yml` for a
starting point).

## M1 — Wire a real model into the gateway

- Provision Azure OpenAI (or AI Foundry) with two deployments: something cheap
  for `triage`, something strong for `reasoning`.
- Point `Gateway:Routes:*` at provider `azure-openai` with your deployment
  names; run the gateway (`dotnet run --project src/DeterminationAgent.Gateway`).
- Run the eval through it:
  `dotnet run --project eval/DeterminationAgent.Eval -- --gateway http://localhost:5000`
- **Deliverable:** a real-model eval report side by side with
  `docs/baseline-eval-report.md`. The interesting rows are the two cases the
  stub gets wrong (WGV-002, WGV-003) — a competent model should fix both.
- Then replace the api-key with managed identity / `DefaultAzureCredential`
  bearer tokens (you know this drill from the portal).

## M2 — Prompt iteration under regression gating

- Tune `PromptBuilder.SystemPrompt` (few-shot examples, tighter JSON rules)
  and re-run the suite after every change.
- Wire `--min-criterion-accuracy` into CI at your current score minus a small
  margin, so a regressing prompt change fails the build.
- **Deliverable:** a short log of prompt versions and their scores.

## M3 — Real retrieval (RAG)

- Stand up Azure AI Search; chunk and embed the policy corpus (one chunk per
  clause — the ids must survive round-trip).
- Implement `IPolicyStore` against it (hybrid search on payer + service +
  indication + clause text) and swap it in behind the same interface.
- Add retrieval-specific eval cases: near-miss policies, paraphrased
  indications. MRI-004 (wrong-policy trap) is the seed of this family.
- **Deliverable:** retrieval precision numbers in the eval report.

## M4 — Portal integration (the tool boundary becomes real)

Portal side (in `prior-auth-portal`):
- `POST /priorauth/{id}/recommendation` accepting the `DeterminationDraft`
  contract; persist alongside the request; new `DeterminationAgent` app role
  with client-credentials auth (daemon flow — milestone 7 of your second-pass
  checklist).
- `GET /priorauth/{id}/evidence` returning clinical data + metadata in the
  evidence-item shape the agent consumes.
- Publish an event (Service Bus topic or queue) when a request transitions to
  `UnderReview` — that's the agent's trigger, exactly where the human
  bottleneck is today.

Agent side:
- `HttpPriorAuthCaseClient : IPriorAuthCaseClient` calling the evidence
  endpoint with a client-credentials token.
- A worker (console or Function) subscribing to the UnderReview event:
  fetch case → orchestrate → post draft back.
- **Deliverable:** submit a manual-review request in the portal UI, watch a
  draft recommendation appear on the reviewer detail page.

## M5 — Human-in-the-loop capture

- Reviewer detail UI renders the draft (per-criterion statuses, citations,
  gaps) next to the clinical data; approve/edit/reject captures the delta
  between draft and final decision.
- Feed the deltas back as labeled eval cases; reviewer override rate per
  criterion becomes the online quality metric.
- **Deliverable:** eval corpus grows from production disagreements.

## M6 — Infrastructure and calibration

- Bicep for the agent stack (gateway + worker on App Service/Container Apps,
  AI Search, OpenAI, App Insights), following the portal's `infra/` pattern.
- Confidence calibration: from eval data, measure how often high-confidence
  triage answers were wrong; tune the escalation rule and measure the
  cost/quality trade-off using the gateway usage log.
- **Deliverable:** a cost-per-determination number and an
  escalation-threshold justification, both derived from logged data.

## Scope guardrails

Same rules as ever: no UI framework work beyond the portal's existing reviewer
page, no general chat interface, no multi-policy reasoning. If a milestone
exceeds a few sittings, cut scope, not rigor.
