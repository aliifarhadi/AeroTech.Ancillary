# V11 Hold contract R1 — questions and conflicts

Date: 2026-10-26. Branch `feat/ancillary-offers-v9`, base `296197a`, Gates 1–3 uncommitted.
Trigger: the Owner found `baggage` listed under `meals` and every other family of the `Service-Holds` request.
Status: answered 2026-10-26. The Owner accepted every recommendation: Q1 (a), Q2 (a), Q3 (a), Q4 (a), Q5 (a). The answers are merged into the pack (01 §J, 02 §1.1–§1.5 and §3, 03, 07 W10/W28/W29); this file is history.

## 1. Findings

I built these faults in Gate 1 (request) and Gate 2 (GET). I did not check each field per family before coding, and I did not report the fault in the pack. Every finding below is backed by source lines read on 2026-10-26.

### F1 — the request carries every family's options under every family (design)

- `src/AeroTech.Ancillary.RestApi/V1/AncillaryReservationAggregate/Requests/HoldAncillaryServicesRequest.cs:12-19`: all eight arrays (`baggages` … `upgrades`) use the same `AncillaryVariantHoldInput`.
- That chain ends in `SelectedUnitInput(int? Quantity, SelectionOptionsInput? Options)` (`.../HoldAncillaryServices/SelectedUnitInput.cs:7`).
- `SelectionOptionsInput` (`.../PriceAncillaryOffers/SelectionOptionsInput.cs:7-13`) has six keys: `baggage`, `pet`, `assistedTravel`, `airportService`, `priority`, `connectivity`.
- Effect:
  - The schema offers all six keys under every family. `meals` and `upgrades` show six option keys, yet no meal or upgrade variant takes any option.
  - A wrong key is not impossible by contract. It is only caught at runtime by `HoldAncillaryServicesValidator.OptionsFit` (`HoldAncillaryServicesValidator.cs:26-63`), which returns 400 "Validation failed" (corrected on 2026-10-27; this line first said 16402/422).
- Source of the fault: Price's `AncillarySelectionLine(OfferItemId, Quantity, Options)` is a flat line across families, so a union fits there. Inside a request that is already grouped by family, the union is a copied field bag.

### F2 — the GET unit is one record for nine families (design)

`src/AeroTech.Ancillary.Query/AncillaryReservationAggregate/Dto/AncillaryUnitHoldDto.cs` defines one `AncillaryUnitHoldDto` for baggages, meals, assistedTravel, airportServices, priorityServices, connectivity, upgrades and seats:

- `seatNumber` and `seatMapSelectionId` are seat-only, but they appear in the schema of every family.
- `selected` is Resolve's `SelectedUnitDto`. Its `options` is `SelectionOptionsDto<JsonObject>`, so the same six keys appear in every family. Each value is an untyped `JsonObject`, so the schema names no fields at all (`AncillaryHoldMapper.cs:100-120` builds typed records and then serialises them to `JsonObject`).

### F3 — variant-level fields that no holdable variant can use (design, small)

- `assistedTravel` options include `infantRef` and `guardianRef` (`AssistedTravelSelectionOptions.cs`).
  - Only A18 Bassinet asks for them (`TypedSelectionForm.cs:39-44`), and A18 is GATED.
  - The holdable A15 and A16 take `assistanceSsrCode` only.
  - The Domain stores only the SSR code (`HoldSelectionVerifier.cs:95`).
- The `pets` array and the `pet` options key are in the schema, but every pet variant (A13/A14) is GATED, so the array is always refused.

### F4 — `unit: {}` is accepted (minor)

`HoldUnitInput(long? TravellerId = null)` and `HoldUnitInputValidator` accept an empty `unit`. That gives two encodings of "no traveller": `unit` omitted and `unit: {}`.

### Not a finding

These fields are common to every family in Offers and Resolve (`OfferUnitDto`, `OfferCoverageDto`, `SelectedUnitDto.Quantity`), and their validators are strict:

- `variantCode`, `products[].provisionId`, `units[].orderServiceId`
- `coverage.flightIds`: not empty, positive, distinct
- `selected.quantity`
- the Receipt `{holdId, reference, status, expiresAt, units[unitId, orderServiceId, status]}`

## 2. Sibling evidence

- **Offers** (`Query/Shopping/AncillaryOffers/Dto/AncillaryOffersDto.cs`) and **Resolve** (`Dto/Service/ServiceResolvedSelectionsDto.cs`) type each family by a generic parameter: `AncillaryVariantGroupDto<BaggageDetailsDto>`, `ServiceResolvedSelectionDto<MealDetailsDto>`, and so on.
- Their `options` and `selected` stayed the shared six-key union: `AncillaryUnitOfferDto.cs:13-14` and `SelectedUnitDto.cs:6-8`. **The same F1/F2 fault exists on the Offers and Resolve output.**
- **Price** input `AncillarySelectionLine` is flat across families, so its union is correct there.

## 3. What each family array can really carry

| Family array | Holdable variants | `selected.options` key | Fields a holdable variant uses |
|---|---|---|---|
| `baggages` | A01, A02, A03, A04, A05, A06 | `baggage` | `bagRef`, `measuredWeightKg` (A04); `dimensions` (A05, A06); `weightKg` (A06) |
| `meals` | A11, A12 | none | — |
| `pets` | none (A13/A14 GATED) | — | — |
| `assistedTravel` | A15, A16 | `assistedTravel` | `assistanceSsrCode` |
| `airportServices` | A20, A21, A22 | `airportService` | `timeWithOffset`, `guestCount` |
| `priorityServices` | A23 | `priority` | `optIn` (must be `true`) |
| `connectivity` | A24 | `connectivity` | `deviceCount` |
| `upgrades` | A10 (`FixedAncillary`) | none | — |

Inside a family, the variant still chooses the subset (for example, A04 vs A05). `HoldSelectionFields` checks that subset against `TypedSelectionForm.Fields`, the same form Offers returns per unit.

## 4. Questions

### Q1 — request: options typed per family

**(a) Recommended.** Use the Offers generic pattern: `AncillaryVariantHoldInput<TSelected>` → `AncillaryProductHoldInput<TSelected>` → `AncillaryUnitHoldInput<TSelected>`, with one `selected` record per family.

- `baggages` → `selected {quantity?, options? {baggage}}`.
- The same pattern applies to `assistedTravel`, `airportServices`, `priorityServices` and `connectivity`.
- `meals` and `upgrades` → `selected {quantity?}` with no `options`.
- A valid request stays byte-identical to the pack 02 §1.1 examples and to the `selected` that Resolve writes.
- A wrong key becomes a JSON binding error (400, `JsonUnmappedMemberHandling.Disallow`) instead of a command-validator error (also 400, "Validation failed"; corrected on 2026-10-27, this line first said 16402/422).
- `OptionsFit` is deleted.

**(b)** Drop the family key, so that `baggages[]…selected.options` is `{bagRef, …}` directly.

- Fewer records.
- But the request no longer equals Resolve's `selected`, so the caller has to reshape it.

### Q2 — GET: unit typed per family

**(a) Recommended.**

- Type `selected` per family as in Q1, with typed option records instead of `JsonObject`.
- Seat units get their own unit record that carries `seatNumber` and `seatMapSelectionId`. Every other family loses those two fields.
- The stored data and SQL do not change, so no migration is needed.

**(b)** Keep the GET as it is.

### Q3 — AssistedTravel and Pets in the Hold

**(a) Recommended.**

- The Hold's `assistedTravel` options carry `assistanceSsrCode` only.
- The `pets` array leaves the Hold request until a stage opens pets. A client that sends it gets 400.

**(b)** Keep both, refused at runtime (today: 16402 for an unused field, 16419 for a pet).

### Q4 — `unit.travellerId` required when `unit` is sent

**(a) Recommended:** `HoldUnitInput(long TravellerId)`, so `unit` is either omitted or carries a traveller.

**(b)** Keep it optional.

### Q5 — the same union in the Offers and Resolve output

**(a) Recommended:** fix it in a separate step after the Hold correction, and only on the Owner's order, because the frontend reads those DTOs.

**(b)** Fix it in the same round as Q1/Q2.

## 5. Effect of the recommended answers

- Pack 02 §1.1/§1.3/§1.4, 03 and the W rows of 07 are updated with the answers. The pack keeps LF.
- Gate 1 and Gate 2 records, validators, mapper and tests change. The reservation freeze pins are recomputed.
- No DB migration.
- Gate 3 (inventory) is independent of this contract. Its code is done and its test run is in progress. Its report waits until this round is answered.
