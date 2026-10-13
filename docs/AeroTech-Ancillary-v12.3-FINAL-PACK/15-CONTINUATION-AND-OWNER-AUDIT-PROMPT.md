# 15 — Next chat / owner audit continuation

We are working on `aliifarhadi/AeroTech.Ancillary` only. Latest audited pre-change branch `k8s-stg@8fda5f1e8c2586a08fa995a0f8701e9299182986`, date 2026-10-13. Current authority is the standalone **AeroTech Ancillary v12.3** pack (00–17), not previous v12.2/R1 addenda. The owner explicitly says no historical data; clean greenfield schema edits are permitted. All replies to the owner should be Persian; the coding agent follows English technical pack.

Business architecture: Seat A07/A08/A09 commercial catalog, Provision and Pricing, independent of physical seat ownership. **No Seat AncillaryInventoryPolicy**. No direct FlightFlow clients, provider/delegation references or SeatMap endpoints in Ancillary. NonSeat inventory policies still exist and retain local Count/Weight/Slot, Supplier/Unlimited truthful semantics. P2 Shopping is **already built**; it MUST be adjusted now: `SeatOffers` separate HTTP surface from `AncillaryOffers` but one canonical `AncillaryShoppingEngine`. Seat commercial price can be by category; per-number pricing requires trusted characteristic facts (no direct FF calls or arbitrary client claims). Real SeatMap and physical seat availability belong outside Ancillary; don't implement their composition in this task. P3 Hold/Reservation/Ordering out of scope. Latest source intentionally allows Provisions without POS restrictions; keep that.

On receipt of agent report or push:
1. Inspect actual new GitHub `k8s-stg` HEAD and diff from `8fda5f1e`.
2. Check README/reports/source/tests/DI/migrations against all pack chapters; verify Route + DTO and true greenfield no-policy Seat scenario.
3. Confirm compiler/SQL/HTTP evidence, no direct FlightFlow references in runtime src, no SeatMap stub, no Shopping Engine duplication, no P3 modifications, and no nonSeat regression.
4. Report PASS/FAIL/BLOCKED with exact code source locations. If more code changes needed, give agent-ready markdown prompt; no invented source findings.
