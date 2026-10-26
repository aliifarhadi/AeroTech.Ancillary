# 04 — Domain Aggregate / Entity / Value Object / EF — قرارداد فیلدبه‌فیلد

## 1. Aggregate Root: `AncillaryReservation` — **حفظ v10**
| Field | CLR type | Owns / Invariant |
|---|---|---|
| `Id` | `long` | HoldId اصلی، Generated یک‌بار |
| `IdempotencyKey` | `string` <=128 | Create canonical replay؛ در Unique Index موجود |
| `OrderId` | `long` | فقط Caller correlation، بدون Order lookup |
| `Reference` | `string` <=128 | Caller reference، ثابت |
| `RequestedExpiresAt` | `DateTimeOffset?` legacy | برای Hold v11 required future؛ null فقط historical legacy |
| `ExpiresAt` | `DateTimeOffset?` legacy | موعد واقعی؛ بعد Confirm برای تاریخچه تغییر نکند |
| `Status` | `AncillaryReservationStatus` existing | Held/Confirmed/Released/Expired/Cancelled؛ numeric values unchanged |
| `CreatedAt`, `UpdatedAt` | `DateTimeOffset` | Audit |
| `_units / Units` | `List<AncillaryReservationUnit>` | owned children؛ تغییر فقط از Root |

**رفتار Root:** `Hold`, `AppendSeats`, `Confirm`, `Release(unitIds?)`, `Cancel(reasonCode,unitIds?)`, `Expire`. Root همه تغییرهای status/append را هدایت می‌کند. No new per-family Aggregate, no outbox, no Pending/Saga, no invented intermediate order.

## 2. Entity: `AncillaryReservationUnit` — حفظ هویت؛ افزودن Selection معنایی
| Field | CLR type | Source | Rule |
|---|---|---|---|
| `Id` | long | Generator | UnitId؛ مرجع Partial عملیات |
| `AncillaryReservationId` | long | Root | FK |
| `OrderServiceId` | long | `units[].orderServiceId` | >0، یکتا در Request، بدون Ordering membership check |
| `ServiceDefinitionId` | long | `Provision.ServiceDefinitionId` | **فقط internal**؛ از Caller دریافت نشود |
| `ProvisionId` | long | `products[].provisionId` | >0؛ Active |
| `VariantCode` | string(3) | `Definition.VariantCode` | immutable، مورد تأیید Family |
| `ServiceDefinitionRef` | string | `Definition.ServiceDefinitionRef` | snapshot at Hold (Owner Q4); GET `products[].serviceDefinitionRef`; also the A02 package and the A24 plan |
| `CoverageScope` | existing enum | `Provision.CoverageScope` | internal derived، never wire input |
| `TravellerId` | long? | `units[].unit.travellerId` | required only Pax-scoped؛ no membership check |
| `CoveredFlightIds` | collection<long> | `units[].coverage.flightIds` | positive/distinct; Sector exactly 1; stored in the child table `AncillaryReservationUnitFlights` (Owner Q5), not in a JSON column |
| `Quantity` | int | `selected.quantity` یا fixed 1 | immutable after Hold; check Provision.Quantity |
| `Status` | existing enum | Root operation | released/expired/cancelled historical retained |
| `CancellationReasonCode` | string? <=50 | Cancel operation | فقط Cancelled |
| `SelectedDetails` | typed immutable owned/child | `selected.options` + fixed source snapshot | exactly one family-specific choice when required، no generic JSON |
| `Allocations` | `IReadOnlyCollection<AncillaryReservationUnitAllocation>` | Local Policy/Source | ledger موجود، no direct caller values |
| `SeatMapSelectionId`, `SeatNumber`, `PricingId` | موجود | Seat verifier | Seat commercial only؛ untouched |
| `AppendIdempotencyKey` | string? | Seat append request | existing |
| `AppointmentTimeWithOffset`, `GuestCount` | existing nullable | Airport Selected | می‌توان به Airport VO انتقال/Encapsulate کرد با migration امن، نه duplicate truth |
| `StockPoolId`, `ProviderUnitRef` | legacy existing | none new | بدون استفاده در Wire؛ حذف schema فقط با audit مستقل |

## 3. Selectionهای تخصصی — **فقط در صورت تفاوت واقعی داده**
`SelectedDetails` در Domain یک گروه VO/Owned typed است؛ **نه `JsonObject`, `string SelectedJson`, dictionary یا TPH/TPT 24 نوعی**. Suggested bounded representation:

| Family selection type | Persisted selected fields (CLR) | Fixed details captured at Hold if needed | Forbidden |
|---|---|---|---|
| BaggageSelection | `BagRef:string?`, `MeasuredWeightKg:decimal?`, `Dimensions:SelectedDimensions?`, `WeightKg:decimal?` | `EquipmentKind:string?` (A06) | inventing a package or equipment value |
| MealSelection | — | `MealCode:string?` (A11), `MenuItemRef:string?` (A12) | any meal value not taken from the Definition |
| PetSelection | not built while A13/A14 are GATED (Owner Q1) | — | storing pet data without an approved confirmation flow |
| AssistedTravelSelection | `AssistanceSsrCode:string` (A15/A16) | — | A17/A18/A19 data while GATED |
| AirportServiceSelection | `TimeWithOffset:DateTimeOffset?`, `GuestCount:int?` | `AirportId:int`, `FacilityId:long?` | a general `inventoryFacts` or a source id from the caller |
| PrioritySelection | `OptIn:bool` | هیچ مگر Fixed Service Code برای Fulfillment | false unit held |
| ConnectivitySelection | `DeviceCount:int?` | — (the plan is the unit's `ServiceDefinitionRef`) | a guessed plan code |
| UpgradeSelection | — | `ToCabinId:int` (A10 `FixedAncillary` only) | enabling a quote without a real quote contract |
| SeatSelection | `SeatMapSelectionId:string`, `SeatNumber:string`, `PricingId:long` existing | — | Physical FlightFlow SeatHeld flag |

**نکته Storage:** هر Family فعال در صورت داشتن داده Typed، یک **Owned child/table** کم‌فیلد (1:0..1) برای Unit. List مثل `DocumentAcknowledgements[]`, `IncludedComponentCodes[]`, `CoveredFlightIds[]` طبق قانون `CLAUDE.md` در child table relational؛ **نه JSON nvarchar(max)**. اگر یک Family فعلاً هیچ داده Selected ندارد، جدول خالی برای آن نسازید. تعداد جدول‌های نهایی از Variantهای واقعی قابل رزرو نتیجه شود؛ **هیچ اجباری به ۹ جدول از روز اول نیست**.

## 4. Allocation Entity — **بدون تغییر schema و حسابداری**
| Field | CLR | Rule |
|---|---|---|
| `Id` | long | child ID |
| `AncillaryReservationUnitId` | long | FK to Unit |
| `ResourceKind` | existing `InventoryResourceKind` | Count/Weight/Slot |
| `InventorySourceId` | long | source id از policy/lookup محلی، نه Caller |
| `Amount` | `decimal(19,3)` | >0، Weight ≤3 decimals؛ Count/Slot integral |

Active usage = Confirmed + Held nonexpired؛ Released/Cancelled/Expired ignore. `TotalCapacity` و `CapacityKg` تغییر نکنند. SQL `sp_getapplock` روی Sourceهای هم‌پوشان و یک Transaction. Preservation of `InventoryLedger` و ۳ Source aggregate موجود الزامی است.

## 5. Invariants and transition ownership
- Root `Hold` موفق **هیچ Unit بدون انتخاب معنایی صحیح** نمی‌سازد؛ لزوماً SelectedDetails row برای fixed item لازم نیست ولی GET باید fixed facts snapshotشده را بازسازی کند.
- `CoverageScope` از Provision؛ `VariantCode` از Definition، `InventoryAuthority/Pattern/ResourceId` از Policy/Source؛ Request نمی‌تواند این‌ها را جعل کند.
- هر `orderServiceId` در درخواست یکتا؛ دو Provision مختلف برای یک Traveller/Flight در صورت مجاز بودن کسب‌وکار مجاز.
- تغییر Options/Qty بعد از Hold ممنوع؛ تغییر واقعی با Release/Cancel و Create جدید در caller. استثنا/Update method مخفی نسازید.
- `Confirmed` یک Unit هیچ‌وقت با `Release` به Held/Released برنگردد؛ فقط Cancel و مطابق قواعد.
- Partial operation atomic؛ replay idempotent؛ انقضا lazy حتی بدون Poller، poller status persist می‌کند.
- GET historical از Reservation + child data؛ حتی اگر Definition Retired شد، `SelectedDetails` نباید regenerate شود.
- Dev reservation data is test data (Owner Q5): all reservation rows on dev are deleted before the v11 migration; the migration is plain (no backfill); the generated EF migration SQL is checked and Down/Up is proven on a test database.

## 6. فیچرهایی که عمداً نمی‌سازیم
No `AncillaryReservationSeat`, `AncillaryReservationBaggage`, ... به‌عنوان ۲۴ Aggregate/Entity دارای Lifecycle مستقل؛ no event bus, no generic adapter framework, no supplier saga, no new Stock engine, no Order reader, no active pricing requote. Domain/SQL پیچیده موجود فقط به‌قدر لازم تغییر کند.
