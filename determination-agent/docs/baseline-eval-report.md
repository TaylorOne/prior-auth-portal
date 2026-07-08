# Determination Agent — Eval Report

- Generated: 2026-07-08 13:50 UTC
- Model source: in-process stub (stub-heuristic-v1)
- Cases: 18

## Summary

| Metric | Value |
|---|---|
| Recommendation accuracy | 17/18 (94.4 %) |
| Per-criterion accuracy | 64/66 (97.0 %) |
| Precision on not-met (unjustified denials) | 100.0 % |
| Recall on not-met | 83.3 % |
| Precision on insufficient-evidence | 83.3 % |
| Gap-detection recall (insufficient-evidence) | 100.0 % |
| Hallucinated criteria (dropped by guard) | 0 |
| Citation downgrades (guard-enforced) | 0 |
| Escalation rate | 5/18 |

## Per-case results

| Case | Tags | Recommendation | Criteria | Notes |
|---|---|---|---|---|
| BRCA-001-clear-approve | clear-approve | OK (approve) | 4/4 |  |
| BRCA-002-pend-missing-counseling | pend, missing-evidence | OK (pend-for-info) | 4/4 |  |
| BRCA-003-deny-counseling-declined | clear-deny, negation | OK (deny) | 4/4 |  |
| HUM-PSA-001-clear-approve | clear-approve | OK (approve) | 3/3 |  |
| HUM-PSA-002-deny-no-prior-therapy | clear-deny, negation | OK (deny) | 3/3 |  |
| HUM-RA-001-clear-approve | clear-approve | OK (approve) | 4/4 |  |
| HUM-RA-002-deny-short-dmard-trial | clear-deny, numeric-threshold | OK (deny) | 4/4 |  |
| HUM-RA-003-pend-vague-tb-screening | pend, ambiguous-evidence | OK (pend-for-info) | 4/4 |  |
| MRI-001-clear-approve | clear-approve | OK (approve) | 4/4 |  |
| MRI-002-deny-short-therapy | clear-deny, numeric-threshold | OK (deny) | 4/4 |  |
| MRI-003-pend-missing-radiograph | pend, missing-evidence | OK (pend-for-info) | 4/4 |  |
| MRI-004-wrong-policy-trap | wrong-policy, no-governing-policy | OK (pend-for-info) | 0/0 |  |
| WGV-001-clear-approve | clear-approve | OK (approve) | 4/4 |  |
| WGV-002-deny-bmi-below-threshold | clear-deny, numeric-threshold | WRONG (expected deny, got approve) | 3/4 | C1: expected not-met, got met |
| WGV-003-pend-no-program-documentation | pend, missing-evidence | OK (pend-for-info) | 3/4 | C1: expected met, got insufficient-evidence |
| XAR-001-clear-approve | clear-approve | OK (approve) | 4/4 |  |
| XAR-002-deny-low-stroke-risk | clear-deny, numeric-threshold | OK (deny) | 4/4 |  |
| XAR-003-pend-missing-renal-function | pend, missing-evidence | OK (pend-for-info) | 4/4 |  |
