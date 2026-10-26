# 07 — ماتریس پذیرش مستقل v11 — Contract/Domain/Inventory

**قانون PASS:** فقط آزمون واقعاً اجراشده با نام Test Method و فرمان و نتیجه و Source SHA؛ `GATED`/`NOT VERIFIED` هرگز PASS محسوب نشود. Source `reports/V10-RESERVATION-FINAL-CLOSURE.md` نشان می‌دهد E2E Ordering و Live Local پیش‌تر تأیید نشده بود؛ v11 نباید آن‌ها را سبز فرض کند.

## W — قرارداد HTTP و DTO

| ID | Test setup / Input | Expected |
|---|---|---|
| W01 | Create با `baggages[]→variantCode→products[]→units[]` درست | 201 Receipt کوچک |
| W02 | ورودی با `services[]` قدیمی | 400 و هیچ write |
| W03 | ورودی `coverageScope` روی Unit | 400؛ Scope از Provision |
| W04 | ورودی `serviceDefinitionId` | 400؛ Definition از Provision |
| W05 | ورودی `inventoryFacts` | 400؛ Airport Selected source |
| W06 | `productKey` نمایشی به جای `provisionId` | 400؛ ممنوع |
| W07 | Product با variantCode دیگر | 422؛ بدون write |
| W08 | Product با Family غلط | 422؛ بدون write |
| W09 | یک `orderServiceId` تکراری بین Familyها | reject all |
| W10 | selected options با کلید Family اشتباه | 400 at binding: the key is not a member of that family's `selected` (Owner R1 Q1) |
| W11 | unknown JSON field در nested selected | 400 |
| W12 | fixed Qty=1 و selected omitted | Unit.Quantity=1 |
| W13 | انتخاب QuantityChoice بدون selected.quantity | 422 |
| W14 | Qty خارج Min/Max | 422 |
| W15 | نبود کل Unitها در Familyها | 400/422 و بدون write |
| W16 | Receipt فقط holdId/reference/status/expiresAt/units minimal | exact shape |
| W17 | GET typed grouping and fields, including the `seats` family and the product `serviceDefinitionRef`/`details` | API contract exact (02 §3) |
| W18 | GET تاریخچه Released/Cancelled/Expired با selected immutable | same values |
| W19 | `unit.travellerRef` Offer scoped در Hold | strict reject |
| W20 | `coverage.flightRefs` string در Hold به جای `flightIds` numeric | strict reject |
| W21 | `coveredFlightIds` قدیمی روی Root | 400 |
| W22 | PerOrder بدون Traveller/Flights در Variant مجاز | accepted when Policy allows |
| W23 | Sector با 0/2 flightIds | reject |
| W24 | Portion دو flightIds distinct | one Unit, both stock sources |
| W25 | GET `seats` family: variant → product → unit with `seatNumber` and `seatMapSelectionId` | exact shape; no `pricingId` (Owner Q3) |
| W26 | GET product `details` per family | only `equipmentKind` / `mealCode`, `menuItemRef` / `toCabinId` / `airportId`, `facilityId`; omitted when the family has none (Owner Q4) |
| W27 | Wire representation | every `long` written as a JSON string, enums by name; numeric ids accepted on input (02 §0) |
| W28 | `pets` array, `assistedTravel.infantRef`/`guardianRef`, `unit:{}` | 400: the first two are not members; `unit:{}` fails the shape rule (Owner R1 Q3, Q4) |
| W29 | GET `selected` typed per family; `seatNumber`/`seatMapSelectionId` only on seat units | exact shape, null members omitted (Owner R1 Q2) |

## D — Domain انتخاب و تاریخچه

| ID | Test setup / Input | Expected |
|---|---|---|
| D01 | A01 Qty2 → SQL → GET | same quantity and covered flights |
| D02 | A04 dimensions → SQL → GET | exact length/width/height |
| D03 | A13/A14 pet Hold | explicit refusal before any write; the Definition forces `SubjectToConfirmation` (Owner Q1, GATED) |
| D04 | A15 WCHR vs WCHS در دو Unit | distinct immutable choice |
| D05 | A20 timeWithOffset/guestCount → SQL → GET | offset semantically stable; instant equal |
| D06 | A24 deviceCount → SQL → GET | exactly selected; the plan is the product `serviceDefinitionRef` |
| D07 | A11 fixed MealCode snapshot before retirement | GET same MealCode without DB live |
| D08 | A12 `menuItemRef` fixed by the Definition → SQL → GET | same value after the Definition is retired |
| D09 | A02 package = `serviceDefinitionRef` → SQL → GET | same value after the Definition is retired |
| D10 | A06 `equipmentKind` fixed by the Definition → SQL → GET | same value in product `details` |
| D11 | A17 medical fields without security | GATED, no PII persisted |
| D12 | A19 guardian proof missing | GATED |
| D13 | A09 ExtraSeat unsupported | no false success |
| D14 | A10 external quote without provider | explicit unsupported |
| D15 | after Hold option changes attempted by caller | no mutable Update endpoint |
| D16 | retired Definition after Held | GET read historic selection without relinking |
| D17 | 2 different provisions same Traveller/flight | allowed if real business guards permit |
| D18 | duplicate orderServiceId | rejected before write |
| D19 | Unit VariantCode derived Definition, cannot spoof | 422 mismatch |
| D20 | Owner invariants: Unit only one family selection | no cross-family detail |
| D21 | A10 `FixedAncillary` → SQL → GET | `toCabinId` snapshotted; same after the Definition is retired |
| D22 | A18 bassinet Hold | explicit refusal before any write (offer-scoped refs, Owner Q1) |
| D23 | Option value outside the Definition/Provision rules (SSR not allowed, weight or dimensions out of range, too many guests or devices) | 422 before any write; static depth only (Owner Q2) |

## I — Inventory و همزمانی SQL واقعی

| ID | Test setup / Input | Expected |
|---|---|---|
| I01 | Unlimited | Held and zero allocation |
| I02 | Missing / Draft / Retired policy | reject not assumed Unlimited |
| I03 | Count shared source across Products | aggregated demand |
| I04 | FixedKg × Quantity | exact weight allocation |
| I05 | CountPlusWeight second fails | entire transaction rollback |
| I06 | Portion two Flights one lacks source | no partial allocation |
| I07 | AirportSlot 2 overlapping contiguous | 2 slice allocations |
| I08 | AirportSlot gap | reject entire Hold |
| I09 | guestCount increases persons | according to policy |
| I10 | no appointment when Slot requires | reject, no UtcNow substitute |
| I11 | two Orders compete last Count | exactly one commit SQL |
| I12 | two Orders compete last Kg | exactly one commit SQL |
| I13 | two Orders compete last AirportSlot | exactly one commit SQL |
| I14 | Admin Adjust below active usage | reject |
| I15 | Admin Retire source activeUsage>0 | reject |
| I16 | no mutation of TotalCapacity on Hold | unchanged |
| I17 | confirmed allocations counted even beyond expiry | counted |
| I18 | expired Held not counted before poller | capacity available |
| I19 | unsupported Supplier | explicit fail before write |
| I20 | AcceptedWeight/per-kilo unsupported | explicit fail |
| I21 | transaction fails after Save | rollback all new rows |
| I22 | 2 products consuming same source in same Hold | sum before shortage check |
| I23 | true SQL lock timeout | standard conflict; no oversell |

## L — Lifecycle و Seat facade

| ID | Test setup / Input | Expected |
|---|---|---|
| L01 | same idempotencyKey and equal typed payload | same Hold/Unit IDs |
| L02 | same idempotencyKey changed options | 409 |
| L03 | same key but arrays reordered semantically | same Hold |
| L04 | confirm after one Unit released | other held confirm, released stays |
| L05 | release one Held by unitIds | others unchanged |
| L06 | release one Confirmed by unitIds | 409 all-or-nothing |
| L07 | cancel one Confirmed and preserve reason | others unchanged |
| L08 | cancel same unit with different reason | 409 |
| L09 | canceled/expired Unit remains GET with selection | historical intact |
| L10 | duplicate release/cancel replay | no double release |
| L11 | expired before confirm | reject even if poller delayed |
| L12 | SeatMap standalone commercial | 1 Hold + Seat Unit, zero allocations |
| L13 | SeatMap append into mixed Hold | same holdId; existing Units untouched |
| L14 | SeatMap append replay | no new Units/seat verifier call |
| L15 | SeatMap physical FlightFlow requests | exactly ZERO from Ancillary |
| L16 | SeatMap/Price/Resolve APIs source regression | unchanged contract |
| L17 | no Order API/reader call during any Reservation operation | exactly ZERO |
| L18 | Seat cancel commercial | no FlightFlow release call |
| L19 | empty partial `unitIds` vs absent | explicit defined semantics test |
| L20 | partial mixed valid/foreign unitIds | no mutations |
| L21 | no duplicated aggregate/service by family | architecture inspection |

## R — استقرار و انطباق

| ID | Test setup / Input | Expected |
|---|---|---|
| R01 | v11 migration on dev after all reservation rows are deleted (Owner Q5) | plain migration applies; Down/Up proven on a test database |
| R02 | relational Selected details and lists | no JSON list column introduced |
| R03 | Build + discovered and executed tests | counts match |
| R04 | SQL concurrent tests are real | no fake in-memory proof |
| R05 | HTTP with at least 3 families using supported real policies | real DB evidence; dev dataset per Owner Q6: A01, one A15 with `Immediate` confirmation, one A20 with a local Supplier and an Unlimited policy, and a seat append |
| R06 | Live Local HTTP without source connections | GATED not PASS |
| R07 | Ordering actual consumer end-to-end | NOT VERIFIED until tested independently |
| R08 | API schema snapshot DTOs only named source fields | file-level evidence |
| R09 | no commit/push without Owner authorization | report |
| R10 | no prior v10 conflicting contract left in executable prompts | audit |

## خروجی گزارش الزامی ایجنت
`Test ID | Source method | command | detected count | executed count | PASS/FAIL/GATED | evidence path | commit`؛ مشخصات SQL Database/transaction برای I11–I13 الزامی است؛ برای هر Variant فعال Round-trip شامل exact data equality و GET after Retire انجام شود.
