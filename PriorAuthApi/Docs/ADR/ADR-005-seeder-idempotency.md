# ADR-005: Seeder Idempotency — Per-Record AnyAsync Checks

**Status:** Accepted  
**Date:** 2025-04

---

## Context

The application uses seeded data for all demo accounts, organizations, practitioners, patients, and auth rules. No self-registration exists — all identities are pre-seeded. This means the seeder runs on every startup (or on-demand) and must be safe to run repeatedly without duplicating records.

Two idempotency strategies were evaluated:

1. **Bulk existence check** — `if (!context.Organizations.Any()) { seed all organizations }`. Simple, but all-or-nothing: if even one record exists, the entire seed block is skipped. A partially seeded database (e.g., after a failed first run) would silently remain incomplete.
2. **Per-record existence check** — `if (!await context.Organizations.AnyAsync(o => o.Id == id)) { seed this record }`. More verbose, but each record is independently idempotent.

---

## Decision

Use per-record `AnyAsync` checks throughout all seeders. Each seed record is guarded individually before insertion.

The primary driver is the planned **24-hour auto-reset Azure Function**, which will delete all `PriorAuthRequest` records (and their children) to keep the demo environment fresh, while leaving reference data (organizations, practitioners, patients, auth rules) intact. If the reset Function ever needs to also re-seed specific records, per-record idempotency means the seeder can be re-run safely against a partially reset database without risking duplicates or skipped blocks.

---

## Consequences

- Seeders are more verbose than bulk-check equivalents. This is an acceptable tradeoff — seeder code is run infrequently and the clarity of per-record guards outweighs the line count.
- A failed or partial seed run on first startup can be resolved by simply restarting — the seeder will fill in any missing records without touching existing ones.
- The auto-reset Azure Function can be scoped narrowly (delete transactional data only) with confidence that re-running the seeder afterward will safely restore any reference data that was affected.
- This pattern also makes it straightforward to add new seed records in future (e.g., a sixth PA scenario) without modifying existing seed logic — just append a new guarded block.
