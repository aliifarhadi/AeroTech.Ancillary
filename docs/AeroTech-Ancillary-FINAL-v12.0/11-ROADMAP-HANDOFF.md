# 11 - Three-phase roadmap and Owner-controlled gates

## Phase 1 NOW - catalog and commercial authoring
**Done means** all 3 commercial aggregate roots with typed child entities + correct invariants/behaviors, two DB persistence/migrations, Backoffice, 13 family tests, 1000 date test, one-active-pricing race test. No stock/reservation changes. Source baseline recommendation: evolve current `6b0a80f` rather than reset. Owner independently audits actual commit and issues Phase-1 CLOSED only after proofs.

### Minimal release sequence inside Phase 1
1. Metadata/ServiceDefinition and Supplier compatibility.
2. Normalized Provision domain and per-row CRUD.
3. Pricing domain and transactional publication.
4. Migrations, projections, Backoffice, data reconciliation.
5. Domain + SQL acceptance conformance + negative boundary audit.
No owner authorization for subsequent phase is implied by green tests.

## Phase 2 NEXT - stock/capacity, gated
After explicit Owner signal, inspect real FlightFlow and actual external supplier stock ownership. Finalize typed capacity occurrence key and stock ownership, then implement stock aggregate + admin + concurrency-safe counters and query read. **Do not** implement operational holds yet. `StockPoolId?` currently present on reservation unit is historical unfinished metadata only. Exit: no over-allocation; no Reservation code changes; Owner closes Phase 2.

## Phase 3 LAST - reservation/fulfillment/servicing, gated
After explicit Owner signal, inspect actual FlightFlow, AirAvail, Ordering, supplier and issuing contracts. Rework frozen AncillaryReservation only as justified by source. End-to-end Hold/Confirm/Release/Expiry/Cancel/partial change/refund/EMD boundary/stock allocation and integrations; no invented cross-service source of truth. Exit with scenario tests and owner audit. Project has **exactly three phases**, no open-ended subproject.

## Risks deliberately registered
- `AirPrice` uses EF-owned value-object many collections, while v12 user needs row identity for independent authoring; use typed entity FK tables while preserving aggregation boundaries.
- `PricingUnit` cannot be inferred solely from v11 service subcodes. Migration mapping per legacy ServiceDefinition is required.
- v11 fixed fee lines may not map to one base rate if historical data has multiple charge lines; record before dropping legacy columns.
- Future service-time timezone/age calculation reference and provider/stock occurrence keys cannot be invented during Phase 1.
- Business decision on operational supplier capability is in Phase 3; don't interpret legacy fulfillment fields as a frozen adapter contract.

## No implied owner decisions
Deferring an uncertain future field does not authorize ignoring current Phase-1 normalized conditions. Conversely, a complete v12 pack does not authorize Phase-2/3 code. All next-stage coding prompts must be issued only after source reviews and explicit Owner gate.
