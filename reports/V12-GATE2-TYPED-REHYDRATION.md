# V12.1 Gate 2 - Typed Domain and SQL Rehydration

Date: 2026-10-27
Authority: `CLAUDE.md`, then `docs/Pack-v12.1/AeroTech-Ancillary-Reservation-Pack-v12.1-GATE1-MINIMAL/12-AGENT-GATE2-DOMAIN-AND-MIGRATION.md`
Result: **NO CODE CHANGE REQUIRED.** The audit found no source-proven fault in Domain or Persistence against the six Gate 2 points. Only tests were added, for pack rows that had no direct test. One replay question is open for the Owner (§7). Nothing is committed. Gate 3 has not started.

## 1. Source state

| Item | Value |
|---|---|
| Branch | `feat/ancillary-offers-v9` |
| HEAD | `e6a2012ee101546f5728db6a68844adcb6051204` (unchanged since Gate 1) |
| Dirty from Gate 1 | `ServiceController.cs`, three pin test files, `V10InventoryAllocationAcceptanceTests.cs`, new `Fixtures/ReservationHttpHost.cs`, `reports/V12-GATE1-MINIMAL-SAFETY.md` |
| Dirty from Gate 2 | four test files under `tests/.../Holds/` (§5), this report |
| Untracked, the Owner's | `docs/Pack-v12.1/`, the v12.1 zip |

No `src` file changed in Gate 2. The reservation source pin is unchanged: 133 files, `BB7E5038C5CDE87014A85FEFC94801D779241782DD242F31875E4AD4EA9890B7`. The Gate 2 test files are outside the pinned set.

## 2. Audit against the six Gate 2 points

### Point 1: options and fixed choices reach the hold

| Check | Evidence | Verdict |
|---|---|---|
| Every family passes its options | `AncillaryHoldFamilies.cs:9-17`. Baggage, AssistedTravel, AirportService, Priority and Connectivity go through `OptionFamily` with their options. Meal and Upgrade take a quantity only. `Family(...)` at `:39-56` copies family, variantCode, provisionId, orderServiceId, travellerId, flightIds, quantity and options. | KEEP |
| The verifier checks the request against the catalog | `HoldSelectionVerifier.cs`, in this order: | KEEP |
| | `:52` family and variantCode must match the definition (16402) | |
| | `:55` `EnsureHoldable` | |
| | `:56` option fields against the definition descriptors | |
| | `:58` A23 needs `optIn: true` | |
| | `:66` an omitted quantity is allowed only on a fixed-quantity provision | |
| Fixed choices are captured at hold time | `TypedSelectionForm.cs:55` `WithFixedChoices`, called at `HoldSelectionVerifier.cs:61`. `SelectionOf` maps each fixed choice onto the reservation args (see below). | KEEP |

Where each fixed choice is stored:

| Variant | Fixed choice | Taken from | Stored as |
|---|---|---|---|
| A02 | `PackageProductRef` | the definition's `ServiceDefinitionRef` | the unit's `ServiceDefinitionRef` column |
| A06 | `EquipmentKind` | the definition | baggage `EquipmentKind` |
| A10 | `TargetCabinId` | the definition | upgrade `ToCabinId` |
| A11 | `MealCode` | the definition | meal `MealCode` |
| A12 | `MenuItemRef` | the definition | meal `MenuItemRef` |
| A20–A22 | `AirportId`, and `FacilityRef` | `FacilityRef` comes from the provision rule, else from the definition | airport-service `AirportId`, `FacilityId` |
| A24 | `PlanCode` | the definition's `ServiceDefinitionRef` | the unit's `ServiceDefinitionRef` column |

### Point 2: the unit and its storage

| Check | Evidence | Verdict |
|---|---|---|
| At most one family part per unit, and it must match the variant's profile | `AncillaryReservationUnit.cs:49` | KEEP |
| Flight ids stored in a child table | `AncillaryReservationUnit.cs:87`; `AncillaryReservationUnitConfiguration.cs:26-33`: `OwnsMany`, table `AncillaryReservationUnitFlights`, key `(AncillaryReservationUnitId, FlightId)`, field access | KEEP |
| Seat number normalised | `AncillaryReservationUnit.cs:90,207`: trim, then upper-invariant | KEEP |
| Column types match the EF configuration | `AncillaryReservationUnitConfiguration.cs:35-82` equals migration `20261026190047_V11ReservationSelections.cs:47-189`. Details below. | KEEP |
| No silent rounding of measures | `SpecificationRules.cs:7,17`: a measure must be > 0, < 1,000,000 and have at most 3 decimals. `BaggageSelection.Create` and `SelectedDimensions.Create` enforce this, so a fourth decimal is refused (16402) before SQL. | KEEP |
| No list or JSON column | The read-back test counts `nvarchar(max)` columns on `AncillaryReservation%` tables and finds 0 | KEEP |

Column types (configuration = migration):

| Table | Column | Type |
|---|---|---|
| Baggage selections | `BagRef` | nvarchar(64) |
| | `MeasuredWeightKg`, `WeightKg`, `LengthCm`, `WidthCm`, `HeightCm` | decimal(18,3) |
| | `EquipmentKind` | nvarchar(20) |
| Meal selections | `MealCode` | nvarchar(4) |
| | `MenuItemRef` | nvarchar(40) |
| Assisted-travel selections | `AssistanceSsrCode` | nvarchar(4), required |
| Airport-service selections | `TimeWithOffset` | datetimeoffset |
| | `GuestCount`, `AirportId` | int |
| | `FacilityId` | bigint |
| Connectivity selections | `DeviceCount` | int |
| Upgrade selections | `ToCabinId` | int |
| Allocations | `Amount` | decimal(19,3) (`AncillaryReservationUnitAllocationConfiguration.cs:15`, `20261026114827_V10ReservationAllocations.cs:45`) |

### Point 3: GET rebuilds the hold from reservation data only

| Check | Evidence | Verdict |
|---|---|---|
| Only the reservation is loaded | `GetAncillaryHoldByIdService` loads the aggregate with `IAncillaryReservationRepository.GetAsync`. The repository includes Units → Allocations; EF loads the owned flights and selections with the owner. | KEEP |
| No catalog or offer lookup | `AncillaryHoldMapper` reads only unit columns and the static in-code table `AncillaryVariant.Find` (A01–A24). It has no definition, provision, pricing or offer reader. | KEEP |
| No invented data | The A23 `optIn: true` shown on GET is derived from the stored `VariantCode`; a hold of A23 is accepted only with `optIn: true`. No provider ref is produced. `StockPoolId` and `ProviderUnitRef` are legacy nullable columns, never mapped to GET. | KEEP |

### Point 4: replay compares the selected content

Replay runs before the verifier and without the catalog, so it still works after retirement (`HoldAncillaryServicesService.cs:48-55`). `AncillaryReservation.EnsureSameContent` (`:149-162`) calls `AncillaryReservationUnit.IsSelectedBy` (`:183-193`), which compares:

- `OrderServiceId`, `ProvisionId`, `TravellerId` and the flights;
- the quantity;
- baggage `BagRef`, `MeasuredWeightKg`, dimensions and `WeightKg`;
- assisted-travel SSR;
- airport-service `TimeWithOffset` and `GuestCount`;
- connectivity `DeviceCount`.

Order id, reference and requested expiry are compared as well. Any difference answers 16403 / 409 before any write. A replay after the provision **and** the definition are retired returns the stored hold. Verdict: KEEP. One open question remains on how strict replay should be (§7).

### Point 5: GATED variants stay GATED

| Variant | Gate in source |
|---|---|
| A09 | no `extraSeats` member; the request has no `pets`, `seats` or `extraSeats` member (strict JSON) |
| A13, A14 | no `pets` member; the definition forces confirmation |
| A17, A18, A19 | `AncillaryHoldChecks.cs:16-20`, refused at `:58-59` |
| A10 quote | `:61-62` |
| SubjectToConfirmation | `:55-56` |
| External supplier | `:38-39` (16104) |
| Supplier stock | `:76-77` |
| AcceptedWeight | `:82-83` |

Tests: `V11_matrix_a_gated_variant…` (A17, A19), `V11_D22…` (A18), `V11_W02_W06_W10_W11_W19_W21_W28…` (pets and other unknown members). Verdict: KEEP.

### Point 6: migration

See §4. No migration is missing or defective. No schema change.

## 3. Keep / refactor table per file

| File | Verdict |
|---|---|
| `Domain/.../AncillaryReservation.cs` | KEEP |
| `Domain/.../Entities/AncillaryReservationUnit.cs` | KEEP (open question §7) |
| `Domain/.../Entities/AncillaryReservationUnitFlight.cs`, `AncillaryReservationUnitAllocation.cs` | KEEP |
| `Domain/.../Selections/*` (six families + `SelectedDimensions`) | KEEP |
| `Domain/.../Arguments/AncillaryReservationSelectionArgs.cs`, `AncillaryReservationUnitArgs.cs` | KEEP |
| `Domain/.../Arguments/AncillaryReservationServiceSelectionArgs.cs` | KEEP (open question §7) |
| `Domain/.../AncillaryHoldChecks.cs` | KEEP |
| `Application/.../HoldAncillaryServices/AncillaryHoldFamilies.cs` | KEEP |
| `Application/.../HoldAncillaryServices/HoldAncillaryServicesService.cs` | KEEP (open question §7) |
| `Query/Shopping/AncillaryOffers/Queries/VerifyHoldSelections/HoldSelectionVerifier.cs`, `HoldSelectionFields.cs` | KEEP |
| `Shopping/Options/TypedSelectionForm.cs` (`WithFixedChoices`) | KEEP, frozen |
| `Query/AncillaryReservationAggregate/Queries/GetAncillaryHoldById/*` | KEEP |
| `Persistence/AncillaryReservationAggregate/AncillaryReservationUnitConfiguration.cs`, `AncillaryReservationUnitAllocationConfiguration.cs`, `AncillaryReservationRepository.cs` | KEEP |
| `Persistence/Migrations/20261026190047_V11ReservationSelections.cs` | KEEP; do not redo |

Nothing is a candidate for deletion.

## 4. Migration status

| Check | Command | Result |
|---|---|---|
| Newest migration in source | `ls src/AeroTech.Ancillary.Persistence/Migrations` | `20261026190047_V11ReservationSelections` (command); `20261016205637_V132ProvisionCancellationRulesQuery` (query) |
| Applied on dev (read-only `SELECT`) | `SELECT TOP 3 MigrationId FROM dbo.__CommandsMigrationHistory ORDER BY MigrationId DESC` (and the query history) | `20261026190047_V11ReservationSelections` is the last command migration; query is `20261016205637_V132ProvisionCancellationRulesQuery`; 9 `Ancillary.AncillaryReservationUnit%` tables exist |
| Pending model changes | side build of ServiceHost (0 errors), then `ef migrations has-pending-model-changes --context AncillaryDbContext` | "No changes have been made to the model since the last migration." |
| Domain/Persistence diff | `git diff --stat 8c2fb08 HEAD` and `git diff --stat HEAD` on `src/AeroTech.Ancillary.Persistence` and `src/AeroTech.Ancillary.Domain` | empty |

No migration was added, no `Down`/`Up` was run, and no dev row was written or deleted.

## 5. Tests added

Four existing test methods were extended, and one refusal case was added. No test was removed or skipped. Each method now names the v12 rows it covers.

| Test (file) | Added | Rows |
|---|---|---|
| `V11_L02_L03_V12_W02_W03_…_without_a_write` (`V11LifecycleAcceptanceTests`) | Besides the BagRef change: a changed SSR (`WCHR` → `BLND`) and a changed quantity (1 → 2) under the same key each answer 16403 / 409. SQL then shows still one hold with 4 units for the order. | W02, W03 |
| `V10_I17_…_V12_D07_W03_…` (`V10InventoryAllocationAcceptanceTests`) | GET from a fresh scope returns the appointment `2027-04-06T13:30+03:30` with its `+03:30` offset, the same instant and guest count 2. A changed appointment time under the same key answers 16403. | D07, W03 |
| `V11_L01_V12_D10_…` (`V11WireAcceptanceTests`) | The provision **and** the definition are retired. The replay returns the same hold and unit, and the GET JSON is byte-equal to the JSON read before retirement. | D06, D10, point 4 |
| `V11_D01_…_V12_D02_…_with_three_decimals` (`V11TypedSelectionAcceptanceTests`) | The A03 measured weight is now `27.125` kg and reads back exactly from `decimal(18,3)`. | D02, owned precision |
| `V11_D23_V12_D02_…` (same file) | New case `SCALE`: `27.1255` kg is refused with 16402 / 422, and no reservation is written. | D02, owned precision |

Pack 07 rows and their tests:

| Row | Test |
|---|---|
| D01–D10 | `V11_D01_D10_D21_W17_W18_V12_D02…` (A03, A06, A10, A11, A12, A15, A20, A23, A24 read back from SQL in a fresh scope, then again after release and provision retirement), `V11_matrix_a_supported_variant…` (A02, A04, A05, A16, A21, A22), `V11_L09…`, `V11_L04_L05_L07_L12_L13…`, `V11_L01_V12_D10…`, `V10_I17…V12_D07…`, `SEAT_A01`, `V10_S01…`, `V10_S02_S05…` |
| D11 | every reservation test reloads in a new `AncillaryScope` (new DbContext) on a database built by the migrations (`TestDatabase` runs `MigrateAsync` for the three contexts). A host restart is not tested separately. |
| D12 | `V11_matrix_a_gated_variant…` (A17, A19), `V11_D22…` (A18), `V11_W02_W06_W10_W11_W19_W21_W28…` (pets), Domain `V11_D14` (A10 quote) |
| W02, W03 | `V11_L02_L03_V12_W02_W03…`, `V10_I17…V12_D07_W03…`, `V11_L01_V12_D10…` (changed traveller) |
| Schema (no list column) | `V11_D01…` (0 `nvarchar(max)` columns on reservation tables) |

## 6. Commands and counts

All commands target `tests/AeroTech.Ancillary.Application.AcceptanceTests/AeroTech.Ancillary.Application.AcceptanceTests.csproj`. The database is the isolated `DotAirAncillary_Tests_{guid}`; no host, no hosted service and no dev DB were used.

| Step | Command | Result |
|---|---|---|
| Build | `dotnet build <csproj> -v q --nologo -nodeReuse:false` | 0 Error(s), 38 Warning(s) |
| Discover | `dotnet test <csproj> --no-build --list-tests --filter "FullyQualifiedName~.Holds.\|FullyQualifiedName~.Boundary."` | 99 discovered |
| Run | `dotnet test <csproj> --no-build --filter "<same filter>" --logger "console;verbosity=normal"` | Test Run Successful: Total tests 99, Passed 99. No Failed or Skipped line, no aborted line. Total time 8.9 minutes. All five extended tests are in the passed list. |

The full suites were not rerun: Gate 2 changed no production code.

## 7. Question for the Owner

**Q1. Should a replay refuse a request that a fresh hold would refuse?**

Today a replay is accepted when the stored content matches, without the catalog. Three cases pass on replay although a fresh hold would answer 16402:

| Case | Why it passes | Effect |
|---|---|---|
| (i) The same `provisionId` sent under another family array or another `variantCode`, with no options | `AncillaryReservationServiceSelectionArgs` has no family or variantCode (`Arguments/AncillaryReservationServiceSelectionArgs.cs:3-9`). `HoldAncillaryServicesService.cs:181-188` drops `selection.Family` and `selection.VariantCode`, and `IsSelectedBy` (`AncillaryReservationUnit.cs:183-193`) does not compare them. The command validator does not tie a `variantCode` to its family array (`AncillaryVariantHoldInputValidator.cs`). | The stored receipt is returned. Nothing is written. |
| (ii) An omitted quantity after an explicit one | `AncillaryReservationUnit.cs:189`: `Quantity == (service.Quantity ?? Quantity)` | Correct on a fixed-quantity provision, where omitted means the fixed value. On a range provision a fresh hold would refuse it, but replay cannot know the range without the catalog. |
| (iii) A23 with `optIn: false` | `HoldAncillaryServicesService.cs:201-214`: `Chosen` maps priority options to `null` | The stored receipt is returned. |

Options:

- **(a) Keep.** Replay compares only the stored content (the v11 decision "replay first, survives retirement").
- **(b) Compare the request's family and `variantCode` with the stored unit.** Both are known without the catalog: `VariantCode` is stored on the unit, and its family comes from the static `AncillaryVariant` table. This needs one member on `AncillaryReservationServiceSelectionArgs`, one condition in `IsSelectedBy`, one argument in `HoldAncillaryServicesService.SelectionArgs`, a pin update and one test. Cases (ii) and (iii) stay as they are, because they need the provision range or a stored opt-in that replay does not read.

Recommendation: **(b).** Idempotency practice (for example Stripe: "can only be used with the same parameters") treats a different product identity under the same key as a conflict. Case (i) is a different variant, not a retry. (ii) and (iii) return the true stored hold and write nothing.

Answer:

## 8. Remaining (outside Gate 2)

| Item | State |
|---|---|
| I05 same-key race test for the generic Hold | Gate 3 |
| Ordering end-to-end | NOT VERIFIED |
| Local stock on the dev database | GATED |
| D11 host restart | not separately tested (rehydration through a new DbContext is tested) |

## 9. Stop

Gate 2 ends here. Gate 3 was not started. Nothing was committed, pushed or merged.
