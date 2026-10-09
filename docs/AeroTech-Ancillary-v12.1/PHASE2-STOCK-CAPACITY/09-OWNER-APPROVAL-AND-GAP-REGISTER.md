# Owner approval, baseline audit and non-negotiable decisions

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


## Approved decisions D1-D12
The Owner explicitly approved the 12 decisions in document 07, including rejection of universal `AncillaryStockPool`, typed model separation, standalone policy, Flight Count and Weight, Airport Slot, optional locally managed Daily and Room Night, deferred AssignedAsset, passenger usage rules distinct from physical capacity, delegated supplier and FlightFlow authority, and absolute prohibition of Phase3 code.

## Baseline source state (verified through GitHub connector on 2026-10-08)
- Branch `k8s-stg` SHA `933b7b7a793b426dbcb6362bbf8519886edf9080`; merged PR #1.
- Source `reports/V12.1-Phase1-Implementation-Report.md`: 169 domain and 112 acceptance tests passing according to author; no independent rerun here.
- Source report OPEN: `ServiceDateBasis` unset for 31 existing service definitions; six active XBAG_WEIGHT provisions retain incompatible historical PerItem/Kilogram combination for NEW publication; ReferenceData FK existence unverified; old AirAvail SQL consumer queries superseded read model; v12.1 legacy Down does not backfill newly authored windows; pricing-line concurrent edits are out of previous scope.
- Preserve this legacy data and *do not silently fix* or assign inventory authority, dates, prices, weight, or units. Inventory policy for legacy product identity is `NotConfigured` until explicit approved mapping. If `MustCheckAvailability` contradicts a proposed authority, reject activation and report owner-required reconciliation.
- `AncillaryReservationUnit.StockPoolId?` is legacy and cannot force a generic stock root; existing `HoldAncillaryServicesService` does not allocate any stock. No reservation code change is authorized.

## Product identity and policy
`AncillaryInventoryPolicy` keyed by `(OwnerAirlineId, ServiceDefinitionRef)` across commercial versions; preserve `ServiceDefinitionId` as a validated current-version reference, *not* identity of physical resource. Policy authority exactly `Unlimited | Local | Supplier | FlightFlow`. No fallback defaults. Null/missing policy = `NotConfigured`. Draft policy is inert. Active/Suspended/Retired lifecycle and controlled revisions; status cannot accidentally make a delegated provider local.

## Resource ownership and sharing
Finite counters belong to typed physical/contractually allotted resources and occurrence, not one counter per service. Separate service SKUs can bind to same ResourceId. ResourceId requires owner-verified reference and source-of-truth; do not manufacture a cross-service Resource registry unless one is genuinely absent and Owner separately approves a bounded reference structure. In that situation, stop affected activation and report exact missing source; code must still compile and test with fixture reference resources.

## Truth in capacity
Phase2 `ConfiguredCapacity` is an authoring value, not available-to-sell or guaranteed hold. No Held/Confirmed/Sold counters created or inferred from the previous reservation tables. Do not expose `AvailableQuantity`, `GuaranteedSellable=true`, or `SoldOut` from local capacity tables without allocation evidence. Physical consumption, provider attempts, TTL, retry, lease, release, correction and multi-resource atomic booking are gated Phase3 tasks.

## Local scope
MANDATORY Phase2 executable families: `AncillaryInventoryPolicy`, `FlightCountInventory`, `FlightWeightInventory`, `AirportSlotInventory`, `PassengerUsageLimit` child, typed finite binding of Count+Weight (one of each maximum). Read-only reference stubs for delegated authority (no speculative HTTP APIs).
DEFERRED UNTIL VERIFIED CONTRACT: `DailyServiceInventory` and `RoomNightInventory` API+tables, although their final typed field designs and future tests are required in this pack. `AssignedAssetInventory` likewise deferred. Activation for such local pattern must return an explicit unsupported/configuration error, not silently delegate or mark unlimited. This is a capability gate, not a new project phase.

## Exit gate
Complete Phase 2 implementation is **not** approval to use limited stock in production booking: Phase3 still needed. Agent must label unsupported operational capacity models and absent provider reference evidence as owner decisions, not silently mark these cases implemented. Any actual API breaking change outside the allowed inventory namespace requires explicit owner escalation.
