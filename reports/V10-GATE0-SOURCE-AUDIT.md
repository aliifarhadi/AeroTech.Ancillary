# Reservation v10 — Gate 0: ممیزی سورس (بدون کد)

**تاریخ:** 2026-10-26
**Pack:** `docs/AeroTech-Ancillary-Reservation-Pack-v10-FINAL` (`VERIFY-PACK.py`: PASS، 16 فایل، 108 شناسه تست)
**سؤال‌ها و تعارض‌ها:** `reports/V10-GATE0-QUESTIONS-AND-CONFLICTS.md`

## 1. Baseline

| مورد | مقدار |
|---|---|
| Ancillary | `feat/ancillary-offers-v9@2025efd4fbdb4ddb9c72a1ac1eb5023c0b29a3c0`، دقیقاً Pin پک. تنها تغییر درخت، پوشه و zip پک است (untracked) |
| FlightFlow | `E:\Projects\DotAir\FlightFlow`، `k8s-stg@962f4188`، دقیقاً Pin پک |
| Ordering | `AeroTech.Ordering.Final@cd50a2a` (برابر origin/k8s-stg)، فقط خوانده شد |
| dev DB | آخرین migration: `20261025231956_SelectionSnapshotLines` |
| Reservationهای dev | 8 عدد: 1 Held (سفارش 1001، متعلق به مالک)، 4 Released، 1 Expired، 2 Cancelled. همه Unitها Seat هستند و هیچ‌کدام `ExpiresAt` خالی ندارد |
| Policyهای dev | 53 فعال: 33 Unlimited، 20 Supplier، 0 Local. منبع Count/Weight/Slot: 0 |
| کد | در این Gate هیچ فایل کد، تست، migration یا تنظیمی تغییر نکرد |

## 2. FlightFlow (هم‌خانواده) چه می‌کند

| موضوع | FlightFlow `962f418` | Ancillary امروز | Pack v10 |
|---|---|---|---|
| مسیرها | `POST Seat-Holds` (201 + `FlightHeldSeatsDto`)، `POST Seat-Holds/{id}/Confirmations` (204)، `PATCH Seat-Holds/{id}` تمدید (204)، `DELETE Seat-Holds/{id}` (204)، `POST …/Splits`، `POST Seat-Confirmations/{id}/Reversals`، `POST Seat-Confirmations/{id}/Cancellation {reasonCode, seatHoldReferences[]}` (200). روی Service مسیر GET ندارد (کامنت شده) | `POST`، `GET`، `DELETE Service-Holds`، `…/Confirmations`، `…/Cancellations` (201/200 + `AncillaryHoldResult`) و `POST SeatMaps/Holds` | همان پنج مسیر، به‌اضافه `DELETE …/Units/{unitId}`، `POST …/Units/{unitId}/Cancellations` و `holdId?` روی `SeatMaps/Holds`. تمدید و Split ندارد |
| مرجع گروه و جزء | `HoldBatchId` و `SeatHoldReference` (= `FlightSeatHold.Id`) | `HoldId` و `UnitId` | همان |
| وضعیت | برای هر صندلی: Held=1، Released=2، Expired=3، Confirmed=4، Cancelled=5. وضعیت تنبل با `StatusAt(now)` | روی Root و Unit: Held=1، Confirmed=2، Released=3، Expired=4، Cancelled=5. وضعیت تنبل با `StatusOf` و `EffectiveStatus` | همان شماره‌ها |
| انتقال وضعیت | Confirm و Release فقط واحدهای `Held` را عوض می‌کنند. Cancel فقط Confirmedهای فهرست‌شده را (`FlightCapacity.cs:222-330`، `371`) | `Transition()` همه Unitها را عوض می‌کند (`AncillaryReservation.cs:182-189`). Cancel هم همه را | فقط واحدهای واجد شرایط |
| ظرفیت | شمارنده‌های `Held`، `Confirmed` و `RemainingCap` روی `FlightCapacity` در هر انتقال تغییر می‌کنند (`FlightCapacity.cs:103-109`، `222-260`) | هیچ؛ Hold ظرفیت را نمی‌سنجد | دفتر تخصیص immutable، بدون شمارنده (Q1) |
| قفل | `IDistributedLocker.ExecuteWithLocksAsync` روی کلید پروازها (مرتب، RedLock، retry) دور load→change→commit (`HoldFlightSeatsCommandHandler.cs`) | `IDistributedLock` روی `ancillary-hold:{orderId}`، فقط برای Hold (`AncillaryHoldLock.cs`) | `sp_getapplock` با مالکیت Transaction برای هر منبع (Q2) |
| Idempotency | `TryGetExistingHold(idempotencyKey)` همان `HoldBatchId` را برمی‌گرداند | `IdempotencyKey` یکتا + `EnsureSameContent` | همان، به‌اضافه facts و کلید append |
| انقضا | job Quartz `ReleaseTimeoutReservedJob` → `ExpiredHold` شمارنده را برمی‌گرداند. تا job اجرا نشود ظرفیت آزاد نمی‌شود (`FlightCapacity.cs:185-215`) | `AncillaryHoldExpiryPoller` + وضعیت تنبل | Held منقضی بدون poller هم شمرده نمی‌شود (I10) |
| لغو جزئی | فهرست `seatHoldReferences` در بدنه همان مسیر Cancellation (`CancelConfirmedFlightSeatsRequest.cs`). Release جزئی ندارد | ندارد | برای هر Unit یک مسیر جدا (Q3) |
| رویداد | برای هر انتقال domain event؛ consumer مدل خواندنی را به‌روز می‌کند | ندارد (`Causes(` در Domain یافت نمی‌شود)؛ GET از CommandDb | projection و Outbox تازه ممنوع (Q6) |
| احراز هویت | `ServiceController : FlightsController : Controller`، بدون `[Authorize]` | بدون `[Authorize]` (حکم مالک، 2026-09-30) | فقط Service-to-Service |

## 3. وضعیت هر عملیات

| عملیات | وضعیت | چه چیزی باید عوض شود (شاهد) |
|---|---|---|
| `POST Service-Holds` | NEEDS_CHANGE | `RequestedExpiresAt` اختیاری است (`HoldAncillaryServicesRequest.cs`). Policy، Active بودن Definition و Provision، اختیار Supplier، `ConfirmationRequirement`، A09 و ظرفیت سنجیده نمی‌شوند؛ `AncillaryHoldChecks.cs` فقط وجود، FK، Supplier خارجی، کمیت و Scope را می‌سنجد. ورودی عمومی فیلدهای Seat دارد (`AncillaryHoldInputs.cs`). guard تکراری `Definition+Traveller+Flights` (`AncillaryReservation.cs:76-82`). فیلد ناشناخته رد نمی‌شود |
| `GET Service-Holds/{id}` | EXISTS | وضعیت تنبل درست است؛ تغییری لازم نیست |
| `POST …/Confirmations` | NEEDS_CHANGE | همه Unitها را Confirmed می‌کند؛ قفل ندارد |
| `DELETE Service-Holds/{id}` | NEEDS_CHANGE | همه Unitها را آزاد می‌کند؛ قفل ندارد |
| `POST …/Cancellations` | NEEDS_CHANGE | همه Unitها را لغو می‌کند. تکرار با دلیل دیگر امروز 200 می‌دهد، پک 409 می‌خواهد (L14). قفل ندارد |
| Release یا Cancel یک Unit | NEW | شکل مسیر: Q3 |
| `POST SeatMaps/Holds` با `holdId` | NEEDS_CHANGE | افزودن به همان Root و `AppendIdempotencyKey`. verifier بدون تغییر |
| انقضا | EXISTS | poller می‌ماند؛ فقط Heldها؛ قفل سفارش برای هر Hold |
| تخصیص Local | NEW | کلاس و جدول Allocation وجود ندارد؛ جست‌وجوی `Allocation` در `src` فقط متن تشخیصی `NoAllocationLedger` را پیدا می‌کند |
| Local در host مستقر | GATED | `NotConnectedInventoryResourceReference` و `NotConnectedAirportFacilityReference` همیشه SourceUnavailable می‌دهند، پس هیچ منبع Count، Weight یا Slot و هیچ Policy محلی فعال‌شدنی نیست (Q4) |
| `AcceptedWeightKg` | GATED | از هیچ محصول فروخته‌شده‌ای به دست نمی‌آید (پاسخ 4 در بخش 4؛ Q5) |
| Supplier واقعی | GATED | adapter ندارد؛ پیش از نوشتن رد می‌شود |
| `AdjustTo` به کمتر از مصرف | NEEDS_CHANGE | سه سرویس Adjust نه قفل منبع می‌گیرند و نه مصرف را جمع می‌زنند |

## 4. پاسخ هشت سؤال اجباری Gate 0

1. **یافتن منبع:** مستقیم از `AncillaryDbContext` (CommandDb) و در همان تراکنش.
   - Count با `(OwnerAirlineId, FlightId, ResourceId)` و Weight با `(OwnerAirlineId, FlightId, WeightResourceId)`. هر دو ایندکس یکتای فیلترشده روی ردیف‌های غیر Retired دارند (`FlightCountInventoryConfiguration.cs:23-25`، `FlightWeightInventoryConfiguration.cs:24-26`).
   - Slot با `(OwnerAirlineId, FacilityId, StartUtc, EndUtc)` و کنترل `AirportId`.
   - `ResourceId` روی Policy کلید کسب‌وکار است، نه شناسه ردیف. Allocation شناسه ردیف منبع را نگه می‌دارد.
2. **دقت kg:** `InventoryRules.KgScale = 3` و `MaxKg = 999,999,999,999,999.999`. ستون‌های kg امروز `decimal(18,3)` هستند و `Amount decimal(19,3)` پک هر دو را جا می‌دهد.
3. **Slot:**
   - هر ردیف در مرز دقیقه UTC است و حداکثر 24 ساعت طول دارد.
   - ردیف‌های یک (owner, facility) هم‌پوشانی ندارند؛ این در Define زیر `sp_getapplock` با مالکیت Session بررسی می‌شود (`AirportSlotInventoryRepository.cs`، `HasOverlapAsync`).
   - پس فرض پوشش پیوسته برقرار است. Shopping همین قاعده را در `AvailabilityAssessor.SlotAsync` دارد.
4. **`acceptedWeightKgTotal`:** از هیچ محصول فروخته‌شده‌ای به دست نمی‌آید.
   - `AcceptedWeightKg` فقط با `PricingUnit.PerKilogram` فعال می‌شود (`AncillaryInventoryPolicy.cs:267-268`).
   - `SellableCandidate.Is` محصول PerKilogram را در `/Price` رد می‌کند (`SellableCandidate.cs:32`).
   - زمان و تعداد همراه Slot اما از Order قابل دریافت‌اند: `AirportServiceSelectionOptions(TimeWithOffset, GuestCount)` در Price و Resolve هست.
5. **نگاشت در Ordering:** وجود ندارد. Ordering.Final فقط `FlightFlowReservationProvider : IReservationProvider` دارد و لغو Seat را با پیام «نیازمند مرحله Ancillary» رد می‌کند (`Order.Cancellation.cs:26`). پس E01 تا E03 فقط با تست قرارداد در Ancillary سنجیده می‌شوند و وضعیتشان NOT VERIFIED است.
6. **عملیات مدیریتی ظرفیت:**
   - `AdjustFlightCountInventoryService`، `AdjustFlightWeightInventoryService` و `AdjustAirportSlotInventoryService` باید قفل منبع و جمع مصرف را بگیرند.
   - Close، Suspend و Retire ظرفیت را کم نمی‌کنند؛ فقط Hold تازه را می‌بندند.
   - تست `P2_X07` واژه‌های Hold، Release، Expire و AncillaryReservation را در فایل‌های inventory ممنوع کرده است. این تست باید به‌روز شود؛ پک 07، ردیف R04 اجازه می‌دهد.
7. **کدهای خطا:**
   - استفاده دوباره: 16402، 16403 (کلید تکراری یا append با محتوای دیگر)، 16404، 16405، 16406، 16407، 16409، 16410، 16411 تا 16414، 16416، 16104، 16607 و 16608.
   - کدهای تازه در همان بلوک:
     - 16417: کمبود ظرفیت (409).
     - 16418: Unit به این Hold تعلق ندارد (404).
     - 16419: خدمت با موجودی‌اش قابل رزرو نیست (409). مثال: Policy نیست یا غیرفعال است، Supplier، تأیید تأمین‌کننده، سقف مصرف مسافر، A09.
8. **احراز هویت Service:** کنترلرها `[Authorize]` ندارند. این حکم مالک است (2026-09-30: «S2S توکن و auth نمی‌خواهد») و FlightFlow هم همین‌طور است. پس blocker حساب نشد و مسیرهای تازه در همان کنترلر می‌روند.

## 5. قرارداد فیلدبه‌فیلد (برای اجرا پس از پاسخ سؤال‌ها)

- **ورودی `Service-Holds`:**
  - `requestedExpiresAt` الزامی می‌شود (breaking).
  - Unit: `orderServiceId`، `serviceDefinitionId`، `provisionId`، `travellerId?`، `coverageScope`، `coveredFlightIds`، `quantity`، `inventoryFacts?`.
  - `inventoryFacts`: `timeWithOffset?` و `guestCount?`. `acceptedWeightKgTotal?` بسته به پاسخ Q5.
  - `seatMapSelectionId`، `seatNumber` و `pricingId` از ورودی عمومی حذف می‌شوند و فقط در مسیر داخلی Seat می‌مانند.
  - فیلد ناشناخته با `[JsonUnmappedMemberHandling(Disallow)]` پاسخ 400 می‌گیرد؛ این الگو در `ServiceSpecificationInputs.cs` هست.
- **`SeatMaps/Holds`:** فقط `holdId?: long` اضافه می‌شود.
- **پاسخ:** `AncillaryHoldResult` بدون تغییر.

## 6. طرح Schema (فقط CommandDb و فقط افزودنی)

| تغییر | جزئیات |
|---|---|
| جدول `Ancillary.AncillaryReservationUnitAllocations` | ستون‌ها: `Id bigint PK`، `AncillaryReservationUnitId bigint` (FK به Units، Restrict)، `ResourceKind int`، `InventorySourceId bigint`، `Amount decimal(19,3)`. ایندکس یکتا: `(UnitId, ResourceKind, InventorySourceId)`. ایندکس جست‌وجو: `(ResourceKind, InventorySourceId)` |
| ستون‌های Unit | `AppendIdempotencyKey nvarchar(128) null`، `AppointmentTimeWithOffset datetimeoffset null`، `GuestCount int null`. `AcceptedWeightKgTotal decimal(19,3) null` بسته به پاسخ Q5 |
| QueryDb | بدون تغییر؛ GET از CommandDb می‌خواند |
| داده | هر 8 reservation در dev از نوع Seat هستند و Policy محلی ندارند؛ backfill لازم نیست |

## 7. طرح تراکنش و قفل

- **تراکنش:**
  - یک تراکنش صریح روی `AncillaryDbContext`.
  - `UseSqlServer` بدون `EnableRetryOnFailure` تنظیم شده است (`Persistence/DependencyInjection.cs:39-41`)، پس execution strategy لازم نیست.
  - `IUnitOfWork` در host همان `AncillaryUnitOfWork` (Synchronizer) است: اول command را ذخیره می‌کند و بعد، جداگانه، query را. Reservation مدل خواندنی ندارد، پس ذخیره query خالی است.
- **قفل:**
  - قفل سفارش موجود برای همه عملیات Reservation گرفته می‌شود؛ امروز فقط Hold آن را می‌گیرد.
  - Hold و Adjust قفل منبع را به ترتیب `(kind, sourceId)` می‌گیرند. سازوکار قفل: Q2.
  - برخورد RowVersion روی Reservation امروز به 500 می‌رسد، چون در `AncillaryUnitOfWork` نگاشت نشده است. وقتی همه عملیات قفل سفارش بگیرند، این برخورد رخ نمی‌دهد.
- **شرط مصرف فعال:** Unit `Confirmed`، یا Unit `Held` که `ExpiresAt` آن خالی یا بزرگ‌تر از now است.

## 8. تغییرات رفتاری و breaking

1. `requestedExpiresAt` در `Service-Holds` الزامی می‌شود.
2. فیلدهای Seat از ورودی عمومی حذف می‌شوند.
3. Hold در این حالت‌ها با خطای صریح رد می‌شود:
   - Policy فعال وجود ندارد.
   - اختیار موجودی `Supplier` است.
   - تأمین‌کننده External است (این کنترل امروز هم هست).
   - تأیید تأمین‌کننده لازم است (`SubjectToConfirmation`).
   - A09.
   - الگوی موجودی پشتیبانی‌نشده.
   - سقف مصرف مسافر.
   - کمبود ظرفیت.

   **اثر روی dev:**
   - 20 Policy فعال Supplier (A10، A12، A17، A20، A21، A22، A24) در `/Price` فروخته می‌شوند ولی دیگر Hold نمی‌شوند.
   - 33 Policy Unlimited (A01 تا A06، A11، A13 تا A16، A18، A19، A23) Hold می‌شوند.
4. Confirm، Release و Cancel فقط روی Unitهای واجد شرایط کار می‌کنند. Unit آزادشده دست نمی‌خورد.
5. Cancel تکراری با دلیل دیگر پاسخ 409 می‌گیرد.
6. guard تکرار `Definition+Traveller+Flights` حذف می‌شود. کنترل `OrderServiceId` تکراری و Seat تکراری در یک Hold می‌ماند.
7. همه عملیات زیر قفل سفارش اجرا می‌شوند.
8. **Pinهایی که عوض می‌شوند:**
   - `P2_X07`: hash 72 فایل منجمد و واژه‌های ممنوع در inventory.
   - `V12Boundary`: فهرست پوشه‌های Command، مسیرهای POST که Hold دارند، و تعداد مسیرها.
   - فایل‌های `M1Reservation*` به‌روز می‌شوند، حذف نمی‌شوند.

## 9. حکم‌های ثابت مالک که روی پک اعمال می‌شوند

- **I29 و I32 (Hold قدیمی بدون Allocation):**
  - طبق حکم «داده‌ها تستی است»، منطق مسدودکردن منبع ساخته نمی‌شود.
  - شاهد dev: 0 Policy محلی، 0 منبع، 0 Unit زنده روی Policy محلی.
  - شرط مصرف فعال، Held بدون `ExpiresAt` را هم می‌شمارد؛ این هزینه‌ای ندارد.
- **تست‌ها:** کوتاه، و برای هر ردیف 07 یک نماینده. تست کند ساخته نمی‌شود.
- **محدودیت‌ها:** بدون commit یا push، و بدون تغییر در Ordering یا FlightFlow.

## 10. نقشه تست‌های 07

| گروه | فایل |
|---|---|
| A01–A16 | `tests/AeroTech.Ancillary.Application.AcceptanceTests/Holds/V10HoldContractAcceptanceTests.cs` |
| I01–I32 | `…/Holds/V10InventoryAllocationAcceptanceTests.cs`: SQL واقعی؛ I21 تا I23 با دو اتصال هم‌زمان |
| L01–L23 | `tests/AeroTech.Ancillary.Domain.ConformanceTests/V10ReservationLifecycleConformanceTests.cs`؛ کنترل موجودی SQL در فایل I |
| S01–S15 | `…/Holds/V10SeatHoldAcceptanceTests.cs` و به‌روزرسانی `SeatReservationAcceptanceTests.cs` |
| E04–E12 | در همان فایل‌ها. E01 تا E03: NOT VERIFIED |
| R01–R10 | سوئیت‌های موجود، `has-pending-model-changes` و HTTP زنده (Gate 4) |

## 11. تعارض‌های واقعی

شش مورد با شاهد و پیشنهاد در `reports/V10-GATE0-QUESTIONS-AND-CONFLICTS.md` آمده است. Gate 1 پیش از پاسخ آن‌ها شروع نمی‌شود.
