# 03 — مدل Domain و Lifecycle، فیلد به فیلد

**تصمیم:** حفظ یک `AncillaryReservation : AggregateRoot<long>` و `AncillaryReservationUnit : Entity<long>`؛ اضافه‌شدن فقط Allocation child لازم برای ظرفیت Local. ساخت Seat/Baggage/Pet/CIP Aggregateهای رزرو متعدد ممنوع.

## 1. فهرست انواع و منشأ واقعی
- `AncillaryReservation`، `AncillaryReservationUnit`، `AncillaryReservationUnitArgs`، `AncillaryHoldChecks`: **موجود**.
- `AncillaryReservationUnitAllocation`: **جدید** و صرفاً child از Unit؛ نه Aggregate/Repository مستقل تجاری.
- `InventoryResourceKind`: **موجود** `FlightCount=1, FlightWeight=2, AirportSlot=3`.
- `AncillaryReservationStatus` و `AncillaryReservationUnitStatus`: **موجود** `Held=1, Confirmed=2, Released=3, Expired=4, Cancelled=5`؛ renumber و enum proliferation ممنوع.
- `InventoryHoldFacts` DTO/Value-Object حداقلی برای weight و slot: **جدید**؛ داده‌ای که واقعاً در تخصیص نیاز است، نه کپی کل Selection.

## 2. Root موجود ـ جدول کامل
| Field | CLR Type | وضعیت | معنی/Invariant |
|---|---|---|---|
| `Id` | long | موجود | `HoldId`، `>0`؛ تولید در Ancillary |
| `IdempotencyKey` | string | موجود | required max128، یکتا؛ برای Create یک Hold |
| `OrderId` | long | موجود | required >0؛ مرجع Ordering؛ تغییرناپذیر |
| `Reference` | string | موجود | required max128؛ correlation از Ordering، نه PNR/SeatHoldRef |
| `RequestedExpiresAt` | DateTimeOffset? | موجود | برای Hold جدید v10 **non-null**؛ accepted by validator |
| `ExpiresAt` | DateTimeOffset? | موجود | برای Hold جدید v10 **non-null** و > now؛ در append قابل‌تمدید نیست |
| `Status` | `AncillaryReservationStatus` | موجود | operational state؛ active Unitها و transitionها تعیین‌کننده |
| `CreatedAt` | DateTimeOffset | موجود | audit؛ در ایجاد set شود |
| `UpdatedAt` | DateTimeOffset | موجود | هر transition موفق update شود |
| `Units` | IReadOnlyCollection<AncillaryReservationUnit> | موجود | فرزندهای جاری+تاریخی؛ حذف فیزیکی نداریم |

**عدم افزودن:** `Outcome`, `Pending`, `Money`, `Currency`, `EMDId`, `PNR`, `ProviderSaga`, `SelectionSnapshotId`, `FlightFlowSeatHoldId` به Root.

## 3. Unit موجود ـ جدول کامل
| Field | CLR Type | وضعیت | Rule |
|---|---|---|---|
| `Id` | long | موجود | `UnitId` منحصر به فرد؛ reference جزء |
| `AncillaryReservationId` | long | موجود | FK والد؛ تغییرناپذیر |
| `OrderServiceId` | long | موجود | required >0؛ در Hold و بین Live Holds یک OrderService تنها یک Unit فعال |
| `ServiceDefinitionId` | long | موجود | از قرارداد Resolve و Ordering؛ Definition واقعی و active |
| `ProvisionId` | long | موجود | belongs to Definition، active/effective |
| `TravellerId` | long? | موجود | null فقط Order coverage؛ otherwise >0 |
| `CoverageScope` | ServiceCoverageScope | موجود | Sector 1/Portion 2/Journey 3/Order 4؛ تطبیق با Provision |
| `CoveredFlightIds` | IReadOnlyList<long> | موجود | positive/distinct/sorted؛ Sector=یک؛ Portion/Journey=تمام legs/flightids مورد نیاز |
| `Quantity` | int | موجود | >=1، محدوده Provision؛ immutable پس از Hold |
| `SeatMapSelectionId` | string? max256 | موجود | فقط Seat، با SeatNumber+PricingId به شکل all-or-none |
| `SeatNumber` | string? max16 | موجود | فقط Seat، trim+uppercase؛ commercial reference، نه proof physical |
| `PricingId` | long? | موجود | فقط SeatMap pricing identity در current Domain؛ سایر خدمات price در Ordering |
| `StockPoolId` | long? | موجود | **legacy placeholder**؛ از آن به‌عنوان Authority یا مرجع Allocation استفاده نشود |
| `ProviderUnitRef` | string? max100 | موجود | فقط وقتی provider واقعی به‌صورت مستند ref بازگرداند؛ این فاز برای Local/Seat null |
| `Status` | AncillaryReservationUnitStatus | موجود | Held→Confirmed/Released/Expired؛ Confirmed→Cancelled |
| `CancellationReasonCode` | string? max50 | موجود | فقط Cancelled، علّت از Ordering؛ بدون overwrite در retry |
| `Allocations` | IReadOnlyCollection<AncillaryReservationUnitAllocation> | **جدید** | 0..N؛ فقط منابع `InventoryAuthority.Local` |
| `AppendIdempotencyKey` | string? max128 | **جدید فقط Seat append** | برای Unitهای همان درخواست append مشترک؛ فقط برای replay به parent؛ null در Create عمومی/seat-only |
| `AcceptedWeightKgTotal` | decimal? | **جدید conditional** | برای Hold با local accepted-weight؛ immutable، precision InventoryRules |
| `AppointmentTimeWithOffset` | DateTimeOffset? | **جدید conditional** | Slot، زمان سفارش تأییدشده و قابل‌مقایسه به UTC |
| `GuestCount` | int? | **جدید conditional** | Slot، 0..Policy Max؛ در محاسبه persons لازم |

**دلیل ذخیره فقط سه Fact:** برای مقایسه صحیح درخواست idempotent و توضیح تخصیص در آینده لازم‌اند؛ برای هر Variant جدول/JSON مستقل ایجاد نمی‌شود. اطلاعات پزشکی، SSR، Meal details، Passenger documents و پرایس مجدداً اینجا ذخیره نشوند.

## 4. Allocation جدید — دقیق و کوچک
`AncillaryReservationUnitAllocation` child ساده‌ای از Unit است. یک Unit ممکن است 0 یا چند Allocation داشته باشد. Domain methodهای تغییر وضعیت Allocation لازم نیست چون effective allocation از وضعیت Unit و زمان Root محاسبه می‌شود.

| Field | Type | Rule |
|---|---|---|
| `Id` | long | کلید تولیدشده در Ancillary، immutable |
| `AncillaryReservationUnitId` | long | FK اجباری به Unit، `ON DELETE RESTRICT` در تاریخچه |
| `ResourceKind` | InventoryResourceKind | موجود enum: Count=1, Weight=2, Slot=3 |
| `InventorySourceId` | long | ID واقعی `FlightCountInventory` یا `FlightWeightInventory` یا `AirportSlotInventory`، نه `ResourceId`/`FacilityId` تجاری |
| `Amount` | decimal(19,3) | `>0`؛ برای Count و Slot **عدد صحیح**؛ برای Weight `InventoryRules.IsKg`؛ عدد قبول‌شده ثبت می‌شود، immutable |

**Unique index:** `(AncillaryReservationUnitId, ResourceKind, InventorySourceId)`؛ **search index:** `(ResourceKind, InventorySourceId)`؛ FK به Unit. چون sourceها سه جدول جدا هستند، single polymorphic FK مستقیم به آن‌ها تعریف نمی‌شود؛ سرویس هنگام تخصیص وجود/Owner/Status را اثبات می‌کند. برای مصرفی با چند منبع یک Unit چند Allocation دارد. هیچ ستون `Held/Confirmed/ExpiresAt/Price` تکراری در Allocation نگذارید.

### قاعده شمارش فعال
| Unit | Root expiration | Allocation counted? |
|---|---|---|
| Held | `ExpiresAt > now`؛ و فقط برای Holdهای قدیمی با `ExpiresAt=null`، **به‌عنوان مصرف فعال تا reconciliation** | بله |
| Held | `ExpiresAt != null && ExpiresAt <= now` | خیر، حتی قبل از اجرای Poller |
| Confirmed | مهم نیست | بله |
| Released/Expired/Cancelled | مهم نیست | خیر |

این قاعده اساس query موجودی در همان تراکنش است. `TotalCapacity` از AdjustTo مستقل است؛ ظرفیت پایه را هنگام Hold تغییر ندهید.

## 5. Lifecycle جدول کامل
| Operation | Root precondition | Unit transition | Effect |
|---|---|---|---|
| Create Hold | new valid input, expiry future | create Held | allocation atomic; no fake success |
| Append SeatMap | existing Held unexpired, same Order/Reference/ExpiresAt | add seat Held | NO seat allocation; same HoldId |
| Confirm All | Root Held not expired | فقط Held→Confirmed | Allocation باقی می‌ماند؛ Released ثابت |
| Release All | Root Held not expired | فقط Held→Released | Available افزایش می‌یابد |
| Release Unit | Root Held not expired, owned Unit Held | یک Held→Released | فقط allocations همین Unit از مصرف خارج می‌شوند |
| Expire | Root Held and now>=expires | فقط Held→Expired | بعد از deadline حتی قبل poller قابل فروش مجدد |
| Cancel All | Root Confirmed | فقط Confirmed→Cancelled | فقط allocations Cancelled آزاد می‌شوند؛ Refund وظیفه Ordering |
| Cancel Unit | Root Confirmed, owned Unit Confirmed | فقط یک Confirmed→Cancelled | سایر Confirmed باقی می‌مانند |
| Read | هر status | بدون mutation | lazy expiry در response؛ Units truth |

**قاعده ترتیب بررسی:** ابتدا اگر همان Unit یا همان Hold قبلاً دقیقاً به نتیجه مطلوب رسیده است، پاسخ replay/no-op بده، حتی اگر والد بعداً terminal/expired شده باشد؛ پس از آن precondition عملیات تازه بررسی شود.

`Confirm` نباید `foreach all units` قدیمی را اجرا کند چون Unit Released را مجدداً Confirm می‌کند. `Cancel` نباید همه Releasedها را Cancel کند. `Release` نباید Confirmed را Release کند. Retry همان عملیات terminal approved no-op است. Expired unit دوباره Held نمی‌شود.

### Header status ساده و بدون Enum جدید
- **Held:** هنوز دست‌کم یک Held زنده و هیچ Confirmed ندارد. Partial Released هم همچنان Held است.
- **Confirmed:** Hold تأیید شده و دست‌کم یک Confirmed دارد؛ ممکن است برخی Unitها Released/Cancelled باشند. این Status موفقیت تک‌تک Unitها را ادعا نمی‌کند؛ لیست Units مرجع است.
- **Released:** همه Unitهای قبلاً Held آزاد شده‌اند؛ هیچ Confirmed ندارد.
- **Expired:** Root فرصت Confirm را از دست داده و Heldهایش منقضی‌اند؛ تاریخچه Released محفوظ است.
- **Cancelled:** پس از Confirm، دیگر Confirmed فعال ندارد و دست‌کم یک Unit Cancelled بوده است.

اگر هنگام تست ترکیب غیرممکن (همزمان Held و Confirmed) ساخته شود، **409 domain invariant**؛ State `Mixed` اضافه نکنید. Append به Root Confirmed یا Expired ممنوع.

## 6. Uniqueness صحیح
- در یک درخواست `OrderServiceId`های تکراری: رد.
- `OrderServiceId` در Hold دیگری با Unit Held زنده یا Confirmed: رد؛ Released/Cancelled/Expired آزادند (با رعایت هویت OrderService).
- **کلید `Definition+Traveller+Flight` یکتا نیست.** دو Provision/Variant مختلف حتی از همان Definition ممکن است خریده شوند؛ این guard قدیمی Aggregate باید اصلاح شود. قواعد تعارض Shopping/Meal از مسیر accepted order اعمال شده‌اند و در Hold نباید از روی Definition فرض جدید بسازیم.
- در **داخل همان Reservation** دو Seat با `(FlightId, SeatNumber)` یکسان به‌طور همزمان Held/Confirmed پذیرفته نشوند؛ سایر Seat holdهای Orders را FlightFlow کنترل می‌کند، نه Ancillary.
- `AppendIdempotencyKey` در Seat append یک batch identifier است؛ same parent+same append key+same seat payload → پاسخ موجود (no new units)، same key+different payload → conflict. اگر هر کدام از OrderServiceIdهای همان append از قبل با key دیگر فعال‌اند، conflict.
- `Reservation.IdempotencyKey` برای Create global unique موجود باقی می‌ماند؛ replay باید بعد از VerifySameContent معتبر کار کند و Allocation جدید نسازد.

## 7. Supplier و Seat — گیت بدون حالت ساختگی
- `SupplierFulfillmentKind.External` یا `InventoryAuthority.Supplier` و نیز هر Provision محتاج approval/confirmation خارجی: بدون provider proof/ACL واقعی **Reject pre-write**، حتی اگر الگوی Inventory به نام `Unlimited` باشد. کد فعلی External supplier را رد می‌کند؛ هیچ `Pending`/`Failed` جدید نیاز نداریم.
- `SeatMap`: `SeatMapSelectionId/Number/PricingId` از verifier فعلی، `Quantity=1`، `CoverageScope=Sector`، یک FlightId، `Allocations=[]`؛ خطای OccupiedSeat از Ancillary در این فاز وجود ندارد. اگر FlightFlow بعداً Seat را آزاد کند، Ordering باید دستور Release/Cancel تجاری مناسب Ancillary را بدهد.
- Product/Options بیزنسی را بعد از Confirm تغییر ندهید؛ تغییر Seat Number به Release/Cancel + Hold جدید با OrderService جدید و orchestrator Ordering نیاز دارد.

## 8. Migration و compatibility
- EF CommandDb: ایجاد جدول Allocation؛ افزودن ستون‌های conditional Fact و AppendKey روی Unit؛ FK + indexes؛ Default برای داده‌های قدیمی **NULL و empty allocation**؛ هیچ داده تاریخی را به Held With Stock تبدیل نکنید.
- Reservationهای قدیمی که فقط status Held و بدون allocation دارند **تضمین واقعی Local ندارند**؛ نباید آن‌ها را در production معتبر جلوه دهید. **تا زمانی که اثر Stock یک Hold/Confirmed legacy فاقد Allocation روی Source موردنظر قابل اثبات/تعیین‌تکلیف نیست، Hold جدید روی همان Source باید fail-closed شود**؛ در غیر این صورت sum ledger آن مصرف تاریخی را صفر فرض کرده و ممکن است oversell رخ دهد. Gate migration باید pre-existing Held/Confirmed را گزارش و ایمن reconcile کند؛ و بدون اجازه مالک داده‌ها حذف نشوند.
- Status enum integer موجود ثابت؛ Snapshot/Shopping/SeatMap status خارج scope.
