# 06 — اصلاح v10: دقیقاً کجا تغییر، کجا حفظ، و مهاجرت چگونه؟

## A. Matrix of files (source real at v10 HEAD)
| Current source component | v11 action | Reason / constraint |
|---|---|---|
| `Domain/AncillaryReservationAggregate/AncillaryReservation.cs` | **preserve** Root/status/partial/replay; accept TypedUnitArgs | صحیح، rename/refactor فقط در حد نیاز |
| `.../Entities/AncillaryReservationUnit.cs` | **extend** VariantCode and immutable typed choice | P0: reconstruct exact chosen service |
| `.../Entities/AncillaryReservationUnitAllocation.cs` | **preserve** | SQL capacity guarantee سالم |
| `Application/.../HoldAncillaryServices/HoldAncillaryServicesService.cs` | **reuse orchestrator** with one typed mapping adapter | no second Hold service/aggregate |
| `Application/.../HoldAncillaryServices/AncillaryHoldInputs.cs` | **replace externally** `AncillaryHoldServiceInput`; keep internal flat args only | `coverageScope/serviceDefinitionId/inventoryFacts` must not leak |
| `RestApi/.../HoldAncillaryServicesRequest.cs` | **replace body DTO** with 8 typed family arrays | mimic Offers names/nesting |
| `Application/.../Services/AncillaryHoldAllocator.cs` | preserve / map Airport Selected `timeWithOffset`+`guestCount` | no inventoryFacts wire |
| `Persistence/.../InventoryLedger.cs`, `IInventoryLedger*` | preserve | source-lock + usage invariant |
| `Query/.../AncillaryHoldDto.cs` & mapper | **typed GET** projection from persisted selection | no live Definition/Price lookup |
| `Application/.../HoldAncillaryServices/AncillaryHoldResult*` | short Receipt only | avoid echo redundant/internal fields |
| `RestApi/.../ServiceController.cs` | same Routes; DTO changes only | no endpoints per child |
| `RestApi/.../SeatMaps/Requests/HoldSeatSelectionsRequest.cs` | preserve existing | no seat physical complexity |
| `Application/.../HoldSeatSelections/*` | preserve Facade; adapt saved typed Seat to same Root | Seat only commercial |
| `Domain/AncillaryProvisionAggregate/AncillaryProvision.cs` | no business rewrite | source of Scope/Definition/Quantity |
| `Query/Shopping/.../Resolve*`, `Shopping/Offers/*` | **freeze** | source of wire naming, no new Price shape |
| `.../RetireFlight{Count,Weight}InventoryService.cs`, `RetireAirportSlotInventoryService.cs` | small `ActiveUsage >0` guard under existing source lock | stop replacement stock oversell |
| `Persistence/Migrations/*V10ReservationAllocations*` | preserve migration/history, **additive** v11 | do not delete v10 data |

## B. Storage and migration principles
1. Retain Reservation/Unit/Allocation tables, indexes, statuses and all historical rows. No database reset / drop/rename without Owner approval.
2. Add only a `VariantCode` field and **typed family-specific child storage** needed for supported variants. **No single `SelectedJson` blob, no dozens of nullable unrelated columns on Unit.** Use Owned types relational per existing ORM conventions; lists (`FlightIds`/Pet docs) go into child tables per `CLAUDE.md` rather than JSON/NVARCHAR(max).
3. Current `PrimitiveCollection(CoveredFlightIds)` may be mapped to JSON in SQL. **Audit actual migration and query storage in Gate0**; if so, move to small relational `AncillaryReservationUnitFlights` child table with additive backfill verified, not data loss; never rewrite silently.
4. Legacy v10 Unit without typed selection is historical and must not be **fabricated** from current Definition. Read legacy using existing fields and mark unknown specialist detail as **not captured**, without changing status or fabricating booking information. New Hold must always persist required detail.
5. Schema FK indexes, decimal precision (Count/Slot integer; Weight decimal(19,3)), constraints, EF migration SQL and Down/Up only on test DB. No new QueryDb read model unless a real consumer requires it.
6. Keep one UnitOfWork/transaction for Reservation and Inventory allocations. Mapping into persistence must not load Order/Offer to derive selection.

## C. Three work bundles, not rewrite
**Bundle 1 — Wire:** define typed Contract/validators/Mapper with `family→variantCode→products→units`, `unit/coverage/selected`; receipt small; negative tests.
**Bundle 2 — Domain:** Unit durable Typed Selection + relational persistence + deterministic GET historical projection. Source of fixed details at Hold verified, missing source gated; no duplicate truth.
**Bundle 3 — Inventory & lifecycle:** map `unit.coverage.flightIds` and `selected.options.airportService` to existing allocator, preserve ledger, exact SQL concurrency, SeatMap adapter, Retire active usage guard, migrations and comprehensive tests.

## D. Risks requiring explicit evidence, not architecture invention
- `provisionId` is from `Resolve.provision.provisionId`, **not** from presentation-only `AncillaryOffers.products.productKey`; Adapter consumer mapping must be shown in tests, but Ancillary **never calls Ordering**.
- Offer-scoped TravellerRef/FlightRef are strings; actual Order travellerId and Inventory FlightId are numeric. Their conversion is Caller responsibility; Ancillary uses numeric provided, without checking Order membership. This is the only justified local change in leaf DTO shape.
- `CustomerSelectionContract` has A02 packageProductRef, A06 EquipmentKind, A12 menuItemRef etc. Those are **not all in `Price/Resolve.selected.options`**. Do not add unapproved ad hoc Hold fields to hide the Source mismatch; report GATED for ambiguous variants.
- Local live HTTP was GATED because `NotConnectedInventoryResourceReference` / `NotConnectedAirportFacilityReference` remain. Build/SQL PASS does not mean Production/Live completed.
- Root GET must show history after expired/retired Offer and Product. No dependence on selected Snapshot TTL.

## E. Mandatory deletion audit
After Gate 5: no public `AncillaryHoldServiceInput` with `CoverageScope`/`ServiceDefinitionId`/`InventoryFacts`, no duplicative Hold API, no fake Provider refs, no failing source tests suppressed, no broad disallow on healthy A01/A05/A15/A20 variants; no unauthorized schema destructive operations.
