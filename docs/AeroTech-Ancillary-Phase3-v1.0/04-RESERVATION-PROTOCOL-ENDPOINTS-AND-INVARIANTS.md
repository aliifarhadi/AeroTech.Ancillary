# 04 - P3.3 Real Hold/Confirm/Release/Expire/Cancel/Recovery (DESIGN ONLY until P3.2 owner closure)

## A. What exists and why it is insufficient

Current Ancillary REST route is `Service/v1/Ancillaries`: `POST Service-Holds`, `GET Service-Holds/{holdId}`, `POST Service-Holds/{holdId}/Confirmations`. Source: `src/AeroTech.Ancillary.RestApi/V1/AncillaryReservationAggregate/Controllers/ServiceController.cs`.

Current `HoldAncillaryServicesService.NewHoldAsync` verifies Definition/Provision/Supplier and checks duplicate OrderService IDs, then `AncillaryReservation.Hold` sets aggregate `Status=Held` immediately. **There is no transactional FlightCount/Weight/AirportSlot allocation and no authoritative supplier approval evidence in this path.** `AncillaryReservationUnit.StockPoolId?` is a legacy placeholder and must not be construed as a stock authority. Current `Confirm()` transitions every Held unit to Confirmed without provider evidence. Existing operations must **not** be exposed as production truthful reservations before authorized replacement.

The P3.3 objective is to evolve the EXISTING AncillaryReservation aggregate and existing controller/application conventions with the smallest correct set of typed operation/evidence children. **No second booking orchestrator or universal stock aggregate.** Ordering remains the orchestration and accepted-price/Order source-of-truth.

## B. Source-of-truth boundaries and important Seat correction

| Subject | Authority |
|---|---|
| Order, order service, accepted money, payment/guarantee, ticket/EMD/coupon, refund | Ordering |
| Ancillary service policy/eligibility/pricing, local counted capacity, supplier reservation evidence | Ancillary |
| Flight cabin/RBD capacity, seat hold/confirm/release/confirmed-seat cancel | FlightFlow via **existing Ordering reservation provider** |
| Actual provider approval/external ancillary inventory | Real supplier/provider; Ancillary records response/evidence, does not pretend success |

Ordering `FlightFlowReservationProvider.PlanUnits()` maps `OrderSeatService` to its `OrderAirTransportService` and deduplicates seat-consuming flight units; `HoldRequestFor` sends the flight capacity ID and optional requested seat. FlightFlow `FlightCapacity.HoldSeat` updates Held/Remaining counters and `ConfirmHeldSeats` moves Held->Confirmed with locks. **DO NOT make Ancillary call FlightFlow to independently hold A07/A08 on top of the already-held Ordering air service.** The Ancillary commercial/service fulfillment may need verification of the *existing* FlightFlow seat unit, not a second capacity hold. A09 EXST requires an explicit extra occupied capacity and ticket/document semantics: BLOCK until a verified PSS/FlightFlow/Ordering contract exists. A10 Upgrade/Exchange similarly requires verified provider quote and ticketing contract. No silent model invention.

Old v12.2 Phase3 design diagram suggested `Ancillary -> FlightFlow` delegation; the actual Ordering provider shows a second delegated hold would risk double capacity. New architecture must have a **single** seat capacity owner and one orchestration path. Final routing of supplementary A09 and new post-booking seat assignment is an explicit owner integration decision at P3.3 gate, not something the Ancillary agent may solve by changing Ordering.

## C. Minimum reservation aggregate/operation model (existing root evolves)

```
AncillaryReservation : AggregateRoot<long> [EXISTING, EVOLVE]
  Id:long; OrderId:long; IdempotencyKey:string(<=128); Reference:string(<=128)
  RequestedExpiresAt:DateTimeOffset?; EffectiveExpiresAt:DateTimeOffset?
  RequestFingerprint:string              // immutable canonical payload hash
  Status: derived summary of unit states; CreatedAt/UpdatedAt
  Units:IReadOnlyList<AncillaryReservationUnit>
  Attempts:IReadOnlyList<ProviderOperationAttempt> (only actual external operations)

AncillaryReservationUnit : existing Entity<long> [EVOLVE]
  Id; ReservationId; OrderServiceId:long; ServiceDefinitionId:long;
  DefinitionVersionId:long; ProvisionId:long; PricingRevisionId:long?;
  ServiceOfferId:string?; TravellerId:long?; CoverageScope:existing enum;
  CoveredFlightIds:long[]; PortionRefs:string[]?; Quantity:int;
  AcceptedCommercialSnapshotRef:string; FulfillmentProviderKey:string?;
  Status: unit state; RequestedExpiresAt:DateTimeOffset?;
  ProviderOperationRef:string?; ProviderUnitRef:string?;
  FailureReasonCode:string?; LastEvidenceAtUtc:DateTimeOffset?;
  Allocations:IReadOnlyList<CapacityAllocation>;

CapacityAllocation : owned child Entity (ONLY for a real Local resource)
  Id:long; ReservationUnitId:long;
  ResourceKind:FlightCount|FlightWeight|AirportSlot;
  FlightId:long?; ResourceId:long?; AirportId:int?; FacilityId:long?;
  SlotStartUtc:DateTimeOffset?; SlotEndUtc:DateTimeOffset?;
  CountUnits:int?; WeightKg:decimal(18,3)?; OccupiedPersons:int?;
  State:Held|Committed|Released|Expired|Cancelled;
  ExpiresAtUtc:DateTimeOffset?; CorrelationKey:string;
  AllocatedAtUtc:DateTimeOffset; ReleasedAtUtc:DateTimeOffset?;

ProviderOperationAttempt : owned operation evidence record
  Id:long; ReservationUnitId:long?; ProviderKey:string;
  Operation:Hold|Confirm|Release|CancelConfirmed|Read;
  IdempotencyKey:string?; RequestDigest:string;
  StartedAtUtc:DateTimeOffset; CompletedAtUtc:DateTimeOffset?;
  Outcome:Succeeded|Rejected|Pending|Unknown;
  ProviderOperationRef:string?; ProviderUnitRef:string?;
  ProviderStatusCode:string?; SafeFailureCode:string?;
```

This is the **minimum logical schema**, not a mandate to add one table per record if current solution supports a smaller matching persistence shape. Reuse current IDs, enums and status values; add enum members only with explicit migrations/tests and no renumbering. If `RequestFingerprint` duplicates existing `EnsureSameContent`, preserve semantics and implement one canonical identity check rather than two inconsistent sources. No sensitive provider request bodies or personal medical data in permanent logs.

Reservation root status is a DERIVED summary (all held, all confirmed, partially succeeded, pending, terminal etc.), never false full success when one child was rejected/unknown. Domain unit statuses need at least `Requested, HeldWithEvidence, PendingProvider, ReadyForDirectConfirm, ConfirmedWithEvidence, Rejected, Released, Expired, CancelPending, Cancelled` with stable numeric values determined after auditing current enum; `UnknownOutcome` belongs attempt/recovery status rather than falsely allowing fresh stock allocation. Do not use a public `Held` if underlying unit has no allocation/provider evidence.

## D. Reservation request (internal logical contract) and per-unit response

```
ReserveBatchRequest
  OrderId:long*; OrderCommercialVersion:string*;
  IdempotencyKey:string*; CorrelationReference:string*;
  RequestedExpiresAt:DateTimeOffset?;
  Services:ReservationRequestUnit[] (nonempty)

ReservationRequestUnit
  OrderServiceId:long*; ServiceOfferId:string*; TravellerId:long?;
  DefinitionVersionId:long*; ProvisionId:long*;
  Coverage:{FlightIds:long[], PortionRefs:string[], Scope:existing enum}*;
  Quantity:int>=1;
  AcceptedCommercialSnapshotRef:string*;
  AcceptedPriceCurrencyId:int?;
  ProviderKey:string?;
  TypedSelectionReference:string?;

ReservationBatchResult
  AncillaryReservationId:long;
  OperationReference:string;
  Status:Complete|Partial|Pending|Rejected|Unknown;
  EffectiveExpiresAtUtc:DateTimeOffset?;
  Units:ReservationUnitResult[]

ReservationUnitResult
  OrderServiceId:long; ReservationUnitId:long;
  Status:explicit truthful status;
  ProviderOperationRef:string?; ProviderUnitRef:string?;
  CapacityEvidenceRef:string?;
  EffectiveExpiresAtUtc:DateTimeOffset?;
  ReasonCode:string?;
  MayRetry:bool; RequiresReadBack:bool;
```

Fields marked `*` are logical contract requirements; do not claim they already exist on current HTTP. Ordering adapter to this exact DTO has not been approved or implemented. In integration, `OrderServiceId` must belong to the authenticated Order, and accepted snapshot is resolved from Ordering as authority. Do not make a claimant-supplied price valid. Bind `ServiceOfferId` to current exactly validated definition/provision/context/selection/price and TTL from P3.2.

## E. Exact operations and allowed transitions

| Operation | From | Required proof, atomic effect | Result |
|---|---|---|---|
| `Hold` local | Requested | Published product + checked offer, inventory source `Active`, real physical count/weight/slot in same DB transaction, TTL valid | HeldWithEvidence or Rejected |
| `Hold` supplier | Requested | Real provider reply incl reference; async/pending is NOT held | HeldWithEvidence, PendingProvider, Rejected, Unknown |
| `Hold` Unlimited | Requested | No finite *local* stock only; validate MustCheckAvailability, supplier/booking confirmation requirements | ReadyForDirectConfirm or pending with correct policy; NEVER guaranteed just for Unlimited |
| `Hold` FlightFlow seat | Already managed by Ordering flight-unit | VERIFY existing Ordering/FlightFlow reservation evidence where contract permits; do NOT send duplicate hold | No duplicate stock allocation |
| `Confirm` local | HeldWithEvidence | Valid nonexpired allocation still owned by this unit, accepted funding/Order instruction from Ordering | Committed/ConfirmedWithEvidence exactly once |
| `Confirm` supplier | HeldWithEvidence/ReadyForDirectConfirm | Provider confirmation/result or documented no-confirm supplier contract | ConfirmedWithEvidence, PendingProvider, Rejected, Unknown |
| `Release` | HeldWithEvidence/PendingProvider as allowed | Cancel outstanding provider hold or local active allocations safely; compensate if provider outcome unknown | Released; may remain Pending until real evidence |
| `Expire` | HeldWithEvidence and clock >= effective expiry | CAS/locking and exactly-once local release; supplier status may require reconciliation | Expired only with appropriate local/provider semantics |
| `CancelConfirmed` | ConfirmedWithEvidence | Per-service provider cancellation scope or local consumed capacity release, actual policy + owner request | Cancelled/CancelPending; NOT a refund |
| `Read/Recover` | Unknown or Pending operation | Actual provider state read when available, or safe idempotent replay when provider explicitly supports it | Reconcile to truth without duplicate reservation |

**Rules:** Hold + Confirm not guaranteed combined transaction across multiple suppliers. Initial batch may partially succeed by distinct OrderService unit; emit all per-unit outcomes. Local multi-resource Count+Weight allocation MUST be atomic across both resource records. If one leg/portioned service needs all flights, define its own all-or-nothing unit requirement, not a false partial acceptance of a multi-flight purchased service. `Release` before confirmation != `CancelConfirmed` after confirmation. `CancelConfirmed` != refund/void/EMD cancellation. On provider technical timeout unknown, don't create a new hold with a new key until safe status is recovered.

## F. Capacity and concurrency specifics

- `FlightCountInventory` physical key `(OwnerAirlineId,FlightId,ResourceId)`; consumption `quantity*CountPerAcceptedUnit`; count-unit must match resource.
- `FlightWeightInventory` physical key `(OwnerAirlineId,FlightId,WeightResourceId)`; weight `quantity*FixedKgPerUnit` or trusted `AcceptedWeightKg`; decimal(18,3), no unsafe conversion.
- `AirportSlotInventory` physical half-open UTC intervals at verified `(OwnerAirlineId,AirportId,FacilityId)`; occupancy duration/people configured and validated, overlapping interval total usage bounded; 100 concurrent requests to 1 capacity => exactly 1 success.
- For one product with both count+weight requirements, acquire deterministic ordered resource locks in **one** DB transaction, recheck capacities inside transaction, append allocation(s), commit once or none. No global `StockPoolId` inferred from SKU.
- Two products bound to same physical ResourceId share one available pool and ledger; cannot each start at configured total. Active held + confirmed allocated <= current configured total at ALL times. Adjustments may not lower capacity below committed+held occupancy.
- On expiry/release/cancel retry, decrement allocation exactly once; replay same IdempotencyKey same canonical request returns same unit references/semantics, different payload same key => conflict. Concurrent replay returns first committed response, not a second allocated unit.
- Avoid holding SQL transaction/locks while awaiting slow supplier HTTP. Persist operation intent, dispatch via existing safe framework pattern, handle confirmed/pending/error and compensation. No invented infrastructure requirement; use established outbox/inbox if present.
- Evidence freshness matters: `AvailableAsOf` during Shopping is not a Hold. Revalidate at Hold under capacity lock or provider request.

## G. Proposed internal HTTP extensions (P3.3; adapt to existing controller convention)

Existing:
```
POST Service/v1/Ancillaries/Service-Holds
GET  Service/v1/Ancillaries/Service-Holds/{holdId}
POST Service/v1/Ancillaries/Service-Holds/{holdId}/Confirmations
```

Required new logical operations, proposed paths (not pre-existing):
```
POST Service/v1/Ancillaries/Service-Holds/{holdId}/Releases
POST Service/v1/Ancillaries/Service-Holds/{holdId}/Extensions (only with real provider support)
POST Service/v1/Ancillaries/Service-Holds/{holdId}/Recoveries
POST Service/v1/Ancillaries/Service-Holds/{holdId}/Confirmed-Cancellations
GET  Service/v1/Ancillaries/Service-Holds/{holdId}/Units/{unitId}
```

Expire is normally an internal idempotent scheduled application command driven by verified `IClock` + saved time. Internal service may offer explicit `Expire` for operations/repair but don't expose an anonymous public expired-state mutator. Route shape/method may be adjusted to actual ASP.NET conventions with documented evidence, not changed without tests. Require service-to-service authorization, caller scope and order-owner check; `GET` cannot leak other agency reservations. For cancellation support explicit `OrderServiceId`/unit targets, not always whole batch. Never reuse GET to mutate state.

HTTP/semantic failures: invalid request/selection/context, expired offer, stale commercial version, lost/unknown provider outcome, reference source unavailable, insufficient capacity, customer/POS mismatch, already confirmed, already released, and idempotency conflict each have distinct safe machine-readable result. Use current error framework/status mappings instead of invented numeric codes. No accidental auto-confirm after timeout.

## H. Integration readiness blockers and closure

- Need real, documented provider read/hold/confirm/cancel capability and supplier-specific partial/atomic behavior before claiming supplier `Held`.
- Need authoritative resource registries for Local count/weight, facility reference/zone for airport slots, counting-family registry/ledger, and FlightFlow delegation reference. Never bypass Phase2 blocks by making implicit local data.
- Need owner-approved settlement/order instructions contract and accepted snapshot binding from Ordering; Ancillary cannot independently decide payment was guaranteed or issue documents.
- Need to distinguish separate A09 seat inventory and A10 ticket exchange. If not available, mark these variants `BLOCKED_PROVIDER_CONTRACT` with truthful Shopping output rather than advertise reservation as implemented.
- Existing Ordering `IReservationProvider` has explicit capability model. Actual FlightFlow provider declares `AtomicAllOrNothing`, `SupportsReadBack=false`, `PreConfirmationReleaseScope=Operation`, `PostConfirmationCancelScope=Unit`; preserve these semantics rather than pretending all providers can partially release or read back.

## I. Mandatory P3.3 conformance/stress

SQL Server integration against real constraints, 100 concurrent holds cap=1; simultaneous create+release+expire+confirm; count+weight atomic rollback; shared resource across 2 products; overlapping airport interval and DST; duplicate key same/different digest; provider timeout-after-success then replay/read-back; multi-flight atomic unit; partially accepted mixed supplier order; cross-POS/cross-airline denial; expired quote/booking cutoff; confirmed cancellation per unit and replay; supplier pending vs held; free SSR requiring approval; FlightFlow double-hold negative test; A09/A10 explicit blocked test; immutability of accepted price and no EMD issuance from Ancillary. Separate externally blocked integration results from test failures.
