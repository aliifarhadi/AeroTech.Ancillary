# Gate 4 — Coding Agent — Final Evidence & Conformance (No Fake PASS)

## Preconditions
Owner explicitly approved prior Gates. HEAD and clean/dirty working tree captured. Run only within authorized environment; no unrequested commit/push/merge, no data cleanup on owner DB.

## Full requirements
1. `dotnet build AeroTech.Ancillary.sln --no-incremental` and all Domain, Application, Shopping tests. Record discovered/executed/passed/skipped/failed/aborted and 61 warnings baseline differences. No new Skip.
2. Run all W01–W12 (strict Contract), D01–D12 (Typed Round-trip), I01–I10 (SQL real contention and Retire) and L01–L13 including the F02 already-400 evidence and new F03 wrong-query-key safety regression. Use real HTTP + isolated SQL for critical binding, not just mock Controller.
3. Supported A01/A02/A03-per-piece/A04/A05/A06/A07/A08/A10 Fixed/A11/A12/A15/A16/A20/A21/A22/A23/A24: map each to TestId + actual result. GATED A09, A10 Quote, A13/A14, A17/A18/A19, Supplier, AcceptedWeight remain explicit; prove fail-closed, no manufactured references.
4. GET after status changes and after Product Retirement must load SQL only and return exact previously accepted choices, including SSR, dimensions, menu, appointment, Plan, Seat.
5. Search reachable dependencies from Hold/Confirm/Release/Cancel/Expire/GET/Seat Hold: No Ordering data access, no Order ACL, no FlightFlow physical call. Shopping/SeatMap frozen and unaffected. Check route diff, EF schema migration list, JSON blob ban.
6. E2E Ordering only claim VERIFIED if actual integration executed; otherwise `NOT VERIFIED`. Live Local capacity only VERIFIED with real reference resources and live run, otherwise `GATED`.
7. Re-evaluate every source claim against HEAD, including doc v11 stale status; create evidence ledger and risk register. Do not claim full Green if a reproducible safety defect remains; F02 is not a reproduced P0.
8. Run `python3 VERIFY-PACK.py` on a newly extracted copy of v12 ZIP if Pack path is available; Pack integrity distinct from application Closure.

## Final report
Write `reports/V12-FINAL-CLOSURE.md` with `Verdict PASS / PASS WITH GATED / FAIL`, SHA/branch, exact commands, counts, per-scenario evidence, before/after for F03 wrong-query-key guard, migration and roll-forward/back plan if modified, traces to 02/03/04/05, and explicit `NOT VERIFIED` items. **Stop**; wait for Owner instruction before Git commit/push/merge.
