# Coding Agent — Gate 5 — final contract and behavior closure

**Precondition:** Gates0–4 approved. Single authority files 01–08 v11. This is an **audit/repair only**, not license to add new architecture.

1. Run complete Build and discovered tests under `Domain`, `Shopping`, `Application` and Reservation suites. Include SQL independent concurrency, migration `has-pending-model-changes`, migration SQL integrity, no deleted or suppressed tests. Record discovery vs executed counts and skipped results.
2. Exercise HTTP on actual ServiceHost/db: Create **mixed supported** A01 (Qty) + A15 (SSR) + A20 (appointment if Local real available, else Unlimited concrete) + SeatMap append; GET typed; Release one; Confirm; Cancel one; Retry; Expire. Do not invent dev Reference sources. If Local live cannot run, `GATED` explicitly.
3. Assert exact wire shape family→variant→products→units, selected.options values after SQL reload, after expiry/Retired Definition; Receipt minimal. All prohibited `coverageScope`, `serviceDefinitionId`, `inventoryFacts`, `services[]` rejected by strict input.
4. Verify **no** extra surface/Route/Provider/Order reader/FlightFlow SeatHeld path. Inspect all changed files for overengineering: multiple orchestrators, empty selection tables, ad hoc DTO fields, copy of entire Offer/Pricing, `JsonObject` persistence, generic 24-type nullable VO, duplicated inventory source counters. Delete obsolete old API records only after migration tests and reference audit.
5. Compare exact source contract fields `AncillaryOffers` and `Resolve` to v11 names; document unavoidable `travellerRef→travellerId`, `flightRefs→flightIds`, `Resolve.provision.provisionId→products[].provisionId`; must not claim literal identical leaf shapes.
6. Publish supported/gated variant matrix A01–A24 with tests; no unsupported capability marked passed. No Claim Ordering E2E unless user-approved external consumer test was **actually** performed; Ancillary itself must never call Ordering.
7. Report `reports/V11-FINAL-CLOSURE.md`: source SHA, file changes, migration, API JSON snapshots, all test IDs and results, SQL concurrency, raw HTTP evidence, list of remaining GATED/NOT VERIFIED, Gate failures and corrective minimal edits.

**No Commit/Push/Merge** without Owner command. If the new code is ugly/brittle despite green tests, fail closure until structured typed Domain and API fit the current source.
