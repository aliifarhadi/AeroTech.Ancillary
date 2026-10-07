# v9.1 Milestone 1 — Walking Ancillary — Implementation Report

**Status: PARTIAL — STOPPED BY OWNER INSTRUCTION.** During the run the owner ordered "به سرویس Ordering فعلا دست نزن". M1.1–M1.4 (Ancillary + AirAvail) are built, proven and tested; M1.5 (Ordering acceptance + provider orchestration) was **not started** and the Ordering repository is untouched. The exit marker `ANCILLARY_V91_M1_WALKING_SLICE_READY` is therefore **not emitted**: exit proofs 4–7 (accept in Ordering, reserve, confirm through Ordering, idempotent replay through Ordering) cannot run without M1.5.

## 1. Repositories and revisions

| Repository | Checkout | Working tree |
|---|---|---|
| AeroTech.Ancillary | `e9c3ad4` (bootstrap `843cf7c` + v9.1 pack commit) | M1 changes, uncommitted |
| AirAvail AirOffer | `318f7be` (tree identical to pin `8c6f5d1`; verified `git diff 8c6f5d1 HEAD` empty) | M1 changes, uncommitted |
| Ordering.Final | `cd50a2a` (= pin) | **untouched** (only the two pre-existing untracked docs) |
| AirPrice `1b41f08`, FlightFlow `d2180b2` | = pins | untouched |

Nothing was committed or pushed.

## 2. What was built

### 2.1 AeroTech.Ancillary (M1.1–M1.3, Service-Hold surface)

**Contracts** — `Contracts/AeroTech.Messages/Ancillary/Enums/`: 17 v9.1 enums (SupplierFulfillmentKind, SupplierStatus, ServiceSubCodeSource, ServiceDefinitionStatus, AncillaryDocumentType [None=1, EmdAssociated=2, EmdStandalone=3], BookingMethod, ProvisionStatus, ServiceCoverageScope, AncillaryQuantityUnit [Each=1, Piece=2, Kilogram=3], ProvisionApplicationType [M1: Standard=1 only], CommercialDisposition, FeeApplicationUnit [all 10 members], AncillaryPriceLineCategory, ReissueRefundPolicy, FormOfRefund, AncillaryReservationStatus, AncillaryReservationUnitStatus). Explicit numbers in 02-document order, Ordering one-line `[Display]` style.

**Domain** — four aggregates exactly per 02:
- `SupplierAggregate/Supplier` — Id/OwnerAirlineId/Name(100)/FulfillmentKind/FulfillmentProviderKey(50, required iff External, null iff Local, no whitespace)/Status/CreatedAt/RetiredAt. M1 operation: `Register` only.
- `AncillaryServiceDefinitionAggregate/AncillaryServiceDefinition` + `DocumentDefinition`/`BookingDefinition` value objects + read-only in-code `IndustryServiceSubCodeReference` with the two donor-backed fixtures (0BX → F/E/LG/EmdStandalone, 0CC → C/C/BG/B1/EmdAssociated). Industry define copies classification from the reference and rejects contradictions (type/group/document-type/RFIC/RFISC); CarrierDefined supplies its own classification. `Define` (Draft, Version = max existing + 1), `Activate` (Draft only, supplier must be Active and match).
- `AncillaryProvisionAggregate/AncillaryProvision` + QuantityRule/CommercialOutcome/FeeDefinition/SettlementDefinition/AvailabilityDefinition/FulfillmentDefinition value objects + `ProvisionPriceLine` child entity. M1 shape has **no criteria fields** (07: "all applicability criteria" excluded from M1; they are CORE for M3, not dead fields now). Paid ⇔ fee + ≥1 line; lines > 0, exactly 2 decimals; `Activate` gates FeeApplicationUnit to the five fixed-amount units (per-kg/percent units refuse with 16305 until their calculation exists — G09A); `Supersede` retires the prior active row of the same ServiceDefinitionId+Sequence atomically inside activation.
- `AncillaryReservationAggregate/AncillaryReservation` + `AncillaryReservationUnit` — exactly the 02 §28 shape: **no currency, no revenue, no price fields anywhere**. Hold validates fulfillment facts only (idempotency, distinct OrderServiceIds, occurrence identity §30, quantity vs provision rule, coverage identity, definition/provision relationship, supplier routing by typed kind). `AncillaryHoldChecks.Accept` resolves the supplier from the definition (never from the caller — N04) and routes by FulfillmentKind: Local passes; External fails deterministically with 16104 (adapter registry is M2). Confirm: all-Held → Confirmed, replay-safe, expired → 16407; expiry is a view rule (`EffectiveStatus`/`StatusOf`), `ExpiresAt = RequestedExpiresAt` (nullable = no expiry).

**Application** — one folder per use case, per-surface thin command/handler/validator, snapshot projection factories: RegisterSupplier (Backoffice), DefineAncillaryServiceDefinition / ActivateAncillaryServiceDefinition (Backoffice), DefineAncillaryProvision / ActivateAncillaryProvision (Backoffice), HoldAncillaryServices / ConfirmAncillaryHold (Service). Hold uses the donor idempotency pattern: lookup → replay with `EnsureSameContent`, and on any post-lookup failure a winner re-read + replay (race loser never leaks a transient failure).

**Persistence** (`Ancillary` schema) — Suppliers, AncillaryServiceDefinitions (+ unique (OwnerAirlineId, Ref, Version); **filtered unique** (OwnerAirlineId, Ref) WHERE Status=Active), AncillaryProvisions (+ **filtered unique** (ServiceDefinitionId, Sequence) WHERE Status=Active), ProvisionPriceLines, AncillaryReservations (unique IdempotencyKey) + AncillaryReservationUnits. OrderServiceId live-uniqueness is an application check (a filtered index cannot see the root's ExpiresAt); a true race on it resolves through the idempotency re-read or the framework default.

**Query/Synchronizer** (`ReadModel` schema) — Suppliers, AncillaryServiceDefinitions (denormalized SupplierName), AncillaryProvisions, AncillaryProvisionPriceLines; every projection stamps `LastUpdateTime` from `IClock` (AirAvail stamp-probe pattern). GetById queries for Supplier / ServiceDefinition / Provision (Backoffice, EnumValueDto) and GetAncillaryHoldById (Service, via repository + clock, plain enum names).

**RestApi** —
```
POST  Backoffice/v1/Suppliers                                   GET Backoffice/v1/Suppliers/{id}
POST  Backoffice/v1/AncillaryServiceDefinitions                 GET .../{id}        POST .../{id}/Activate
POST  Backoffice/v1/AncillaryProvisions                         GET .../{id}        POST .../{id}/Activate
POST  Service/v1/Ancillaries/Service-Holds                      GET .../{holdId}    POST .../{holdId}/Confirmations
```
Backoffice carries `[Authorize(SurfaceAuthorization.Backoffice)]`; Service surface is anonymous (standing owner decision). No Paginated/Rename/Retire/Revise/Suspend endpoints (03's full set is M3; M1.2 limits to register/detail, define/detail/activate). No Release/Cancel (M2). No Simulate, no Issuance/Activation endpoint (L05, O01–O03).

**Error codes (new, within 16000–16999; the pack does not freeze codes, so these were assigned here):** 16101/16102/16104 supplier; 16201–16208 service definition; 16301–16305 provision; 16401–16410 hold (not-found 404, invalid 422, state 409, idempotency-reuse 409 — full map in `_Shared/Resources/ExceptionFactory.cs`).

### 2.2 AirAvail (M1.3 consumption + M1.4 shopping)

- `Contracts/AeroTech.Messages/Ancillary/Enums/` replaced with the same 17 v9.1 enums; 8 obsolete enums (ProductType, SalesScope, InventoryControl, WeightUnit, Product/PriceRule statuses, old reservation/subcode enums) deleted.
- Persistence: `GetAncillaryCatalogSnapshot.sql` now reads `ReadModel.Suppliers` (Status=Active) + `ReadModel.AncillaryServiceDefinitions` (Status=Active) + `ReadModel.AncillaryProvisions` (Status=Active, joined to active definitions) + `ReadModel.AncillaryProvisionPriceLines`; `GetAncillaryCatalogStamp.sql` unions the three stamped tables. New db models in `AncillaryCatalogQueryResults`. **`Supplier.FulfillmentProviderKey` is never selected** — it stays inside Ancillary (provider-key boundary).
- `AncillaryCatalogSnapshot` reshaped to ServiceDefinitions (with SupplierId/SupplierName) + Provisions (+ lines); `AncillaryCatalogCache.Build` keeps only active rows of active suppliers and drops rows with unknown enum numbers.
- `AncillaryOfferEvaluator` rewritten to the v9.1 matching rule: definition effective at AsOf (DateOnly window) → provisions of the definition ordered by Sequence → first match wins (scope + sales window + priceable); **NotAvailable winner emits no item** (D05/J07); occurrences: Journey per traveller×bound, Sector per traveller×flight (Portion/Order emit no pre-order item in M1); existing-quantity reduction keyed by numeric `ServiceDefinitionId`.
- `AncillaryOfferPricing`: converts the provision's filed `FeeCurrencyId` amounts with the existing `IMonetaryNormalizationService`/ROE path; filed evidence renamed `FiledUnitAmount`/`FiledAmount`; Free disposition prices as zero lines ([]).
- `AncillaryOfferIdCodec` item id now carries ServiceDefinitionId + ProvisionId (same wire format).
- S2S `AncillaryOffers` response: ServiceDefinitionList entries carry OwnerAirlineId, **SupplierName**, CommercialName, RFIC/RFISC, classification (incl. ServiceSubCode + SubCodeSource), Booking (method/SSR/SSIM), DocumentType; QuantityRule gained `Unit`. `Existing[]` request entries now carry `serviceDefinitionId` (long) instead of `productRef`.
- S2S `AncillaryOffers/Details` item now returns exactly what Ordering must snapshot (M01/U07): **`ServiceDefinitionId`, `ProvisionId`, `SupplierId`** + SupplierName, Ref/Version (display), classification/document/booking/**settlement**, `MustCheckAvailability`, **`FulfillmentProviderKey`** (the Ordering-level key from the provision, "Ancillary"), converted price lines + `FiledCurrencyId`/`FiledUnitTotal`/`FiledTotal` + RatesOfExchange.

### 2.3 Ordering — intentionally untouched (owner instruction mid-run).

## 3. Migrations

| Context | Migration | Applied to |
|---|---|---|
| AncillaryDbContext | `InitialAncillary` (Suppliers, AncillaryServiceDefinitions, AncillaryProvisions, ProvisionPriceLines, AncillaryReservations, AncillaryReservationUnits + outbox/inbox) | `DotAirAncillary` (was empty after the owner's manual rebuild) |
| AncillaryQueryDbContext | `InitialAncillaryQuery` (4 ReadModel tables) | `DotAirAncillary` |
| ReferenceDbContext | existing module migrations | `DotAirAncillary` (ReferenceData tables restored) |

Applied via the side-build + `ef.dll` route; the owner's ServiceHost bin was never touched. AirAvail/Ordering have no schema changes.

## 4. Tests — all runs green, executed = discovered, no aborted/crashed lines

| Suite | Result |
|---|---|
| Ancillary Domain.ConformanceTests | **36/36 passed** (discovered 36) |
| Ancillary Application.AcceptanceTests (real SQL Server, per-run database, fixed clock, sequential ids) | **23/23 passed** (discovered 23) |
| AirAvail AeroTech.AirOffer.Services.Tests (whole project incl. untouched flight-offer tests) | **86/86 passed** (discovered 86) |

Builds: `dotnet build AeroTech.Ancillary.sln --no-incremental` → 0 errors, 13 warnings (identical pre-existing Framework/Contracts set); AirAvail solution → 0 errors, 212 warnings (pre-existing; none in ancillary files).

### Scenario coverage (M1 minimum map of 06 §V)

| Scenario | Covered by |
|---|---|
| A01 | `M1_A01_*` (conformance ×4, acceptance ×2) |
| A06 | `M1_A06_two_suppliers…`, `M1_A06_C09_the_service_definition_carries…` (AirAvail), `M1_U01_A06_the_published_rows…` |
| A10 | `M1_A10_a_definition_for_a_missing_supplier_is_refused` |
| A12 | `M1_N04_A12_the_supplier_comes_from_the_definition…`, `M1_A12_A14_an_external_supplier_fails…` |
| A14 (neg) | same two + `M1_A14_no_source_line_switches_on_a_supplier_id` |
| A15 | `M1_A15_*` (conformance + AirAvail) |
| C01, C02 | `M1_C01_*` ×6, `M1_C01_C02_*`, `M1_C01_C04_*` |
| C04 | `M1_C04_*` ×3 |
| C08 (neg), C09 | `M1_C09_M05_the_ref_and_version_stay_for_display…`, `M1_C08_M05_a_reservation_unit_identifies…` |
| C10 (neg) | one definition aggregate for lounge/bag fixtures; `M1_C10_the_same_reference_may_be_active_for_two_airlines` |
| D01 | `M1_D01_*` ×4 |
| D02 | `M1_D02_*` ×5 (incl. filtered-index proof + AirAvail sequence first-match + FX pass-over) |
| D05 | `M1_D05_a_not_available_winner_suppresses…` |
| I01 | `M1_I01_*` (domain 2-decimal rule + AirAvail conversion with filed evidence) |
| I02 | `M1_I02_a_negative_price_line_is_refused` |
| I06–I09 (neg) | `M1_I06_I07_I08_I09_the_reservation_carries_no_money…`, `M1_U08_the_hold_request_shape_carries_no_money…`; no FX code exists in Ancillary |
| J01, J03, J05, J06, J07 | AirAvail `M1_J01_*` ×2, `M1_J03_*` ×9, `M1_J05_*` ×4, `M1_J06_*` ×2, `M1_J07_*` ×2 |
| L01 | the AirAvail evaluator tests themselves (only evaluator) |
| L05, L06 (neg) | `M1_O01_O02_L05_no_issuance_activation_or_simulate_surface_exists`, `M1_L06_the_ancillary_domain_hosts_no_offer_evaluator` |
| M01–M04 | `M1_M01_U07_the_selected_item_returns_the_exact_numeric_identities…`, `M1_M01_the_detail_carries_the_rates…`; M02/M03/M04 are Ordering-side → **pending M1.5** |
| M05, M06 (neg) | `M1_C08_M05_*`, `M1_D02_M06_activation_supersedes…` (old row superseded, never mutated; new Id) |
| N01–N06, N09, N11 | `M1_N01_*` ×2, `M1_N02_*` ×2, `M1_N05_*` ×2, `M1_N04_*`, `M1_N06_*` ×2, `M1_N09_*` ×2, `M1_N11_*` ×4 |
| O01–O03 (neg) | endpoint-scan test + no issuance code exists; EMD numbers nowhere in Ancillary |
| U01 | `M1_U01_*` ×3 (cache) + `M1_U01_A06_the_published_rows…` + live smoke below |
| U07, U08 | Details identity test (U07); hold shape + trust-boundary tests (U08); full cross-service run **pending M1.5** |

## 5. Live proof (performed this session)

1. All three contexts migrated onto the freshly rebuilt empty `DotAirAncillary` → 27 tables (Ancillary/ReadModel/ReferenceData/dbo).
2. Ancillary ServiceHost started from a side build (owner's bin untouched): `GET /api/v1/Ping` → 200 `{"status":"ok"}`; `GET Backoffice/v1/Suppliers/{id}` without token → 401; `POST Service/v1/Ancillaries/Service-Holds` with an unknown definition → 422 `{"code":16404}`.
3. AirAvail ServiceHost started from a side build; `POST Service/v1/AncillaryOffers` (currencyId 70, airline 1, real airport ids 2→6) → 200 with an empty a-la-carte offer read **live from the new ReadModel tables** through the rewritten snapshot/stamp SQL.
4. Both processes I started were stopped by their PIDs afterwards.

**Live authoring (walking Supplier → Definition → Provision → AirAvail items → Hold → Confirm over HTTP) was not run: the stored Backoffice token expired 2026-10-03 and minting one is not mine to do.** The same chain is fully proven end-to-end inside the acceptance tests against a real SQL Server database.

## 6. Decisions taken where the pack is silent (for audit)

1. **Error codes** assigned inside 16000–16999 as listed above (pack freezes no code table).
2. `ProvisionApplicationType` currently has only `Standard` (adding Baggage/Seat members now would be dead values; they arrive with their milestones).
3. `Define` assigns `Version = max(existing)+1` per (OwnerAirlineId, Ref) so the all-versions unique index holds and re-defining a reference is possible before Revise exists (M3).
4. Industry reference entries carry `ServiceTypeCode` (02 requires it; donor fixtures lacked it): 0CC → `C` (baggage charges), 0BX → `F` (flight-related lounge), per 02 §6 CORE examples.
5. Hold unit coverage rule: non-Order scopes require TravellerId and ≥1 flight (Sector exactly 1); Order scope requires null TravellerId (derived from 02 §28/§30 identity rules).
6. M1 evaluator emits Journey and Sector occurrences only; Portion/Order-scoped provisions produce no pre-order item yet (no portion construct in the M1 Offer context).
7. Confirm returns the hold body (200) so the future Ordering provider can read ProviderUnitRef evidence without read-back.
8. M1.4 was implemented on the S2S surface only (the surface Ordering consumes); the Backoffice simulation endpoint of 03 §7 has no M1 scenario row and is left to a later milestone.
9. OrderServiceId live-uniqueness is application-checked (not indexable across root expiry); races fall back to idempotency replay / framework default.

## 7. Gaps / blockers

- **M1.5 + exit proofs 4–7 + marker: on hold by owner instruction** ("don't touch Ordering for now"). Everything Ordering needs (numeric-id Details, Hold/Read/Confirm endpoints) is live and tested on this side.
- **Industry production dataset**: M1 ships the two verified fixtures only — data-provisioning gap per 07/08, reference API is M3.
- **Live HTTP authoring proof**: needs a fresh Backoffice token from the owner.
- AirAvail checkout note: branch tip `318f7be` is tree-identical to pin `8c6f5d1` (merge commit); verified by empty diff.

No material source-vs-pack conflict required `REPORT_GAP_AND_STOP`.

**Exit marker: NOT EMITTED (M1 incomplete by owner's hold on Ordering).**

## 8. Addendum (after commit 2768fc8) — Backoffice Paginated endpoints

Added after the owner noted the lists were missing; shape copied from Ordering `GetOrdersPaginated` (`PaginationQuery` → `GridData<Row>`, `[Grid]` columns, row mapping in the shared `<Agg>Mapper`, fixed ordering, `SortBy` ignored as in Ordering, no query validator as in Ordering).

| Route | Filters (03) | Order |
|---|---|---|
| `GET Backoffice/v1/Suppliers/Paginated` | OwnerAirlineId, FulfillmentKind, Status, Search (name) | Name, Id |
| `GET Backoffice/v1/AncillaryServiceDefinitions/Paginated` | OwnerAirlineId, SupplierId, ServiceDefinitionRef, ServiceSubCode, ServiceTypeCode, GroupCode, Status, Search (ref/name) | Ref, Version desc, Id |
| `GET Backoffice/v1/AncillaryProvisions/Paginated` | ServiceDefinitionId (required; absent → empty page), SupplierId (join), Status, Sequence, SalesDate | Sequence, CreatedAt desc, Id |

Provision rows show the summed filed amount (`n2`) and the currency code read from `ReferenceData.Currencies`.

Not added: the 03 Provision filters TravelDate, PassengerTypeCode, FlightId, AircraftId, FareFamilyId, PointOfSaleId — they filter on criteria fields that arrive with M3; adding them now would be dead filters.

Tests: acceptance +3 (`M1_A01_suppliers_are_listed…`, `M1_C09_service_definitions_are_listed…`, `M1_D02_provisions_of_a_definition_are_listed…`) → 26/26 (discovered 26); conformance 36/36. The test database now also migrates `ReferenceDbContext` and seeds IRR (70) / USD (155). Clean solution build: 0 errors, 13 pre-existing warnings.

Live (owner token, side-build host): Suppliers 200 (2 rows), ServiceDefinitions 200 (3 active), Provisions 200 per definition (45.00 USD, 15,000,000.00 IRR, 38,500,000.00 IRR).
