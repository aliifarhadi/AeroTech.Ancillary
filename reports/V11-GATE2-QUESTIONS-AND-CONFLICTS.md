# Reservation v11 — Gate 2: Sibling evidence and open points

**Base:** `296197a` + Gate 1 (uncommitted, `reports/V11-GATE1-WIRE.md`: clean build 0 errors; Domain 289/289, Shopping 125+1 skipped/126, Application 273/273).
**Authority:** `docs/Pack-v11` 04 and 12, Owner answers Q1–Q6 (Gate 0) and Q1–Q3 (Gate 1).
**Code:** none yet for Gate 2. It starts when Q1–Q4 are answered.

## 1. How the siblings do it

| Concern | Sibling | Evidence |
|---|---|---|
| One typed part per family | The definition owns one nullable specification per profile (`Baggage`, `Meal`, `AssistedTravel`, …), each in its own table through `OwnsOne(...).OwnedBy("BaggageSpecifications")`; dimensions as three columns; lists as owned rows keyed by (owner id, value) | `Persistence/AncillaryServiceDefinitionAggregate/ServiceSpecificationConfiguration.cs:20-137` (Baggage at 20 … Connectivity at 132), helpers `OwnedBy` 140, `Dimensions` 148, `References` 174 |
| One-of family arguments | `ServiceSpecificationArgs(Baggage?, Seat?, …, Connectivity?)`, exactly one set and it must match the profile | `AncillaryServiceDefinition.Profile.cs:102-116` |
| Static option rules | `TypedSelectionForm.Fields` per variant: required fields, allowed values, min / exclusive min / max; `WithFixedChoices`: the fixed values | `Shopping/Options/TypedSelectionForm.cs:15-70` |
| Option checks in Price | only inside the engine with the offer context (`EvaluateSelectionAsync`); no context-free check exists | `SelectionProofService.cs:79-108`, `ProvisionEvaluation.ProfileRules.cs:56-125` |
| `selected` in Resolve | `SelectedUnitDto(quantity, options by family)` built by `SelectionOptionsMapper.Selected` | `Query/Shopping/AncillaryOffers/Queries/GetAncillaryOffers/SelectionOptionsMapper.cs:14-31` |
| Hold GET today | reads the aggregate through `IAncillaryReservationRepository` and maps it with `AncillaryHoldMapper`; no read model | `GetAncillaryHoldByIdService.cs`, `AncillaryHoldMapper.cs` |
| Definition loaded for every unit | `AncillaryHoldAllocator` loads definition, provision and supplier of every unit, seats included | `AncillaryHoldAllocator.cs:61-63` |
| Column lengths in the catalog | `VariantCode` 8, `ServiceDefinitionRef` 30, `EquipmentKind` 20, `MealCode` 4, `MenuItemRef` 40, assistance SSR codes 4; weights and dimensions `decimal(18,3)` | `ServiceSpecificationConfiguration.cs`, `AncillaryServiceDefinitionConfiguration.cs:16` |

## 2. What Gate 2 builds (pack 04 / 12, no open point)
1. `AncillaryReservationUnit` gains immutable `VariantCode` and `ServiceDefinitionRef`, and one nullable owned selection for the families that carry data (Q1). Built from a one-of `AncillaryReservationSelectionArgs`, checked in the entity: at most one family part, and it must be the family of `VariantCode`.
2. `CoveredFlightIds` moves to owned rows `AncillaryReservationUnitFlights(AncillaryReservationUnitId, FlightId)`; the JSON column is dropped.
3. The port (`HoldSelectionVerifier`) adds: options checked against `TypedSelectionForm.Fields(definition, provision, [], "")` — required fields, allowed values, numeric ranges, no field outside the list — and the fixed values from `WithFixedChoices`. The check lives in Query next to the verifier; Shopping does not change (Owner Gate 0 Q2 (a)). A failure answers 16402 with the field name.
4. The replay comparison adds the stored options (pack L02: same key with other options → 409).
5. GET from the aggregate only: families by `AncillaryVariant.Find(VariantCode).Profile`, products by provision, `serviceDefinitionRef`, `details` from the stored fixed values, `selected` = `SelectedUnitDto(quantity, options)` from the stored options, `seats` with `seatNumber`/`seatMapSelectionId`; every status, also after the definition is retired.
6. One plain migration on the emptied dev tables; generated SQL in the report; Down/Up proven on dev as in v10.
7. Tests: one fast representative per family (D01–D23), R01/R02, W17/W18; report `reports/V11-GATE2-TYPED-DOMAIN.md`.

## 3. Open points

### Q1 — Family tables, names and columns
Names follow the pack's classes (04 §3) and the existing `AncillaryReservationUnit…` table prefix (`AncillaryReservationUnitAllocations`, `AncillaryReservationUnitFlights`):

| Domain class (`AncillaryReservationAggregate/Selections`) | Table | Columns |
|---|---|---|
| `BaggageSelection` | `AncillaryReservationUnitBaggageSelections` | `BagRef` nvarchar(64)?, `MeasuredWeightKg` (18,3)?, `LengthCm`/`WidthCm`/`HeightCm` (18,3)?, `WeightKg` (18,3)?, `EquipmentKind` nvarchar(20)? |
| `MealSelection` | `AncillaryReservationUnitMealSelections` | `MealCode` nvarchar(4)?, `MenuItemRef` nvarchar(40)? |
| `AssistedTravelSelection` | `AncillaryReservationUnitAssistedTravelSelections` | `AssistanceSsrCode` nvarchar(4) |
| `AirportServiceSelection` | `AncillaryReservationUnitAirportServiceSelections` | `TimeWithOffset` datetimeoffset?, `GuestCount` int?, `AirportId` int, `FacilityId` bigint? |
| `ConnectivitySelection` | `AncillaryReservationUnitConnectivitySelections` | `DeviceCount` int? |
| `UpgradeSelection` | `AncillaryReservationUnitUpgradeSelections` | `ToCabinId` int |

No table for pets (GATED), priority (Q3) and seats (the existing unit columns stay). `BagRef` has no length rule anywhere in the source; 64 is my proposal.

**Recommendation:** the table above.

**Answer:** As proposed (confirmed in chat, 2026-10-26).

### Q2 — The airport appointment columns that already exist on the unit
`AncillaryReservationUnits.AppointmentTimeWithOffset` and `GuestCount` (v10) hold the same facts as `AirportServiceSelection`.
- (a) Move them into `AirportServiceSelection` and drop the two unit columns (pack 04 §2 allows it; one truth; the allocator reads them from the selection).
- (b) Keep them on the unit; the airport table holds only `AirportId` and `FacilityId`.

**Recommendation:** (a).

**Answer:** (a)

### Q3 — Priority (A23) `optIn`
The hold refuses a missing or false `optIn`, so a held A23 unit always has `optIn = true`.
- (a) No table; the GET writes `"options":{"priority":{"optIn":true}}` for an A23 unit (pack 04 §3: no table for a family without data).
- (b) A one-column table `AncillaryReservationUnitPrioritySelections(OptIn bit)`.

**Recommendation:** (a).

**Answer:** (a)

### Q4 — Where `VariantCode` and `ServiceDefinitionRef` are taken for every unit
Seat units do not pass through the hold port; their definition is known to the seat verifier (frozen SeatMap code) and to the allocator.
- (a) `AncillaryHoldAllocator` sets both from the definition it already loads for every unit, seats included. One place; no SeatMap change.
- (b) The hold port for services and the seat verifier for seats (changes `SeatSelectionVerifier` and `VerifiedSeatSelection`).

**Recommendation:** (a).

**Answer:** (a)
