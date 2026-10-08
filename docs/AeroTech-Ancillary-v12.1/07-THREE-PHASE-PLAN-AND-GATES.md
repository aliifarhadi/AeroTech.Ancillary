# Exactly three delivery phases; owner-controlled gates

## Phase 1 (NOW only after catalog approval): Commercial catalog + independent pricing
**Commercial ARs:** AncillaryServiceDefinition, AncillaryProvision, AncillaryPricing. `Supplier` is existing supporting AR, not a new requested commercial root.
- Define/change/activate/revise definitions with immutable identity, PricingUnit and ServiceDateBasis.
- Define, edit individual rows or group Draft rules, validate semantic date/blackout/day-time/allow-lists and publish Provisions with precedence/outcome.
- Define/edit/revise/switch price versions; preserve price line component and selector invariants.
- Backoffice list/details/routes, accurate Command & Query persistence, safe migration from v12 and historical references, benchmark-family tests.
- A test-only deterministic reference oracle checks all typed rule truth tables and first-match NotAvailable precedence; only bounded pure rule-value predicates may be production helpers. No operational shopping evaluator or supplier calls.
- Exit: all P1 scenarios in 05 verified; zero undocumented ambiguity, no duplicate authoring meaning, migration clone proven, all existing tests and Hold/Get/Confirm regression, no change to unrelated repos. Agent submits report, owner independently audits, agent STOP.

## Phase 2 (FUTURE, gate after Phase-1 closure): Stock / capacity definition
**Future separate AR:** AncillaryStockPool (new when authorized). Proposed child Entities (subject to real FlightFlow/provider boundary): `AncillaryCapacityBucket` (capacity for a typed occurrence/date/resource), `AncillaryStockAdjustment` (audit), optional `StockPoolAllocationRule` if actual provider contract requires it. `StockScope` is a typed VO not a generic string key.
- Modes: Unlimited / LocallyManagedFinite / SupplierManagedExternal / DelegatedFlightFlow where valid. An infinite wheelchair service is not zero stock; seat occupancy is FlightFlow-owned, not mirrored as the Ancillary master; partner hotel inventory is supplier-owned unless an explicit allotment contract exists.
- Author buckets, resource/occurrence keys, availability snapshots and capacity adjustments; uniqueness and concurrency controls; respect PricingUnit vs capacity quantity unit where known. No hold, allocations to orders or release/confirm in this phase.
- Exit: finite, unlimited, external, carrier seat, multiple supplier, concurrent adjustments; canonical ownership and unit compatibility established. Owner approves Phase 3 separately.

## Phase 3 (FUTURE, gate after Phase-2 closure): Reservation / fulfilment
**Existing AR:** AncillaryReservation, with `AncillaryReservationUnit`. Add subordinate attempt/ref entities only after actual Ordering/FlightFlow/provider contracts prove the need; no speculative parallel Reservation AR.
- Verified published commercial snapshot and selected price, stock availability/hold, per-unit supplier refs, idempotency key, TTL/expiration, partial reserve/confirm where supplier supports it, retry/recovery, release/cancel, eventual EMD/order integration under separate Ordering authority.
- Support flight-coupled airline ancillaries and nonflight external product suppliers via explicit capability policy; some suppliers have direct-issue-only path. No invented universal state diagram when supplier cannot reserve.
- Exit: eight or more cross-service stress scenarios, financial and operational idempotency, partial results, supplier failures, race safety, historical Order/Pricing evidence; owner final audit.

## Engineering and governance
No automatic transition between phases. Each phase begins only after owner command and completed source audit. No code generation from this OWNER-REVIEW pack; a future coding-agent prompt must quote exact approved SHA, new source check, contracts, migrations, scenario IDs and allowed files.

## Known red flags in pushed v12 to regress
1. 1000 dates stored as 1000 `ProvisionTravelDate` rows.
2. `TravelDates` and `SeasonalPeriods` are separate positive restrictions with AND intersection; a naive rename/union would change behavior.
3. Peer-level ~25 selectors in Provision root lack explicit typed rule-ownership groups and stable category-aligned semantics.
4. Explicit Deny on DayTime only; `NotAvailable` is required for other selective exclusions but must be tested as priority veto.
5. `PricingUnit` immutable per product identity but category-specific product vs QuantityUnit compatibility not enforced through current Provision define/publish.
6. Local DateBasis missing for insurance, SIM, hotel and transfers; generic flight timezone assumptions are unsafe.
7. v12 DB retains obsolete columns and old AirAvail SQL consumer reads fields now stale for new prices.
8. Existing code/report claims passing tests but those tests assume 1000 rows; green status does not prove desired domain semantics.
