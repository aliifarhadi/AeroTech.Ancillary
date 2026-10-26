# قرارداد مرجع Reservation و Inventory — AeroTech.Ancillary — v10

> **این سند Overview و مرجع واحد تصمیم‌های بیزنسی است.** توضیحات فیلدبه‌فیلد، الگوریتم، تست و پرامپت در فایل‌های بعدی همان Pack آمده‌اند؛ هیچ‌کدام جایگزین این سند نیستند.

## 1. هدف
پس از خرید/انتخاب Ancillary در جریان `AncillaryOffers → Price → Resolve → Create Order`، Ordering از Ancillary درخواست Hold می‌کند. یک Hold می‌تواند چند خدمت با Scope و نوع متفاوت (Baggage، Meal، CIP، Wi-Fi، Seat Selection و ...) داشته باشد. هر خدمت یک `UnitId` مستقل دارد. Hold مرجع اصلی، زمان انقضا و عملیات Confirm/Release/Cancel سراسری دارد؛ یک یا چند Unit را نیز می‌توان بدون از بین بردن بقیه آزاد یا لغو کرد، با فهرست `unitIds` روی **همان مسیرهای گروهی** (مسیر جدا برای هر Unit وجود ندارد).

**حکم قطعی مالک درباره SeatMap:** رزرو واقعی FlightSeat و OccupiedSeat در اختیار **Ordering/FlightFlow** است. Ancillary فقط حق/خدمت تجاری Seat Selection را Hold/Confirm/Release/Cancel می‌کند. نه درخواست FlightFlow می‌فرستد، نه صندلی فیزیکی را Block/Release می‌کند، نه وضعیت اشغال صندلی را کنترل می‌کند. این مرز در تمام مراحل حفظ شود؛ شاید بعداً جداگانه عوض شود، نه در این Pack.

## 2. تصویر ساده جریان
```text
Frontend / Channel                   Ordering                         Ancillary
  POST AncillaryOffers ---------------------------------------------------->
  <----- grouped variants + products + per-unit options -------------------
  POST AncillaryOffers/Price --------------------------------------------->
  <----- selectionId (s_...) + expiry ------------------------------------
                                     POST AncillaryOffers/Resolve -------->
                                     <---- resolved commercial details -----
                                     Create Order + persist accepted facts
                                     FlightFlow seat hold (اگر Seat انتخاب شده)
                                     (رزرو فیزیکی صندلی و رفع double-seat)
                                     POST Service-Holds (other ancillary) -->
                                     <----- holdId + units -----------------
                                     POST SeatMaps/Holds with holdId ------->
                                     <----- SAME holdId + seat units -------
                                     Payment / guarantee / ticketing gates
                                     POST Service-Holds/{id}/Confirmations ->
                                     <----- Confirmed ----------------------
                                     Issue Ticket/EMD در Ordering
```
بدون خدمات غیرSeat: `SeatMaps/Holds` می‌تواند از ابتدا Reservation بسازد. بدون Seat: `Service-Holds` کل مسیر را انجام می‌دهد. اگر یک Hold شامل هر دو باشد، Ordering ابتدا Hold عمومی را می‌سازد و Seat را به **همان HoldId** اضافه می‌کند. در شکست هر گام، Ordering عملیات جبرانی Release را orchestrate می‌کند؛ Ancillary هیچ تراکنش بین‌سرویسی با FlightFlow فرض نمی‌کند.

## 3. شش تصمیم معماری
1. **Reuse:** `AncillaryReservation` و `AncillaryReservationUnit` موجود حفظ می‌شوند؛ Aggregate جایگزین یا `ReservationPreparation` ایجاد نمی‌شود.
2. **Unit نه Variant-Aggregate:** هر خرید یک Unit با OrderServiceId مستقل است؛ برای Baggage/Meal/CIP یک Aggregate جداگانه نمی‌سازیم. داده تخصصی Hold محدود به نیاز **مصرف موجودی** است. بقیه اطلاعات در Order و Resolve حفظ می‌شوند.
3. **Inventory Authority:** `Unlimited` = بدون ظرفیت محدود محلی؛ `Local` = تخصیص واقعی از Inventory موجود؛ `Supplier` = تا اتصال Provider واقعی **گیت‌شده**، نه Fake Held؛ محصولی که بدون Provider confirmation نمی‌توان فوراً رزرو کرد نیز نباید با برچسب Unlimited به دروغ Held شود.
4. **عدم oversell:** برای Local، مصرف و ثبت Unit در **یک تراکنش SQL و با قفل منبع مشترک** (`sp_getapplock` با `@LockOwner='Transaction'`) انجام می‌شود. قفل per Order موجود به‌تنهایی کافی نیست. ظرفیت با Ledger تخصیص سنجیده می‌شود، نه با Counter.
5. **فقط یک Reservation:** یک Hold اصلی، Unitهای مستقل، آزادسازی/لغو یک یا چند جزء با `unitIds` بدون شکستن بقیه؛ Confirm/Release/Cancel کلی روی همان Aggregate. صدور EMD/Ticket/Refund متعلق به Ordering است.
6. **Seat commercial only:** SeatMap با Endpoint اختصاصی، داخل همان Aggregate؛ صفر Local Seat inventory allocation؛ هیچ تغییر در SeatMap shopping/Resolve و FlightFlow.

## 4. توصیف Aggregate به زبان کسب‌وکار
`AncillaryReservation`: مرجع یک عملیات Reservation سفارش؛ `HoldId`، `OrderId`، `Reference`، `IdempotencyKey`، `RequestedExpiresAt`، `ExpiresAt`، `Status` و Units. `ExpiresAt` برای همه Holdهای تازه **الزامی** است. `Reference` شناسه ارتباطی از Ordering است، نه PNR، EMD و نه FlightFlow HoldId.

`AncillaryReservationUnit`: هر سرویس رزرو‌شده با `UnitId`، `OrderServiceId`، Definition/Provision، Traveller/Coverage/Flights، Quantity، Status و در صورت Seat تجاری `SeatMapSelectionId`/`SeatNumber`/`PricingId`. هر واحد را جدا آزاد/لغو می‌کنیم. برای Local به آن تعدادی **InventoryAllocation کوچک** متصل می‌شود؛ خود Reservation منبع ظرفیت نیست.

**مرجع جزء:** `UnitId` همان operational reference مورد نیاز Ordering است؛ ساختن یک `BaggageRef` یا `MealRef` مضاعف ضرورت ندارد. Provider خارجی، اگر در آینده واقعی شد، می‌تواند `ProviderUnitRef` خودش را بدهد؛ با UnitId اشتباه نشود.

## 5. اعتبارسنجی؛ چه کسی چه چیزی را می‌سنجد؟
| مرحله | مسئولیت و مرز |
|---|---|
| Price/Resolve | Ancillary قبلاً قیمت و Eligibility قابل‌فروش را تأیید و اطلاعات تخصصی را به Ordering برگردانده است. |
| Create Order | Ordering صحت تعلق Selection، مسافر، پرواز/Portion، وضعیت Order و accepted commercial terms را **پایدار** ثبت می‌کند. |
| Hold | Ancillary بر اساس درخواست **معتبر Service-to-Service**، Definition/Provision/Quantity/Scope، سیاست موجودی، هویت منابع و ظرفیت واقعی اکنون را کنترل می‌کند؛ قیمت را دوباره محاسبه یا از کلاینت قبول نمی‌کند. |
| SeatMap Hold | Ancillary می‌تواند Seat Selection را از نظر Definition/Provision/Price Identity و Rule تجاری با Verifier فعلی بررسی کند؛ *اشغال شماره صندلی* بررسی نمی‌شود. |
| Confirm | Ordering اعلام می‌کند payment/guarantee مجاز است؛ Ancillary Hold زنده را به Confirmed تبدیل می‌کند و ظرفیت هنوز مصرف‌شده باقی می‌ماند. |
| Release/Expire | Ancillary صرفاً Allocationهای همین Unit را از حساب مصرف خارج می‌کند؛ Seat physical مستقل است. |
| Cancel Confirmed | با دستور Ordering پس از بررسی قوانین بیزنسی/اسناد، Unit یا کل Reservation را Cancel می‌کند؛ Refund/EMD در Ordering. |

**هیچ ورودی `amount`، `currency`، نرخ مشتری یا `EMD` در Hold نداریم.** برای نگاشت Order↔Ancillary، شناسه‌های واقعی OrderService/Traveller/Flight استفاده می‌شود؛ ورودی Hold قرار نیست دوباره کل درخواست Price را حمل کند.

## 6. موجودی واقعی در مقابل ظرفیت پایه
`FlightCountInventory.TotalCapacity`، `FlightWeightInventory.CapacityKg` و `AirportSlotInventory.CapacityPersons` در سورس فعلی **ظرفیت تعریف‌شده** هستند، نه مانده قابل‌فروش. این فیلدها در Hold کاهش نمی‌یابند. یک تخصیص کوچک، immutable و قابل‌ردگیری در `AncillaryReservationUnitAllocation` اضافه می‌شود؛ وضعیت فعال آن از Status Unit و ExpiresAt Reservation نتیجه می‌شود. فرمول:

`Available = ConfiguredCapacity - SUM(active Held allocations not logically expired + Confirmed allocations)`

برای پرواز اتصالی (Portion) می‌توان یک Unit با چند Allocation برای چند پرواز داشت. مصرف وزن در این فاز فقط `FixedKgPerAcceptedUnit × Quantity` است؛ Policy با `AcceptedWeightKg` پیش از هر نوشتن با خطای صریح Unsupported رد می‌شود، چون هیچ محصول قابل‌فروشی وزن کل پذیرفته‌شده را نمی‌آورد. برای `FlightCountPlusWeight` باید Count و Weight **هر دو با هم** موفق شوند. برای `AirportSlot` هر بازه زمانی همپوشانِ لازم باید به صورت اتمیک تخصیص یابد. اگر یکی کم باشد، **هیچ** تخصیصی ثبت نمی‌شود.

ظرفیت `Unlimited` هیچ Allocation محلی ندارد؛ برای `Supplier` یا هر Provision با تأیید اجباری تأمین‌کننده که Adapter واقعی ندارد، نه تخصیص محلی ساخته می‌شود و نه موفقیت جعلی برمی‌گردد. برای Seat نیز تخصیص فیزیکی Ancillary صفر است.

## 7. نمونه یک Hold مختلط
در Ordering: Order `12001` دارای Baggage برای ADT-1 روی دو پرواز `801,802`، Meal برای ADT-2 روی 801 و Seat Selection برای ADT-1 روی 801 است.

1. Ordering اطلاعات Price/Resolve را در Order ثبت می‌کند.
2. اگر Seat وجود دارد، Ordering طبق جریان فعلی FlightFlow، شماره صندلی درخواست‌شده را در Flight Hold فیزیکی ارسال می‌کند؛ Ancillary هیچ کاری با اشغال فیزیکی نمی‌کند.
3. `POST /Service/v1/Ancillaries/Service-Holds` با Baggage و Meal انجام می‌شود. پاسخ `holdId=9001` و UnitIds `9101,9102` است؛ Inventory count/weight با هم تخصیص یافته‌اند.
4. `POST /Service/v1/Ancillaries/SeatMaps/Holds` با `holdId=9001`، همان Order و Seat Selection انجام می‌شود. پاسخ **همان holdId** و Unit سوم `9103` را می‌دهد. هیچ Seat allocation محلی وجود ندارد.
5. اگر مشتری Meal را حذف کند: `DELETE Service-Holds/9001?unitIds=9102` → Meal آزاد می‌شود، Baggage/Seat همچنان Held می‌مانند.
6. `POST Service-Holds/9001/Confirmations` → فقط Unitهای Held به Confirmed تغییر می‌کنند. Unit آزادشده Released می‌ماند. نتیجه Hold واحد با Unit statusهای صریح است.
7. Issue / EMD اگر لازم باشد در Ordering انجام می‌شود.

**Failure:** اگر Inventory یک Baggage کافی نبود، کل `Service-Holds` جدید (جزء غیرSeat) با خطای shortage رد می‌شود و هیچ Allocation نمی‌نویسد. اگر Seat append رد شد، Hold عمومی در حالت Held باقی می‌ماند تا Ordering صریحاً Release/Retry کند؛ Ancillary اجازه ندارد خودش FlightFlow را compensate کند.

## 8. APIهای مجاز این فاز (names مطابق کد موجود)
```http
POST   /Service/v1/Ancillaries/Service-Holds                        # Create non-seat hold
GET    /Service/v1/Ancillaries/Service-Holds/{holdId}               # Read
POST   /Service/v1/Ancillaries/Service-Holds/{holdId}/Confirmations # Confirm all remaining Held
DELETE /Service/v1/Ancillaries/Service-Holds/{holdId}               # Release all remaining Held, or ?unitIds=..&unitIds=.. only those
POST   /Service/v1/Ancillaries/Service-Holds/{holdId}/Cancellations # Cancel all remaining Confirmed, or body unitIds only those
POST   /Service/v1/Ancillaries/SeatMaps/Holds                        # Seat-only create or append to holdId [extend existing]
```
هیچ Endpoint عمومی `Append` برای non-Seat و هیچ مسیر جدا برای یک Unit در این مرحله وجود ندارد؛ عملیات جزئی تغییر هم‌زمان چند Unit در یک Aggregate است. SeatMap Shopping و Resolve اختصاصی خود را حفظ می‌کنند.

## 9. سیاست ساده وضعیت‌ها
Enumهای فعلی `Held=1`, `Confirmed=2`, `Released=3`, `Expired=4`, `Cancelled=5` برای Reservation و Unit حفظ می‌شوند؛ State ساختگی `PartiallyConfirmed`, `Pending`, `Mixed` اضافه نمی‌کنیم. Header وضعیت جریان **Reservation** را می‌گوید، Unitها وضعیت واقعی هر جزء را. Confirm فقط Heldهای فعال را Confirm می‌کند و Releasedها را دست نمی‌زند؛ Cancel فقط Confirmedهای فعال را Cancel می‌کند.

- `Held → Confirmed`؛ retry Confirm = همان نتیجه.
- `Held → Released` (همه یا یک Unit)؛ retry Release = همان نتیجه.
- `Held → Expired` پس از TTL؛ عملیات روی Unit منقضی‌شده fail closed.
- `Confirmed → Cancelled` (همه یا یک Unit)؛ retry Cancel = همان نتیجه.
- `Released/Cancelled/Expired` دوباره موجودی مصرف نمی‌کنند.
- اگر همه Unitهای جاری Held آزاد شدند، Header `Released` می‌شود؛ اگر همه Confirmedها لغو شدند، Header `Cancelled` می‌شود. اگر فقط بعضی آزاد/لغو شدند، Header در وضعیت عملیاتی فعال (`Held` یا `Confirmed`) می‌ماند، و Units حقیقت جزئی را می‌گویند.
- حذف فیزیکی Unit و حذف تاریخچه ممنوع است.

## 10. محدوده و ممنوعیت‌ها
- **داخل Scope:** Local Count، Weight (فقط FixedKg)، Count+Weight، Slot، Unlimited، Seat commercial facade، mixed hold، Release/Cancel انتخابی با `unitIds` روی مسیرهای گروهی، idempotency/retry، lazy و persisted Expire، concurrency و SQL acceptance.
- **خارج Scope اما با failure صریح:** Supplier واقعی بدون Provider contract؛ `FlightWeightConsumptionMode.AcceptedWeightKg`؛ `DailyCount`, `RoomNight`, `AssignedAsset` در کد هنوز unsupported؛ `A09 ExtraSeat` و نیازمند Quote/Approval؛ Seat physical assignment؛ Issue/EMD/Refund؛ تغییر Pricing/Shopping/SeatMap Resolve؛ SAGA عمومی، Queue/Outbox، Domain Event یا Projection جدید.
- **استثنای محدود Shopping/Price:** اگر محصولی در Price پذیرفته شود ولی به‌علت Supplier بدون Adapter در Hold همیشه رد شود، با یک Guard کوچک در همان مسیر عرضه/Price و تست منفی کنار گذاشته می‌شود؛ Engine یا DTO جدید ساخته نمی‌شود.
- اگر درخواست نیازمند Supplier یا الگوی unsupported است، کد باید **قبل از ثبت Hold** fail-closed کند و دلیل مشخص بدهد؛ نه اینکه به Unlimited تبدیل کند.
- همه عملیات عمومی فقط Service-to-Service است؛ Policy احراز هویت موجود پروژه بررسی و حفاظت شود. تغییر DB یا migration فقط additive و با گزارش Impact.

## 11. معیار بسته‌شدن
رزرو مخلوط واقعی، عدم Oversell تحت هم‌زمانی دو Order، مصرف Count+Weight و Slot، آزادسازی/لغو یک یا چند Unit بدون از بین رفتن دیگران، SeatMap commercial در همان Hold بدون FlightFlow call، Retry، Expire و Confirm/Cancel، و سازگاری کامل API و Regression SeatMap/Shopping باید ثابت شود: Local با **SQL واقعی**، و Unlimited/Seat/Lifecycle با **HTTP واقعی**. HTTP زنده Local تا اتصال Reference واقعی GATED است و PASS شمرده نمی‌شود. Gate بدون شواهد Pass نیست.
