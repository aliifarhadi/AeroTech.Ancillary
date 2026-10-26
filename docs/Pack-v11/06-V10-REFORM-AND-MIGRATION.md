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
| new Application port for options and fixed choices (Owner Q2) | **add** one port in `Application/AncillaryReservationAggregate/Services`, implemented in Query by calling `TypedSelectionForm.Fields` and `TypedSelectionForm.WithFixedChoices` | one truth with Price; Shopping unchanged |
| `Persistence/.../AncillaryReservationUnitConfiguration.cs` `PrimitiveCollection(CoveredFlightIds)` | **replace** by the child table `AncillaryReservationUnitFlights` (Owner Q5) | `CLAUDE.md` rule 3; the JSON `nvarchar(max)` column is dropped |
| `Application/.../HoldAncillaryServices/AncillaryHoldResult*` | short Receipt only | avoid echo redundant/internal fields |
| `RestApi/.../ServiceController.cs` | same Routes; DTO changes only | no endpoints per child |
| `RestApi/.../SeatMaps/Requests/HoldSeatSelectionsRequest.cs` | preserve existing | no seat physical complexity |
| `Application/.../HoldSeatSelections/*` | preserve Facade; adapt saved typed Seat to same Root | Seat only commercial |
| `Domain/AncillaryProvisionAggregate/AncillaryProvision.cs` | no business rewrite | source of Scope/Definition/Quantity |
| `Query/Shopping/.../Resolve*`, `Shopping/Offers/*` | **freeze** | source of wire naming, no new Price shape |
| `.../RetireFlight{Count,Weight}InventoryService.cs`, `RetireAirportSlotInventoryService.cs` | small `ActiveUsage >0` guard under existing source lock | stop replacement stock oversell |
| `Persistence/Migrations/*V10ReservationAllocations*` | preserve the migration history; v11 adds one plain migration | dev reservation rows are deleted first (Owner Q5); no backfill |

## B. Storage and migration principles
1. Dev reservation data is test data (Owner Q5): before the v11 migration all rows of `AncillaryReservationUnitAllocations`, `AncillaryReservationUnits` and `AncillaryReservations` are deleted on dev. The tables, indexes and status values stay; no other table is touched.
2. Add `VariantCode`, `ServiceDefinitionRef` and **typed family-specific child storage** only for supported variants. **No single `SelectedJson` blob, no dozens of nullable unrelated columns on Unit.** Lists go into child tables per `CLAUDE.md`.
3. `CoveredFlightIds` is today a `PrimitiveCollection` stored as JSON in `nvarchar(max)` (audited at Gate 0). It is replaced by the relational child table `AncillaryReservationUnitFlights` in a plain migration; no backfill, because the tables are empty.
4. No legacy unit remains on dev, so no "not captured" rendering is needed.
5. Schema FK indexes, decimal precision (Count/Slot integer; Weight decimal(19,3)), constraints, EF migration SQL and Down/Up only on test DB. No new QueryDb read model unless a real consumer requires it.
6. Keep one UnitOfWork/transaction for Reservation and Inventory allocations. Mapping into persistence must not load Order/Offer to derive selection.

## C. Three work bundles, not rewrite
**Bundle 1 — Wire:** define typed Contract/validators/Mapper with `family→variantCode→products→units`, `unit/coverage/selected`; receipt small; negative tests.
**Bundle 2 — Domain:** Unit durable typed selection + relational persistence + deterministic GET historical projection. Fixed details are captured at Hold through the port (Owner Q2); GATED variants are refused; no duplicate truth.
**Bundle 3 — Inventory & lifecycle:** map `unit.coverage.flightIds` and `selected.options.airportService` to existing allocator, preserve ledger, exact SQL concurrency, SeatMap adapter, Retire active usage guard, migrations and comprehensive tests.

## D. Risks requiring explicit evidence, not architecture invention
- `provisionId` is from `Resolve.provision.provisionId`, **not** from presentation-only `AncillaryOffers.products.productKey`; Adapter consumer mapping must be shown in tests, but Ancillary **never calls Ordering**.
- Offer-scoped TravellerRef/FlightRef are strings; actual Order travellerId and Inventory FlightId are numeric. Their conversion is Caller responsibility; Ancillary uses numeric provided, without checking Order membership. This is the only justified local change in leaf DTO shape.
- `CustomerSelectionContract` lists A02 PackageProductRef, A06 EquipmentKind, A10 TargetCabinId, A11 MealCode, A12 MenuItemRef and A24 PlanCode. Gate 0 found that Price fixes each one uniquely from the Definition (`TypedSelectionForm.WithFixedChoices`), so the Hold captures them with the same rule through the port (Owner Q1, Q2); no ad hoc Hold field is added.
- Local live HTTP was GATED because `NotConnectedInventoryResourceReference` / `NotConnectedAirportFacilityReference` remain. Build/SQL PASS does not mean Production/Live completed.
- Root GET must show history after expired/retired Offer and Product. No dependence on selected Snapshot TTL.

## E. Mandatory deletion audit
After Gate 5: no public `AncillaryHoldServiceInput` with `CoverageScope`/`ServiceDefinitionId`/`InventoryFacts`, no duplicative Hold API, no fake Provider refs, no failing source tests suppressed, no broad disallow on healthy A01/A05/A15/A20 variants; no unauthorized schema destructive operations (dropping the `CoveredFlightIds` JSON column after the dev reservation rows are deleted is authorized, Owner Q5).
