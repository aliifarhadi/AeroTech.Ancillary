# Reservation v11 — Gate 2: Typed domain and durable selections

**Base:** `296197a` + Gate 1 (uncommitted, `reports/V11-GATE1-WIRE.md`).
**Authority:** `docs/Pack-v11` 04 and 12; Owner answers in `reports/V11-GATE2-QUESTIONS-AND-CONFLICTS.md` (Q1 as proposed, Q2–Q4 (a)).
**Commit / push:** none.
**Migration:** `20261026190047_V11ReservationSelections`, applied to the dev DB (`DotAirAncillary`, reservation tables were emptied before, Owner Gate 0 Q5); Down to `20261026114827_V10ReservationAllocations` and Up again proven on dev; `migrations has-pending-model-changes` → "No changes have been made to the model since the last migration."

## 1. Storage (ER)

```
AncillaryReservations 1 ──< AncillaryReservationUnits
                                 │  + VariantCode nvarchar(8) NOT NULL, ServiceDefinitionRef nvarchar(30) NOT NULL
                                 │  − CoveredFlightIds nvarchar(max) (JSON)   − AppointmentTimeWithOffset   − GuestCount
                                 ├──< AncillaryReservationUnitAllocations                 (unchanged)
                                 ├──< AncillaryReservationUnitFlights                     PK (AncillaryReservationUnitId, FlightId)
                                 ├── 0..1 AncillaryReservationUnitBaggageSelections        BagRef nvarchar(64)?, MeasuredWeightKg (18,3)?, LengthCm/WidthCm/HeightCm (18,3)?, WeightKg (18,3)?, EquipmentKind nvarchar(20)?
                                 ├── 0..1 AncillaryReservationUnitMealSelections           MealCode nvarchar(4)?, MenuItemRef nvarchar(40)?
                                 ├── 0..1 AncillaryReservationUnitAssistedTravelSelections AssistanceSsrCode nvarchar(4)
                                 ├── 0..1 AncillaryReservationUnitAirportServiceSelections TimeWithOffset datetimeoffset?, GuestCount int?, AirportId int, FacilityId bigint?
                                 ├── 0..1 AncillaryReservationUnitConnectivitySelections   DeviceCount int?
                                 └── 0..1 AncillaryReservationUnitUpgradeSelections        ToCabinId int
```
Every child table is keyed by `AncillaryReservationUnitId` (FK, cascade). No pet, priority or seat table (Owner Q1, Q3; seat facts stay on the unit). No `nvarchar(max)` column remains in any `AncillaryReservation*` table (checked by SQL in test `V11_D01…`).

## 2. Domain

| Type | Members | Rules (all refuse with 16402) |
|---|---|---|
| `AncillaryReservationUnit` (changed) | `VariantCode`, `ServiceDefinitionRef`, `Flights` (owned rows) + computed `CoveredFlightIds`, nullable `Baggage`, `Meal`, `AssistedTravel`, `AirportService`, `Connectivity`, `Upgrade` | variant known (`AncillaryVariant.Find`), ref 1–30; at most one selection part and only of the variant's family; a seat unit takes none |
| `BaggageSelection` | `BagRef`, `MeasuredWeightKg`, `Dimensions` (`SelectedDimensions`), `WeightKg`, `EquipmentKind` | at least one value; measures positive, below 1,000,000, at most 3 decimals (`SpecificationRules.IsMeasure`); `BagRef` trimmed text ≤ 64; `EquipmentKind` code ≤ 20 |
| `MealSelection` | `MealCode`, `MenuItemRef` | at least one; meal code exactly 4; menu ref code ≤ 40 |
| `AssistedTravelSelection` | `AssistanceSsrCode` | code of exactly 4 |
| `AirportServiceSelection` | `TimeWithOffset`, `GuestCount`, `AirportId`, `FacilityId` | guests ≥ 0, airport > 0, facility > 0 |
| `ConnectivitySelection` | `DeviceCount` | > 0 |
| `UpgradeSelection` | `ToCabinId` | > 0 |
| Arguments | `AncillaryReservationSelectionArgs(Baggage?, Meal?, AssistedTravel?, AirportService?, Connectivity?, Upgrade?)` and one args record per family (pattern of `ServiceSpecificationArgs`) | — |

Each selection class has `Create(args)`, `ToArgs()` and, where the customer chooses something, `IsChosenBy(args)` (customer fields only). Pattern copied from the definition's specifications (`MealSpecification`, `ServiceSpecificationConfiguration`).

## 3. Where each value comes from

```
Hold request ─► replay first: stored hold? compare request facts + chosen options (fixed facts not compared)
             ─► HoldSelectionVerifier (Query, port of Owner Gate 0 Q2)
                   variant/family = definition                      → else 16402
                   AncillaryHoldChecks.EnsureHoldable               → confirmation, A17/A18/A19, quote → 16419
                   HoldSelectionFields.Check(TypedSelectionForm.Fields(definition, provision))
                                                                    → required, allowed values, ranges, no extra field → 16402
                   A23: optIn must be true (Price's rule)           → 16402
                   TypedSelectionForm.WithFixedChoices              → EquipmentKind, MealCode, MenuItemRef, ToCabinId, AirportId, FacilityId
             ─► AncillaryHoldAllocator: Accept, then VariantCode + ServiceDefinitionRef from the definition (Owner Q4), slot demand from the airport part (Owner Q2)
             ─► AncillaryReservation.Hold → unit + selection rows
GET          ─► AncillaryHoldMapper from the aggregate only
```
- The check over `TypedSelectionForm.Fields` lives in Query (`HoldSelectionFields`); Shopping is unchanged.
- An A02 package and an A24 plan are the stored `ServiceDefinitionRef`; no separate column.
- Families with no chosen and no fixed value (A01, A02, A05, A23) store no selection row.

## 4. GET `Service-Holds/{holdId}`
Root `AncillaryHoldDto(holdId, orderId, reference, status, expiresAt, baggages?, meals?, assistedTravel?, airportServices?, priorityServices?, connectivity?, upgrades?, seats?)`; a family with no unit is omitted. Families come from `AncillaryVariant.Find(VariantCode).Profile`; seat units (those with a seat number) go to `seats`. Products group units by provision; `details` carries only the stored fixed values; `selected.options` is written with the same JSON options as Price's `SelectedJson` (Web names, nulls omitted). Values asserted by `V11_D01…` (trimmed):
```json
{"holdId":"…","orderId":"…","reference":"ORD-…","status":"Held","expiresAt":"…",
 "baggages":[
  {"variantCode":"A03","products":[{"provisionId":"…","serviceDefinitionRef":"OVERWEIGHT_32","units":[
    {"unitId":"…","orderServiceId":"…","status":"Held","unit":{"travellerId":"71"},"coverage":{"flightIds":["301"]},
     "selected":{"quantity":1,"options":{"baggage":{"bagRef":"B-32","measuredWeightKg":27.500}}}}]}]},
  {"variantCode":"A06","products":[{"provisionId":"…","serviceDefinitionRef":"SPORT_BIKE","details":{"equipmentKind":"BIKE"},"units":[
    {"…":"…","selected":{"quantity":1,"options":{"baggage":{"dimensions":{"lengthCm":150.000,"widthCm":60.000,"heightCm":30.000},"weightKg":18.250}}}}]}]}],
 "meals":[{"variantCode":"A11","products":[{"…":"…","details":{"mealCode":"VGML"}}]},
          {"variantCode":"A12","products":[{"…":"…","details":{"menuItemRef":"MENU_PASTA_01"},"units":[{"…":"…","selected":{"quantity":2}}]}]}],
 "assistedTravel":[{"variantCode":"A15","products":[{"…":"…","units":[{"…":"…","selected":{"quantity":1,"options":{"assistedTravel":{"assistanceSsrCode":"WCHR"}}}}]}]}],
 "airportServices":[{"variantCode":"A20","products":[{"…":"…","details":{"airportId":1},"units":[{"…":"…","selected":{"quantity":1,"options":{"airportService":{"guestCount":1}}}}]}]}],
 "priorityServices":[{"variantCode":"A23","products":[{"…":"…","units":[{"…":"…","selected":{"quantity":1,"options":{"priority":{"optIn":true}}}}]}]}],
 "connectivity":[{"variantCode":"A24","products":[{"…":"…","units":[{"…":"…","selected":{"quantity":1,"options":{"connectivity":{"deviceCount":2}}}}]}]}],
 "upgrades":[{"variantCode":"A10","products":[{"…":"…","details":{"toCabinId":2}}]}]}
```
Decimals are written with the stored scale (`27.500`); the value equals the request (`27.5`).

## 5. Migration SQL (generated, `V10ReservationAllocations` → `V11ReservationSelections`)
```sql
ALTER TABLE [Ancillary].[AncillaryReservationUnits] DROP COLUMN [AppointmentTimeWithOffset];   -- default-constraint guard generated by EF before each drop
ALTER TABLE [Ancillary].[AncillaryReservationUnits] DROP COLUMN [CoveredFlightIds];
ALTER TABLE [Ancillary].[AncillaryReservationUnits] DROP COLUMN [GuestCount];
ALTER TABLE [Ancillary].[AncillaryReservationUnits] ADD [ServiceDefinitionRef] nvarchar(30) NOT NULL DEFAULT N'';
ALTER TABLE [Ancillary].[AncillaryReservationUnits] ADD [VariantCode] nvarchar(8) NOT NULL DEFAULT N'';
CREATE TABLE [Ancillary].[AncillaryReservationUnitAirportServiceSelections] ([AncillaryReservationUnitId] bigint NOT NULL, [TimeWithOffset] datetimeoffset NULL, [GuestCount] int NULL, [AirportId] int NOT NULL, [FacilityId] bigint NULL, PK, FK … ON DELETE CASCADE);
CREATE TABLE [Ancillary].[AncillaryReservationUnitAssistedTravelSelections] ([AncillaryReservationUnitId] bigint NOT NULL, [AssistanceSsrCode] nvarchar(4) NOT NULL, PK, FK … CASCADE);
CREATE TABLE [Ancillary].[AncillaryReservationUnitBaggageSelections] ([AncillaryReservationUnitId] bigint NOT NULL, [BagRef] nvarchar(64) NULL, [MeasuredWeightKg] decimal(18,3) NULL, [LengthCm] decimal(18,3) NULL, [WidthCm] decimal(18,3) NULL, [HeightCm] decimal(18,3) NULL, [WeightKg] decimal(18,3) NULL, [EquipmentKind] nvarchar(20) NULL, PK, FK … CASCADE);
CREATE TABLE [Ancillary].[AncillaryReservationUnitConnectivitySelections] ([AncillaryReservationUnitId] bigint NOT NULL, [DeviceCount] int NULL, PK, FK … CASCADE);
CREATE TABLE [Ancillary].[AncillaryReservationUnitFlights] ([FlightId] bigint NOT NULL, [AncillaryReservationUnitId] bigint NOT NULL, PK ([AncillaryReservationUnitId], [FlightId]), FK … CASCADE);
CREATE TABLE [Ancillary].[AncillaryReservationUnitMealSelections] ([AncillaryReservationUnitId] bigint NOT NULL, [MealCode] nvarchar(4) NULL, [MenuItemRef] nvarchar(40) NULL, PK, FK … CASCADE);
CREATE TABLE [Ancillary].[AncillaryReservationUnitUpgradeSelections] ([AncillaryReservationUnitId] bigint NOT NULL, [ToCabinId] int NOT NULL, PK, FK … CASCADE);
```
The full script (94 lines, one transaction) was generated with `ef migrations script`; the abridged form above lists every statement. The two `DEFAULT N''` columns are safe because the tables were empty (Owner Q5); no backfill.

## 6. Tests
Clean build `dotnet build AeroTech.Ancillary.sln --no-incremental`: **0 errors, 61 warnings** (same count as v10 and Gate 1; none in a changed file).

| Suite | Discovered | Executed | Result |
|---|---|---|---|
| Domain.ConformanceTests | 292 | 292 | 292 passed (Gate 1: 289) |
| Shopping.Tests | 126 | 126 | 125 passed, 1 skipped (unchanged) |
| Application.AcceptanceTests | 276 | 276 | 276 passed (Gate 1: 273), 2 min 4 s |

No `aborted` or `crash` line in any log.

| Test ID | Test |
|---|---|
| D04, D05, D06, D07, D08, D10, D21 | `V11_D01_D10_D21_W17_W18_every_family_reads_back_its_chosen_and_fixed_values_from_sql` — one hold with A03, A06, A10 (fixed), A11, A12, A15, A20, A23, A24; every chosen and fixed value read back through the GET; row counts in the selection tables |
| D01, D02, D09 | represented by A03/A12 (quantity), A06 (dimensions) and `serviceDefinitionRef` of every product in the same test (one representative per family) |
| D16, W18 | same test: one unit released and every provision retired, then the GET is identical apart from that unit's status |
| W17 | same test (family grouping, products by provision, details, seats omitted) |
| R02 | same test: no `nvarchar(max)` column in `AncillaryReservation*` |
| D23 | `V11_D23_an_option_outside_the_definition_rules_is_refused_before_any_write` (SSR not allowed, SSR missing, too many devices, weight at the exclusive minimum, `bagRef` missing, a field the definition does not ask for, `optIn` false; no row written) |
| D20 | Domain `V11_D20_a_unit_keeps_at_most_one_selection_and_only_of_its_own_family` |
| D02, D04 (exact storage) | Domain `V11_D02_D04_a_selection_refuses_a_value_it_cannot_store_exactly` |
| L02 | Domain `V11_L02_a_replay_compares_the_chosen_options_and_not_the_fixed_ones` |
| R01 | migration commands above (apply, Down, Up, no pending changes) |
| — | `V11_every_application_validator_can_be_built_by_the_container` (see 7.1) |

New SQL tests take 1–3 s each. Freeze pins (P2_X07, V12_C04, X20): the reservation set grew from 106 to 115 files; hash `712C9524EBDEDD21B04593B358F927466FB3CCCC50638C93AF1C7A76E3750E00`.

## 7. Discrepancies and points decided inside the answers (for review)
1. **Gate 1 defect, fixed.** The Gate 1 nested validators took the family as a constructor argument. Application registers validators by assembly scan (`AddValidatorsFromAssembly`) and the container validates on build in Development, so the host would not have started; the Gate 1 tests build services by hand and missed it (the EF tool exposed it). Now the nested validators have no argument and the "options key must fit the family" rule sits in `HoldAncillaryServicesValidator`, one rule per array. A container test guards it.
2. **Seat appends go through the allocator.** `AppendSeatsAsync` skipped the allocator, so appended seats had no `VariantCode`. They now pass the same `Accept` checks (active definition and provision, local supplier) as a seat create and get the stamp (Owner Q4 (a) says "every unit").
3. **`EnsureHoldable` extracted.** The refusals that do not depend on the unit (confirmation, A17/A18/A19, quote) moved into `AncillaryHoldChecks.EnsureHoldable`; `Accept` calls it at the same place, and the verifier calls it before checking options so a GATED variant answers 16419, not a field error.
4. **Airport appointment is the customer's choice now.** v10 refused an appointment or guests when the policy was not a slot policy ("inventory facts belong only to a slot policy") and required an appointment before guests. Both rules are gone: the values are what the customer chose (`TypedSelectionForm` allows guests without an appointment when the service needs none) and are stored for any policy; a slot policy still requires the appointment. Tests `V10_G2_D02` and `V10_I27` were rewritten accordingly.
5. **M1 pin `M1_C08_M05`.** It forbade any `…Ref` property on the unit; it now allows `ServiceDefinitionRef`, the snapshot of Owner Gate 0 Q4. The unit still references the definition by `ServiceDefinitionId`.
6. **Price mapping shared.** The options → `AncillarySelection` mapping moved from Price's `SelectionVerifier` to `Query/Shopping/Common/TypedSelectionMapper` and is used by both verifiers; no behaviour change.
7. **`DeviceCount` nullable column** as approved in Q1, while the domain requires a positive value whenever the row exists.
8. **Open, not decided:** an `AirportSlot` policy can be active for a definition with `RequiresSpecificAppointment = false`. Such a product can never be held: the customer is not asked for a time (static check refuses `timeWithOffset`) and the slot needs one. The test catalog had this combination; the slot test product now requires an appointment. Should activating a slot policy require `RequiresSpecificAppointment`? (Policy aggregate, not changed; dev has no local policy today.)

## 8. Not in Gate 2
Gate 3 (inventory from the typed selection, retire-with-usage guard) and Gate 4 (seat facade and lifecycle) are not started.
