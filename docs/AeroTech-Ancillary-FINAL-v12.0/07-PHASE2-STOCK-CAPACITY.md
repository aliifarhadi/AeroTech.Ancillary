# 07 - Phase 2 - Stock and capacity ONLY (future Owner authorization)

**STATE: DESIGN SCOPE / NOT APPROVED FOR IMPLEMENTATION NOW.** No Agent may create these tables/routes/types in Phase 1. This is phase 2 of exactly three phases, not a separate subproduct. Do not modify existing AncillaryReservation/Hold/Confirm in Phase 2.

## Business goal
Allow an airline/supplier analyst to state which AncillaryServiceDefinitions have finite stock vs effectively unlimited sale, define and administer capacity by the relevant resource/occurrence, and expose audited available stock for later reservation. Avoid pretending every ancillary owns seat inventory: seat occupancy is already FlightFlow, partner hotel/insurance/SIM stock may be externally managed.

## Proposed constrained model - validation required when phase opens
One `AncillaryStockPool` aggregate root (new only in Phase 2), with typed `AncillaryStockAllocationScope`/`AncillaryStockAdjustment` children, to be finalized from actual provider and FlightFlow references at Owner gate. Logical fields to ratify:
- `StockPool`: `Id:long`, `ServiceDefinitionId:long`, `SupplierId:long`, `Mode:Unlimited|Finite|ExternalManaged`, `ScopeKind:Service|ServiceDate|Flight|SupplierResource` (candidate closed set, not a final authority), `ScopeReference` typed by approved resource reference contract rather than a generic string bag, `CapacityUnit:AncillaryQuantityUnit`, `TotalCapacity:int?`, `Status:Draft|Active|Suspended|Retired`, `CreatedAt`, `UpdatedAt`, version/concurrency marker.
- `StockAdjustment`: `Id:long`, `StockPoolId:long`, `QuantityDelta:int`, `ReasonCode` from approved typed reasons, `OccurredAt:DateTimeOffset`, `ActorId` provenance, immutable ledger row.
- Initial finite capacity must be >=0; no negative free capacity; no double sale; only one authoritative active stock pool for a given definition/resource/scope key; stock quantity unit must be compatible with ServiceDefinition and Provision.Quantity.
- `Unlimited` has no finite capacity counter; `ExternalManaged` is not represented as an owned zero/finite pool without an actual supplier contract. Inventory could be FlightFlow-owned: define only a read/reference boundary for seat availability, never mirror FlightFlow capacity as local authoritative stock.

## What Phase 2 will implement after source-based decision
Backoffice StockPool define/list/detail/change Draft/activate/suspend/retire and adjustments with audit and optimistic concurrency; capacity CRUD only and exposure of authoritative stock snapshot, with tests for finite/unlimited/external, duplicate scopes and 1,000 simultaneous updates. No reservation allocation or booking hold; the reserved quantity part remains phase 3. Any historical `AncillaryReservationUnit.StockPoolId` legacy field is not permission to implement its behavior in phase 2.

## Owner gate inputs required when phase begins
1. Actual provider inventory API and which services are airline-managed vs external-managed; 2. FlightFlow state/capacity contract; 3. stock scope identity for flight/date/resource/room/vehicle; 4. finite vs unlimited semantics per catalog family; 5. what counts as capacity decrement before vs after confirmation; 6. exact interaction between published services, occurrence validity and supplier maintenance. Without these, no arbitrary generalized stock model is allowed. Every unresolved candidate field remains design guidance, not an Agent coding order.

## Phase 2 exit
Physical stock accounting and administration proven under concurrency, no over-allocation, no mutation of reservation files, all original Phase 1 conformance still passing. Owner then authorizes Phase 3 explicitly.
