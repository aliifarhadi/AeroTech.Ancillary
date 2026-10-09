# Application workflow and API-level exact contract

> **Project authority:** AeroTech Ancillary **v12.1 — Phase 2: Stock & Capacity**. Continuation of the approved Phase 1 v12.1; not a separately versioned specification. Phase 3 remains closed.


## 1. Read/Write boundaries
CommandDb: `IInventoryPolicyRepository`, `IFlightCountInventoryRepository`, `IFlightWeightInventoryRepository`, `IAirportSlotInventoryRepository`. No GenericInventoryRepository. Read-model repositories/synchronizers per physical root and Policy. Reuse existing framework conventions (MediatR, command validators, UnitOfWork, outbox/synchronizer, query pagination, permissions, exception factory, strong ID patterns).

Service layer commands (canonical intent; align DTO names with repository style, preserve semantics):
- `DefineInventoryPolicy(ownerAirlineId, serviceDefinitionRef, serviceDefinitionId, authority, pattern?, providerKey?, typed consumptions?, passengerUsageLimits?)` -> `Draft`.
- `ChangeDraftInventoryPolicy(policyId, changes..., expectedVersion)` and `ActivatePolicy`, `SuspendPolicy`, `RetirePolicy` (no direct active mutation; new draft successor/revision only after verified lifecycle needs; preserve stable identity).
- `DefineFlightCountCapacity(flightId, resourceId, ownerAirlineId, countUnit, initialTotal)`; `AdjustFlightCountCapacity(id, newTotal, reason, actor, correlation, expectedVersion)`; `CloseForSale`, `OpenForSale`, `Suspend`, `Retire`.
- `DefineFlightWeightCapacity(flightId, weightResourceId, ownerAirlineId, capacityKg)` + same lifecycle/adjust with decimal precision.
- `DefineAirportSlotCapacity(airportId, facilityId, startUtc, endUtc, capacityPersons)` + same, interval overlap guard.
- `GetInventoryPolicyById`, `GetPolicyByServiceIdentity`, `ListInventoryPolicies`, typed `List/Get` for each family, and `GetInventoryConfigurationSnapshot` (NOT booking availability).

Proposed HTTP surface under EXISTING Backoffice convention `Backoffice/v1`:

| Method | Route suffix | Required safety |
|---|---|---|
| POST | `/AncillaryInventoryPolicies` | owner identity, typed discriminator, no default |
| PUT | `/AncillaryInventoryPolicies/{id}` | only Draft, expectedVersion; current non-Retired identity unique |
| POST | `/AncillaryInventoryPolicies/{id}/Activate` | all authority+resource evidence, no legacy mapping guess |
| POST | `/AncillaryInventoryPolicies/{id}/Suspend` | audited |
| POST | `/AncillaryInventoryPolicies/{id}/Retire` | audited, non-destructive |
| GET | `/AncillaryInventoryPolicies/{id}` | typed bindings + usage limits |
| GET | `/AncillaryInventoryPolicies/Paginated` | preserve conventions |
| POST | `/FlightCountInventories` | initial typed source; no duplicate physical key |
| POST | `/FlightCountInventories/{id}/Adjust` | absolute newTotal + expectedVersion + actor + reason + correlation |
| POST | `/FlightWeightInventories` | kg precision 18,3; physically distinct key |
| POST | `/FlightWeightInventories/{id}/Adjust` | absolute newKg + expectedVersion |
| POST | `/AirportSlotInventories` | UTC half-open, facility lock against overlap |
| POST | `/AirportSlotInventories/{id}/Adjust` | absolute new persons + expectedVersion |
| POST | `/.../{id}/CloseForSale` / `OpenForSale` | preserve quantities |
| GET | each typed `/{id}` and `/Paginated` | scoped authorization; no accidental all-tenant query |
| GET | `/AncillaryInventoryConfigurationSnapshots` | never guaranteed availability |

Use existing REST route naming/casing for final class/DTO files; do not introduce public `/Sell`, `/Reserve`, `/Hold`, `/Confirm`, `/Release`, `/Expire` or provider-allocation calls in Phase2.

## 2. Activation completeness matrix
- `Unlimited`: exactly one authority, no LocalPattern, no finite binding, no provider key required; commercial Provider for fulfillment still separate.
- `Supplier`: canonical provider key + contract/evidence reference required, no finite binding. If no validated provider configuration exists, keep Draft/explicit `Unknown`, never pretend `Available`.
- `FlightFlow`: validated delegated scope and flight/seat source reference, no local counter; check distinction ticket seat-capacity vs ancillary seat assignment.
- `Local FlightCount`: exactly one CountConsumption, positive coefficient and verified ResourceId; no weight/airport binding.
- `Local FlightWeight`: exactly one WeightConsumption; accepted weight semantics explicit.
- `Local CountPlusWeight`: exactly one of each and same occurrence FlightId at evaluation in later phase; no two-count generalization.
- `Local AirportSlot`: facility + occupancy duration + per-unit persons, bounded and preconfigured slots.
- DailyCount, RoomNight and AssignedAsset: `UnsupportedPattern/NotConfigured` until approved verified source; no service can activate with imaginary local quota.

## 3. Typed snapshots and provider edges
`InventoryConfigurationSnapshot { PolicyId?, OwnerAirlineId, ServiceDefinitionRef, Authority?, Pattern?, ResourceKind?, TypedResourceLocator?, ConfiguredCount?, ConfiguredKg?, ClosedForSale?, State, ObservedAt, StaleAfter?, IsGuaranteed=false, ReasonCode? }`. For local snapshot no `Held`, `Confirmed`, `Available`, or guaranteed return; external snapshots only if real provider reader contract exists, otherwise `DelegatedCheckRequired` or `Unknown`, with no fabricated numbers.

`IInventorySourceAvailabilityReader` MAY be a dispatch facade on the read side only, backed by typed readers. Do not write a fake external adapter or infer supplier `Reserve` support.

## 4. Database and race scenarios
- Typed unique physical source keys and filtered uniqueness, plus activation rechecks. SQL transaction prevents overlapping facility slots, including concurrent insert of two DIFFERENT overlapping intervals. Exact-key unique index alone does NOT protect this.
- On adjustment two concurrent updates with same version -> one commit, one domain 409, two increments/adjustment rows prohibited; idempotent correlation key collision with different payload -> conflict.
- In-memory domain tests and **real SQL Server** acceptance tests for filtering/transaction concurrency; do not claim DB race passed on SQLite or mocked repository.
- Persist and read all actor/correlation/reason/time information in write and read model as appropriate; migrations pending-model-changes false across Command and Query contexts.

## 5. Concrete response examples
### Existing product without configured policy
```json
{"serviceDefinitionRef":"MEAL_CHML","authority":null,"state":"NotConfigured","configuredCount":null,"isGuaranteed":false}
```
### Locally filed flight count
```json
{"serviceDefinitionRef":"MEAL_CHML","authority":"Local","pattern":"FlightCount","resource":{"flightId":1024,"resourceId":8001},"configuredCount":40,"state":"ConfiguredNotGuaranteed","isGuaranteed":false}
```
### Supplier-managed hotel
```json
{"serviceDefinitionRef":"CIP_EXT","authority":"Supplier","state":"DelegatedCheckRequired","configuredCount":null,"isGuaranteed":false}
```
Examples are proposed Phase2 application DTO semantics, NOT live endpoint output.
