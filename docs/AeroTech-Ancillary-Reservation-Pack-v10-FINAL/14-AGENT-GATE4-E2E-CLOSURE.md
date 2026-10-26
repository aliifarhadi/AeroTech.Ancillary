# Gate 4 — Coding Agent — End-to-End Closure با SQL و HTTP

**Precondition:** Gates0..3 reviewed/accepted. Authority entire v10 Pack; 01 Master wins. Do not silently reference old v12/v13.

## Final verification order
1. Print current HEAD/branch/working tree, commits per gate, diff summary to baseline `2025efd4...`.
2. `dotnet build AeroTech.Ancillary.sln --no-incremental` with zero new errors; report warnings vs baseline.
3. Run Domain conformance, Shopping (including current V9), Application Acceptance, Reservation+Inventory SQL tests. Report discovered/pass/fail counts for each; not omit slow tests.
4. `dotnet ef migrations has-pending-model-changes` on `AncillaryDbContext` and validate new schema precision/indexes.
5. SQL concurrency tests from I21/I22/I23 twice with independent OrderIDs/connections; capture lock and availability outcomes.
6. HTTP live with test authorization on `Service` endpoints (no invented production creds). Test Create mixed NonSeat, GET, SeatAppend same holdId, unit Release, Confirm, unit Cancel, all Cancel/Release, expiry, retry, conflict.
7. Confirm SeatMap HTTP path requires no physical availability or FlightFlow calls; existing `SeatMaps/Resolve` behavior unchanged.
8. Prove `No write on invalid request`, no legacy virtual-hold fake success, no Supplier fake response. Unsupported patterns intentionally gated.
9. Evaluate and report Ordering adapter mapping contract `05`: if Ordering repo isn't accessible, **mark consumer integration unverified**, not PASS. No changes in Ordering or FlightFlow.
10. Verify no health checks/BackgroundServices for expiry are duplicated; existing poller honored.

## Closure evidence table format
| ID | Test name | Test command / HTTP method path | Result PASS/FAIL/GATED | Evidence | Changed SHA |
|---|---|---|---|---|---|

Create `reports/V10-RESERVATION-FINAL-CLOSURE.md` including:
- Current authority chain and actual source SHA; precise Additive Migration SQL.
- Field-by-field before/after contract summary; no unnecessary endpoints.
- SQL stock from **two competing orders**, Count/Weight/Slot, partial release; before/after usage.
- Unit status table for mixed Hold before and after Partial Release/Confirm/Partial Cancel.
- SeatMap commercial-only proof and FlightFlow zero calls.
- All old regression suites, no deleted/disabled tests.
- Unverified/gated Supplier and Ordering integration separately, no success claim.
- Exact remaining risks, if any, and no background work promises.

**Release gate:** all P0 Local inventory safety tests PASS and no new SeatMap/Price/Resolve regression. If any of these fail, do not close or merge to release branch. Coding Agent must not claim EMD/Ordering end-to-end functionality from Ancillary-only tests.
