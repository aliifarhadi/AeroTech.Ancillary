# AeroTech Ancillary v12.1 — Phase 2 Stock & Capacity — Initial Conformance Gaps

Date: 2026-10-09
Written before any Phase 2 code. Source: `k8s-stg@933b7b7a793b426dbcb6362bbf8519886edf9080`, branch `feat/ancillary-v12.1-phase2-stock` created from it.
Authority: `docs/AeroTech-Ancillary-v12.1/PHASE2-STOCK-CAPACITY/` (00–15; all 16 manifest entries of that folder match their SHA-256).

## 1. Starting state

| Item | Value |
|---|---|
| `origin/k8s-stg` | `933b7b7` = PR #1 merge; local `k8s-stg` identical |
| Working tree at branch creation | not clean, all unrelated to code: `docs/AeroTech-Ancillary-v12.1/README.md` modified and `PHASE1-APPROVED-REFERENCE/`, `PHASE2-STOCK-CAPACITY/`, the pack zip untracked (placed by the Owner); `reports/V12.1-Phase1-Implementation-Report.md` modified (status wording, uncommitted). All preserved. |
| Branch name | the prompt names two (`feat/ancillary-v12.1-phase2-stock`, `feat/ancillary-phase2-stock`); neither existed; the first was used. |

## 2. Phase 1 public surface that Phase 2 builds on

- Aggregates: `Supplier`, `AncillaryServiceDefinition`, `AncillaryProvision`, `AncillaryPricing`, frozen `AncillaryReservation`. 57 REST routes; 47 command service contracts.
- Layering per use case: `I…Command`, `I…Service`, `…Service`, `…Validator<T>`, `Backoffice/…Command|Handler|Validator`; one repository and one query synchronizer per aggregate; `AncillaryUnitOfWork` saves the command context, then the read-model context (two contexts, two saves).
- Error codes used: 16001–16005, 161xx supplier, 162xx definition, 163xx provision, 164xx reservation, 165xx pricing. **166xx is free and is taken for inventory.**
- Optimistic concurrency: `AggregateRoot.RowVersion` (`rowversion`); a concurrency failure on a commercial aggregate becomes 16005 (409). No aggregate has an explicit numeric version today.
- No inventory, capacity or stock type exists. `AncillaryReservationUnit.StockPoolId?` exists in the frozen aggregate and is written by nothing.
- The Phase 1 boundary test forbids the words `Inventory` and `Quota` in source and requires the `Providers` project to hold only `DependencyInjection.cs`. Phase 2 must relax exactly those two assertions.

## 3. Identifier types found in the code

| Reference | Type | Source |
|---|---|---|
| `OwnerAirlineId`, `AirportId`, city, country, currency | `int` | ReferenceData read models and tables |
| aggregate ids, `ServiceDefinitionId`, `FlightId` (hold units) | `long` | domain |
| airline office, customer, operator settings | `long` | ReferenceData |
| `OperatorSettings.HomeAirlineId` | `long` | ReferenceData (dev row: `HOME_OPERATOR` → 1) |

## 4. Authorization and tenancy

- Every Backoffice controller carries `[Authorize(SurfaceAuthorization.Backoffice)]` (surface claim + scope). No per-permission attribute exists.
- The token has **no airline claim**. `ICallerContext` exposes `ContextType`, `AirlineUserId`, `AirlineOfficeId`, `ActorId`. Phase 1 takes `OwnerAirlineId` from the request body and does not check it.
- The only recorded tenant fact is `ReferenceData.OperatorSettings.HomeAirlineId`. Phase 2 will accept an inventory command only from a caller in `Airline` context and only for the operator's home airline; anything else is refused with 403. Rows of another owner are not found (404).
- Actor for audit comes from `ICallerContext.ActorId`, never from the request.

## 5. Reference and provider evidence

| Needed by Phase 2 | Found | Consequence |
|---|---|---|
| Service definition identity `(OwnerAirlineId, ServiceDefinitionRef)` | yes, in the repository | validated on every policy command |
| Supplier provider key | yes: `Supplier.FulfillmentKind = External` + `FulfillmentProviderKey` | `Supplier` authority can be activated against it |
| Airport | yes: `ReferenceData.Airports` (`int`) | validated |
| Flight occurrence (`FlightId`) | **no** local master and no FlightFlow client | activation of flight count/weight sources is blocked |
| Shared count / weight resource registry (`ResourceId`, `WeightResourceId`) | **no** | activation of local flight policies and sources is blocked |
| Airport facility with IANA time zone (`FacilityId`) | **no** (only airport terminals, without time zone) | activation of airport slot sources is blocked |
| FlightFlow delegated seat source | **no** contract in this repository | activation of `FlightFlow` policies is blocked |
| Service-family registry for `CountingFamilyCode` | **no** | activation of a policy with a passenger usage limit is blocked |
| Daily / room-night / asset ownership | **no** | patterns 5–7 are refused as unsupported; no schema is created |

"Blocked" means: the record can be authored as Draft, the activation command returns an explicit 409 naming the missing source, and nothing falls back to Unlimited. Each missing source is a small read-only port; tests use fixture implementations. No registry and no external client is invented.

## 6. Migrations

Command: `InitialAncillary`, `V11Phase1CommercialAuthoring`, `V12Phase1NormalizedProvisionAndPricing`, `V121Phase1RuleGroups`. Read model: the four `…Query` twins. Phase 2 adds one additive migration per context.

## 7. Phase 1 open issues (left as they are)

`ServiceDateBasis` was assigned to the 31 dev definitions after the Phase 1 report (dev data only); six Active `XBAG_WEIGHT_*` provisions keep `PerItem` with `Kilogram`; reference-id existence is not validated; AirAvail SQL on superseded read tables is unreviewed; the v12.1 Down migration does not rebuild restrictions authored later; pricing-line edits do not version the pricing row. None is touched by Phase 2. No policy is created for any of the 31 definitions.

## 8. Differences between the pack documents

| Topic | Documents | Choice |
|---|---|---|
| Consumption field names | 03 (`CountPerUnit`, `KgPerUnit`, `OccupancyDurationMinutes`) vs 10 (`CountPerAcceptedUnit`, `FixedKgPerUnit` + mode, `OccupancyMinutes`, `PeoplePerAcceptedUnit`) | document 10 (higher in the authority order of 08) |
| Adjustment fields | 03 (`ActorId`, `At`) vs 10 (`ActorId`, `CorrelationId`, `OccurredAt`) | document 10 |
| Read states | 03, 05 vs 10 (`CapacityReadState`) | document 10 |
| Slot unique key | 03 `(FacilityId, StartUtc, EndUtc)` vs 10 (owner + facility, no overlap) | document 10; overlap guard includes exact duplicates |
| Policy reactivation | 10 mentions suspend/reactivate/retire; 11 lists no reactivate route | `Activate` also reactivates a Suspended record |

## 9. Red tests written first

The cases required by the prompt (absent policy is NotConfigured; two products share one physical count key; 5 kg package binding; duplicate physical key refused; overlapping non-identical slots refused; stale `expectedVersion`; no capacity guarantee; frozen reservation files unchanged) are written against the planned API before the implementation exists. Their failing state is recorded in the implementation report.
