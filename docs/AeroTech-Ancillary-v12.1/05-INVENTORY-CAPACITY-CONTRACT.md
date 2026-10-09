# Phase 2 Inventory/Capacity — correct semantics, real-world constraints, no universal stock

## Authoring is not allocation
**AncillaryInventoryPolicy** describes where availability/capacity authority resides. Sources store only **configured administrative maxima**; Phase 2 does not calculate `Available=Total-Consumed` and does not accept reservations. `MustCheckAvailability` flags a possible *commercial or operational* check and is orthogonal to stock quantity. A request requiring confirmation may be listed without a numeric stock pool. Stock API results must distinguish `NotConfigured`, `UnlimitedNoLocalQuota`, `ConfiguredNotGuaranteed`, `ClosedForSale`, `DelegatedCheckRequired`, `Unverified`, `UnsupportedPattern`, and `Unknown` as relevant; reuse existing enums/statuses where possible, adding only missing explicit distinctions. NEVER use `Unlimited` as an assertion of confirmed provider stock.

## Choose exactly one authority per stable service identity
| Authority | Meaning | What is stored | Operational truth |
|---|---|---|---|
| `Unlimited` | No finite quota tracked by Ancillary; **no automatic supplier guarantee** | policy only, zero capacity rows | Published product still subject to Provision restrictions and any required confirmation |
| `Supplier` | Supplier owns capacity/acceptance | supplier provider key, no stock mirror | Phase 3 will check/confirm; provider unverified = unknown |
| `FlightFlow` | Actual seat/flight-related source is owned elsewhere | authoritative delegation reference, no local seat stock | cannot make active without verified delegation under D11 |
| `Local` | An independently documented owned operational/quota source can be configured | specific Count/Weight/Slot ARs and capacity adjustments | admin totals are not available-to-sell until Phase 3 allocates |

**Explicit D10 correction:** Remove `EnsureUnlimitedIsCredible` rule that refuses `Unlimited` whenever an active Provision says `MustCheckAvailability`. These facts represent DIFFERENT axes. Still fail closed when the provisioning actually declares a verified supplier/flight managed resource; do not mislabel known external inventory as Unlimited. Domain test required: `Unlimited` + `MustCheckAvailability=true` is valid authoring, read state `Unlimited` with `Guarantee=false`, and any future booking must honor the check flag.

## Local inventory sources (Only 3 concrete models)
- `FlightCountInventory`: resource can mean 8 pet carriers, 30 meals, 2 sports equipment spaces **only when operator has actual documented quota**. Key `(OwnerAirlineId,FlightId,ResourceId)`; shared among service variants if they consume same real resource. Not an automatic implication of service type.
- `FlightWeightInventory`: sold commercial weight **only if airline has created a real limited booking quota**. Key `(OwnerAirlineId,FlightId,WeightResourceId)`. It is not aircraft weight-and-balance calculation, baggage entitlement ledger, free allowance, or load-sheet. decimal(18,3) with exact precision.
- `AirportSlotInventory`: capacity for facility + time interval `[startUtc,endUtc)` with no overlapping Active source for the same `(OwnerAirlineId,FacilityId)`; adjacent slots legal; interval locks under SQL and consistent IANA/local-offset conversion from validated real facility source. No invented 'every lounge is hourly' assumption.
- `FlightCountPlusWeight`: **one Count + one Weight resource** bound via Policy, same flight occurrence; no arbitrary N resource JSON/evaluation language. Authoring can configure both, but Phase 3 atomic two-resource hold deferred.
- Existing immutable `*Adjustment` entities per source: `Previous/Next`, `ActorId`, `ReasonCode`, `CorrelationId`, `Expected/ResultingVersion`, `OccurredAt`; unique `(parent,correlation)`; concurrency on `rowversion`; SQL interval overlap serialized.

## Real source check and D11 (KEEP CURRENT)
`FlightOccurrence`, `InventoryResource`, `AirportFacility` with timezone, `FlightFlowDelegation`, `CountingFamily` currently unresolved/NotConnected. **DO NOT create synthetic source records or bypass validators to force green status.** Draft authoring, query, safe rejection and source-supplied tests should work; production Local activation remains blocked with clear `SourceUnavailable` until the canonical owner/source is connected in a separately authorized step. No change to other services is allowed. This is a truthful operational limitation, not an invitation to add a generic local registry.

## Policy identity on ServiceDefinition revision
Existing `(OwnerAirlineId,ServiceDefinitionRef)` uniquely identifies a policy across commercial versions; a new `AncillaryServiceDefinition.Id` MUST NOT clone a physical source/policy or make old `ServiceDefinitionId` the controlling identity. Keep persisted `ServiceDefinitionId` only as last validated source pointer and update it through audited policy identity reconciliation if/when needed; always resolve the latest applicable ServiceDefinition version using stable key and verified owner, with no stale activation after revision. A historical pointer may be retained in audit. Tests must create and publish a new product version while preserving Policy ID and physical keys unchanged.

## Passenger usage vs provider quotas
PassengerUsageLimit (`PerOrder`, `PerFlightOccurrence`, `PerServiceDate`) is a **limit on purchase entitlement**; it never decrements capacity or creates Passenger-specific capacity rows. It can exist simultaneously with Local or Supplier policy. Actual cross-order usage and stable traveller identity enforcement are Phase 3 only. Unknown counting family => fail activation under D11, not quietly accept.

## Inventory family and capacity setup
| Service family | Normal authoring default / possible legitimate capacity | Do NOT do |
|---|---|---|
| Extra baggage (kg/piece), overweight/oversize | `Unlimited` no local quota, policy and commercial restrictions; `Supplier` if supplier-controlled; `Local` only factual quota | auto-create 500kg or 40-bag flight pool |
| Sports / PETC / AVIH | `Supplier` confirmation, or `Local FlightCount` if real confirmed 2 pets/flight | guarantee from SSR listing |
| Seat selection / Extra seat EXST | `FlightFlow` if delegation verified | keep another physical seat ledger |
| Meals | `Unlimited`/confirmation or `Local FlightCount` only if actual catering allocation | infer fixed meal count from seats |
| WCHR/WCHS/WCHC, UMNR | usually `Unlimited` *local quota* plus `SubjectToConfirmation`/`MustCheckAvailability`; supplier/delegated per real carrier | reject all without local wheelchair source; treat as paid stock |
| Priority boarding | `Unlimited` no physical quota; one per eligible person/segment | invented boarding-seat pool |
| Lounge/CIP/Fast Track | `Supplier` under actual partner; `Local AirportSlot` only with facility capacity agreement | assume per-hour capacity everywhere |
| Wi-Fi / eSIM / insurance | `Unlimited` no local quota if issuance unlimited; `Supplier` where external availability/issuance governs | assert provider fulfillment successful on policy activation |
| Hotel / transfer | `Supplier` by default. Room Night / Assigned Asset only once genuine owned allotment is proven later | synthesize hotel room counts or vehicle schedules |

## Phase 2 admin invariants and tests
- Entity identity unique even across concurrent Draft creates; no duplicate non-retired Policy per stable key, no duplicate non-retired physical key per owner+scope.
- `AdjustToAbsolute` only with expectedVersion, actor from caller, reason, correlation, actual difference, rollback on conflict; replay same correlation+payload idempotent, mismatched replay conflict.
- Slot intervals reject overlap at SQL concurrency, not only in-memory; adjacent allowed.
- Currency/Price/Provision edits must not alter capacity resource IDs or totals. Physical source capacities are shared only where same real resource, never one per Provision price/consumer.
- `NotConfigured` never falls back to `Unlimited`; `SourceUnavailable` never defaults to zero or available.
- No Phase 3 allocation, Hold, Confirm, expiry, release or EMD. No new fully operational Daily/RoomNight/AssignedAsset tables absent evidence.
