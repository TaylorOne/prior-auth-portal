# Failure-Mode Catalog

Each failure mode has a code-level guard, a metric in the eval report, and —
where the stub can exercise it — a test case. When a new failure appears in
production or during model swaps, add a labeled case here first, then fix.

| # | Failure mode | Guard | Metric | Exercised by |
|---|---|---|---|---|
| F1 | **Hallucinated criteria** — model asserts a finding for a clause that doesn't exist in the policy | `CitationGuard` drops the finding entirely | "Hallucinated criteria" count | `CitationGuardTests.FindingForNonexistentClause_IsDroppedAndReported` (real-model runs are where this becomes non-zero) |
| F2 | **Ungrounded assertion** — `met`/`not-met` without a valid citation to the governing clause | `CitationGuard` downgrades to `insufficient-evidence` with a `[citation-invalid]` marker | "Citation downgrades" count | `CitationGuardTests.MetFindingWithoutValidCitation_IsDowngradedToInsufficient` |
| F3 | **Wrong-policy retrieval** — request resolves to no policy (or, with RAG, the wrong one) | Orchestrator pends for a human; never assesses against a guessed policy | Per-case policy mismatch note | `MRI-004-wrong-policy-trap` |
| F4 | **Over-confident denial** — asserting `not-met` when evidence doesn't support it | Aggregation makes any `not-met` decisive, so its precision is watched hardest | "Precision on not-met (unjustified denials)" | All `clear-deny` cases; watch this metric on model swaps |
| F5 | **Missed gaps** — failing to flag absent/insufficient evidence | Skipped clauses are force-filled as `insufficient-evidence` | "Gap-detection recall" | `*-pend-*` cases (missing radiograph, renal function, counseling, program docs) |
| F6 | **Unparseable model output** — prose, broken JSON, refusals | Fail closed: treated as zero findings → every clause `insufficient-evidence` → pend | Shows up as criterion mismatches on affected cases | `ModelResponseParser.TryParse` returning null path |
| F7 | **Silent scope creep by the model** — model "decides" the outcome in prose | The recommendation is computed in code from findings; model output has no recommendation field to honor | n/a (structural) | `OrchestratorTests.Aggregate_DerivesRecommendationFromFindings` |
| F8 | **Compound-threshold misreads** — e.g. "BMI ≥ 30, or ≥ 27 with comorbidity" | None yet — this defeats the heuristic baseline by design | `WGV-002` (baseline fails it: unjustified approval) | The first thing a real model must beat |

Working agreement: **the baseline's failures are features.** Do not tune the
stub to pass WGV-002/WGV-003 — the gap between the stub and a real model is
the measurement the whole project exists to make.
