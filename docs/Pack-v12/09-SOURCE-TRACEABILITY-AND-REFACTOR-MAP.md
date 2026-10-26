# 09 — Current Source Evidence / Keep–Refactor–Replace–Delete

**SHA audit:** `feat/ancillary-offers-v9@8c2fb087d43c04a06cf9eb18dc1bdd5734fff51d`؛ با `7a74055c` مقایسه شد و دو commit و Migration v11 مشاهده شد. مسیرهای زیر relative به repository root هستند. این سند Source-review است؛ build/test در runtime مستقل اجرا نشده است.

| File/Component | HEAD observation | Verdict | Migration/test impact |
|---|---|---|---|
| `CLAUDE.md` | no commit without consent, relational storage, sibling pattern | KEEP authority | no change |
| `src/AeroTech.Ancillary.Domain/AncillaryReservationAggregate/AncillaryReservation.cs` | Root statuses, idempotency, AppendSeats, partial ops | KEEP | test L01–L13 |
| `.../Entities/AncillaryReservationUnit.cs` | Variant/ServiceDefinitionRef, owned selection, per Unit status, Seat | KEEP; verify | D01–D12 |
| `.../Entities/AncillaryReservationUnitAllocation.cs` | immutable usage ledger child | KEEP | SQL concurrency |
| `.../Entities/AncillaryReservationUnitFlight.cs` | relational flight child | KEEP | EF reload |
| `.../Arguments/AncillaryReservationSelectionArgs.cs` | selection args for six supported families | KEEP | check exact fields |
| `.../Selections/{Baggage,Meal,AssistedTravel,AirportService,Connectivity,Upgrade}Selection.cs` | typed owned details, no JSON blob | KEEP | round-trip, retirement |
| `src/AeroTech.Ancillary.Application/AncillaryReservationAggregate/Commands/HoldAncillaryServices/AncillaryHoldFamilies.cs` | typed family flatten | KEEP | W01, W05 |
| `.../HoldAncillaryServicesService.cs` | verifier + existing lock/ledger/Root, replay without live catalog | KEEP | W02/W03, I01–I10 |
| `.../IHoldAncillaryServicesCommand.cs` and `RestApi/.../HoldAncillaryServicesRequest.cs` | Baggages/Meals/Assisted/Airport/Priority/Connectivity/Upgrades, no Pets | KEEP | schema snapshots |
| `.../SelectedUnitInput.cs`, `.../AncillaryUnitHoldInput.cs`, `.../HoldUnitInput.cs`, `.../HoldCoverageInput.cs` | generic nesting typed per family, minimal unit/coverage | KEEP | unknown-field tests |
| `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/VerifyHoldSelections/HoldSelectionVerifier.cs` | same static source rules used by Price, no Order callback | KEEP | bounded dependencies; no shopping redesign |
| `.../HoldSelectionFields.cs` | option fields checking against descriptors | KEEP / **VERIFY** | date/dimensions/numeric validation tests |
| `src/AeroTech.Ancillary.Query/AncillaryReservationAggregate/Queries/GetAncillaryHoldById/AncillaryHoldMapper.cs` | typed GET from stored children | KEEP | D01–D12, no live Definition |
| `src/AeroTech.Ancillary.Query/AncillaryReservationAggregate/Dto/AncillaryHoldDto.cs` | per family typed arrays and seats | KEEP | Swagger GET shape |
| `src/AeroTech.Ancillary.Persistence/AncillaryReservationAggregate/AncillaryReservationUnitConfiguration.cs` | OwnsOne/OwnsMany, Flights child | KEEP | migration state check |
| `src/AeroTech.Ancillary.Persistence/Migrations/20261026190047_V11ReservationSelections.cs` | adds 7 child tables + new columns | KEEP, DO NOT REDO | SQL schema/data safety |
| `src/AeroTech.Ancillary.Persistence/AncillaryReservationAggregate/InventoryLedger.cs` | SQL lock and active allocations | KEEP | last-stock/rollback |
| `src/AeroTech.Ancillary.Application/AncillaryReservationAggregate/Services/AncillaryHoldAllocator.cs` | derives allocation from local stock policy | KEEP/VERIFY | I01–I10 |
| `src/AeroTech.Ancillary.RestApi/V1/AncillaryReservationAggregate/Controllers/ServiceController.cs` | `Length>0 ? unitIds : null` in DELETE | **REFACTOR P0** | `unitIds` present empty => 400, no data change; no migration |
| `.../Commands/ReleaseAncillaryHold/ReleaseAncillaryHoldValidator.cs` | `NotEmpty` for non-null arrays | KEEP, ensure controller does not swallow [] | L03/L04 |
| `src/AeroTech.Ancillary.RestApi/V1/AncillaryReservationAggregate/Requests/CancelAncillaryHoldRequest.cs` | `reasonCode`, `unitIds` | KEEP | L09–L11 |
| `src/AeroTech.Ancillary.Application/AncillaryReservationAggregate/Commands/HoldSeatSelections/HoldSeatSelectionsService.cs` | verifier to same root | KEEP | L12/L13 |
| `src/AeroTech.Ancillary.RestApi/V1/SeatMaps/Requests/HoldSeatSelectionsRequest.cs` | special Seat inputs | KEEP/FREEZE | no generic hold shoehorn |
| `src/AeroTech.Ancillary.Application/{FlightCountInventoryAggregate,FlightWeightInventoryAggregate,AirportSlotInventoryAggregate}/Commands/Retire*/*Service.cs` | v11 modified to protect active usage (report) | KEEP/VERIFY | I09 race and lock |
| `src/AeroTech.Ancillary.Domain/AncillaryServiceDefinitionAggregate/{AncillaryVariant,CustomerSelectionContract}.cs` | fixed A01–A24 and customer selection field definitions | KEEP/FREEZE | matrix 03 |
| `docs/Pack-v11/00-START-HERE.md` | states "target, not implemented" although HEAD now has v11 code | **STALE DOC**; superseded by this v12 Pack | docs only |
| `docs/Pack-v11/02-EXACT-RESERVATION-API-AND-DTOS.md` | wire basis but missing new explicit-empty P0 | SUPERSEDED as execution authority | tests/docs only |

**Delete policy:** هیچ کلاس Domain یا Migration در این مرحله برای حذف نامزد نشده است. DTO قدیمی `AncillaryHoldInputs.cs`, `AncillaryHoldResult.cs` در compare v10→v11 حذف شده‌اند؛ دوباره ایجاد نشوند. `SelectedJson` یا Adapter fake اصلاً نساز. Cleanup صرفاً با proof of dead code و tests و اجازه Owner.

## Quick proof commands for agent
```bash
git rev-parse HEAD
git status --short
git diff --stat 7a74055c774b2466fe239b8307ec93b15264e50c HEAD
rg -n 'unitIds is \{ Length: > 0 \} \? unitIds : null' src
rg -n 'JsonUnmappedMemberHandling|InventoryFacts|CoverageScope' src/AeroTech.Ancillary.RestApi/V1/AncillaryReservationAggregate
rg -n 'sp_getapplock|OpenAsync|CommitAsync' src/AeroTech.Ancillary.Persistence src/AeroTech.Ancillary.Application/AncillaryReservationAggregate
```
No source changes before Owner approves Gate 1.
