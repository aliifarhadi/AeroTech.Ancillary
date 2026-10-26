# Reservation v10 — Gate 0: سؤال‌ها و تعارض‌ها

**تاریخ:** 2026-10-26
**مبنا:** `2025efd` (Pin پک)
**گزارش کامل:** `reports/V10-GATE0-SOURCE-AUDIT.md`

## 1. FlightFlow (هم‌خانواده) چه می‌کند

جدول کامل در بخش 2 گزارش Gate 0 آمده است. خلاصه آن:

- **مرجع و وضعیت:** یک مرجع گروه (`HoldBatchId`) و یک مرجع برای هر صندلی (`SeatHoldReference`)، با وضعیت مستقل برای هر صندلی.
- **انتقال وضعیت:** Confirm و Release فقط واحدهای Held را عوض می‌کنند و Cancel فقط Confirmedهای فهرست‌شده را.
- **ظرفیت:** شمارنده‌های `Held`، `Confirmed` و `RemainingCap` روی خود منبع (`FlightCapacity`) در هر انتقال عوض می‌شوند.
- **قفل:** RedLock روی کلید پروازها دور کل عملیات.
- **انقضا:** با job Quartz. ظرفیت فقط وقتی job اجرا شود برمی‌گردد.
- **لغو جزئی:** با فهرست `seatHoldReferences` در بدنه همان مسیر Cancellation. مسیر جدا برای هر صندلی ندارد و Release جزئی هم ندارد.

## 2. سؤال‌ها

### Q1. مدل ظرفیت: دفتر تخصیص (پک) یا شمارنده روی منبع (FlightFlow)

- **FlightFlow:**
  - `FlightCapacity.Held`، `Confirmed` و `RemainingCap` در هر Hold، Release، Confirm، Expire و Cancel عوض می‌شوند.
  - ظرفیتِ منقضی فقط وقتی job اجرا شود برمی‌گردد (`FlightCapacity.cs:185-215`).
- **پک:** 04 §8 هیچ شمارنده قابل‌تغییری نمی‌خواهد؛ «دفتر تخصیص + وضعیت Unit» تنها منبع حقیقت است. I10 می‌خواهد Held منقضی، پیش از اجرای poller هم دیگر شمرده نشود.
- **اثر شمارنده در Ancillary:**
  - هر Hold نسخه `FlightCountInventory` را بالا می‌برد و با `expectedVersion` عملیات مدیریتی (Adjust، Close) برخورد می‌کند.
  - I10 بدون اجرای job ممکن نمی‌شود.
- **پیشنهاد:** دفتر تخصیص پک، یعنی جدول Allocation و جمع مصرف در لحظه. اصول FlightFlow می‌ماند: مرجع گروه و جزء، انتقال فقط روی واحد واجد شرایط، و replay.

Answer:

### Q2. سازوکار قفل منبع

- **FlightFlow:** `IDistributedLocker.ExecuteWithLocksAsync` (RedLock، کلیدهای مرتب، retry) دور کل عملیات. Framework این مخزن فقط `IDistributedLock.AcquireAsync(key, expiry)` تک‌کلیدی دارد.
- **پک:** `sp_getapplock` با `@LockOwner='Transaction'` برای هر منبع، به ترتیب ثابت. نمونه‌اش در همین مخزن هست: `AirportSlotInventoryRepository`، با مالکیت Session.
- **پیشنهاد:** `sp_getapplock` با مالکیت Transaction.
  - قفل دقیقاً با commit یا rollback همان تراکنش آزاد می‌شود؛ قفل Redis با TTL ممکن است پیش از commit تمام شود.
  - به Redis وابسته نیست.
  - قفل سفارش موجود (Redis) برای ترتیب عملیات یک سفارش می‌ماند.

Answer:

### Q3. شکل Release و Cancel جزئی

- **پک:** دو مسیر تازه، هر بار برای یک Unit:
  - `DELETE Service-Holds/{holdId}/Units/{unitId}`
  - `POST Service-Holds/{holdId}/Units/{unitId}/Cancellations {reasonCode}`
- **FlightFlow:** مسیر جدا برای هر جزء ندارد.
  - لغو جزئی = همان `POST Seat-Confirmations/{id}/Cancellation` با `seatHoldReferences[]` در بدنه (`CancelConfirmedFlightSeatsRequest.cs`).
  - Release جزئی ندارد.
- **قواعد CLAUDE.md:** قاعده 2: «برای هر child entity مسیر جدا نداریم». قاعده 1: الگوی هم‌خانواده بر سند مقدم است.
- **گزینه‌ها:**
  - (الف) دو مسیر پک.
  - (ب) شکل FlightFlow، بدون مسیر تازه. اگر فهرست Unit نیاید، عملیات روی همه Unitها اجرا می‌شود:
    - لغو: `POST …/Cancellations {reasonCode, unitIds?}`
    - آزادسازی: `DELETE Service-Holds/{holdId}?unitIds=…`
- **پیشنهاد:** (ب)، چون الگوی هم‌خانواده است و چند Unit در یک فراخوانی انجام می‌شود.

Answer:

### Q4. موجودی Local در محیط مستقر قابل استفاده نیست

- **شاهد:**
  - `NotConnectedInventoryResourceReference` و `NotConnectedAirportFacilityReference` همیشه SourceUnavailable می‌دهند (`Providers/DependencyInjection.cs:52-53`).
  - در نتیجه `Activate` منبع Count، Weight و Slot و `AncillaryInventoryPolicy.Activate` برای Local با خطای 16608 رد می‌شوند.
  - dev: 0 Policy محلی و 0 منبع.
- **نتیجه:** تخصیص Local (اولویت P0 پک) فقط در تست SQL با fake اثبات‌پذیر است. ردیف R07 برای Count، Weight و Slot در HTTP زنده ممکن نیست.
- **سابقه:** در R8 برای Counting Family فرمودید «اگر چنین اشتراکی نداریم، نباید بی‌دلیل منطق جدید تولید کنیم».
- **گزینه‌ها:**
  - (الف) طبق پک ساخته شود و با SQL اثبات شود. HTTP زنده برای Local تا اتصال منبع GATED بماند.
  - (ب) اول منبع resource و facility وصل شود. در FlightFlow و AirInfo قراردادی برای آن نیست.
  - (ج) Local عقب بیفتد و فقط Unlimited، چرخه عمر و Seat ساخته شوند.
- **پیشنهاد:** (الف). پک آن را P0 گذاشته است، و بدون آن به محض اتصال منبع، Hold بدون کنترل ظرفیت می‌نویسد.

Answer:

### Q5. `AcceptedWeightKg`

- **شاهد:**
  - `AcceptedWeightKg` فقط با `PricingUnit.PerKilogram` فعال می‌شود (`AncillaryInventoryPolicy.cs:267-268`).
  - `/Price` محصول PerKilogram را رد می‌کند (`SellableCandidate.cs:32`).
  - پس هیچ Order‌ای وزن کل پذیرفته‌شده را نمی‌آورد.
- **پک:** 02 و 03 فیلد `acceptedWeightKgTotal` و ستون آن را می‌خواهند. پک 12 می‌گوید اگر اثبات‌پذیر نیست، همان محصول یا Policy را گیت کن.
- **گزینه‌ها:**
  - (الف) فیلد و ستون ساخته شوند، ولی هیچ درخواستی با آن‌ها موفق نشود.
  - (ب) Hold با Policy از نوع `AcceptedWeightKg` با خطای صریح رد شود و فیلد و ستون فعلاً ساخته نشوند.
- **پیشنهاد:** (ب).

Answer:

### Q6. رویداد دامنه

- **FlightFlow:** برای هر انتقال domain event می‌سازد و consumer مدل خواندنی را به‌روز می‌کند.
- **Ancillary امروز:**
  - هیچ رویدادی ندارد؛ `Causes(` در Domain پیدا نمی‌شود.
  - GET از CommandDb می‌خواند.
  - پک 06 projection و Outbox تازه را منع کرده است.
- **پیشنهاد:** رویداد ساخته نشود، چون هیچ مصرف‌کننده‌ای ندارد.

Answer:
