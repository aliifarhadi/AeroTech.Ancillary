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
| `CoverageScope` | existing enum | `Provision.CoverageScope` | internal derived، never wire input |
| `TravellerId` | long? | `units[].unit.travellerId` | required only Pax-scoped؛ no membership check |
| `CoveredFlightIds` | collection<long> | `units[].coverage.flightIds` | positive/distinct؛ numeric Stock/Fulfillment coverage؛ Sector exactly 1 |
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
| BaggageSelection | `BagRef:string?`, `MeasuredWeightKg:decimal?`, `Dimensions:SelectedDimensions?`, `WeightKg:decimal?` | `EquipmentKind:string?`, `PackageWeightKg:decimal?` فقط اگر Source یکتا | پرکردن Optionهای A02/A06 با string فرضی |
| MealSelection | — (اگر A11/A12 ثابت از Definition) | `MealCode:string?`, `MenuItemRef:string?`, `DietaryCode:string?` در صورت معلوم‌بودن قطعی | Meal متغیر بدون Source و بدون stored value |
| PetSelection | `AnimalType:PetAnimalType`, `OtherAnimalCode:string?`, `AnimalAgeWeeks:int?`, `CombinedWeightKg:decimal`, `CarrierDimensions:SelectedDimensions`, `DocumentAcknowledgements:string[]?`, `SizeBracket:string?` | `TransportMode` از Definition در صورت نیاز | حساسیت اطلاعات واقعی بیش از Required |
| AssistedTravelSelection | `AssistanceSsrCode:string?`, `InfantRef:string?`, `GuardianRef:string?` | `AssistanceKind` اگر برای Fulfillment لازم | A17/A19 medical/guardian data until gate |
| AirportServiceSelection | `TimeWithOffset:DateTimeOffset?`, `GuestCount:int?` | `AirportId:int`, `FacilityId:long?`, `IncludedComponentCodes:string[]?` اگر لازم | `inventoryFacts` عمومی یا SourceId از Caller |
| PrioritySelection | `OptIn:bool` | هیچ مگر Fixed Service Code برای Fulfillment | false unit held |
| ConnectivitySelection | `DeviceCount:int?` | `PlanCode:string?` اگر Definition یکتا | PlanCode حدسی |
| UpgradeSelection | `TargetCabinId/QuoteRef` فقط مطابق **Quote Contract واقعی** | — | فعال‌کردن A10 با Quote جعلی |
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
- Restore و Migration تست database بدون wipe و بدون drop original data؛ migration additive با unique FK/indices کافی؛ بازسازی SQL مدل EF باید اثبات شود.

## 6. فیچرهایی که عمداً نمی‌سازیم
No `AncillaryReservationSeat`, `AncillaryReservationBaggage`, ... به‌عنوان ۲۴ Aggregate/Entity دارای Lifecycle مستقل؛ no event bus, no generic adapter framework, no supplier saga, no new Stock engine, no Order reader, no active pricing requote. Domain/SQL پیچیده موجود فقط به‌قدر لازم تغییر کند.
