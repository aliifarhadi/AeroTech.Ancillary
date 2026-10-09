# AeroTech Ancillary v12.1 — Phase 1 implementation authority for Coding Agent

**AUTHORIZATION:** Owner approved design decisions A–I on 2026-10-08. Implement **Phase 1 only**, then STOP for Owner audit. This prompt supersedes the `AWAITING OWNER APPROVAL` state in the original design-review files; those review files are now APPROVED as the design baseline, except any explicit conflict resolved in this prompt. No coding is required in a different repository.

## 0. Mandatory source and authority check

- Repository: `aliifarhadi/AeroTech.Ancillary`, branch `k8s-stg`, verified starting SHA `70c3de85eb8efd1000c6c1237cefd2c64773afda` (v12 Phase1). Create working branch `feat/ancillary-v12.1-phase1` from this exact SHA; do not force-reset live branches. If working HEAD differs, REPORT and STOP for reconciliation; never apply patches to an assumed baseline.
- Reference only: `aliifarhadi/Aerotech.AirPrice` `k8s-stg@1b41f08e9a22d3c27b807d5ea275120f0f7273ad`. Inspect real `AirFare` value objects and owned collections for DayTime, Seasonality, Blackouts, FlightApplication and AirChargeCondition. Reuse canonical enums/IDs, not code from elsewhere wholesale.
- Authority sequence: this prompt and v12.1 documents 00–08 (owner-approved), then verified actual source/contracts, then v12 for backward compatibility, then AirPrice/ATPCO benchmarks for semantics. If source conflicts with authority and cannot be migrated without loss, report exact diff and stop that change, do not invent fields or behavior.
- Source pack: `docs/AeroTech-Ancillary-v12.1-OWNER-REVIEW/` when copied into repo, or provided external ZIP. Maintain approved 3 phases. No changing AirPrice, AirAvail, Ordering, FlightFlow, JetPay or other repos.
- No new rule engine, EAV/JSON expression store, runtime shopping evaluator, seat inventory, supplier adapters, StockPool, Hold/Confirm changes, or EMD. Supplier aggregate untouched. Operational `AncillaryReservationAggregate` and all related API/query/tests frozen functionally.

## 1. Required design graph (implement exact model)

`Supplier` [existing] -> `AncillaryServiceDefinition` [commercial AR 1] -> N `AncillaryProvision` [commercial AR 2] -> N `AncillaryPricing` [commercial AR 3] -> N `AncillaryPricingLine`.

Provision's rule ownership is **ten optional typed rule-group child entities**, with typed child rows, not new aggregate roots:

1. `ProvisionPassengerEligibilityRule`: `ProvisionPassengerType`, `ProvisionEligibleAgeBand`.
2. `ProvisionSalesRestrictionsRule`: `ProvisionPointOfSale`, `ProvisionCustomer`, `ProvisionCustomerType`.
3. `ProvisionGeographyRule`: `ProvisionOriginAirport`, `ProvisionDestinationAirport`, `ProvisionViaAirport`, `ProvisionRoutePair`, `ProvisionServiceLocation`, `ProvisionCoverageCountry`.
4. `ProvisionFlightApplicationRule`: `ProvisionMarketingAirline`, `ProvisionOperatingAirline`, `ProvisionFlightNumber`, `ProvisionFlight`, `ProvisionAircraft`.
5. `ProvisionFareApplicationRule`: `ProvisionAirFare`, `ProvisionAirFareType`, `ProvisionFareFamily`, `ProvisionFareBasis`, `ProvisionCabinClass`, `ProvisionRbd`.
6. `ProvisionTravelDateRule`: `ProvisionPermittedTravelPeriod`, `ProvisionBlackoutPeriod`.
7. `ProvisionDayTimeApplicationRule`: `ProvisionDayTimeWindow`.
8. `ProvisionAdvancePurchaseRule`: typed minimal VO properties, no extra child entity.
9. `ProvisionBaggageApplicationRule`: typed minimal baggage values, no extra child entity.
10. `ProvisionSeatApplicationRule`: `ProvisionSeatNumber`, `ProvisionSeatCharacteristic`.

**Field types, cardinalities, validations and supported context** are specified in `02-ENTITY-CATALOG.md`, `03-RULE-SEMANTICS-AND-INVARIANTS.md` and `04-PRICING-AND-PUBLICATION.md`; implement them without renaming or adding independent concepts. Rule-group children are optional ownership boundaries: absence or empty allowed list = unrestricted. Preserve publication history and stable IDs for unchanged rows. Do not interpret a group name as a license to add generic rule rows.

## 2. Mandatory corrections to v12

- Replace peer-level 25 criteria lists and `ProvisionTravelDate(DateOnly)` with typed group ownership and `ProvisionPermittedTravelPeriod(StartDate,EndDate)`. One day = equal endpoints, consecutive 1000 days = **one range**. Multiple disjoint ranges allowed; canonicalize overlapping/adjacent Draft ranges. Range endpoints inclusive.
- Remove independent `ProvisionSeasonalPeriod` rule semantics. Seasonality is a label for permitted periods, NOT a second AND filter. Preserve exact pre-v12.1 meaning when converting existing v12 data: if BOTH exact dates and seasonal intervals exist, calculate set intersection first; if one is absent, use the other; if the intersection is empty, preserve an **unsaleable** meaning (never convert it to unrestricted empty). For unrestricted original conditions keep unrestricted. Never rewrite published historical provision data in place.
- `ProvisionBlackoutPeriod(StartDate,EndDate)` is a **negative** rule; excludes matching service date even inside permitted periods. Date-basis comes from definition, not assumed flight date for all products.
- `ProvisionDayTimeWindow(DaysOfWeekMask,StartLocalTime?,EndLocalTime?,Effect)` where Mon=1 Tue=2 Wed=4 Thu=8 Fri=16 Sat=32 Sun=64; mask in 1..127. Start inclusive, end exclusive; null start/end are open day boundaries. Allow windows OR; Deny windows override; no Allow windows means unrestricted positive filter. Night spans must be explicitly split into two rows with correct day masks. Use applicable *local* service time; fail closed for missing required context; do not silently use UTC as local.
- Passenger, Sales, Geography, Flight and Fare selectors are positive allow-lists. To exclude a selected value without an explicit negative dimension, use a **higher-priority matching** `NotAvailable` Provision (lower numeric Sequence) followed by a broader Paid/Free Provision. Do not add `Effect` to every selector. Freeze first-match and NoMatch truth tables in test oracle; no production matcher.
- Add immutable `ServiceDateBasis` to product definition: FlightDeparture, ServiceStart, CheckIn, CoverageStart, Activation, per catalog. Validate applicability by product context and prove local occurrence source. No pseudo-global TravelDate context.
- Keep `PricingUnit` fixed by product identity; ensure compatible `Quantity.Unit` at activation and publication: PerPassenger/PerRoom/PerItem/PerVehicle/PerSeat => Each; PerPiece => Piece; PerKilogram => Kilogram. `FeeApplicationUnit` distinct. Price-rate PTC/age selectors only when PricingUnit=PerPassenger. Do not implement per-night/per-day price multiplication without approved definition.
- Preserve existing v12 Pricing aggregate, atomic unique filtered Active price index and publication protections; test no active Paid Provision without one active Pricing and zero active Pricing for Free/NotAvailable, including races. Do not add a PricingRate aggregate.
- Preserve `SalesEffectiveFrom/DiscontinueAt`, Outcome, CoverageScope, Quantity, Settlement, Availability and legacy Fulfillment until an explicit authority changes them. Remove stale JSON/list ownership in EF/domain only after verified migration; do not drop legacy DB columns in this phase.

## 3. Execution in small auditable checkpoints

**P1.0 Baseline audit (no code):** exact SHA, clean worktree, authoritative files, database schema/version, baseline full build and tests, existing migration status, unique indexes, actual canonical enum values, 36 frozen operational files SHA hashes. Identify breaking DTO/SQL readers including AirAvail old SQL; report as a contract gap without editing that repo.

**P1.1 Acceptance-first:** add/adjust tests from `05-SCENARIO-MATRIX.md` for failing v12 cases before production edits. Each scenario has an ID mapped to test(s), expected inputs/outputs and currently failing evidence. Test-only deterministic reference oracle for all rule truth tables is allowed. No production matcher/service API.

**P1.2 Domain:** introduce approved typed rule groups/child rows, immutable ServiceDateBasis, correct date/time and Allow/Deny semantics, pricing/quantity compatibility and cross-aggregate publication checks. Explicit Add/Change/Remove typed APIs on Draft only, stable row identity, no orphaned rows. Maintain aggregate behavior and existing framework.

**P1.3 Database and migration:** command/query EF mappings to relational typed tables, appropriate foreign keys, filtered unique indexes and optimistic concurrency. Write reversible additive migrations with auditable old->new ID lineage. Handle all four migration truth cases: dates-only; seasons-only; both => intersection; neither => unrestricted. Also test Blackout overrides, invalid/inconsistent legacy cases, unsatisfiable intersection and missing DateBasis. For cases without safe mapping, quarantine/report and block publishing; never silently widen eligibility. Run on realistic **database clone/backup**; reconcile row counts, IDs, outcomes, prices and unchanged operational tables. Legacy v11 columns may remain physically until separate cleanup; no dual-write drift.

**P1.4 Backoffice and query:** create/edit/read/list/publish rule groups and their typed rows with independent child IDs; return explicit names/effects and DateBasis. GET detail must round-trip every field and preserve historic versions; paginated grids return counts, not 1000 rows. Where contract compatibility breaks, document consumer impact; don't call another repository or silently invent v2 endpoints.

**P1.5 Verify:** all scenario IDs and 13 family fixtures, 1000 consecutive days => 1 period; 1000 scattered dates => appropriately minimal set of periods; intersections, empty intersections, edge boundaries, timezone and midnight, NotAvailable precedence and NoMatch, age/PTC, tax/fee, quantity-unit compatibility, races, Published immutability, migration clone, read-model parity and frozen reservation checks. Full solution build + full tests + no pending EF migration changes. Capture exact logs, results and SHA, plus file diff. Do NOT claim passing tests unless actually executed.

## 4. Required output and stop gate

Deliver `reports/V12.1-Phase1-Implementation-Report.md` with actual commit SHA, detailed diff, A–I conformance table, scenario-to-test matrix, exact build/test commands and PASS/FAIL counts, migration reconciliation logs, frozen files hashes and source scan showing no production evaluator/stock/reservation changes. Preserve all other existing tests. Note any known gaps explicitly; NEVER close a failed or unverified item as completed.

When fully passing, emit `ANCILLARY_V12_1_PHASE1_IMPLEMENTED_READY_FOR_OWNER_AUDIT` and **STOP**. Do not start Phase 2, Phase 3, rewrite operational code, or auto-merge/push to protected baseline. Owner alone performs closure and future phase authorization.

If blocked by a significant ambiguity, destructive migration risk, contract break requiring another repository, or changed source SHA, emit `REPORT_GAP_AND_STOP` with concrete evidence and minimum options. Do not choose an undocumented design.
