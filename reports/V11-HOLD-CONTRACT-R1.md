# V11 Hold contract R1 — implementation

Date: 2026-10-27. Branch `feat/ancillary-offers-v9`, base `296197a`; Gates 1–3 and R1 are uncommitted.
Authority: the Owner's answers to `reports/V11-HOLD-CONTRACT-R1-QUESTIONS-AND-CONFLICTS.md` (every recommendation accepted), merged into `docs/Pack-v11` 01 §J, 02 §1.1–§1.5 and §3, 03, and 07 W10/W28/W29. `VERIFY-PACK.py` PASS: 15 Markdown, 106 unique tests; the pack stays LF.
Verdict: INTERNAL_READY for the Owner's review. Nothing is committed or pushed.

## 1. Sibling followed

Offers and Resolve type each family array by a generic parameter (`AncillaryVariantGroupDto<BaggageDetailsDto>`, `ServiceResolvedSelectionDto<MealDetailsDto>`). The Hold request and the Hold GET now use the same pattern for the per-family part (`selected` on input; `selected` and the unit on output).

## 2. Request (`POST /Service/v1/Ancillaries/Service-Holds`)

| Family array | CLR type | `selected` on the wire |
|---|---|---|
| `baggages` | `AncillaryVariantHoldInput<SelectedUnitInput<BaggageHoldOptionsInput>>` | `{quantity?, options?{baggage{bagRef?, measuredWeightKg?, dimensions?, weightKg?}}}` |
| `meals` | `AncillaryVariantHoldInput<SelectedUnitInput>` | `{quantity?}` |
| `assistedTravel` | `AncillaryVariantHoldInput<SelectedUnitInput<AssistedTravelHoldOptionsInput>>` | `{quantity?, options?{assistedTravel{assistanceSsrCode?}}}` |
| `airportServices` | `AncillaryVariantHoldInput<SelectedUnitInput<AirportServiceHoldOptionsInput>>` | `{quantity?, options?{airportService{timeWithOffset?, guestCount?}}}` |
| `priorityServices` | `AncillaryVariantHoldInput<SelectedUnitInput<PriorityHoldOptionsInput>>` | `{quantity?, options?{priority{optIn}}}` |
| `connectivity` | `AncillaryVariantHoldInput<SelectedUnitInput<ConnectivityHoldOptionsInput>>` | `{quantity?, options?{connectivity{deviceCount?}}}` |
| `upgrades` | `AncillaryVariantHoldInput<SelectedUnitInput>` | `{quantity?}` |

- The generic chain is `AncillaryVariantHoldInput<TSelected>` → `AncillaryProductHoldInput<TSelected>` → `AncillaryUnitHoldInput<TSelected>`.
  - Option content reuses Price's records (`BaggageSelectionOptions`, `AirportServiceSelectionOptions`, `PrioritySelectionOptions`, `ConnectivitySelectionOptions`).
  - `assistedTravel` uses the new `AssistedTravelHoldSelectionOptions(assistanceSsrCode)` (Q3).
- `pets` is no longer a member (Q3).
- `unit` is `HoldUnitInput(long TravellerId)`: when `unit` is sent, it must carry a positive traveller (Q4).
- A valid request is byte-identical to the pack 02 §1 sample; `V11_W01` binds the same body.
- Behaviour changes for invalid input:

| Input | Before | Now |
|---|---|---|
| `selected.options` key of another family (or a second key) | 400 "Validation failed" from the command validator (`OptionsFit`) | 400 at JSON binding (strict records); `OptionsFit` deleted |
| `meals`/`upgrades` with `options` | 400 "Validation failed" (`OptionsFit`) | 400 at JSON binding |
| `pets` array | accepted by binding, then 16419/409 at hold time (forced confirmation) | 400 at JSON binding |
| `assistedTravel.infantRef` / `guardianRef` | 16402/422 from the Definition field check | 400 at JSON binding |
| `unit:{}` | accepted as "no traveller" | 400 "Validation failed" (shape rule: `travellerId` > 0) |

Command-validator failures map to 400 "Validation failed" (`Framework/AeroTech.Framework.Presentation/Middlewares/ExceptionHandlingMiddleware.cs:48`), and strict JSON binding failures are 400 (`[ApiController]` model binding).

- Shape validators stay shape-only.
  - The variant → product → unit validators are generic and receive the family's `selected` validator.
  - One small validator per options record: `NotNull` on the family key, plus Price's `BaggageSelectionOptionsValidator` for baggage.
  - The open generic validators are not registered by `AddValidatorsFromAssembly`. The container test builds the real command validator with `ValidateOnBuild` (`V11_every_application_validator_can_be_built_by_the_container`).
- The Application port record `HoldSelection` became family-neutral: family, variant, provision, order service, traveller, flights, quantity, `ISelectionOptions?`.
  - `AncillaryHoldFamilies` builds it from the seven typed arrays.
  - The Query verifier (`HoldSelectionVerifier`, `HoldSelectionFields`) and the replay mapping (`HoldAncillaryServicesService.Chosen`) switch on the options record type. Behaviour is unchanged.

## 3. GET (`GET /Service/v1/Ancillaries/Service-Holds/{holdId}`)

| Family | CLR type |
|---|---|
| `baggages` | `AncillaryVariantHoldDto<HeldBaggageDetailsDto, AncillaryUnitHoldDto<HeldBaggageOptionsDto>>` |
| `meals` | `AncillaryVariantHoldDto<HeldMealDetailsDto, AncillaryUnitHoldDto>` |
| `assistedTravel` | `AncillaryVariantHoldDto<AncillaryUnitHoldDto<HeldAssistedTravelOptionsDto>>` |
| `airportServices` | `AncillaryVariantHoldDto<HeldAirportServiceDetailsDto, AncillaryUnitHoldDto<HeldAirportServiceOptionsDto>>` |
| `priorityServices` | `AncillaryVariantHoldDto<AncillaryUnitHoldDto<HeldPriorityOptionsDto>>` |
| `connectivity` | `AncillaryVariantHoldDto<AncillaryUnitHoldDto<HeldConnectivityOptionsDto>>` |
| `upgrades` | `AncillaryVariantHoldDto<HeldUpgradeDetailsDto, AncillaryUnitHoldDto>` |
| `seats` | `AncillaryVariantHoldDto<SeatUnitHoldDto>` |

- `seatNumber` and `seatMapSelectionId` exist only on `SeatUnitHoldDto` (Q2).
- `selected` is `HeldSelectedUnitDto(quantity)` or `HeldSelectedUnitDto<TOptions>(quantity, options?)` with typed records. Null members are omitted, so a GET returns the same `selected` JSON the Create sent (`V11_W29`, `V11_D01_D10_D21_W17_W18`).
- `JsonObject` and Resolve's `SelectedUnitDto` are no longer used by the Hold.
- Storage did not change. No migration.

## 4. Not done (Q5)

The same six-key union remains in the Offers and Resolve output (`AncillaryUnitOfferDto.Options`, `SelectedUnitDto`). It is fixed only on the Owner's order, in a separate step.

## 5. Tests

- **Changed:**
  - `V11_W02_W06_W10_W11_W19_W21_W28` (strict JSON theory) now also covers three W10 cases and three W28 cases (`pets`, `infantRef`, `guardianRef`).
  - `V11_W09_W15_W28` covers `unit:{}`.
  - `V11_W25_W26` uses the typed records.
  - The `V11_D01…` and `V11_D23` typed asserts were updated.
  - `M1_U08` and `SEAT_A07` (money-member checks) now list the generic and option records.
  - The fixtures `M1Commands.Hold`/`Lounge` and `HeldUnits.Units()` were rewritten.
- **New:** `V11_W29_the_hold_projection_types_selected_per_family_and_keeps_seat_fields_on_seat_units`.
- **Freeze pins:** the reservation file set grew from 115 to 133 files; hash `3E998D046B404D8BA7794F390B1CED88FC638B75CB22700694E152389CCB1E90`, updated in the three pin tests.
- **Run** (2026-10-27, after this change and Gate 3; logs in scratch `v11/full3`):

| Step | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 errors, 59 warnings |
| Domain conformance | 293 discovered, 293 passed |
| Shopping | 126 discovered, 125 passed, 1 skipped |
| Application acceptance | 288 discovered, 288 passed |
| Aborted or crashed runs | none |
