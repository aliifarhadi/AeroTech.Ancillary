# Typed capacity rules and consistency contracts — implementation authority

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


## 1. Names and type dimensions
- `InventoryAuthority`: `Unlimited=1`, `Local=2`, `Supplier=3`, `FlightFlow=4`. Missing policy is NOT an enum value and returns `NotConfigured` at lookup.
- `LocalInventoryPattern`: `FlightCount=1`, `FlightWeight=2`, `FlightCountPlusWeight=3`, `AirportSlot=4`, `DailyCount=5`, `RoomNight=6`, `AssignedAsset=7`. Only patterns 1-4 can become Active in this implementation; 5-7 require separately verified resource ownership before activation and code.
- `InventoryRecordStatus`: `Draft=1`, `Active=2`, `Suspended=3`, `Retired=4`; store only supported transitions. `ClosedForSale` is independent Boolean and must NOT delete/zero the configured quantity.
- `CapacityReadState`: `NotConfigured`, `Unlimited`, `ConfiguredNotGuaranteed`, `ClosedForSale`, `DelegatedCheckRequired`, `Unknown`, `UnsupportedPattern`; phase2 does not publish authoritative `Available`, `SoldOut`, or `Reserved` states.
- Count unit: `Person|Piece|Item|AnimalCarrier|Equipment`, confirmed use-dependent. Weight stored only in `Kg decimal(18,3)`. AirportSlot persons int. Never conflate `Quantity.Unit`, `PricingUnit`, `CapacityUnit` and `FeeApplicationUnit`.

## 2. Schema and unique source identity
`AncillaryInventoryPolicy`: `Id long; OwnerAirlineId int; ServiceDefinitionRef string(30); ServiceDefinitionId long; Authority enum; LocalPattern enum?; ProviderKey string(50)?; Status enum; Version long; CreatedAt,UpdatedAt,ActivatedAt?,SuspendedAt?,RetiredAt? DateTimeOffset;` bindings are typed children, not scalar `ScopeKind/ScopeValue`.
- Policy uses one current non-Retired row per `(OwnerAirlineId,ServiceDefinitionRef)` (unique filtered on Status IN Draft/Active/Suspended); Draft may change with optimistic version, Active immutable except suspend/reactivate/retire. After Retire, a replacement Policy with a new Id may be defined. Prior retired rows remain audit history. A new commercial ServiceDefinition revision must reuse the current non-Retired Policy, not create a new one.
- `PassengerUsageLimit {Id long, InventoryPolicyId long, LimitScope PerOrder|PerFlightOccurrence|PerServiceDate, MaxUnits int>0, CountingFamilyCode string}` validated against canonical service-family registry/owner reference; no usage ledger in Phase2.
- `FlightCountConsumption {ResourceId long, CountPerAcceptedUnit int>0, CountUnit enum}`.
- `FlightWeightConsumption {WeightResourceId long, ConsumptionMode FixedKgPerAcceptedUnit|AcceptedWeightKg, FixedKgPerUnit decimal(18,3)?}`: exactly one mode contract. Required accepted quantity/weight not guessed from `XBAG_WEIGHT_5KG` or any subcode.
- `AirportSlotConsumption {FacilityId long, OccupancyMinutes int>0, PeoplePerAcceptedUnit int>0}`; facility/airport/timezone backed by verified reference.
- One policy with Local pattern CountPlusWeight has *exactly one count binding and one weight binding*. No two-count, no arbitrary N-dimensional resource expression.

`FlightCountInventory`: `Id long, OwnerAirlineId int, FlightId long, ResourceId long, CountUnit enum, TotalCapacity int>=0, ClosedForSale bool, Status, Version long, CreatedAt,UpdatedAt DateTimeOffset`; child immutable `FlightCountAdjustment {Id,InventoryId,PreviousTotal,NewTotal,ReasonCode,ActorId,CorrelationId,OccurredAt}`; unique active/Suspended physical key `(OwnerAirlineId,FlightId,ResourceId)`.

`FlightWeightInventory`: `Id,OwnerAirlineId,FlightId,WeightResourceId long,CapacityKg decimal(18,3)>=0,ClosedForSale,Status,Version,CreatedAt,UpdatedAt`; child `FlightWeightAdjustment {Id,InventoryId,PreviousKg,NewKg,ReasonCode,ActorId,CorrelationId,OccurredAt}`; unique active/Suspended `(OwnerAirlineId,FlightId,WeightResourceId)`.

`AirportSlotInventory`: `Id long,OwnerAirlineId int,AirportId int,FacilityId long,StartUtc,EndUtc DateTimeOffset,CapacityPersons int>=0,ClosedForSale,Status,Version,CreatedAt,UpdatedAt`; child `AirportSlotAdjustment {Id,InventoryId,PreviousTotal,NewTotal,ReasonCode,ActorId,CorrelationId,OccurredAt}`. Validate `StartUtc < EndUtc`, UTC-normalized boundaries and IANA timezone identity resolved from Facility reference; half-open `[start,end)`. Intervals for *same physical facility and same owner* may not overlap while Active/Suspended. Exact unique key is not sufficient: lock against range overlaps in DB transaction (`Serializable` or facility-scoped application lock) and recheck overlapping dates before writing. Adjacent intervals allowed.

For Draft duplicates versus history, maintain one current active/suspended source per physical key, and on activation recheck conflicts transactionally. If a retired record exists, a new replacement may be created with a new id; never overwrite an old audit trail.

## 3. Adjustment semantics
- Initial capacity is set only on new Draft. Every change on an existing capacity row is `AdjustTo(newTotal, reasonCode, actorId, correlationId, expectedVersion)`; bump Version and append exactly one immutable adjustment in SAME transaction with read model update. `newTotal>=0`; numeric precision valid; integer overflow guarded; duplicate correlation/idempotency key never double-applies. Return previous/new and version.
- Current absence of allocated holds means no operational lower-bound check can be proven. In Phase3 implement `newTotal >= Held+Confirmed` atomically. Do not advertise this guard as implemented in Phase2.
- `Suspend`, `ClosedForSale` preserve configured total and historical adjustments. `Retire` cannot silently erase references; Phase3 cutover must reconcile future committed use.
- Optimistic compare-and-swap for updates via EF concurrency token or actual framework version handling. Independent concurrent request writes with same expectedVersion: exactly one succeeds, loser returns 409 conflict. Do not use naïve read-check-write with lost updates.
- All cross-tenant owner and source IDs validated on each command; no bare ID access across airlines; mandatory Authorization and actor provenance.

## 4. Capacity assignment and use
- Unlimited: no finite binding, counter, or stock row. Supplier/FlightFlow: typed provider reference; no local counter, adjust endpoint blocked. Local: corresponding typed consumption references, verified owner source, no fallback.
- Per-passenger restriction does not reserve named physical inventory: `PassengerUsageLimit` is policy only. Actual enforcement between orders requires stable traveller identity in Phase3. Existing Order.TravellerId may not be globally stable; do not equate it with identity across orders.
- `MustCheckAvailability` is a commercial requirement flag, not a replacement for inventory authority. Conflicts between active Provision flag and proposed policy must be reported, not auto-converted. Missing policy stays `NotConfigured` even if false.
- `QuantityRule`, PricingUnit and inventory consumption alignment must be explicit for each local binding. Example: `PerItem` 5kg fixed package consumes 5kg, NOT one kg; 2 packages require 10kg. If accepted request weight is decimal, check accepted weight evidence. `CountPerAcceptedUnit * Quantity` overflow checked.
- Two products sharing resource consume SAME finite counter once a phase3 hold is implemented. Phase2 tests assert identical source key and no duplicate counters; do not simulate guaranteed sales with configured values.
- Day-only capacities are independent counters on each DateOnly. A 1000-day *commercial* travel validity rule stays ONE range; a 1000-day *consumable daily inventory* is 1000 bucket values. Distinguish clearly.
- Multi-night accommodation: `[CheckIn,CheckOut)` hotel-local nights, each night uses same RoomType pool across rate plans. Supplier-managed PMS has NO local stock source. These are approved design requirements; implementation conditional on verified guaranteed local allotment.
- Named assets require one physical AssetId per exclusive interval and conflict prevention; deferred pending explicit operator-owned resource.

## 5. Read-side truthfulness and migration
Snapshot per policy/occurrence includes `PolicyId?, Authority?, Pattern?, Resource reference (typed), ConfiguredQuantity?, ClosedForSale?, State, ObservedAt, StaleAfter?, IsGuaranteed=false, Reason`. It is a **catalog/admin snapshot**, NOT booking availability. A local finite stock record without Hold ledger must show `ConfiguredNotGuaranteed` even if 0.
- Legacy 31 definitions: no inferred Unlimited/Authority; absent policies are `NotConfigured`, and stale `ServiceDateBasis` or Quantity Unit issues remain explicit exceptions (see 09).
- Never modify `AncillaryReservationAggregate`, including `StockPoolId?`. Never create Hold/Confirm/Expire/Release/decrement/TTL/EMD in this phase.
- Forward-only additive EF migrations for new policy/typed stock tables in CommandDb and QueryDb; new indexes and FK where valid. No drops of v12.1 tables/columns, no silent destructive backfill. Query projections follow actual synchronizer framework and reconcile counts and fields. Down migration must be reversibly executable for this Phase2 schema, without rewriting commercial history.
- Existing Phase1 Domain/Acceptance tests, API and ReferenceData stay green; do not alter 36 frozen reservation files or unrelated repo code.
