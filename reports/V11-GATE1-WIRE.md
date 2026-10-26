# Reservation v11 — Gate 1: Wire contract

**Base:** `296197a` · **Authority:** `docs/Pack-v11` (pack 11) and the Owner's Gate 1 answers (`reports/V11-GATE1-QUESTIONS-AND-CONFLICTS.md`: Q1 (a), Q2 the pack, Q3 the proposed names).
**Commit / push:** none. **Migration:** none (Gate 2). **Dev DB:** reservation rows deleted before this gate (Owner Q5), no other change.
**Production-ready:** no. The typed selection is not persisted yet (Gate 2); see section 6.

## 1. What changed for the caller

### 1.1 `POST /Service/v1/Ancillaries/Service-Holds`
Header plus eight optional family arrays named like `AncillaryOffers`: `baggages`, `meals`, `pets`, `assistedTravel`, `airportServices`, `priorityServices`, `connectivity`, `upgrades`. Each array holds `variantCode → products[] → units[]`. `services[]`, `serviceDefinitionId`, `coverageScope`, `coveredFlightIds`, `inventoryFacts`, `productKey`, `travellerRef`, `flightRefs`, `extraSeats` and `travelInsurance` are refused by strict JSON (400).

| Wire | C# record (Application, `HoldAncillaryServices`) | Fields |
|---|---|---|
| body | `HoldAncillaryServicesRequest` (RestApi) | `idempotencyKey`, `orderId`, `reference`, `requestedExpiresAt`, eight family arrays (each optional) |
| family item | `AncillaryVariantHoldInput` | `variantCode`, `products` |
| `products[]` | `AncillaryProductHoldInput` | `provisionId`, `units` |
| `units[]` | `AncillaryUnitHoldInput` | `orderServiceId`, `unit?`, `coverage?`, `selected?` |
| `unit` | `HoldUnitInput` | `travellerId?` |
| `coverage` | `HoldCoverageInput` | `flightIds` |
| `selected` | `SelectedUnitInput` | `quantity?`, `options?` = the existing `SelectionOptionsInput` (same as Price) |

Sample (pack 02 §1, bound and validated by test `V11_W01`):
```json
{"idempotencyKey":"O-12001-H1","orderId":12001,"reference":"ORD-12001","requestedExpiresAt":"2026-10-27T10:30:00Z",
 "baggages":[{"variantCode":"A01","products":[{"provisionId":701,"units":[
   {"orderServiceId":501,"unit":{"travellerId":1001},"coverage":{"flightIds":[801,802]},"selected":{"quantity":2}}]}]}],
 "airportServices":[{"variantCode":"A20","products":[{"provisionId":704,"units":[
   {"orderServiceId":504,"unit":{"travellerId":1001},"selected":{"options":{"airportService":{"timeWithOffset":"2026-10-27T08:00:00+03:30","guestCount":1}}}}]}]}]}
```

### 1.2 Receipt for every write
Create, `SeatMaps/Holds`, Confirm, Release and Cancel answer the same receipt. Routes and status codes are unchanged (201 for the two creates, 200 for the rest).
```json
{"holdId":"9001","reference":"ORD-12001","status":"Held","expiresAt":"2026-10-27T10:30:00+00:00",
 "units":[{"unitId":"9101","orderServiceId":"501","status":"Held"},{"unitId":"9102","orderServiceId":"502","status":"Released"}]}
```
Records: `AncillaryHoldReceipt(HoldId, Reference, Status, ExpiresAt, Units)` and `AncillaryHoldUnitReceipt(UnitId, OrderServiceId, Status)`; they replace `AncillaryHoldResult` / `AncillaryHoldUnitResult` (13 fields per unit). Every `long` is written as a string and enums by name (server converters, unchanged).

### 1.3 GET
`GET Service-Holds/{holdId}` still returns the v10 shape. The v11 projection records are defined in Query `AncillaryReservationAggregate/Dto` and serialization-tested (W25, W26); the GET switches to them in Gate 2 when the data is persisted:
- `AncillaryVariantHoldDto<TDetails>` / `AncillaryProductHoldDto<TDetails>` for families with fixed facts, details `HeldBaggageDetailsDto(equipmentKind)`, `HeldMealDetailsDto(mealCode, menuItemRef)`, `HeldUpgradeDetailsDto(toCabinId)`, `HeldAirportServiceDetailsDto(airportId, facilityId)`;
- `AncillaryVariantHoldDto` / `AncillaryProductHoldDto` (no details) for assisted travel, priority, connectivity and seats;
- `AncillaryUnitHoldDto(unitId, orderServiceId, status, cancellationReasonCode?, unit?, coverage?, selected, seatNumber?, seatMapSelectionId?)` with `HoldUnitDto(travellerId)`, `HoldCoverageDto(flightIds)` and the Resolve `SelectedUnitDto`; no `pricingId`.

## 2. How the hold is resolved inside

```
RestApi request ─► ServiceHoldAncillaryServicesCommand ─► validators (shape only)
                                                           │
HoldAncillaryServicesService.HoldAsync(command)            ▼
  1. AncillaryHoldFamilies.Selections → HoldSelection(family, variantCode, provisionId, unit) per unit
  2. replay first: stored hold for the key → AncillaryReservation.EnsureSameContent(request facts) → receipt
  3. IHoldSelectionVerifier (Application port) ─► HoldSelectionVerifier (Query, active batch readers)
  4. AncillaryReservationUnitArgs (definition, scope, quantity from the provision; appointment from options.airportService)
  5. existing v10 path: order lock → allocator + AncillaryHoldChecks → ledger → save → receipt
```

| Rule | Where | Error |
|---|---|---|
| shape: positive ids, non-empty arrays, at least one unit, `orderServiceId` once per request, `flightIds` non-empty and distinct, `quantity` > 0, `options` holds exactly the key of its family (`meals`, `upgrades` take none) | validators per record (`HoldAncillaryServicesValidator`, `AncillaryVariantHoldInputValidator`, … `SelectedUnitInputValidator`) | 400 |
| provision or its definition not active (or unknown) | `HoldSelectionVerifier` | 16419 (409) |
| `variantCode` or family does not match the definition | `HoldSelectionVerifier` | 16402 (422) |
| `quantity` omitted on a range provision (Owner Q2) | `HoldSelectionVerifier` | 16402 (422) |
| `quantity` omitted on a fixed provision | `HoldSelectionVerifier` | takes min = max |
| A17, A18, A19 and a provision priced by `ExternalQuote` (Owner Q1) | Domain `AncillaryHoldChecks.Accept`, next to the existing `SubjectToConfirmation` refusal (which already refuses pets) | 16419 (409) |
| replay with another body | Domain `AncillaryReservation.EnsureSameContent(…, AncillaryReservationServiceSelectionArgs)` | 16403 (409) |

## 3. Points decided inside the approved answers (for review)
1. **Port names and place.** `IHoldSelectionVerifier`, `HoldSelectionVerification`, `HoldSelection`, `VerifiedHoldSelection` in Application `AncillaryReservationAggregate/Services`; implementation `Query/Shopping/AncillaryOffers/Queries/VerifyHoldSelections/HoldSelectionVerifier`. Copied from the seat port (`ISeatSelectionVerifier`, `SeatSelectionVerification`, `VerifiedSeatSelection`) and Price's `VerifySelections` folder. `VerifiedHoldSelection` carries only what the port derives (definition id, scope, quantity); Gate 2 adds the fixed choices.
2. **Replay before verification.** The readers return active rows only, so verifying first would turn a retry after a retirement into a refusal. The service looks up the stored hold first and compares the request facts (order service, provision, traveller, flights, quantity, appointment, guests), the same order the seat facade uses (`FindHeldSeatsAsync` before `VerifyAsync`). An omitted quantity matches the stored one; an Active provision cannot change its quantity rule (`AncillaryProvision.Change` requires Draft).
3. **Unknown provision.** The active-only reader cannot tell an unknown id from an inactive one; both answer 16419 "provision … is not active". In v10 a client-sent definition id could give 16404/16405; those cases no longer exist on the wire (test `M1_N05` updated).
4. **Families without fixed facts.** A non-generic `AncillaryVariantHoldDto` / `AncillaryProductHoldDto` next to the generic pair, the pattern of `ApiResult` / `ApiResult<T>` in the Framework, instead of an untyped `object` details.
5. **Validator literals.** `variantCode` `MaximumLength(8)` copied from the definition validators; `coverage.flightIds` must be non-empty when `coverage` is sent (pack 02 §1.3: omit `coverage` when there is no flight).

## 4. Changed files
- **Application** `AncillaryReservationAggregate`: new input records and validators (`AncillaryVariantHoldInput`, `AncillaryProductHoldInput`, `AncillaryUnitHoldInput`, `HoldUnitInput`, `HoldCoverageInput`, `SelectedUnitInput` + validators), `AncillaryHoldFamilies`, receipt records and factory, port records (`Services/`); deleted `AncillaryHoldInputs`, `AncillaryHoldServiceInputValidator`, `InventoryHoldFactsInput(+Validator)`, `AncillaryHoldResult`, `AncillaryHoldResultFactory`; `HoldAncillaryServicesService`, command, interface, handlers of the six operations (type rename only outside Hold).
- **Domain** `AncillaryReservationAggregate`: `AncillaryReservationServiceSelectionArgs`, `AncillaryReservation.EnsureSameContent` overload, `AncillaryReservationUnit.IsSelectedBy` overload, `AncillaryHoldChecks` gated variants and quote.
- **Query**: `HoldSelectionVerifier` + registration; GET projection records (section 1.3).
- **RestApi**: `HoldAncillaryServicesRequest`, `ServiceController.Hold` mapping.
- **Tests**: new `Holds/V11WireAcceptanceTests` (9 methods, 19 cases) and Domain `V11HoldConformanceTests` (2); fixture `M1Commands` (`Hold` by family, `Lounge` selection), `AncillaryScope` (real verifier on the SQL readers); adapted `M1ReservationAcceptanceTests`, `V10ReservationAcceptanceTests`, `V10InventoryAllocationAcceptanceTests`, `V10SeatAndLifecycleAcceptanceTests`, `SeatReservationAcceptanceTests`.
- **Freeze pins** (P2_X07, V12_C04, X20): the reservation folder grew from 82 to 106 files; hash `EE2A928D70377BC86F45CAEC2CABA08CD506B513300A97B4636B60E71411F1A9`.

## 5. Tests
Clean build `dotnet build AeroTech.Ancillary.sln --no-incremental`: **0 errors, 61 warnings** (same count as v10; NuGet advisories and Framework/Contracts warnings, none in a changed file).

| Suite | Discovered | Executed | Result |
|---|---|---|---|
| Domain.ConformanceTests | 289 | 289 | 289 passed (v10: 287) |
| Shopping.Tests | 126 | 126 | 125 passed, 1 skipped (unchanged since v10) |
| Application.AcceptanceTests | 273 | 273 | 273 passed (v10: 254), 8 min 11 s |

No `aborted` or `crash` line in any log.

| Test ID | Test |
|---|---|
| W01 | `V11_W01_the_pack_sample_binds_into_four_families_and_passes_the_shape_rules`, `V11_W01_W12_a_family_hold_takes_the_definition_the_scope_and_a_fixed_quantity_from_the_provision` |
| W02–W06, W11, W19–W21, extra families | `V11_W02_W06_W11_W19_W21_a_member_outside_the_contract_is_refused_by_strict_json` (11 cases) |
| W07, W08, W13, W14 | `V11_W07_W08_W13_W14_a_variant_family_or_quantity_that_does_not_fit_the_provision_is_refused_before_any_write` (also proves no row was written) |
| W09, W10, W15 | `V11_W09_W10_W15_the_shape_rules_refuse_duplicates_foreign_option_keys_and_empty_holds` |
| W12 | `V11_W01_W12_…` (fixed quantity taken from the provision) |
| W16, W27 | `V11_W16_W27_the_receipt_has_five_members_with_ids_as_strings_and_statuses_by_name` |
| W25, W26 | `V11_W25_W26_the_hold_projection_records_write_the_agreed_family_shape` |
| W23 | existing Domain rule `AncillaryReservationUnit` (Sector = exactly one flight), unchanged |
| W24 | `V10_I16_I28_a_portion_takes_every_flight_or_nothing…` (now sent through the family wire) |
| D14 (quote) | Domain `V11_D14_a_provision_priced_by_an_external_quote_cannot_be_held` |
| D22 (A18) | `V11_D22_a_bassinet_is_refused_by_the_hold_even_without_a_confirmation_step` (A18 made `Immediate`, so only the new gate can refuse it) |
| L01–L03 replay | Domain `V11_L01_L02_L03_a_replay_is_compared_on_the_requested_facts_in_any_order`, acceptance `V11_L01_a_replay_answers_from_the_stored_hold_after_the_provision_is_retired` |

All v10 hold tests now go through the family wire (`M1Commands.Hold` builds the families). The v10 contract test `V10_A01_A02_A16_I13` was rewritten for the new body; `M1_N05` now proves an unknown provision is refused (16419).

## 6. Not done in Gate 1 (by the pack)
- `selected.options` other than `airportService` is checked for shape only and is not stored; Definition/Provision value checks and fixed-choice capture are Gate 2 (Owner Q2). Until then a hold accepts e.g. `assistanceSsrCode` without keeping it.
- GET keeps the v10 shape; W17, W18 run in Gate 2.
- W22 (Order scope without traveller and flights) has no Order-scope fixture yet; the rule itself is the existing Domain rule (`AncillaryReservationUnit`: Order scope ⇒ no traveller).
- Pets (A13/A14) are refused by the existing `SubjectToConfirmation` check; no separate pet test was added.
