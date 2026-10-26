# Reservation v10 — Gate 3: Seat تجاری و چرخه‌ی عمر کامل

**تاریخ:** 2026-10-26 · **مبنا:** `feat/ancillary-offers-v9@824d88b`؛ Gate 1 تا 3 commit نشده‌اند
**مرجع:** Pack v10 ادغام‌شده، فایل‌های `02`، `03`، `05`، `06`، `07` و `13`

## 1. وضعیت
Gate 3 ساخته و آزموده شد: روی SQL واقعی، و روی HTTP زنده با یک Host جانبی (پورت 5299، پایگاه dev).
- Seat حالا به همان Hold اضافه می‌شود.
- Confirm، Release، Cancel، Expire و Append همه زیر قفل سفارش اجرا می‌شوند. Expire برای هر سفارش جدا انجام می‌شود.
- Ancillary هیچ فراخوانی‌ای به FlightFlow ندارد.

چیزی commit یا مستقر نشده است.

## 2. چه ساخته شد

### SeatMaps/Holds با `holdId?`
- `POST Service/v1/Ancillaries/SeatMaps/Holds` همان Facade قبلی است. فقط `holdId?: long` به Request، Command و Validator اضافه شد (فقط شکل: خالی، یا بزرگ‌تر از صفر). Route تازه‌ای ساخته نشد. پاسخ مثل قبل `201` با همان `AncillaryHoldResult` است.
- **بدون `holdId`:** Hold فقط-Seat با مسیر موجود (`IHoldAncillaryServicesService.HoldAsync`) ساخته می‌شود.
- **با `holdId`:** `AppendSeatsAsync` روی همان Root اجرا می‌شود:
  1. `OrderId` ریشه خوانده می‌شود و قفل `ancillary-hold:{orderId}` گرفته می‌شود.
  2. Root بارگذاری و `AncillaryReservation.AppendSeats` اجرا می‌شود. این متد replay، برابری OrderId، Reference و Expiry، وضعیت Held زنده، Seat تکراری و OrderService زنده در همان Root را می‌سنجد.
  3. OrderService زنده در Holdهای دیگر بررسی می‌شود (16406).
  4. ذخیره. `holdId` تغییر نمی‌کند. Unitهای Seat کلید `AppendIdempotencyKey` و `Allocations=[]` دارند.
- **Replay پیش از Verify** (بند 4 فایل `05`): Facade اول `FindHeldSeatsAsync` را صدا می‌زند.
  - هویت درخواست = `orderId` + `reference` + `requestedExpiresAt` + مجموعه‌ی (`orderServiceId`، `travellerId`، `seatMapSelectionId`). در append فقط Unitهای همان کلید append مقایسه می‌شوند؛ در create فقط Unitهای Create.
  - برابر بودن یعنی نتیجه‌ی ذخیره‌شده بدون Verify دوباره برمی‌گردد. پس تغییر قیمت یا Policy یک Hold واقعاً ساخته‌شده را در retry بی‌اعتبار نمی‌کند.
  - تفاوت یعنی 16403.
- **اصلاح Domain:** replay در `AppendSeats` حالا Header را هم می‌سنجد. قبلاً همان کلید append با `orderId` دیگر بی‌صدا no-op می‌شد؛ حالا 16403 می‌دهد.

### قفل سفارش روی همه‌ی عملیات
- `Confirm`، `Release` و `Cancel`: ابتدا `FindOrderIdAsync(holdId)`، بعد قفل سفارش، بعد بارگذاری تازه و عملیات. سفارش مشغول یعنی 16416 و هیچ نوشتنی انجام نمی‌شود.
- **Expire** به الگوی Ordering (`ReservationDeadlineService` + `ReservationDeadlinePoller`) درآمد:
  - `ListDueOrderIdsAsync(batch)` در یک Scope اجرا می‌شود.
  - برای هر سفارش در Scope جدا `ExpireAsync(orderId)` زیر قفل اجرا می‌شود.
  - `BusinessException` در سطح Information ثبت می‌شود و بقیه در سطح Error.
- قفل منبع موجودی روی Release، Cancel و Expire گرفته نمی‌شود، چون فقط مصرف را کم می‌کنند (تصمیم Owner).

### مرز FlightFlow
`SeatSelectionVerifier` فقط این‌ها را می‌خواند: `ISeatMapPricingEngine`، `IAircraftSeatMapProvider` (AirInfo)، Reference Data و ساعت. `IOccupiedSeatProvider` فقط در `GetSeatMapService` (نمایش SeatMap در Shopping) مصرف می‌شود.

## 3. شواهد Sibling
| موضوع | Ordering (`AeroTech.Ordering.Final@cd50a2a`) | این Gate |
|---|---|---|
| قفل | `ReservationLock.AcquireAsync(orderId)` → `IDistributedLock` با کلید `reservation:{orderId}`؛ اشغال یعنی BusinessException | `AncillaryHoldLock` با کلید `ancillary-hold:{orderId}`؛ اشغال یعنی 16416 |
| Release/Confirm | ابتدا قفل، بعد بارگذاری و عملیات (`ReleaseReservationService`) | همان ترتیب. چون Route فقط `holdId` دارد، `OrderId` تغییرناپذیر پیش از قفل خوانده می‌شود |
| انقضا | Poller: فهرست سفارش‌های سررسید در یک Scope، سپس هر سفارش در Scope خودش و زیر قفل | همان شکل |

## 4. نتیجه‌ها (همه در همین نوبت اجرا شدند)
| کنترل | نتیجه |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 خطا، 61 هشدار (بدون تغییر) |
| Domain conformance | 287 از 287 (discovered 287) |
| Shopping | 123 موفق + 1 skip از 124 (discovered 124)، بدون تغییر |
| Application acceptance (SQL) | 254 از 254 (discovered 254)، بدون abort، 1.08 دقیقه |
| `ef migrations has-pending-model-changes` | No changes؛ Gate 3 ستون یا Migration تازه ندارد |
| HTTP زنده روی Host جانبی | 10 از 10 گام و 9 از 9 کنترل (بند 6) |
| فراخوانی‌های خروجی Host در همان اجرا | 3 درخواست، هر سه `GET AirInfo …/Aircrafts/1/DisplaySeatMap`. **FlightFlow: صفر**. Replay هیچ درخواستی نفرستاد |
| تغییر در `Query/Shopping`، `Shopping` و Queryهای رزرو | هیچ (`git diff --stat` خالی) |

تست‌های جدید:
- کلاس SQL `Holds/V10SeatAndLifecycleAcceptanceTests` با 9 تست.
- Domain: `V10_S05_E05_a_seat_replay_is_recognised_by_its_selection_identity_and_a_changed_header_or_seat_is_a_conflict`.

## 5. پوشش ماتریس

| شناسه | تست | نتیجه‌ی اثبات‌شده |
|---|---|---|
| S01, S08, S09, S15 | SQL `V10_S01_S08_S09_S15_…` + گام 9 زنده | Hold فقط-Seat: یک Unit با Quantity 1، Sector، یک Flight، SeatNumber و PricingId، بدون Allocation و بدون مدرک اشغال فیزیکی |
| S02, S05, S09, S11, E08 | SQL `V10_S02_S05_S09_S11_E08_…` + گام‌های 1 تا 8 زنده | Append همان HoldId را نگه می‌دارد؛ replay همان UnitIdها را برمی‌گرداند و Verify را فقط یک‌بار صدا می‌زند؛ Seat بدون Allocation است حتی وقتی Bag در همان Hold Allocation دارد |
| S02 (هم‌زمان) | SQL `V10_S02_concurrent_appends_…` | دو Append هم‌زمان: یکی 16416 می‌گیرد یا هر دو پشت‌سرهم اجرا می‌شوند. Append تکراری فقط یک Unit می‌سازد. Seat تکراری وجود ندارد |
| S02 (قرارداد) | `V10_S02_the_seat_hold_contract_…` | `holdId` خالی یا مثبت است؛ 0 و منفی رد می‌شوند |
| S03, S04, L23, E07 | SQL `V10_S03_S04_L23_E07_…` + Domain (Gate 1) + گام 4 زنده | Order دیگر، Reference دیگر و Expiry دیگر → 16402. Seat فروش‌نرفتنی → 16415. Hold ناموجود → 16401. Root تأییدشده → 16411. Root منقضی → 16407. در همه‌ی این موارد Root، Held با یک Unit و Expiry ثابت باقی می‌ماند |
| S05, S06, E05 | Domain `V10_S05_E05_…` + SQL `V10_E05_…` | replay با هویت انتخاب شناخته می‌شود و بعد از حذف Seat از کاتالوگ هم بدون Verify همان Hold را برمی‌گرداند؛ تغییر Header یا Seat → 16403 |
| S06, S07, L21 | SQL `V10_S06_S07_L21_…` | کلید append تکراری با Seat دیگر → 16403؛ همان Seat و پرواز با OrderService دیگر → 16402؛ OrderService زنده در همین Hold یا Hold دیگر → 16406؛ هیچ Unitی در SQL اضافه نشد |
| S10, S11 | `V10_S10_S11_…` + لاگ Host زنده | پیمایش گراف سازنده‌ها از Facade، Verifier و همه‌ی عملیات چرخه‌ی عمر به هیچ نوعی در `*.FlightFlow` یا `IOccupiedSeatProvider` نمی‌رسد؛ در اجرای زنده صفر درخواست به FlightFlow |
| S12 | Shopping 124 + `git diff` | SeatMap pricing، Resolve و Verifier بدون تغییر |
| S13 | — | Ancillary بین دو سفارش درباره‌ی یک صندلی داوری نمی‌کند؛ بررسی تکرار Seat فقط درون یک Root است (`AncillaryReservation.Add`). مالکیت با Ordering/FlightFlow است. تست جداگانه ندارد |
| S14 | `SEAT_A08_…` | Validator موجود برای Coupon در Hold بلیت‌خورده بدون تغییر رد می‌کند |
| L01–L15 | Domain Gate 1 (`V10_L01_…`، `V10_L09_…`، `V10_L11_…`، `V10_L15_…`) + SQL `V10_L02_L08_L10_L12_L14_E08_…` | بدون تغییر و سبز |
| L03, L06, L11, L16 (قفل) | SQL `V10_L03_L06_L11_L16_…` | با قفل گرفته‌شده‌ی سفارش، Confirm، Release، Cancel، Expire و Append همه 16416 می‌دهند و چیزی تغییر نمی‌کند |
| L16, L17 | همان تست + `SEAT_A05_…` + Domain `V10_L16_L17_L18_…` | در مهلت، مصرف بدون poller صفر است؛ `ListDueOrderIdsAsync` سفارش را برمی‌گرداند؛ `ExpireAsync` بار اول 1 و بار دوم 0؛ وضعیت Expired در SQL ذخیره شد |
| L18, L22 | Domain Gate 1 | بدون تغییر |
| L19, L20 | SQL Gate 2 `V10_I17_…_L19_L20` + Domain `V10_L19_L20_…` | بدون تغییر |
| E06 | — | جبران شکست صندلی فیزیکی کار Ordering است؛ Ancillary هیچ فراخوانی FlightFlow ندارد (S10). **NOT VERIFIED** در سمت مصرف‌کننده |

## 6. نمونه‌های HTTP زنده (Host جانبی 5299، پایگاه dev، اجرای 2)
محصولات dev: `BAG_CABIN_EXTRA` و `BAG_OVERWEIGHT` (Unlimited، Sector) و Pricing صندلی `1560040688677727841`. اسکریپت: scratch `v10/g3_live.py`؛ Trace کامل: scratch `v10/g3-live-run2.json`.

نمونه‌ی درخواست append (بدون آرایه‌ی `seats`):
```json
{"idempotencyKey":"G3-LIVE-SEAT-2","orderId":9930020,"orderCommercialVersion":1,"reference":"ORD-9930020","requestedExpiresAt":"2026-10-26T14:59:50+00:00","pointOfSaleId":1,"currencyId":70,"ticketedAtUtc":null,"holdId":1564284931058696192}
```

Hold `1564284931058696192`. قالب Unit: `unitId/orderServiceId/status`.

| گام | درخواست | HTTP | وضعیت Hold | Unitها |
|---|---|---|---|---|
| 1 | `POST Service-Holds` (دو Bag) | 201 | Held | `…193/99300201/Held`، `…194/99300202/Held` |
| 2 | `POST SeatMaps/Holds` با `holdId` | 201 | Held، همان HoldId | دو Unit قبلی + `1564284950973251584/99300203/Held/12A` |
| 3 | همان درخواست دوباره | 201 | همان | همان سه UnitId |
| 4 | append با Reference دیگر | 422 | — | خطای 16402 |
| 5 | `DELETE …?unitIds=…194` | 200 | Held | `…194` Released، بقیه Held |
| 6 | `POST …/Confirmations` | 200 | Confirmed | `…193` و صندلی Confirmed؛ `…194` Released ماند |
| 7 | `POST …/Cancellations {"reasonCode":"CUSTOMER_REQUEST","unitIds":[صندلی]}` | 200 | Confirmed | فقط صندلی Cancelled |
| 8 | `GET Service-Holds/{id}` | 200 | Confirmed | برابر گام 7 |
| 9 | `POST SeatMaps/Holds` بدون `holdId` | 201 | Hold تازه `1564284967498809344`، Held | `…345/99300211/Held/12B` |
| 10 | `DELETE Service-Holds/{id}` | 200 | Released | صندلی Released |

## 7. Allocationهای SQL قبل و بعد (Hold ترکیبی: 2 Bag با موجودی Local، 1 Meal Unlimited، 1 Seat)

| گام | ردیف‌های Allocation سفارش | مصرف فعال منبع Bag | ظرفیت |
|---|---|---|---|
| بعد از Hold | دو ردیف `FlightCount:{source}:1.000` | 2.000 | `5\|2` |
| بعد از append صندلی | همان دو ردیف؛ صندلی صفر ردیف | 2.000 | `5\|2` |
| بعد از Release وعده‌ی غذا | بدون تغییر | 2.000 | `5\|2` |
| بعد از Confirm | بدون تغییر | 2.000 | `5\|2` |
| بعد از Cancel یکی از Bagها | بدون تغییر؛ ردیف باقی می‌ماند و فقط وضعیت Unit تغییر می‌کند | 1.000 | `5\|2` |
| تست قفل: Hold یک Bag، پس از رسیدن مهلت | یک ردیف | 0.000 پیش از poller | — |

## 8. فایل‌های تغییرکرده در Gate 3

| لایه | فایل |
|---|---|
| Domain | `AncillaryReservation` (`HoldsSeats`، replay در `AppendSeats` با Header)؛ `AncillaryReservationUnit.IsSelectedBy`؛ `Arguments/AncillaryReservationSeatSelectionArgs` (جدید)؛ `IAncillaryReservationRepository` (`FindAsync`، `FindOrderIdAsync`، `ListOrderIdsWithExpiredHeldAsync`، `ListExpiredHeldAsync(orderId)`) |
| Application | `HoldAncillaryServicesService` و Interface (`AppendSeatsAsync`، `FindHeldSeatsAsync`)؛ `HoldSeatSelectionsService`، `IHoldSeatSelectionsCommand`، `ServiceHoldSeatSelectionsCommand`، `HoldSeatSelectionsValidator`؛ `Confirm`، `Release` و `CancelAncillaryHoldService` (قفل)؛ `ExpireAncillaryHoldsService` و Interface |
| Persistence | `AncillaryReservationRepository` |
| Consumers | `Jobs/AncillaryHoldExpiryPoller` |
| RestApi | `V1/SeatMaps/Controllers/ServiceController`، `V1/SeatMaps/Requests/HoldSeatSelectionsRequest` |
| Tests | جدید: `Holds/V10SeatAndLifecycleAcceptanceTests`. تغییرکرده: `V10ReservationLifecycleConformanceTests` (+1)، `AncillaryScope` (قفل مشترک اختیاری)، `TestOnlyHoldLock` (thread-safe)، `M1Commands` (`HoldId`)، `SeatReservationAcceptanceTests` (`SEAT_A05` با Expire برای هر سفارش، `SEAT_A08` با `HoldId`)، `InventoryHarness` (Helperهای SQL مشترک `AllocationsAsync` و `UsageAsync`)، `V10InventoryAllocationAcceptanceTests` (همان Helperها)، سه Pin |

**Pin:** مجموعه‌ی قفل‌شده‌ی رزرو حالا 82 فایل است (+`AncillaryReservationSeatSelectionArgs`). Hash `758010CE0F03CA960ACECEF62843BED3B7F7FCA52929066C6C963396CB8EABDD` در `P2_X07`، `V12_C04` و `X20`.

## 9. نکته‌ها
- **S10:** با یک پیمایش ایستای گراف وابستگی‌ها و لاگ خروجی Host زنده اثبات شد، نه با Mock تزریقی روی HttpClient. Verifier واقعی به AirInfo نیاز دارد، و لاگ زنده نشان می‌دهد فقط AirInfo صدا زده شده است.
- **داده‌ی dev:** اجرای 1 اسکریپت پس از ساخت Hold سفارش `9930010` به‌خاطر خطای خود اسکریپت (پاکت `data`) متوقف شد. آن Hold، Held مانده و با رسیدن مهلتش poller آن را Expired می‌کند. داده‌های dev آزمایشی‌اند.
- **تغییر رفتاری:** `holdId` اختیاری است و درخواست‌های قبلی SeatMaps/Holds بدون تغییر کار می‌کنند. مهلت اجباری Hold تغییر Gate 1 بود.

## 10. قدم بعد
Gate 4 شروع نشده است. دو نکته‌ی باز Gate 2 هنوز پاسخ ندارند: Retire منبع با مصرف زنده، و سقف Pool اتصال. Commit و Push انجام نشده است.
