# 07 — Phase2 complete Inventory and capacity (configuration, not reservation)

## What is and isn't capacity
| Concept | Owner | P2 behavior | P3 behavior (design ONLY) |
|---|---|---|---|
| Stepper max / selectable number | Provision / UI contract | `MaxQuantity` / selection display metadata, with `0` = no sale | enforce quantity positive when accepted |
| Cumulative traveller entitlement | PassengerUsageLimit / commercial family | define Bound/Flight/Date/Order scoped cap | calculate consumed across orders/holds/refunds, idempotently |
| Physical finite resource | typed FlightCount/Weight/Slot AR | create/adjust/close and reference validation | actual Hold/Release ledger |
| Provider approval / supplier limit | external Supplier | policy authority and verified reference only | authorization result Pending/Confirmed/Rejected |
| Seat availability / EXST | FlightFlow | FlightFlowManaged policy, zero local seat buckets | FlightFlow seat Hold/Assign/Release |
| Baggage allowance | fare ticket/airline conditions | typed Piece/Weight concept, not a physical stock balance | eligibility/quote from accepted fare and airline baggage policy |

## Policy exact fields
Retain v12.1 existing `AncillaryInventoryPolicy` fields: `Id:long, OwnerAirlineId:int, ServiceDefinitionRef:string(30), ServiceDefinitionId:long` (last validated version pointer, NOT physical key), `Authority`, `LocalPattern?`, `ProviderKey?`, `CountConsumption?`, `WeightConsumption?`, `SlotConsumption?`, `PassengerUsageLimits[]`, `Status`, `Version`, timestamps, SQL rowversion. Current resolved Definition version exposed read-only as `CurrentServiceDefinitionId` in details/config snapshot.

`Authority` allowed: `Unlimited=NoLocalFinitePool`, `Supplier=OwnerIsSupplier`, `FlightFlow=SeatOrFlightResourceOwner`, `Local=ExplicitAirlinePool`. No implicit no-policy=>Unlimited. If no Policy, return `NotConfigured`. If provider unverified, `DelegatedCheckRequired` or `Unknown`, **NOT** `Guaranteed`. `Unlimited + MustCheckAvailability` supported. `ClosedForSale` distinct from `TotalCapacity=0`.

### Consumption VOs
- `FlightCountConsumption(ResourceId:long,CountPerAcceptedUnit:int>0,CountUnit:Person|Piece|Item|AnimalCarrier|Equipment)` with `units = quantity * CountPerAcceptedUnit`, checked integer overflow. Pet physically counted by carrier count, not by number of Pet products unless explicit binding.
- `FlightWeightConsumption(WeightResourceId:long,ConsumptionMode:FixedKgPerAcceptedUnit|AcceptedWeightKg,FixedKgPerUnit:decimal(18,3)?)`. If fixed, positive fixed weight and accepted unit; if accepted weight requires later trusted input, P2 must NOT magically consume merely purchased 10kg package without correct binding.
- `AirportSlotConsumption(FacilityId:long,OccupancyMinutes:int>0,PeoplePerAcceptedUnit:int>0)`. Timezone in verified facility, actual UTC slots nonoverlap; adjacency allowed.
- `FlightCountPlusWeight` only exact pair above, not generic N-dimensional capacity engine.

### Actual resource aggregates and fields
| AR | Physical uniqueness | Settings | Adjustments |
|---|---|---|---|
| `FlightCountInventory` | owner+FlightId+ResourceId (non-retired unique) | `TotalCapacity:int>=0`, `CountUnit`, `ClosedForSale`, Status, Version, RowVersion | append-only `(Id,ParentId,PreviousTotal,NewTotal,ExpectedVersion,ResultingVersion,ReasonCode,ActorId,CorrelationId,OccurredAt)` |
| `FlightWeightInventory` | owner+FlightId+WeightResourceId | `CapacityKg:decimal(18,3)>=0`, ClosedForSale, Status, Version, RowVersion | same + previous/new `decimal(18,3)` |
| `AirportSlotInventory` | owner+AirportId+FacilityId+half-open `[StartUtc,EndUtc)` | `CapacityPersons:int>=0`, ClosedForSale, Status, Version, RowVersion | same previous/new people; SQL facility-level lock/serializable against overlapping ranges |

Migrations MUST add filtered unique keys, protected concurrent Update and slot range exclusion via SQL transaction/locking. A simple unique index on exact start/end **does not prevent overlapping non-identical ranges**. Unique adjustment `(ParentId,CorrelationId)` idempotency; same correlation/different amount => conflict; immutable append-only record.

## Per-profile capacity outcomes (P2)
- A01–A05 Baggage allowances: `Unlimited` by default only when explicitly authored; passenger Bound max enforced **later P3**; no Count or Weight counter by name alone. Oversize requires confirmation when operational supplier demands it.
- A06 equipment: Supplier (or verified FlightCount/Weight); A07–A10 Seat/Upgrade: FlightFlow (and A10 quote authority); A11 SSR Meals: Unlimited+Check/Catering Supplier; A12 Paid Meal: Supplier/verified local Count; A13/A14 Pet: explicit verified carrier count or Supplier/ground handler; A15/A16 SSR assistance: Unlimited+Check or Supplier; A17 Medical: Supplier approval/verified resource; A18 bassinet: source of real bassinet + seat compatibility; A19 UMNR: supplier operational quota/verified Count; A20–A22 airport: Supplier or Local slot with facility/timezone proofs; A23 Priority: explicitly Unlimited with entitlement limit; A24 WiFi: verified supplier/capability, no capacity guarantee.
- DailyCount, RoomNight, AssignedAsset designed for future only; P2 must reject activation without real ownership/source evidence. Hotel/Car are outside A01–A24 primary coverage.

## Phase2 snapshot truth table
| Situation | P2 State | IsGuaranteed | RequiresAvailabilityCheck |
|---|---|---|---|
| no Policy | NotConfigured | false | from active current Definition Provision only |
| Draft not activated | NotConfigured (or UnsupportedPattern) | false | same |
| Unlimited active | Unlimited | false | from Provision `MustCheckAvailability` |
| Supplier/FlightFlow active & reference valid | DelegatedCheckRequired | false | true as necessary |
| Local active but flight/slot source missing | NotConfigured or Unknown(reason) | false | true |
| Local source configured/open | ConfiguredNotGuaranteed | false | true when finite |
| Local source closed/suspended | ClosedForSale | false | true |
| unknown/unsupported resource | UnsupportedPattern/Unknown | false | true |

**Keep v12.1 fixes**: one current product version resolution for `RequiresAvailabilityCheck`; separate stored `ServiceDefinitionId` vs read-time `CurrentServiceDefinitionId`; source validation fail-closed; no AirAvail SQL updates.

## Backoffice operations phase2
- Policy: Define, Change Draft (expectedVersion), Activate (reference source), Suspend/Reactivate/Retire, Get/List + ConfigurationSnapshot. Unique stable identity and tenant guard.
- Typed inventory: Create Draft, Set/Adjust Total (expectedVersion + correlation + reason + actor), Activate, CloseForSale/Open, Suspend/Reactivate/Retire, Get/List + audit history. Use existing repository/handler/route conventions.
- FlightCount share resource: two commercial SKU policies MAY bind one ResourceId, **not two counters**. Mixed `CountUnit` on same physical resource forbidden.
- Slot: use `[start,end)` UTC, lock facility for overlapping interval decisions, adjacent intervals allowed; timezone and Airport/Facility evidence checked.

## Non-negotiable release gates
SQL Server 100-way concurrency for same physical record expectedVersion (one winner), 100 overlapping slot creations (one winner), idempotent adjustment replay and different-payload same key rejection, shared two products/one resource, 3-decimal Kg round trip, tenant isolation, no `Remaining` reported before P3 ledger. No `Held/Sold/Available` counters invented in P2. `StockPoolId?` existing P3 schema untouched.
