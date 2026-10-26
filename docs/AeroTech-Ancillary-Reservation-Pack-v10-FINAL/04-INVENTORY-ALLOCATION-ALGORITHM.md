# 04 — الگوریتم قطعی Inventory Allocation و جلوگیری از Oversell

**هدف:** استفاده از منابع موجود Phase2؛ جلوگیری اتمیک از oversell؛ بدون Aggregate تازه Inventory، بدون تغییر `TotalCapacity`، بدون Provider خیالی. `Allocation` و Unit status تنها ledger مصرف‌اند.

## 1. منابع موجود و نگاشت Policy
| Authority / Pattern | Source existing | Required mapping | Hold effect |
|---|---|---|---|
| `Unlimited` | none | Policy Active + Provision valid | Unit Held، Allocation صفر |
| `Local FlightCount` | `FlightCountInventory` | `(OwnerAirlineId, FlightId, CountConsumption.ResourceId)` | `Amount = Quantity * CountPerAcceptedUnit` برای هر flight مشمول |
| `Local FlightWeight` | `FlightWeightInventory` | `(OwnerAirlineId, FlightId, WeightConsumption.WeightResourceId)` | فقط fixed: `Quantity * FixedKgPerUnit`؛ `AcceptedWeightKg` پیش از نوشتن با خطای صریح Unsupported رد می‌شود |
| `Local FlightCountPlusWeight` | هر دو source بالا | هر دو mapping برای هر flight | Count + Weight در **همان SQL transaction** |
| `Local AirportSlot` | `AirportSlotInventory` | `(OwnerAirlineId, AirportId, FacilityId, contiguous time slots overlapping interval)` | `Amount = PeoplePerAcceptedUnit * (Quantity + GuestCount)` برای هر slice |
| `Supplier` | provider external | real S2S ACL absent | reject pre-write، هیچ Fake Held |
| `Local DailyCount/RoomNight/AssignedAsset` | unsupported current Policy activation | none | reject، no new inventory type |
| `Seat` | FlightFlow via Ordering | SeatMap commercial ID | **Allocation = 0، no FlightFlow or occupancy lookup** |

این جدول از `AncillaryInventoryPolicy.Authority/LocalPattern`, `CountConsumption`, `WeightConsumption`, `SlotConsumption` و source Aggregateهای v9 ناشی می‌شود. `Policy` باید **Active** و bound به Definition معتبر همان `OwnerAirlineId` باشد؛ absent/draft/suspended/retired نباید Unlimited تعبیر شود.

## 2. شناسه منبع و نوع مصرف
- کلید Stock واقعی **Source Aggregate Id** است، نه `ServiceDefinitionId` و نه `ProvisionId`. دو محصول اگر یک `(ResourceKind, InventorySourceId)` مصرف کنند، **مانده مشترک** دارند.
- `Capacity` خام منبع نام‌برده در جدول بالاست. `Held زنده (و legacy بدون تاریخ انقضای تعیین‌شده)+Confirmed` به‌ازای SourceId از `AncillaryReservationUnitAllocation` join `AncillaryReservationUnit` و `AncillaryReservation` و status واقعی خوانده می‌شود.
- Count و Persons در `Amount decimal(19,3)` به صورت صحیح `1.000`، `2.000` و ... هستند. Weight Kg precision طبق `InventoryRules.IsKg` موجود validate شود.
- برای Holdهای v10، `Reservation.ExpiresAt > now` معیار فعال بودن Held است، نه timestamp Poller. **Holdهای legacy با ExpiresAt=null را تا زمان تعیین تکلیف هنوز فعال/مصرف‌کننده حساب کنید** تا oversell ایجاد نشود؛ ایجاد Hold جدید با Expiry=null ممنوع است. **رکورد legacy بدون Allocation قابل‌شمارش نیست؛** پیش از Migration هر محیط یک Query فقط‌خواندنی آن را گزارش می‌کند و اگر وجود داشت، همان محیط برای منابع متأثر تا تعیین تکلیف Fail Closed می‌ماند؛ مصرفش صفر فرض نمی‌شود و سیستم Runtime Reconciliation ساخته نمی‌شود.

## 3. Algorithm: Create Hold (local/unlimited) — pseudocode واحد اجرایی
```text
Validate service DTO, order+idempotency+expiry (> now UTC), distinct OrderServiceId
Reject Provision requiring supplier/approval evidence if no real, supported provider contract exists, even when InventoryAuthority=Unlimited
If same Create idempotency key: compare exact canonical request INCLUDING inventoryFacts
 -> return same hold without any allocation; mismatched input -> 409
Acquire existing order lock (ancillary-hold:{orderId})
BEGIN SQL TRANSACTION
  Load service Definition/Provision/Supplier/ACTIVE InventoryPolicy for each Unit
  Check ownership, Status, Quantity, Scope, CoveredFlights; reject external supplier
  Reject AcceptedWeightKg policies explicitly (Unsupported) before any write
  Resolve LocalSourceIds and build immutable per-unit Allocation demands
  Validate required slot facts; no inferred unavailable facts
  Sort all DISTINCT resource keys (kind, sourceId) ascending
  Acquire SQL transaction-owned lock for each source key in that order; check the return code, rollback when < 0
  Load CURRENT source capacity/status/closedForSale and SUM active allocations at now (after the lock, READ COMMITTED)
  Aggregate all DEMANDS of this request per source key
  If demand > capacity - activeAllocations, THROW shortage: rollback all
  Validate OrderServiceId not already live under the same order lock
  Create ONE AncillaryReservation + all Units + child Allocations
  SaveChangesAsync ONCE, COMMIT
Return holdId, expiry, units
```
**DB (تصمیم مالک Q2):** SQL Server واقعی با `sp_getapplock`، `@LockOwner='Transaction'` و `@LockMode='Exclusive'` با `Resource='Ancillary.Inventory.{kind}.{sourceId}'` تا قفل با Commit/Rollback آزاد شود؛ بدون آنکه `ResourceId` تجاری را در lock key به جای source ID وارد کنیم. `sp_getapplock`، خواندن مصرف و `SaveChanges` روی **همان Connection و Transaction** اجرا شوند؛ مصرف فعال **پس از گرفتن قفل** با `READ COMMITTED` از آخرین داده Commit‌شده خوانده شود. `@LockTimeout` محدود باشد؛ کد بازگشتی `< 0` بررسی و تراکنش Rollback شود؛ انتظار بی‌نهایت ممنوع است. سطح Isolation یا قفل دوم، RedLock چندکلیدی، Redis+SQL دوگانه یا RowVersion دوم برای Allocation ساخته نشود. رعایت Lock Ordering ثابت برای همه درخواست‌ها ضروری است. قفل Session فعلی `AirportSlotInventoryRepository.LockFacilityAsync` فقط برای تعریف بازه و جلوگیری از هم‌پوشانی است و با این قفل مصرف اشتباه گرفته نشود.

**تست اجباری:** دو Order متفاوت هم‌زمان آخرین واحد را درخواست کنند: تنها یکی Success، دیگری shortage و count never negative. تست Mock `IDistributedLock` کافی نیست. `AcquireAsync(orderId)` فعلی مانع oversell cross-order نمی‌شود.

## 4. Counter capacity & administrative AdjustTo
- `AdjustTo(newTotal)` در `FlightCountInventory`, `FlightWeightInventory`, `AirportSlotInventory` یک تغییر **capacity definition** است؛ Hold نباید آن را صدا بزند.
- عملیات Admin که TotalCapacity/CapacityKg/CapacityPersons را پایین می‌آورد باید زیر همان Resource Lock و در transaction بررسی کند `newTotal >= activeAllocations`. اگر پایین‌تر است با conflict رد شود یا سیاست تجاری صریح مالک لازم دارد؛ **هیچ overcommit خاموشی**.
- `Suspend`, `CloseForSale`, `Retire`: Hold جدید را منع کند و چون فروش‌پذیری منبع را در رقابت با Hold تغییر می‌دهند، زیر همان قفل منبع اجرا شوند؛ تخصیص‌های قدیمی و Cancel/Release همچنان طبق Status همان Unit مدیریت شوند. Retire کردن منبع با تخصیص فعال بدون سیاست تایید‌شده نباید مصرف تاریخی را مخفی کند.
- Release، Cancel و Expire فقط مصرف را کم می‌کنند؛ برای آن‌ها قفل منبع لازم نیست و قفل سفارش + تراکنش فعلی کافی است.
- `FlightCountInventory` منبعی در همان پرواز و `ResourceId` است؛ `FlightWeightInventory` به `WeightResourceId` مستقل اشاره دارد. از تعداد صندلی FlightFlow برای Baggage stock استفاده نشود.

## 5. FlightCount example
- Source FlightId=801، ResourceId=40، `TotalCapacity=10`؛ از دو سفارش قبلی Active allocations=8؛ موجود=2.
- Unit Baggage Qty=2، Policy `CountPerAcceptedUnit=1` → demand=2 ⇒ Hold okay، ledger=2.
- واحد دیگری Qty=1 → shortage؛ حتی اگر `DefinitionId` متفاوت ولی ResourceId/Source همان باشد.
- برای Portion با Flights [801,802]، **هر دو** source بررسی و ledgerهای مستقل ساخته شوند؛ اگر Flight802 ناموجود، کل Unit fail و هیچ ledger نوشته نشود.

## 6. FlightWeight example
- Weight source Flight801 capacity=100kg، فعال قبلی 89.0kg.
- `FixedKgPerAcceptedUnit=5kg`, Qty2 → 10kg demand، okay (available1kg بعد).
- `AcceptedWeightKg` mode: پیش از هر Allocation با خطای صریح Unsupported رد می‌شود؛ هیچ وزن تخمینی، `1kg` پیش‌فرض یا داده وزن از Client پذیرفته نمی‌شود. اگر بعدها `PerKilogram` در Shopping/Price واقعاً پشتیبانی شد، قرارداد وزن پذیرفته‌شده در همان فاز افزوده می‌شود.
- `FlightCountPlusWeight`: در نمونه Qty2/Weight10 باید هر دو source در یک transaction allocate شوند؛ کمبود یکی، تخصیص هر دو rollback.

## 7. AirportSlot example
Policy: `AirportId=11`, `FacilityId=200`, `OccupancyMinutes=60`, `PeoplePerAcceptedUnit=1`, Qty1، GuestCount1، appointment `16:00+03:30` → UTC = `12:30Z`، interval `[12:30Z,13:30Z)`، demand **2 persons**.
- Source rows ممکن است بازه‌های `[12:00Z,13:00Z)` و `[13:00Z,14:00Z)` باشند؛ هر دو باید Active/Open و **contiguous cover** کل interval باشند.
- برای هر slice همپوشان، allocation به اندازه 2 persons ثبت شود؛ برای هر slice مجموع concurrent allocations ≤ CapacityPersons.
- اگر یک gap بین sliceها، Facility نادرست، timezone invalid یا closed-for-sale وجود داشت: کل Hold fail.
- Policy `SlotConsumption` صرفاً عدد و facility؛ زمان و guestCount از Order accepted selection می‌آیند، نه از Payload خودسر Client.

## 8. Release / Expire / Confirm / Cancel — محاسبه بدون counter اضافی
- **Hold:** allocation immutable نوشته شد؛ Unit Held+not expired ⇒ counted.
- **Confirm:** فقط status Unit Held→Confirmed؛ allocation همان است؛ available تغییر نمی‌کند.
- **Release انتخابی (`unitIds`) / Release All:** در یک transaction وضعیت Unit Held→Released؛ allocated resources به‌سبب query predicate دیگر counted نیستند؛ delete ledger ممنوع.
- **Expire:** Held past expiry در لحظه query counted نیست؛ poller status را persisting Expired می‌کند؛ inventory را دوباره «دو بار» آزاد نمی‌کند.
- **Cancel Confirmed:** Unit Confirmed→Cancelled فقط با دستور Ordering که لغو قبل از مصرف سرویس و سیاست‌های تجاری را تایید کرده؛ allocation دیگر counted نیست؛ ticket/EMD refund آنجا مدیریت می‌شود.
- هیچ `ReservedCount` یا `RemainingCount` mutable جدا در InventorySource و Reservation ذخیره نمی‌شود؛ **یک ledger + وضعیت Unit منبع حقیقت** است.

## 9. ناشناخته‌ها را حدس نزنید
- `FlightWeight AcceptedWeightKg` در این فاز Unsupported است (تصمیم مالک Q5)؛ آن Variant را با وزن تخمینی Hold نکنید.
- اگر `Supplier` provider adapter واقعی ندارد، Fail/Report. افزوده‌شدن provider به Domain یا Outbox تا زمان قرارداد رسمی ممنوع.
- در case `AirportSlot` اگر `TimeWithOffset` در Order موجود نیست، Fail/Report، نه `DateTimeOffset.UtcNow`.
- `PassengerUsageLimit` موجودی فیزیکی مستقل نیست؛ برای Cross-Order CountFamily enforcement بدون stable traveller identity شواهد لازم است. در این فاز **نه را دور بزنید، نه منبع جدید اختراع کنید**؛ مورد را در Gate0 audit و تست منفی صریح ثبت کنید. محاسبه واقعی ظرفیت Count/Weight/Slot اولویت اول است.

## 10. دقت و indexes SQL
- `Amount` explicit `decimal(19,3)` نه global EF `decimal(18,2)`؛ Weight source precision و Validation موجود رعایت شود.
- Index `(ResourceKind, InventorySourceId)` برای sum active usage ضروری؛ `(AncillaryReservationUnitId, ResourceKind, InventorySourceId)` unique؛ index Unit OrderServiceId موجود حفظ شود.
- Reservation/Unit/Allocation یک DbContext و **یک transaction**؛ connection lock owner transaction؛ تست rollback قبل/بعد Save و deadlock handling طبق Framework فعلی.
