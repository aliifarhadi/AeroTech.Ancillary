# AeroTech Ancillary Domain Pack v12.1 - OWNER REVIEW (NOT IMPLEMENTATION AUTHORITY)

Date: 2026-10-08. Status: PROPOSED / AWAITING OWNER APPROVAL. This pack supersedes the **design proposals** in v12.0 only if approved; it does not retrospectively authorize editing the current repository. Do not run a coding agent against it before owner approval and an implementation prompt is issued. **No changes to Ordering, AirPrice, FlightFlow or AirAvail are authorized.**

## Source of truth and frozen code
- Ancillary current HEAD inspected: `aliifarhadi/AeroTech.Ancillary`, `k8s-stg@70c3de85eb8efd1000c6c1237cefd2c64773afda` (2026-10-08). Prior baseline `6b0a80ff708815ce3a6a957fbef3380b83a3f4cd`.
- AirPrice read-only reference: `aliifarhadi/Aerotech.AirPrice`, `k8s-stg@1b41f08e9a22d3c27b807d5ea275120f0f7273ad`. It implements typed `DayTimePermissionRule`, `SeasonalityRule`, `BlackoutsRule`, `FlightApplicationRule`, and stores many rule rows through EF `OwnsMany`; its validators/matchers are not a contract to copy indiscriminately.
- Benchmark: ATPCO Optional Services S5 (service identity), S7 (provisions), optional-services Travel Date Table; Fare Rules Category 2/3/4/5/11/15 are conceptual inspiration, NOT a claim that an Ancillary S7 provision is a Fare Rule Category record.
- Platform .NET framework, IDs, repository, Migrations, domain exceptions, query projections and 13 existing family fixtures remain the reuse target.

## Owner decision requested
Approve or amend the three Phase-1 aggregate roots and their typed rule-child entities in `02-ENTITY-CATALOG.md`. The following are *specific design proposals*, not retroactive claims about already implemented v12:
1. Keep `AncillaryServiceDefinition -> AncillaryProvision -> AncillaryPricing`; keep existing `Supplier` separate. Do not introduce a shared reusable many-to-many Provision library.
2. Replace 25 peer-level condition collections and redundant travel dates with typed **rule groups** owned by Provision. Each group is an optional Entity; recurring rows underneath remain separate relational child entities, never CSV/JSON/EAV. Do not introduce 25 new aggregate roots.
3. Exactly **one travel-date rule with multiple positive date ranges and multiple negative blackout ranges**. No separate `TravelDate`-per-day and `SeasonalPeriod` concept. Single date is a period with equal bounds. Annual recurring seasons are explicitly out of Phase 1.
4. Include/Exclude is NOT a universal flag on every basic selector. Standard passenger, flight, fare, geography and sales selector groups are positive **allow-lists**. Explicit denial is expressed by (a) Blackout ranges; (b) Day/Time Deny windows; (c) a higher-priority matching `NotAvailable` Provision. This supports exclusions such as a flight or customer without 25 different negative record types.
5. `PricingUnit` is fixed on the product identity, distinct from `QuantityUnit`, `FeeApplicationUnit`, PTC and age-band selectors. Validate compatibility. Keep the existing `AncillaryPricingLine` rather than inventing another PricingRate layer.
6. Group rule ownership and naming are derived from ATPCO/actual AirPrice semantics; exact IATA ATPCO table numbers are not embedded as enum/category numbers for these locally designed rule entities.
7. Phase-1 is **authoring, publishing and deterministic local rule invariants**. No full shopping itinerary matcher, stock decrement, Hold/Confirm, EMD or supplier execution. Phase-2 stock/capacity; Phase-3 reservation/fulfilment. Each phase requires explicit owner approval.
8. For already-published Provision IDs, change with successor Draft/version, never mutate published commercial history. Migration of old representations must be lossless at the **eligibility meaning** level; preserve old row ID mapping as audit metadata when several rows coalesce into one range.

## GO / NO-GO
Current v12 is **NO-GO for final Phase-1 domain closure** because it duplicates two positive date mechanisms, persists consecutive days as 1000 rows, has ungrouped flat peer-level dimension semantics, and lacks several compatibility/ambiguity guards. Existing v12 source is a better engineering starting point than reverting to initial skeleton `843cf7ccda45a1e16b1bd537172346fcdd76ae35`. This is a DESIGN verdict only: GitHub shows no independent CI status for v12; its report claims 184 passing tests but they are not rerun here.

## Required approvals before code
- Approve entity catalog, including new `ServiceDateBasis` and supported nonflight coverage.
- Confirm the proposed positive-whitelist policy with explicit `NotAvailable` override for exclusions in other dimensions.
- Confirm that recurrent annual travel seasons remain out of scope.
- Confirm the minimal v12.1 dates plus Day/Time semantics and Price/Quantity compatibility matrix.
- Only then author an implementable coding-agent prompt and migration plan for approved differences. Until then STOP.
