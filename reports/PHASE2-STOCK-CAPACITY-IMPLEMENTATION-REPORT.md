# AeroTech Ancillary v12.1 — Phase 2 Stock & Capacity — Implementation Report

Date: 2026-10-09
Authority: `docs/AeroTech-Ancillary-v12.1/PHASE2-STOCK-CAPACITY/` (00–15), prompt 13 "OWNER APPROVED".
Scope: Phase 2 only. No Phase 3, no hold / confirm / release / expire / decrement, no other repository.
Section numbers 1–12 follow document 14. Annexes A–C hold the decision register, the items that need the Owner, and the known limitations. Annex D records the legacy schema cleanup the Owner ordered afterwards; it is outside the Phase 2 scope.

**Verdict: `PHASE2_READY_FOR_OWNER_AUDIT`**
**`PRODUCTION_BOOKING_WITH_LIMITED_STOCK=NO_GO`**

Three facts to read before anything else:

1. **Nothing is committed.** The whole phase is a working-tree change on top of `933b7b7`.
2. **In a deployed environment only `Unlimited` and `Supplier` policies can become Active today.** Flight, resource, facility, FlightFlow-delegation and counting-family references have no source of truth in this repository, so every activation that needs one stops with 16608 and the record stays an inert Draft. The tests prove the activation behaviour against a test fixture that stands in for those sources (section 5).
3. **The authorized HTTP smoke passed: 76 of 76 requests as expected**, with the Owner's Backoffice token on a host built from this tree. Its first run found one defect, which was corrected before the figures below were produced (section 9). Still not covered over HTTP: 403 for a caller that is not an airline user, because that needs a token of another context type.

All figures below were produced on 2026-10-09 on this branch, after the last code change (the validator correction of section 9).

---

## 1. Source SHA, branch, diff, working tree

| Item | Value |
|---|---|
| Base | `k8s-stg@933b7b7a793b426dbcb6362bbf8519886edf9080` (local `k8s-stg` = `origin/k8s-stg` = this SHA) |
| Branch | `feat/ancillary-v12.1-phase2-stock`, created from the base. The prompt names two branch names; neither existed and the first was used. |
| HEAD | `933b7b7` — no commit was made |
| Commit SHA of Phase 2 | none; uncommitted, waiting for the Owner's approval |
| Tracked files modified | 24 — 17 belong to Phase 2, 5 more test files were changed by the legacy cleanup of Annex D, 2 belong to neither (below) |
| New files | 380 before this report: 8 contract enums, Domain 41, Application 201, Persistence 15, Providers 5, Query 76, RestApi 15, ServiceHost 2, Synchronizer 4, acceptance tests 9, domain tests 3, `reports/PHASE2-INITIAL-CONFORMANCE-GAPS.md`. The legacy cleanup of Annex D added 5 more: two migrations with their designer files and one test class. |

Working-tree items that are not Phase 2 code and were left as found:

- `docs/AeroTech-Ancillary-v12.1/README.md` (modified) and 31 untracked paths under `docs/` (`PHASE1-APPROVED-REFERENCE/`, `PHASE2-STOCK-CAPACITY/`, the pack zip) — placed by the Owner.
- `reports/V12.1-Phase1-Implementation-Report.md` (modified) — the correction the Owner ordered after the Phase 1 commit; still uncommitted.

Inherited v12.1 files changed by Phase 2 (17):

| File | Change |
|---|---|
| `Domain/_Shared/Resources/ExceptionFactory.cs`, `ExceptionMessages.cs` | +19 error codes 16601–16619 |
| `Application/DependencyInjection.cs` | registrations: 26 inventory command services, caller scope, evidence builder |
| `Persistence/AncillaryDbContext.cs` | 4 `DbSet`s |
| `Persistence/DependencyInjection.cs` | 4 repositories, commercial-facts reader |
| `Query/_Shared/DbContexts/AncillaryQueryDbContext.cs`, `Query/DependencyInjection.cs` | read-model `DbSet`s, 10 query services |
| `Synchronizer/DependencyInjection.cs` | 4 synchronizers |
| `Synchronizer/_Shared/AncillaryUnitOfWork.cs` | a concurrency failure on an inventory entity becomes 16605; a unique-index failure becomes 16604 (policy identity), 16614 (physical key) or 16605 (adjustment correlation). The existing mapping for commercial aggregates is unchanged; `IsCommercial` was only rewritten over a shared helper. |
| `Providers/DependencyInjection.cs` | 5 `NotConnected…Reference` registrations |
| `ServiceHost/Program.cs` | 2 registrations (operator airline, airport) |
| Two EF model snapshots | generated |
| `tests/…/Boundary/V12BoundaryAcceptanceTests.cs` | pins moved to the new surface: routes 57 → 93, command service contracts 47 → 73, Providers file list +5. Two guards were loosened: the term `Inventory` is no longer forbidden, and the word `FlightFlow` is now allowed in inventory files, the delegation port, `ExceptionMessages.cs` and `DependencyInjection.cs` (it names the authority). One guard was added: `AeroTech.FlightFlow`, `Messages.FlightFlow`, `AeroTech.Ordering`, `Messages.Ordering` are forbidden everywhere, and no route may contain `Sell`, `Reserve`, `Release` or `Expire`. |
| `tests/…/P1BoundaryConformanceTests.cs` | aggregate list 5 → 9 |
| `tests/…/Fixtures/BusinessAssert.cs`, `Migration/LegacySeeds.cs` | one helper, two migration names |

No Phase 1 test expectation about behaviour was changed; only the structural pins and guards listed above.

---

## 2. Model: aggregates, entities, value objects, enums, keys, indexes, precision, lifecycle

### 2.1 Aggregates

| Aggregate root | Children | Purpose | Holds a counter |
|---|---|---|---|
| `AncillaryInventoryPolicy` | `PassengerUsageLimit` (entity); `FlightCountConsumption`, `FlightWeightConsumption`, `AirportSlotConsumption` (value objects) | Declares who is the authority for one service identity and how one accepted unit consumes a resource | No |
| `FlightCountInventory` | `FlightCountAdjustment` | Countable capacity of one resource on one flight | Configured total only |
| `FlightWeightInventory` | `FlightWeightAdjustment` | Commercial kilogram quota of one resource on one flight | Configured kg only |
| `AirportSlotInventory` | `AirportSlotAdjustment` | Person capacity of one facility in one UTC interval | Configured persons only |

There is no reserved, held, sold or available quantity anywhere. `FlightCountPlusWeight` is not a fifth aggregate: it is a policy that binds exactly one count resource and one weight resource. `DailyCount`, `RoomNight` and `AssignedAsset` exist only as enum members; no table, aggregate or repository was created for them.

### 2.2 `Ancillary.AncillaryInventoryPolicies`

| Column | Type | Note |
|---|---|---|
| `Id` | bigint | snowflake |
| `OwnerAirlineId` | int | identity part 1 |
| `ServiceDefinitionRef` | nvarchar(30) | identity part 2 |
| `ServiceDefinitionId` | bigint | FK → `AncillaryServiceDefinitions` (Restrict) |
| `Authority` | int | `InventoryAuthority` |
| `LocalPattern` | int null | `LocalInventoryPattern`, only when Authority = Local |
| `ProviderKey` | nvarchar(50) null | only Supplier / FlightFlow |
| `CountResourceId`, `CountPerAcceptedUnit`, `CountUnit` | bigint / int / int, null | count binding |
| `WeightResourceId`, `WeightConsumptionMode`, `WeightFixedKgPerUnit` | bigint / int / decimal(18,3), null | weight binding |
| `SlotFacilityId`, `SlotOccupancyMinutes`, `SlotPeoplePerAcceptedUnit` | bigint / int / int, null | slot binding |
| `Status`, `Version` | int, bigint | `Version` is an EF concurrency token |
| `CreatedAt`, `UpdatedAt`, `ActivatedAt`, `SuspendedAt`, `RetiredAt` | datetimeoffset | |
| `LastUpdateTime`, `LastUpdatedBy`, `RowVersion` | framework columns | `rowversion` |

Indexes: `IX_AncillaryInventoryPolicies_Identity_Current` unique on `(OwnerAirlineId, ServiceDefinitionRef)` filtered `[Status] <> 4`; plain indexes on `ServiceDefinitionId` and `Status`.

`Ancillary.InventoryPassengerUsageLimits`: `Id`, `InventoryPolicyId` (FK, Cascade), `LimitScope` int, `MaxUnits` int, `CountingFamilyCode` nvarchar(30); unique `(InventoryPolicyId, LimitScope)`.

### 2.3 Capacity sources

| Table | Physical key (unique, filtered `[Status] <> 4`) | Quantity | Other columns |
|---|---|---|---|
| `FlightCountInventories` | `(OwnerAirlineId, FlightId, ResourceId)` | `TotalCapacity` int ≥ 0 | `CountUnit` int; extra index `(OwnerAirlineId, ResourceId)` |
| `FlightWeightInventories` | `(OwnerAirlineId, FlightId, WeightResourceId)` | `CapacityKg` decimal(18,3) ≥ 0 | — |
| `AirportSlotInventories` | `(OwnerAirlineId, FacilityId, StartUtc, EndUtc)` | `CapacityPersons` int ≥ 0 | `AirportId` int; extra index `(OwnerAirlineId, FacilityId, EndUtc)` |

Every source also has `ClosedForSale` bit, `Status`, `Version` (concurrency token), `CreatedAt`, `UpdatedAt`, framework columns and `RowVersion`.

Adjustment tables (`FlightCountAdjustments`, `FlightWeightAdjustments`, `AirportSlotAdjustments`): parent id (FK, Restrict), previous and new value (`int`, or `decimal(18,3)` for weight), `ReasonCode` nvarchar(50), `ActorId` bigint, `CorrelationId` nvarchar(64), `OccurredAt`, `ExpectedVersion`, `ResultingVersion`; unique `(parent id, CorrelationId)`. A row is inserted once and never updated; the aggregate exposes no method that edits or removes one.

Precision: kilograms are `decimal(18,3)` in four places of each migration; a fourth decimal is refused, never rounded. Counts are whole `int`s.

### 2.4 Read model (`ReadModel` schema)

`AncillaryInventoryPolicies`, `AncillaryInventoryPassengerUsageLimits`, `FlightCountInventories`, `FlightCountAdjustments`, `FlightWeightInventories`, `FlightWeightAdjustments`, `AirportSlotInventories`, `AirportSlotAdjustments`. Source read models add `AdjustmentCount`.

### 2.5 Enums (`Contracts/AeroTech.Messages/Ancillary/Enums/`)

| Enum | Members |
|---|---|
| `InventoryAuthority` | Unlimited 1, Local 2, Supplier 3, FlightFlow 4 |
| `LocalInventoryPattern` | FlightCount 1, FlightWeight 2, FlightCountPlusWeight 3, AirportSlot 4, DailyCount 5, RoomNight 6, AssignedAsset 7 |
| `InventoryRecordStatus` | Draft 1, Active 2, Suspended 3, Retired 4 |
| `InventoryCapacityReadState` | NotConfigured 1, Unlimited 2, ConfiguredNotGuaranteed 3, ClosedForSale 4, DelegatedCheckRequired 5, Unknown 6, UnsupportedPattern 7 |
| `InventoryCountUnit` | Person 1, Piece 2, Item 3, AnimalCarrier 4, Equipment 5 |
| `PassengerUsageLimitScope` | PerOrder 1, PerFlightOccurrence 2, PerServiceDate 3 |
| `FlightWeightConsumptionMode` | FixedKgPerAcceptedUnit 1, AcceptedWeightKg 2 |
| `InventoryResourceKind` | FlightCount 1, FlightWeight 2, AirportSlot 3 |

### 2.6 Lifecycle

Policy and sources share one status machine:

- `Define` → Draft.
- `Activate` from Draft or Suspended → Active (needs reference evidence, section 5).
- `Suspend` from Active → Suspended.
- `Retire` from Draft, Active or Suspended → Retired (terminal; frees the unique key).
- Policy only: `Change` (full replace) while Draft.
- Sources only: `AdjustTo` (absolute new total), `CloseForSale`, `OpenForSale` while not Retired. A source has no edit command; its key is fixed at Define.

Every mutating command carries `ExpectedVersion`; a mismatch is 16605 (409) and nothing is written. Each accepted change increases `Version` by one.

Activation matrix of a policy:

| Authority / pattern | Required | Refusal |
|---|---|---|
| Unlimited | no binding, pattern or provider key; no Active provision of the identity with `MustCheckAvailability` | 16606 |
| Supplier | `ProviderKey` equal to the recorded key of the Active External supplier of the service; no local binding | 16606 |
| FlightFlow | provider key and a Verified delegation; no local binding | 16606 / 16608 |
| Local FlightCount, FlightWeight, FlightCountPlusWeight, AirportSlot | exactly the bindings of the pattern, each resource Verified, units aligned | 16606 / 16608 / 16609 / 16618 |
| Local DailyCount, RoomNight, AssignedAsset | — | 16607 always |
| Any policy with usage limits | every counting family Verified | 16608 / 16609 |

Unit alignment (16618): the service must have a `PricingUnit`; `AcceptedWeightKg` needs `PerKilogram`; `FixedKgPerAcceptedUnit` and count-only patterns forbid `PerKilogram`; the binding's `CountUnit` must equal the `CountUnit` of the live sources of the same resource.

### 2.7 Error codes 16601–16619

| Code | Name | HTTP |
|---|---|---|
| 16601 | InventoryPolicyNotFound | 404 |
| 16602 | InventoryPolicyIsInvalid | 422 |
| 16603 | InventoryPolicyStatusChangeNotAllowed | 409 |
| 16604 | InventoryPolicyAlreadyExists | 409 |
| 16605 | InventoryVersionConflict | 409 |
| 16606 | InventoryPolicyActivationRefused | 409 |
| 16607 | InventoryPatternNotSupported | 409 |
| 16608 | InventoryReferenceSourceUnavailable | 409 |
| 16609 | InventoryReferenceNotFound | 422 |
| 16610 | InventoryOwnerNotAuthorized | 403 |
| 16611 | InventorySourceNotFound | 404 |
| 16612 | InventorySourceIsInvalid | 422 |
| 16613 | InventorySourceStatusChangeNotAllowed | 409 |
| 16614 | InventorySourceAlreadyExists | 409 |
| 16615 | InventorySlotOverlap | 409 |
| 16616 | InventoryCorrelationConflict | 409 |
| 16617 | InventoryPolicyServiceDefinitionMismatch | 422 |
| 16618 | InventoryUnitMismatch | 409 |
| 16619 | InventoryFacilityBusy | 409 |

---

## 3. The 31 pre-existing definitions

Dev database `DotAirAncillary`, owner airline 1, queried 2026-10-09. **No inventory policy is recorded for any of them, so every one reads `NotConfigured`.** No mapping was proposed, inferred or seeded. The columns other than the last two are existing facts, shown only because they constrain what the Owner may later record.

| ServiceDefinitionRef | PricingUnit | Supplier | Active provisions | of which must check availability | Recorded policy | Snapshot state |
|---|---|---|---|---|---|---|
| ASSIST_STRETCHER | PerPassenger | Local | 3 | 2 | none | NotConfigured |
| ASSIST_WCHC | PerPassenger | Local | 1 | 1 | none | NotConfigured |
| ASSIST_WCHR | PerPassenger | Local | 1 | 0 | none | NotConfigured |
| ASSIST_WCHS | PerPassenger | Local | 1 | 0 | none | NotConfigured |
| CIP_ARRIVAL_IKA | PerPassenger | Local | 3 | 0 | none | NotConfigured |
| CIP_DEPARTURE_IKA | PerPassenger | Local | 4 | 0 | none | NotConfigured |
| FASTTRACK_IKA | PerPassenger | Local | 5 | 0 | none | NotConfigured |
| INS_DOMESTIC | PerPassenger | External `SafeTripInsurance` | 1 | 0 | none | NotConfigured |
| INS_TRAVEL_BASIC | PerPassenger | External `SafeTripInsurance` | 4 | 0 | none | NotConfigured |
| INS_TRAVEL_PLUS | PerPassenger | External `SafeTripInsurance` | 4 | 0 | none | NotConfigured |
| LOUNGE_CIP_IKA | PerPassenger | Local | 4 | 0 | none | NotConfigured |
| LOUNGE_DOTAIR_IKA | PerPassenger | Local | 4 | 0 | none | NotConfigured |
| LOUNGE_DOTAIR_THR | PerPassenger | Local | 2 | 0 | none | NotConfigured |
| MAAS_IKA | PerPassenger | Local | 2 | 0 | none | NotConfigured |
| MEAL_CHML | PerPassenger | Local | 1 | 0 | none | NotConfigured |
| MEAL_PREORDER_HOT | PerPassenger | Local | 5 | 0 | none | NotConfigured |
| MEAL_VGML | PerPassenger | Local | 2 | 0 | none | NotConfigured |
| PET_IN_CABIN | PerItem | Local | 4 | 2 | none | NotConfigured |
| PET_IN_HOLD | PerItem | Local | 3 | 2 | none | NotConfigured |
| PRIORITY_BOARDING | PerPassenger | Local | 6 | 0 | none | NotConfigured |
| SEAT_SELECTION | PerSeat | Local | 14 | 13 | none | NotConfigured |
| SPORT_BIKE | PerPiece | Local | 2 | 0 | none | NotConfigured |
| SPORT_DIVING | PerPiece | Local | 1 | 0 | none | NotConfigured |
| SPORT_SKI | PerPiece | Local | 1 | 0 | none | NotConfigured |
| UMNR_SERVICE | PerPassenger | Local | 3 | 2 | none | NotConfigured |
| WIFI_FULL_FLIGHT | PerItem | Local | 4 | 0 | none | NotConfigured |
| WIFI_MESSAGING | PerItem | Local | 2 | 0 | none | NotConfigured |
| XBAG_PIECE_23KG | PerPiece | Local | 6 | 0 | none | NotConfigured |
| XBAG_WEIGHT_10KG | PerItem | Local | 2 | 0 | none | NotConfigured |
| XBAG_WEIGHT_20KG | PerItem | Local | 3 | 0 | none | NotConfigured |
| XBAG_WEIGHT_5KG | PerItem | Local | 1 | 0 | none | NotConfigured |

Row counts in dev after the migrations: 0 policies, 0 usage limits, 0 sources and 0 adjustments in both the command and the read schema. Tests `P2_C02`, `P2_X09` (acceptance) and `P2_X09` (migration) assert that a definition without a policy is `NotConfigured` and that the migration creates no policy.

---

## 4. Phase 1 open issues and exception register

Phase 2 changed none of these.

| Phase 1 open issue | State now | Effect on inventory authoring |
|---|---|---|
| `ServiceDateBasis` unset on 31 definitions | Document 14 is out of date here: all 31 were set to `FlightDeparture` on dev on 2026-10-08 through the API with the Owner's approval, before Phase 2 started. Phase 2 did not touch them. | None in Phase 2. Inventory code does not read `ServiceDateBasis`; resolving the service date of a `PerServiceDate` limit is Phase 3. |
| Six Active `XBAG_WEIGHT_*` provisions: `PerItem` definition with a kilogram quantity | unchanged | Not blocking. A `PerItem` weight package is consistent only with `FixedKgPerAcceptedUnit`; `AcceptedWeightKg` would be refused with 16618. |
| AirAvail SQL consumer of superseded read-model tables | not reviewed; the objects it reads were dropped by the Owner-ordered cleanup (Annex D) | None for inventory |
| Reference-id existence not validated in Phase 1 authoring | unchanged | None. Phase 2 validates only its own references. |
| Legacy migrations `Down` | unchanged | None |
| Pricing-line concurrency | unchanged | None |

Exception register — inventory authoring that cannot complete today:

| # | Blocked operation | Cause | Result | State |
|---|---|---|---|---|
| E1 | Activate any `FlightCountInventory` or `FlightWeightInventory` | no flight-occurrence source, no resource registry | 16608, stays Draft | Awaiting owner/source evidence |
| E2 | Activate any `AirportSlotInventory` | no facility + IANA time-zone source | 16608, stays Draft | Awaiting owner/source evidence |
| E3 | Activate a Local policy of patterns 1–4 | same as E1 / E2 for its resources | 16608 | Awaiting owner/source evidence |
| E4 | Activate a FlightFlow policy | no FlightFlow delegation contract | 16608 | Awaiting owner/source evidence |
| E5 | Activate any policy that carries a passenger usage limit | no counting-family registry | 16608 | Awaiting owner/source evidence |
| E6 | Activate Local DailyCount / RoomNight / AssignedAsset | deferred by decision | 16607 | By design |
| E7 | Declare `Unlimited` for ASSIST_STRETCHER, ASSIST_WCHC, PET_IN_CABIN, PET_IN_HOLD, SEAT_SELECTION, UMNR_SERVICE | each has an Active provision with `MustCheckAvailability` | 16606 | By design |

None of E1–E7 is caused by a Phase 1 open issue.

---

## 5. Reference and ownership evidence

| Reference | Source of truth found | Implementation | Result today |
|---|---|---|---|
| Owner airline | `ReferenceData.OperatorSettings`, `ScopeKey = HOME_OPERATOR` → `HomeAirlineId` (dev: 1) | `ReferenceDataOperatorAirlineResolver` | Connected |
| Caller | `ICallerContext` (verified token). The token carries no airline claim. | `InventoryCallerScope`: `ContextType` must be `Airline`, owner = home airline, `ActorId` > 0 | Connected |
| Service identity, pricing unit, must-check flag | Ancillary's own command tables | `InventoryCommercialFactsReader` | Connected |
| Supplier provider key | `Suppliers.FulfillmentProviderKey` of the Active External supplier of the service | same reader | Connected |
| Airport | `ReferenceData.Airports` | `ReferenceDataAirportReference` | Connected |
| Flight occurrence (`FlightId`) | none in this repository; calling FlightFlow would be an adapter without evidence | `NotConnectedFlightOccurrenceReference` | `SourceUnavailable` |
| Count / weight resource id | none; no registry exists and none was invented | `NotConnectedInventoryResourceReference` | `SourceUnavailable` |
| Airport facility and its IANA time zone | none | `NotConnectedAirportFacilityReference` | `SourceUnavailable` |
| FlightFlow delegation | none | `NotConnectedFlightFlowDelegationReference` | `SourceUnavailable` |
| Counting family of a usage limit | none | `NotConnectedCountingFamilyReference` | `SourceUnavailable` |

Rules that follow:

- Resource, facility and flight ids are opaque `long`s owned by an external master. Ancillary does not generate, list or validate them locally.
- Ownership comes from the verified caller. A Define request states `ownerAirlineId`, and it must equal the owner derived from the caller (airline context + operator home airline); any other value, or a caller that is not an airline user, gets 403 (16610). Commands and queries on existing rows use the derived owner only; a row of another owner answers 404, never its content.
- A missing source is never treated as success: `SourceUnavailable` → 16608, `NotFound` → 16609. A Draft is inert for the snapshot.

Awaiting owner/source evidence: flight occurrence, resource registry, airport facility with time zone, FlightFlow delegation, counting family. Until a source exists, the affected activations are E1–E5 of section 4. In the tests these five ports are served by `Fakes/InventoryFixture`; that fixture is test code and is not registered in the host.

---

## 6. Scenario matrix — 62 IDs

Result: **50 executed PASS, 0 FAIL, 12 deferred** (`DEFERRED_PHASE3` 4, `DEFERRED_PROVIDER_EVIDENCE` 6, `DEFERRED_ASSET_EVIDENCE` 2). No claim of 62 passing tests is made.

Test classes: **DP** `P2InventoryPolicyConformanceTests`, **DS** `P2CapacitySourceConformanceTests` (domain); **AP** `Inventory.P2InventoryPolicyAcceptanceTests`, **AF** `Inventory.P2FlightCapacityAcceptanceTests`, **AS** `Inventory.P2AirportSlotAcceptanceTests`, **AB** `Boundary.P2BoundaryAcceptanceTests`, **AM** `Migration.P2StockCapacityMigrationAcceptanceTests` (acceptance, real SQL Server).

| ID | Result | Test method | What is proven, or why deferred |
|---|---|---|---|
| C01 | PASS | DP `P2_C01_unlimited_policy_has_no_binding_pattern_or_provider_and_activates_without_any_counter`; AP `P2_C01_unlimited_policy_creates_no_stock_rows` | Unlimited has no pattern, binding, provider or amount; activation writes no source row |
| C02 | PASS | AP `P2_C02_unmapped_legacy_product_is_not_configured_not_unlimited` | no policy → `NotConfigured` |
| C03 | PASS | DP `P2_C03_H05_supplier_policy_needs_the_recorded_provider_key_and_owns_no_local_binding`; AP `P2_C03_H05_X11_a_supplier_managed_policy_keeps_no_local_stock_and_answers_delegated_check_required` | no local stock; snapshot names the provider and answers `DelegatedCheckRequired`. No provider is consulted: `observedAt` is the time the configuration was read and `staleAfter` is null. |
| C04 | PASS (no-mirror guard) | DP `P2_C04_X12_flightflow_policy_owns_no_local_counter_and_activates_only_with_a_verified_delegation`; AP `P2_C04_X12_a_flightflow_managed_policy_never_mirrors_seat_capacity_and_needs_a_connected_delegation` | no capacity mirror. Blocked / occupied / available stay FlightFlow's; they are not read (no adapter, per document 12). |
| C05 | PASS | DS and AF `P2_C05_F08_…` | total 0, closed for sale and suspended are three states; none reads as Unlimited; no sold-out state exists |
| C06 | PASS | AF `P2_F10_C06_the_provision_keeps_the_purchase_cutoff_and_the_commercial_outcome_whatever_the_capacity_says` | a NotAvailable provision stays the commercial outcome with positive capacity |
| C07 | PASS | DP `P2_C07_a_unit_mismatch_between_pricing_and_consumption_is_refused_at_activation`; AP `P2_C07_…` | 16618 |
| F01 | PASS | DS `P2_F01_F03_…`; AF `P2_F01_F03_F05_a_flight_count_source_is_one_row_per_owner_flight_and_resource_enforced_by_the_database` | key = owner + flight + resource, total 4 |
| F02 | PASS | DP `P2_F02_F09_…`; AF `P2_F02_pet_products_share_one_physical_flight_count_key` | two products, one resource id, one row |
| F03 | PASS | same as F01 | a second flight is a separate row |
| F04 | PASS | DS and AF `P2_F04_…`; AP `P2_X02_inventory_is_managed_only_by_an_airline_caller_of_the_operator_and_never_across_owners` | activation needs a verified flight and resource; another owner is refused. A source has no supplier column, so supplier mismatch is checked on the policy (C03). |
| F05 | PASS | AF `P2_F01_F03_F05_…` | filtered unique index; 100 concurrent defines of one key → 1 accepted, 99 × 16614 |
| F06 | PASS | DS `P2_F06_…`; AF `P2_F06_F07_an_adjustment_is_an_audited_absolute_change_that_refuses_a_stale_version_and_replays_idempotently` | 8 → 5, one immutable line with actor, reason, correlation |
| F07 | PASS | DS `P2_F07_X01_…`; AF `P2_F06_F07_…` | 16605 / 409, nothing overwritten |
| F08 | PASS | DS and AF `P2_C05_F08_…` | suspended closes sale and keeps the total |
| F09 | PASS | DP `P2_F02_F09_…`; AF `P2_F09_meal_variants_share_one_catering_quota` | CHML and VGML bind one quota of 30 |
| F10 | PASS | AF `P2_F10_C06_…` | the 24-hour cut-off stays on the Provision; inventory has no cut-off field |
| W01 | PASS | DP and AF `P2_W01_fixed_5kg_bundle_uses_5kg_in_binding_even_when_price_per_item` | 5 kg per accepted unit, explicit |
| W02 | PASS | DP `P2_W02_W03_weight_demand_is_quantity_times_fixed_kg_or_the_accepted_weight_with_three_decimals` | 2 × 10 kg = 20 kg |
| W03 | PASS | DP `P2_W02_W03_…`; DS `P2_W03_W05_…`; AF `P2_W03_same_expected_version_weight_adjustment_has_one_winner_and_exact_precision_sql` | decimal(18,3); a fourth decimal is refused |
| W04 | PASS | DP and AF `P2_W04_count_and_weight_binding_is_closed_two_resource_contract` | exactly one count and one weight binding |
| W05 | PASS | DS `P2_W03_W05_a_weight_source_is_commercial_kilograms_with_three_decimals_and_makes_no_load_control_claim` | commercial quota only |
| W06 | DEFERRED_PHASE3 | — | needs the obligations ledger of Phase 3 |
| A01 | PASS | DS `P2_A01_A02_…`; AS `P2_A01_A02_A05_adjacent_utc_slots_are_accepted` | capacity per facility |
| A02 | PASS | same | bounded UTC interval, whole minutes, at most 24 h |
| A03 | PASS (contract); consumption DEFERRED_PHASE3 | DS `P2_A03_an_occupancy_of_ninety_minutes_intersects_every_slot_it_touches_and_not_only_the_arrival_slot` | the intersection rule is proven; atomic consumption of the intersecting slots is Phase 3 |
| A04 | PASS | DS `P2_A04_A05_…`; AS `P2_A04_an_interval_that_overlaps_a_current_slot_of_the_same_facility_is_refused_even_when_not_identical`; AS `P2_A04_concurrent_overlapping_facility_slot_creation_has_single_winner_sql` | 16615, also under 100 concurrent requests |
| A05 | PASS | DS `P2_A04_A05_…`; AS `P2_A01_A02_A05_…` | half-open intervals; 100 adjacent slots all accepted |
| A06 | PASS | DS and AS `P2_A06_dst_local_time_must_have_authoritative_timezone_and_utc_resolution` | an instant without offset is refused; activation needs the facility's IANA zone |
| A07 | PASS | DP `P2_A07_…`; AS `P2_A07_A08_…`; AP `P2_P01_A07_…` | usage limit and slot binding are separate |
| A08 | PASS | DS `P2_A08_…`; AS `P2_A07_A08_a_lounge_slot_is_reported_per_facility_and_instant_beside_its_passenger_usage_limit` | one facility closed, the other of the same airport unaffected. Proven with closed-for-sale; "sold out" does not exist in Phase 2. |
| D01 | DEFERRED_PROVIDER_EVIDENCE | — | no verified local daily source; no schema invented |
| D02 | DEFERRED_PROVIDER_EVIDENCE | — | same |
| D03 | DEFERRED_PROVIDER_EVIDENCE | — | same; no bulk command exists |
| D04 | PASS (fail-closed path) | DP `P2_D04_H06_R03_a_local_pattern_without_a_verified_source_model_stays_draft_and_is_never_unlimited`; AP `P2_D04_H06_R03_a_pattern_without_a_verified_source_model_is_reported_unsupported_and_never_unlimited` (3 cases each) | DailyCount: Draft only, activation 16607, snapshot `UnsupportedPattern` |
| H01 | DEFERRED_PROVIDER_EVIDENCE | — | no signed local allotment |
| H02 | DEFERRED_PHASE3 | — | stay consumption |
| H03 | DEFERRED_PHASE3 | — | stay consumption |
| H04 | DEFERRED_PROVIDER_EVIDENCE | — | no room-night source |
| H05 | PASS | as C03 | supplier-managed, no local copy |
| H06 | PASS (fail-closed path) | as D04, RoomNight case | a local room-night policy cannot activate and is never unlimited. The allotment source itself is DEFERRED_PROVIDER_EVIDENCE. |
| H07 | DEFERRED_PROVIDER_EVIDENCE | — | no room-night source |
| P01 | PASS | DP `P2_P01_passenger_usage_limit_is_a_policy_child_beside_the_physical_binding_and_never_a_counter`; AP `P2_P01_A07_passenger_usage_limits_are_policy_rows_beside_the_binding_and_need_a_registered_family` | limit and flight count co-exist |
| P02 | PASS (identity contract) | DP `P2_P02_P03_P04_a_usage_scope_is_keyed_by_a_stable_traveller_identity_and_never_falls_back_to_the_order` | the key needs a stable traveller identity. Cross-order enforcement is Phase 3. |
| P03 | PASS | same | different traveller → different key |
| P04 | PASS | same | different flight → different key |
| P05 | PASS (absence proof) | AB `P2_R03_P05_only_the_four_approved_inventory_aggregates_exist_and_no_entitlement_or_usage_ledger_is_spoofed` | no entitlement or usage-ledger type exists; not modelled as stock |
| R01 | DEFERRED_ASSET_EVIDENCE | — | no operator evidence |
| R02 | DEFERRED_ASSET_EVIDENCE | — | no operator evidence |
| R03 | PASS | AB `P2_R03_P05_…`; D04 tests, AssignedAsset case | no asset aggregate |
| X01 | PASS | DS `P2_F07_X01_…`, `P2_X01_a_replayed_correlation_returns_the_first_adjustment_and_a_different_payload_conflicts`; AF `P2_X01_same_expected_version_capacity_adjustment_has_one_winner_sql`, `P2_X01_a_lost_projection_is_healed_by_replaying_the_same_correlation_without_a_second_adjustment`; AS `P2_X01_same_expected_version_slot_adjustment_has_one_winner_sql` | section 8 |
| X02 | PASS | DS `P2_X02_…` (2); DP `P2_X02_…`; AP `P2_X02_inventory_is_managed_only_by_…`, `P2_X02_the_backoffice_validators_check_the_shape_of_every_inventory_input` | negative capacity, invalid times, wrong ids → 422 / 409 / 403 / 404; the same statuses were observed over HTTP (section 9) |
| X03 | PASS | AF `P2_X03_one_hundred_sources_of_distinct_flights_are_all_accepted_concurrently` | no batch command exists; each command is one save; a refused command leaves no row |
| X04 | DEFERRED_PHASE3 | — | obligations do not exist yet |
| X05 | PASS | AP `P2_X05_the_policy_read_model_equals_the_command_store_after_every_step`; reconciliation asserted in 9 AF / AS tests | section 7.5 |
| X06 | PASS | AB `P2_X06_…`; AM `P2_X06_…`; AB `P2_X07_…` | 36 frozen files hash-identical; 22 M1 hold/confirm tests pass |
| X07 | PASS | AB `P2_X07_no_cross_repository_or_phase3_changes` | section 10 |
| X08 | PASS | AP `P2_X08_snapshot_cannot_claim_booking_guarantee` | `IsGuaranteed` is always false |
| X09 | PASS | AP `P2_X09_legacy_definition_authority_is_not_inferred`; AM `P2_X09_…`; DP `P2_C01_X09_…` | section 3 |
| X10 | PASS | DP `P2_X10_…`; AP `P2_X10_one_current_policy_per_service_identity_even_under_one_hundred_concurrent_requests` | section 8 |
| X11 | PASS | AP `P2_C03_H05_X11_…` | a supplier policy never yields a stock figure. No provider call exists in Phase 2, so a real time-out path is not exercised. |
| X12 | PASS (no-mirror guard) | as C04 | no local seat availability |

---

## 7. Build, tests, migrations, reconciliation

### 7.1 Build

```
dotnet build --no-incremental -v q --nologo
    13 Warning(s)
    0 Error(s)
```

The 13 warnings are the same file, position and code as on the base build (Framework and other services' contract files); none is in Phase 2 code.

### 7.2 Tests

```
dotnet test tests/AeroTech.Ancillary.Domain.ConformanceTests --no-build
Test Run Successful.  Total tests: 200   Passed: 200   Total time: 2.3 s

dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests --no-build
Test Run Successful.  Total tests: 153   Passed: 153   Total time: 6.08 min
```

| Suite | Discovered (`--list-tests`) | Executed | Passed | Failed | Skipped | Base `933b7b7` | Phase 2 tests |
|---|---|---|---|---|---|---|---|
| Domain conformance | 200 | 200 | 200 | 0 | 0 | 169 | 31 |
| Application acceptance | 153 | 153 | 153 | 0 | 0 | 112 | 39, plus 2 for the legacy cleanup |

No "aborted" or crash line in either output. This is the last full run of the day, after the legacy cleanup of Annex D; the runs before the validator correction and before the cleanup were also green (151 acceptance tests then). Acceptance tests run on SQL Server `localhost\SQLEXPRESS`, one database per run (`DotAirAncillary_Tests_<guid>`). The original M1 hold/confirm tests are part of these totals: 13 domain and 9 acceptance, all passed.

Red-first evidence: the Phase 2 domain tests were written before the model and failed to compile with 56 errors (CS0246 × 27, CS0234 × 20, CS0103 × 9); the starting gaps are in `reports/PHASE2-INITIAL-CONFORMANCE-GAPS.md`.

### 7.3 Migrations

| Context | Migration | Content |
|---|---|---|
| `AncillaryDbContext` | `20261008212718_V121Phase2StockCapacity` | `Up`: 8 `CreateTable`, 15 `CreateIndex`, nothing else. `Down`: 8 `DropTable`. |
| `AncillaryQueryDbContext` | `20261008212728_V121Phase2StockCapacityQuery` | `Up`: 8 `CreateTable`, 8 `CreateIndex`. `Down`: 8 `DropTable`. |

In these two Phase 2 migrations no existing table or column is altered, there is no `Sql(...)` statement and no data movement. Test AB `P2_X06` asserts this from the migration source. The two later cleanup migrations are destructive by order of the Owner and are described in Annex D.

```
ef migrations has-pending-model-changes --context AncillaryDbContext
No changes have been made to the model since the last migration.
ef migrations has-pending-model-changes --context AncillaryQueryDbContext
No changes have been made to the model since the last migration.
```

Round trip: AM `P2_X06_the_stock_capacity_migrations_are_reversible_and_leave_every_existing_row_untouched` builds a v12.1 database with seeded rows, migrates up, down and up again, and compares every existing row. It ran in the acceptance run above. It runs on a seeded test database, not on a clone of dev.

### 7.4 Dev database `DotAirAncillary`

Both migrations are applied (`ef migrations list` shows them last and not pending).

| Measure | Before Phase 2 | After |
|---|---|---|
| Tables in `Ancillary` + `ReadModel` | 85 | 101 (57 + 44); 93 after the legacy cleanup of Annex D |
| Definitions / provisions / pricings (command) | 31 / 99 / 69 | 31 / 99 / 69 |
| Definitions / provisions / pricings (read) | 31 / 99 / 69 | 31 / 99 / 69 |
| Inventory rows (all eight tables, both schemas) | — | 0 |

The "after" column was queried on 2026-10-09, after the HTTP smoke and its clean-up. The smoke created 4 policies, 1 usage limit, 4 sources and 2 adjustments in each schema; all were retired through the API and then deleted by SQL, so dev again holds 0 inventory rows and no policy for any definition. The row checksums of the three commercial tables were compared when the migrations were applied, earlier in this run, and were identical; they were not recomputed at closure.

### 7.5 Command / read reconciliation

The read model is written in the same unit of work call as the command store. Tests compare the two after every step: AP `P2_X05` for policies and usage limits, and a column-by-column difference query (`SourceDifferencesAsync`) in 9 flight and slot tests, covering totals, status, version, timestamps and adjustment counts. On dev, after the HTTP smoke and before its clean-up, both schemas held the same row counts and a join on id found 0 rows that differ in status, version, authority, total, kilograms or closed-for-sale.

---

## 8. Concurrency proof

All on real SQL Server, 100 parallel requests each, from the acceptance run of section 7.2.

| Scenario | Test | Asserted outcome |
|---|---|---|
| Same-version adjustment, flight count | AF `P2_X01_same_expected_version_capacity_adjustment_has_one_winner_sql` | 1 success, 99 × 16605, one adjustment row |
| Same-version adjustment, weight | AF `P2_W03_same_expected_version_weight_adjustment_has_one_winner_and_exact_precision_sql` | 1 success, 99 × 16605, exact three-decimal value |
| Same-version adjustment, slot | AS `P2_X01_same_expected_version_slot_adjustment_has_one_winner_sql` | 1 success, 99 × 16605 |
| Duplicate policy definition | AP `P2_X10_…` | 1 success, 99 × 16604 |
| Duplicate policy activation | AP `P2_X10_…` | 1 success, 99 × 16605; command and read row both Active at version 2 |
| Duplicate source definition, one key | AF `P2_F01_F03_F05_…` | 1 success, 99 × 16614 |
| Overlapping, non-identical slots of one facility (each shifted by one minute) | AS `P2_A04_concurrent_overlapping_facility_slot_creation_has_single_winner_sql` | 1 success, 99 × 16615; an overlap query on the stored rows returns 0 |
| Adjacent slots of one facility | same test | 100 success; 101 rows; 0 overlaps |
| Distinct flights | AF `P2_X03_…` | 100 success, no global lock |
| Ambiguous response, then retry | AF `P2_X01_a_lost_projection_is_healed_…` | command saved, read model lost; the same correlation and payload returns the first adjustment, writes no second row and repairs the read model |
| Same correlation, different payload | DS `P2_X01_a_replayed_correlation_…`; AF `P2_F06_F07_…` | 16616 |

Mechanisms:

- Version: `Version` is an EF concurrency token on each root, next to the framework `RowVersion`. The loser of a race is mapped to 16605 by the unit of work.
- Uniqueness: the four filtered unique indexes of section 2 and the unique `(parent, CorrelationId)` index of each adjustment table.
- Slot overlap: an interval overlap cannot be expressed as a unique index, so Define takes a session-scoped `sp_getapplock` on `Ancillary.AirportSlot.{owner}.{facility}`, checks overlap, saves and releases. The lock is requested without waiting and retried with a short random delay for up to 20 seconds, releasing the connection between attempts; after that the caller gets 16619. Different facilities do not contend.

One defect was found and fixed during this phase: the first lock version waited inside SQL Server while holding a pooled connection and exhausted the pool under 100 parallel requests (1 of 151 acceptance tests failed). The non-blocking version above replaced it; the slot test class then passed in three consecutive runs and in the full run.

---

## 9. Read models and HTTP

Routes: 36 new Backoffice v1 operations (confirmed in the generated OpenAPI document of the running host; the boundary test pins 93 routes in total).

| Route family | Operations |
|---|---|
| `Backoffice/v1/AncillaryInventoryPolicies` | POST; PUT `{policyId}`; POST `{policyId}/Activate`, `/Suspend`, `/Retire`; GET `Paginated`, `ByServiceIdentity`, `{policyId}` |
| `Backoffice/v1/AncillaryInventoryConfigurationSnapshots` | GET `?serviceDefinitionRef&flightId&atUtc` |
| `Backoffice/v1/FlightCountInventories`, `FlightWeightInventories`, `AirportSlotInventories` | POST; POST `{inventoryId}/Activate`, `/Adjust`, `/CloseForSale`, `/OpenForSale`, `/Suspend`, `/Retire`; GET `Paginated`, `{inventoryId}` |

Snapshot contract: `IsGuaranteed` is always `false` and `StaleAfter` is always null.

Evaluated in this order:

| Situation | State | Reason code |
|---|---|---|
| No policy | NotConfigured | PolicyNotConfigured |
| Draft policy of pattern 5–7 | UnsupportedPattern | PatternNotSupported |
| Any other Draft policy | NotConfigured | PolicyNotActive |
| Unlimited, Supplier or FlightFlow policy, Suspended | ClosedForSale | PolicySuspended |
| Active Unlimited | Unlimited | none |
| Active Supplier or FlightFlow | DelegatedCheckRequired | DelegatedSourceNotConnected |
| Local flight pattern without `flightId` | Unknown | FlightRequired |
| Local slot pattern without `atUtc` | Unknown | InstantRequired |
| Local, a bound source missing or Draft | NotConfigured | SourceNotConfigured |
| Local, policy Suspended | ClosedForSale | PolicySuspended |
| Local, source closed for sale or Suspended | ClosedForSale | SourceClosedForSale |
| Local, source Active and open | ConfiguredNotGuaranteed | NoAllocationLedger |

### HTTP smoke

Host built from this tree, database `DotAirAncillary`, port 5299, stopped afterwards. Authorized calls used the Owner's Backoffice token (airline context, `pss.backoffice`). 76 requests, **76 as expected**: 200 × 42, 400 × 1, 401 × 1, 403 × 2, 404 × 3, 409 × 16, 422 × 11.

| Status | Business code | Request |
|---|---|---|
| 401 | — | no token. Earlier the same day: 9 inventory routes without a token and 3 with an expired token, 401 each. |
| 403 | 16610 | Define policy and Define flight count source with `ownerAirlineId` of another airline |
| 404 | 16601, 16611 | unknown policy id; unknown flight count id; unknown slot id |
| 409 | 16604 | second policy for the same service identity |
| 409 | 16605 | Change of a policy and Adjust of a source with a stale version |
| 409 | 16603 | Change of an Active policy; Retire of a Retired policy |
| 409 | 16606 | Activate `Unlimited` on SEAT_SELECTION (must check availability); Activate Supplier with a key that is not the supplier's |
| 409 | 16607 | Activate a DailyCount policy |
| 409 | 16608 | Activate a Local FlightCount policy, a FlightFlow policy, a policy with a usage limit, a flight count source and a slot — each needs a reference source that is not connected |
| 409 | 16614, 16615, 16616 | second source for one key; overlapping slot; same correlation with another payload |
| 422 | 16617 | definition id of another service; unknown definition id |
| 422 | 16602 | zero count per accepted unit; zero usage limit |
| 422 | 16612 | negative total at Define and at Adjust; kilograms with four decimals; instant without offset; end before start; negative slot capacity |
| 422 | 16609 | airport that does not exist |
| 400 | — | empty body (framework model binding) |
| 200 | — | define, change, activate, suspend, reactivate, retire; adjust 4 → 3 and 100.500 → 80.250 kg; replay of the same correlation returning the same adjustment; close for sale; adjacent slot; reads and lists |

Snapshots over HTTP, `isGuaranteed` false in every one: no policy → `NotConfigured`; Active Unlimited → `Unlimited`; Suspended → `ClosedForSale` / `PolicySuspended`; Active Supplier → `DelegatedCheckRequired`; Local Draft → `NotConfigured` / `PolicyNotActive`; DailyCount Draft → `UnsupportedPattern`; after Retire → `NotConfigured`.

Audit columns: the adjustment rows carry the airline user id of the token as `ActorId`; it was never sent in a request.

Defect found by the first smoke run (69 of 71 as expected) and corrected: the Backoffice validators rejected business numbers — a negative total, a fourth decimal — with 400 before the domain was reached. Phase 1 validators check shape only (required, length, enum, id > 0) and leave business numbers to the domain, and scenario X02 expects domain errors. Twelve numeric rules were removed from seven Phase 2 validator files (`TotalCapacity`, `NewTotal` × 2, `CapacityKg`, `NewKg`, `CapacityPersons`, `EndUtc > StartUtc`, `CountPerAcceptedUnit`, `FixedKgPerUnit`, `OccupancyMinutes`, `PeoplePerAcceptedUnit`, `MaxUnits`); the domain already enforced every one of them. Test AP `P2_X02_the_backoffice_validators_check_the_shape_of_every_inventory_input` now pins that these values pass the validator. The build and both suites were re-run after the change (section 7), then the smoke was repeated on the new build with five more numeric cases.

Not covered over HTTP:

- 403 for a caller that is not an airline user — needs a token of another context type. Proven at application level by AP `P2_X02_inventory_is_managed_only_by_an_airline_caller_of_the_operator_and_never_across_owners`.
- Reading a row of another owner — an environment has one operator, so no such row can exist there. Proven at application level by the same test.
- 16613, 16618 and 16619, and every path of an Active local source — not reachable while the reference sources are not connected. Proven at application level.

Data used: flight, resource and facility ids were synthetic placeholders, because no source of truth exists for them; definition ids, the airport (IKA) and the supplier key were read from the dev database. Every row the smoke created was retired through the API and then deleted by SQL from both schemas (section 7.4).

---

## 10. No change outside the allowed surface

| Area | Evidence | Result |
|---|---|---|
| `**/AncillaryReservationAggregate/**`, M1 reservation tests (36 files) | `git status` lists none of them; AB `P2_X07` recomputes the combined SHA-256 `6974ABA2C62BAFE7CDE159BDCE8A4969F1CAECCF25368012F30D1BE7DDA4E486` | unchanged |
| `AncillaryReservationUnit.StockPoolId` | asserted present and referenced nowhere else | unchanged |
| `Framework/`, `src/AeroTech.Ancillary.ReferenceData/`, contract folders other than `Ancillary/` | `git diff --stat 933b7b7` and `git status` on these paths are empty | unchanged |
| Inventory code vocabulary | AB `P2_X07`: no `Hold`, `Confirm`, `Release`, `Expire`, `ReservedQuantity`, `Decrement`, `SoldOut`, `AvailableQuantity`, `StockPool`, `ScopeKind`, `ScopeReference`, `ScopeValue`, `HttpClient`, `JsonSerializer`, `JsonDocument`, `HasConversion` in more than 250 inventory files | none found |
| Aggregate shape | AB `P2_R03_P05`: exactly 4 inventory aggregates and 4 repositories, no abstract inventory base, no generic or catch-all type | as approved |
| Other repositories (read-only `git status`) | FlightFlow `d2180b2e`, AirPrice `1b41f08`, AeroTech.JetPay `b501b89`, JetPay `df8ea9d`, AirInfo `4a41575`: clean. AeroTech.Ordering.Final `cd50a2a`: 2 changed paths, newest dated 2026-09-28. AirAvail `fix/offerId@318f7be`: 48 changed paths, newest dated 2026-10-07 18:02. | nothing changed after Phase 2 began; this run issued no write command outside this repository |

---

## 11. Provider and source capability matrix

| Authority / pattern | Local capacity kept by Ancillary | Built in Phase 2 | Can be Active in a deployed environment today | Open for Phase 3 or evidence |
|---|---|---|---|---|
| Unlimited | none | policy | Yes (unless a provision must check availability) | — |
| Supplier | none | policy with provider key | Yes | provider availability and reservation calls; stale and time-out handling |
| FlightFlow | none | policy; no adapter, no mirror | No (E4) | delegation contract. A paid seat assignment is FlightFlow's own operation and is **not** equated with a flight ticket hold; Ancillary keeps no seat counter. |
| Local FlightCount | total per flight and resource | policy, source, adjustments | No (E1) | flight and resource source; allocation ledger |
| Local FlightWeight | kg per flight and resource | policy, source, adjustments | No (E1) | same; commercial quota only, never a load-control approval |
| Local FlightCountPlusWeight | one of each | policy with two bindings | No (E1) | atomic two-resource allocation |
| Local AirportSlot | persons per facility and UTC interval | policy, source, adjustments, overlap protection | No (E2) | facility and time-zone source; atomic multi-slot occupancy |
| Local DailyCount | — | enum member only; activation refused | No (E6) | documented only: owner, service identity, date bucket. Needs provider evidence. |
| Local RoomNight | — | enum member only; activation refused | No (E6) | documented only: property, room type, night; rate plans share one stock. Needs a signed local allotment. |
| Local AssignedAsset | — | enum member only; activation refused | No (E6) | documented only: named asset, exclusive interval. Needs operator evidence. |
| Passenger usage limit | none (a rule, not a counter) | policy child and identity key | No (E5) | counting-family registry; cross-order usage ledger |

---

## 12. Verdict

**`PHASE2_READY_FOR_OWNER_AUDIT`**

**`PRODUCTION_BOOKING_WITH_LIMITED_STOCK=NO_GO`** — no allocation ledger, hold, confirm, release or cut-over exists; a snapshot never guarantees a sale. Phase 3 was not started and will not start without the Owner's gate.

| Gate | State |
|---|---|
| Clean build | PASS — 0 errors |
| Full Phase 1 suites | PASS — included in 200 / 200 and 153 / 153 |
| Phase 2 tests on SQL Server | PASS — 31 domain, 39 acceptance |
| Pending model changes, both contexts | none |
| Additive migrations, reversible | PASS |
| 62 scenario ids accounted for | 50 PASS, 12 deferred with class and reason |
| SQL concurrency | PASS — section 8 |
| Frozen reservation code, other repositories | unchanged |
| No hold / confirm / release / decrement / stock pool / EAV / JSON list / fake adapter | PASS |
| Authorized HTTP smoke | PASS — 76 of 76; 403 for a non-airline caller not coverable with the available token |
| Commit | not made — needs the Owner's approval |

---

## Annex A — Decision register

Decisions taken where the pack leaves a choice open. Each can be reversed at audit.

| # | Decision | Reason |
|---|---|---|
| A1 | Inventory commands and queries require an airline caller, and the owner is the operator's home airline; an `ownerAirlineId` in a request must equal it and is never trusted on its own | the token has no airline claim; this is the only verified owner available |
| A2 | Five reference ports are `NotConnected` stubs that answer `SourceUnavailable` | no source of truth exists; a fake adapter is banned; fail-closed |
| A3 | One non-Retired row per policy identity and per physical key, Draft included | stricter than "one Active"; prevents two competing drafts |
| A4 | Sources have no edit command; `Activate` also reactivates a Suspended record | the key is the identity of a source; fewer routes |
| A5 | An adjustment that does not change the total is refused (16612) | an audit line must record a change |
| A6 | Adjustment rows also store `ExpectedVersion` and `ResultingVersion` | needed to answer an idempotent replay with the original result |
| A7 | A replay with the same correlation and payload returns the first adjustment and writes the read model again; a different payload is 16616 | heals a lost projection without a second ledger row |
| A8 | Slots: UTC offset zero, whole minutes, start before end, at most 24 hours; REST accepts an instant only with an explicit offset or `Z` | no silent time-zone guess (A06) |
| A9 | Slot overlap is guarded by a facility-scoped application lock with a 20-second limit (16619) | an interval overlap cannot be a unique index |
| A10 | `Unlimited` is refused when an Active provision of the identity must check availability | the two statements contradict each other |
| A11 | A Draft policy of pattern 5–7 may be stored and reads `UnsupportedPattern`; it can never be activated | records intent without inventing a schema |
| A12 | The unit alignment rules of section 2.6 | C07 gives the principle, not the table |
| A13 | The policy keeps `ServiceDefinitionRef` as identity, as the pack states, and adds `ServiceDefinitionId` as a foreign key | the standing rule is that aggregates refer to each other by id; both are kept |
| A14 | `InventoryCountUnit` holds only the five units catalog 03 lists for a flight count source; `Session`, listed for the deferred daily source, was not added | no enum member for a model that is not built |
| A15 | Backoffice validators check shape only (required, length, enum, id and version > 0); business numbers are refused by the domain with 422 and a business code | the Phase 1 validators work this way and X02 expects domain errors; corrected after the HTTP smoke (section 9) |

## Annex B — Needs the Owner

1. **Commit and push** of Phase 2 — nothing is committed. The corrected Phase 1 report is also still uncommitted.
2. **Reference sources** for flight occurrence, count / weight resource, airport facility with time zone, FlightFlow delegation and counting family. Until they exist, exceptions E1–E5 stand.
3. **Policy mapping of the 31 definitions.** None is recorded. Only the Owner can state the authority of each.
4. Confirmation or reversal of the decisions in Annex A, in particular A1, A3, A13 and A15.
5. A token of a non-airline context, only if 403 for such a caller must also be shown over HTTP.
6. `Ancillary.ProvisionRuleMigrationAudit`: keep it as the audit trail of the v12.1 migration, or drop it too (Annex D).
7. AirAvail: its uncommitted M1 copy reads read-model objects that the cleanup removed (Annex D).

## Annex C — Known limitations

- The command store and the read model are saved by two contexts in sequence, not in one transaction (inherited unit of work). A lost projection of an adjustment is repaired by replaying the same correlation; other commands have no repair path yet.
- The count unit of a resource is kept consistent across flights by an application check at Define and Activate, not by the database, because no resource registry exists.
- No command re-points `ServiceDefinitionId` after a commercial revision creates a new definition version.
- A `PerServiceDate` usage limit does not check the definition's `ServiceDateBasis`; the service date is resolved in Phase 3.
- The migration round trip is proven on a seeded test database, not on a clone of the dev database.

## Annex D — Legacy schema cleanup (ordered by the Owner on 2026-10-09)

This is not part of the Phase 2 scope. Pack document 06 calls it "a separate explicit owner-approved deletion operation"; the Owner ordered it in this conversation after the Phase 2 work was reported. It is delivered as its own two migrations so that it can be reviewed, kept or reverted separately.

| Context | Migration | `Up` | `Down` |
|---|---|---|---|
| `AncillaryDbContext` | `20261009092108_V121LegacySchemaCleanup` | drops 4 tables and 39 columns of `Ancillary.AncillaryProvisions` | re-creates them with the same types, defaults, keys and indexes, empty |
| `AncillaryQueryDbContext` | `20261009092117_V121LegacySchemaCleanupQuery` | drops 4 tables and 27 columns of `ReadModel.AncillaryProvisions` | same |

What was removed:

- `Ancillary.AncillaryProvisions`: the v11 list and scalar columns that v12 and v12.1 replaced with typed child tables — `PassengerTypeCodes`, `CustomerTypes`, `CustomerIds`, `PointOfSaleIds`, the airport, airline, flight, aircraft, cabin, fare, RBD and seat lists, `DaysOfWeek`, `TimeFrom`, `TimeTo`, `TravelFrom`, `TravelTo`, `FeeCurrencyId`, `FeeApplicationUnit`, `SalesEffectiveFrom`, `SalesDiscontinueAt`, `AdvancePurchase*` and `Baggage*`.
- `ReadModel.AncillaryProvisions`: the same list columns, `DaysOfWeek`, the time and travel columns and `FeeCurrencyId`.
- Tables: `Ancillary.ProvisionPriceLines`, `ProvisionTravelDates`, `ProvisionSeasonalPeriods`, `ProvisionDayTimeRestrictions` and their four `ReadModel.AncillaryProvision…` counterparts.

How the list was established: a scratch database was created from the three EF models with `ef dbcontext script` and compared column by column (name, type, length, precision, nullability) with `DotAirAncillary`. Before the cleanup, 123 columns existed only in the dev database and none only in the model. No index, key or check constraint depended on a dropped column.

Result on `DotAirAncillary` (backup taken first: `DotAirAncillary_before_legacy_cleanup.bak`):

| Measure | Before | After |
|---|---|---|
| Tables in `Ancillary` + `ReadModel` | 101 | 93 |
| Columns that exist in the database but not in the model | 123 | 7 |
| Columns that exist in the model but not in the database | 0 | 0 |
| Definitions / provisions / pricings / passenger-type rows / read provisions | 31 / 99 / 69 / 43 / 99 | 31 / 99 / 69 / 43 / 99 |

**The 7 remaining columns are one table, `Ancillary.ProvisionRuleMigrationAudit`.** It is not a leftover of an earlier pack: the v12.1 rule-group migration creates it as its audit trail, and document 06 asks for that trail. It was left in place and needs the Owner's word before it is dropped (9 rows on dev). `ReferenceData` has no difference.

Tests:

- New, acceptance on SQL Server: `Migration.V121LegacySchemaCleanupAcceptanceTests`
  - `V121_M05_the_approved_cleanup_drops_only_the_superseded_columns_and_tables_and_leaves_the_database_exactly_as_the_model` — every stored column of every table equals the EF model, apart from the audit table; the mapped columns of every current table have the same row count and checksum before and after.
  - `V121_M12_the_cleanup_is_reversible_in_shape_while_the_dropped_values_are_gone_for_good` — `Down` restores columns, defaults, indexes and keys exactly; the old values do not come back.
- Changed: the Phase 1 and Phase 2 migration tests that read the superseded columns now migrate to the last migration before the cleanup instead of the latest one (`TestDatabase.InitializeAsync(command, query)`); `V121_M05_weights_and_amounts_…` and `V121_B_an_unrestricted_provision_…` now assert that the superseded columns and tables are gone.
- Suites after the cleanup: domain 200 / 200, acceptance 153 / 153; no pending model changes in either context.

Consequences to know:

- The dropped values cannot be restored by `Down`; only the backup holds them.
- A database that is migrated down past the cleanup and up again re-runs the v12 and v12.1 data conversions on empty legacy columns. Do not take a database with data below the cleanup migration.
- The uncommitted AirAvail M1 working copy reads `ReadModel.AncillaryProvisions.FeeCurrencyId` / `FeeApplicationUnit` and `ReadModel.AncillaryProvisionPriceLines` by SQL. Those objects no longer exist in `DotAirAncillary`. AirAvail was not touched.
