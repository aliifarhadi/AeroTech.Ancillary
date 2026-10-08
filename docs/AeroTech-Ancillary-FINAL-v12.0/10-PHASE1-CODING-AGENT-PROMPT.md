# 10 - CODING AGENT IMPLEMENTATION PROMPT - AEROTECH ANCILLARY v12.0 / PHASE 1 ONLY

You are the IMPLEMENTATION AGENT, not the Owner/architect. Work in `aliifarhadi/AeroTech.Ancillary`, branch `k8s-stg`. Start from verified baseline `6b0a80ff708815ce3a6a957fbef3380b83a3f4cd` (check actual HEAD and diff before editing). Do NOT reset to `843cf7ccda45a1e16b1bd537172346fcdd76ae35`. Preserve latest approved Owner decisions; no autonomous phase 2/3 or external repository edits. Write report in Persian. Follow project's coding conventions; you may code and test, not invent business architecture.

## Absolute task
Implement ALL **Phase 1** deliverables of `AeroTech-Ancillary-FINAL-v12.0` by reading docs `00` through `09` in order. This pack overrides v11 conflicts. Phase 1 is catalog definitions + NORMALIZED sale/travel/fare/passenger conditions + independently versioned fixed Pricing + complete Backoffice authoring. No shopping runtime evaluator or stock/reservation changes.

### Pinned AirPrice benchmark (MUST read relevant real files)
`https://github.com/aliifarhadi/Aerotech.AirPrice` / `k8s-stg@1b41f08e9a22d3c27b807d5ea275120f0f7273ad`:
- `AeroTech.AirPrice/AirPrice/src/AeroTech.AirPrice.Domain/AirFareAggregate/ValueObjects/{Seasonality/SeasonalityRule,Seasonality/PermittedTravelDateRange,Blackouts/BlackoutsRule,Blackouts/TravelBlackoutDateRange,DayTime/DayTimePermissionRule,DayTime/DayTimePermissionTimeRange}.cs`
- `AeroTech.AirPrice/AirPrice/src/AeroTech.AirPrice.Persistence/AirFareAggregate/AirFareEntityTypeConfiguration.cs`
- `AeroTech.AirPrice/AirPrice/src/AeroTech.AirPrice.Domain/AirChargeAggregate/{AirCharge.cs,Entities/AirChargeCondition.cs}`.
Borrow typed rule + normalized child table patterns. Do NOT copy AirPrice runtime chain checkers or their incomplete validators / accidental equality implementation. Reuse canonical `AeroTech.Messages.AirPrice.Enums` from the Ancillary Contracts when values really match.

## Source and guard audit BEFORE code
1. Read and list all current aggregate roots, child entities, value objects, DTOs, application use cases, Backoffice controllers, command/query EF mappings, migration snapshots, CQRS synchronizers and tests for Supplier, ServiceDefinition, Provision, Pricing (new), Reservation (freeze).
2. Diff `e2a8c9f... -> HEAD` and `843cf7c... -> HEAD`. Verify `HEAD` against `6b0a80f`; on material unexpected change report/stop rather than overwriting concurrent changes.
3. Record a source-frozen reservation file list and hashes. Preserve all files under `AncillaryReservationAggregate`, Hold/Get/Confirm Service APIs and tests unchanged. No new evaluator, stock pool, supplier adapter.
4. Run existing build/test baseline; record ACTUAL outputs rather than relying on `reports/V11-Phase1-Authoring-Report.md` claims.

## Commercial aggregates EXACT target
`Supplier` remains existing supporting AR untouched. Three Phase 1 commercial ARs:
```
AncillaryServiceDefinition -> * AncillaryProvision -> * AncillaryPricing -> * AncillaryPricingLine
```
The ServiceDefinition-to-Provision FK is 1:N, not M:N. Aggregates communicate by IDs and validated application transactions, not EF parent aggregate navigation.

### AR1 - ServiceDefinition
Preserve source identity/classification, supplier, booking/document metadata, Version, status/lifecycle; add one `PricingUnit` typed enum. At minimum `PerPassenger, PerRoom, PerItem, PerVehicle, PerSeat, PerPiece, PerKilogram` with explicit code values (see doc04). PricingUnit cannot vary across versions of same owner airline/ServiceDefinitionRef after activation. Methods Define, ChangeDraft, Activate, Suspend, Reactivate, Retire, Revise; guard supplier active, Industry subcode reference vs CarrierDefined as source. Do not copy full ATPCO code catalog or duplicate AirPrice master data.

### AR2 - Provision
Preserve root Id, ServiceDefinitionId, Sequence, Status, Sales window, CoverageScope, AdvancePurchase, Quantity, Standard/Baggage/Seat Application, CommercialOutcome, Settlement, Availability and FROZEN Fulfillment metadata. Move Fee/Currency and PriceLines to separate Pricing. Implement explicit typed relational CHILD ENTITIES for every populated selector in `03-PROVISION-ENTITY-SCHEMA.md`: PTC, POS, Customer, CustomerType, origin/destination/via airports, directional RoutePairs, marketing/operating airline, FlightNumber, FlightId, AircraftId, AirFareId, AirFareType, FareFamilyId, FareBasis, CabinClass, Rbd, seat number and characteristic. Critical new entities:
- `ProvisionTravelDate(Id,ProvisionId,TravelDate)` one row PER exact allowed date (prove 1000 rows).
- `ProvisionSeasonalPeriod(Id,ProvisionId,StartDate,EndDate)` multiple allowed ranges.
- `ProvisionBlackoutPeriod(Id,ProvisionId,StartDate,EndDate)` multiple denied ranges.
- `ProvisionDayTimeRestriction(Id,ProvisionId,DayOfWeek,StartTime?,EndTime?,Effect Allow|Deny)`.
All children have stable identity, typed FK and validation; no comma-delimited columns, JSON/EAV rule DSL or reflective matcher. Local-date and time are authoring data only. Distinct dimensions AND; positives OR; deny wins; empty unconstrained; no annual implicit recurrence. All edits only while Draft. Active immutable. Unique active (ServiceDefinitionId,Sequence). No evaluating actual flight/fare/passenger now.

### AR3 - Pricing
`AncillaryPricing(Id,AncillaryProvisionId,PricingUnit,Version,CurrencyId,FeeApplicationUnit?,Status,CreatedAt,ActivatedAt?,SuspendedAt?,RetiredAt?,PriceLines[])`. PricingUnit derived from service identity and checked. Prices are fixed decimal(18,2); no FX/dynamic. Multiple versions per Provision but **only one ACTIVE** enforced with filtered SQL UNIQUE index and atomic transactional switch.
`AncillaryPricingLine(Id,AncillaryPricingId,PassengerTypeCode?,AgeFromInclusive?,AgeToExclusive?,Category,Code?,Name?,CountryId?,StationAirportId?,Amount)`. Category enum preserved `Ancillary|Tax|Fee`. One positive Ancillary base per nonoverlapping PTC/age selector key, zero or more uniquely coded tax/fee components for SAME selector; total is sum WITHIN one selector, never sum alternatives. For PerPassenger support ADT/CHD/INF and ages `[0,65)` / `[65,+inf)`; no overlapping age/fallback ambiguity. Other units forbid PTC/age. One CurrencyId on root. Pricing line Status NOT created: only root lifecycle. Separate product PricingUnit vs FeeApplicationUnit. Active Paid Provision requires 1 active Pricing; Free/NotAvailable zero active Paid Pricing. `SwitchActivePricing` coordinated application transaction and race-safe.

## Implementation order; no big-bang rewrite
P1.A: source map + guard hashes + baseline tests.
P1.B: AR1 PricingUnit and invariants; tests.
P1.C: AR2 typed child entity domain/guards; test 1000 dates and lifecycle; do not create evaluator.
P1.D: AR3 price root/lines/version and aggregate/application guards, unique DB index and atomic price switch; negative/race tests.
P1.E: command/read EF migrations and **data-preserving** V11 backfill with ID/status/amount/condition counts; no destructive drop before proven reconciliation. If old data's PricingUnit or price-line mapping ambiguous: `REPORT_GAP_AND_STOP` for mapping ONLY; never default PerItem indiscriminately.
P1.F: Backoffice create/read/list/edit/lifecycle for all 3 commercial ARs, plus typed child row operations for individual travel dates/seasons/blackouts/day-time. Read-model sync persists all row IDs and typed values. Retain Supplier route.
P1.G: run all `09` scenario tests, 13 family fixtures, old tests (update only to reflect approved v12 schema without weakening expected business behavior), full build, command/query migration check, no Reservation diff. Fix only in-scope defects.

## Nonnegotiable prohibitions
No Phase 2 stock or inventory; no Phase 3 Reservation/Hold/Get/Confirm/Cancel/EMD; NO edits to `AncillaryReservationAggregate/**`, AirAvail, Ordering, FlightFlow, Payment/JetPay, external provider source. No runtime eligibility matcher, Evaluation endpoint, Preview, Simulate, Bulk feature, workflow/rules DSL, shared M:N Provision, EAV/JSON typed condition storage, new subcode guessed from memory, parallel pricing domain, fake test-only implementation, silently removed v11 historical data or destructive table migration.

## Migration / behavior requirements
Retain all existing identity/SSR/EMD metadata; preserve Provenance and old PriceLine Category/Code/Name/Country/Station and money. Map v11 fields to v12 typed rows, old Fee/PriceLine to an initial Pricing per paid Provision, including status. No blind PTC consolidation. Keep active published data immutable; support atomic publish of Draft Paid Provision+Pricing. Maintain query DB projection with actual child IDs. Report any breaking consumer contract and obtain Owner decision rather than silently modify other service.

## Mandatory final evidence
- exact base/head SHA and touched files
- AirPrice paths examined + deviations from benchmark
- physical command/query table migrations and uniqueness indexes
- row counts before/after, per-id price/conditions mapping; unresolved records
- full `dotnet build`, `dotnet test`, SQL acceptance commands/results; pending EF changes check
- 13 families, 1000-date case, day/time Allow/Deny, seasonality, blackout, PTC/age, room/vehicle/item, atomic Pricing active-switch, concurrency, no historical data lost
- `git diff --name-only` proving reservation/hold/confirm paths unchanged, no other service writes
- verdict GO / GO_WITH_CORRECTIONS / NO_GO
Finish with `PHASE_1_V12_READY_FOR_OWNER_AUDIT`, then STOP. No Phase 2 work until Owner explicitly authorizes. If an ambiguity materially blocks code, `REPORT_GAP_AND_STOP` with exact file/line and minimal choices; do not invent.
