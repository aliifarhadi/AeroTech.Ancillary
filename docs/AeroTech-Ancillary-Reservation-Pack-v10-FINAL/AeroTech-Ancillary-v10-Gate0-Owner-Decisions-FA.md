# AeroTech.Ancillary — پاسخ نهایی به Gate 0 بسته Reservation v10

**نوع سند:** تصمیم معماری و دستور اصلاحی اجرایی برای Coding Agent — یک سند واحد  
**تاریخ:** ۲۰۲۶-۱۰-۲۶  
**وضعیت:** شش تعارض Gate 0 تعیین تکلیف شده‌اند؛ شروع Gate 1 فقط با رعایت این تصمیم‌ها مجاز است.  
**مخزن:** `aliifarhadi/AeroTech.Ancillary`  
**گزارش جدید بررسی‌شده:** `feat/ancillary-offers-v9@debd0657bd68b2574ba4376fb5a232ce2742f921`  
**مبنای کد پیش از گزارش:** `2025efd4fbdb4ddb9c72a1ac1eb5023c0b29a3c0`  
**منبع اصلی سؤال‌ها:** `reports/V10-GATE0-QUESTIONS-AND-CONFLICTS.md`  
**منبع ممیزی:** `reports/V10-GATE0-SOURCE-AUDIT.md`  
**Pack مرجع:** `docs/AeroTech-Ancillary-Reservation-Pack-v10-FINAL`.

> **قاعده مرجع واحد:** این سند «پاسخ مالک به Gate 0» است، نه یک طراحی مستقل دیگر. تصمیم‌های مشخص زیر اصلاحیه الزام‌آور بندهای متناظر Pack v10 هستند. ایجنت قبل از تغییر کد باید همین تصمیم‌ها را در Master و اسناد مرتبط همان Pack ادغام کند و از آن پس فقط **Pack v10 یکپارچه‌شده** را اجرا کند. اسناد قدیمی v12/v13، پیشنهادهای قدیمی Phase3 و جزئیات FlightFlow مرجع اجرایی دوم نیستند. در صورت تعارض، تصمیم صریح مالک بر تقلید از سرویس هم‌خانواده مقدم است.

---

## ۱. حکم کلی و ارزیابی گزارش ایجنت

گزارش Gate 0 در تشخیص شکاف‌ها صحیح و مفید است. برای تکمیل Reservation، نیازی به بازطراحی `AncillaryReservation`، ایجاد Aggregate جدید برای هر Ancillary، ساختن Saga، Queue، Outbox یا کپی دقیق FlightFlow نداریم.

**طراحی مصوب نهایی:**

1. یک `AncillaryReservation` موجود با `AncillaryReservationUnit`های مستقل باقی بماند.
2. برای Local محدود، فقط یک Entity/جدول کوچک **InventoryAllocation** زیر Unit لازم است؛ ظرفیت پایه Inventory تغییر نکند.
3. تمام `Create Hold`های چندخدمتی از نظر ثبت Local stock در **یک SQL Transaction** اتمیک باشند؛ برای هر Source ظرفیت مشترک با قفل مناسب حفاظت شود.
4. `Unlimited` واقعی Hold تجاری بدون Allocation محلی دارد. `Supplier` بدون Adapter واقعی **نباید Held جعلی** بگیرد.
5. SeatMap فقط خدمت **تجاری Seat Selection** را در همین Reservation ثبت می‌کند. رزرو/آزادسازی/اشغال واقعی شماره صندلی تنها با **Ordering و FlightFlow** است. Ancillary نه `OccupiedSeats` را می‌خواند، نه `Seat-Holds` را فراخوانی می‌کند.
6. Cancel/Release به‌صورت کلی و انتخابی بر اساس `unitIds[]` در **مسیرهای گروهی موجود** انجام شود؛ Endpoint مخصوص Entity فرزند نسازیم.
7. هیچ Event جدید، Counter دوم، Status اضافی، Provider abstraction بدون نیاز یا چارچوب Idempotency مستقل نسازیم.
8. هدف اصلی **Flow صحیح و قابل‌استفاده** است؛ قابلیت فاقد Provider واقعی باید واضحاً غیرفعال و در گزارش Gateها مشخص شود.

### خلاصه پاسخ‌ها

| سؤال | تصمیم قطعی | تغییر نسبت به Pack v10 |
|---|---|---|
| Q1 — Ledger یا Counter | **Ledger + وضعیت Unit + انقضای منطقی** | خیر؛ پیشنهاد ایجنت تأیید شد |
| Q2 — قفل Stock | **SQL `sp_getapplock` با `LockOwner=Transaction`** | خیر؛ پیشنهاد ایجنت با الزامات اتمیک تأیید شد |
| Q3 — Partial Operations | **همان Endpointهای گروهی، با `unitIds[]` اختیاری** | **بله؛ مسیرهای `/Units/{unitId}` حذف شوند** |
| Q4 — Local References قطع است | **پیاده‌سازی Core Local + تست SQL واقعی؛ Live Local = GATED** | اصلاح شواهد Closure؛ بدون ساخت Adapter ساختگی |
| Q5 — AcceptedWeightKg | **فعلاً Unsupported صریح؛ فیلد/ستون بی‌مصرف ساخته نشود** | **بله؛ حذف `acceptedWeightKgTotal` از v10** |
| Q6 — Domain Events | **هیچ Event/Outbox/Projection جدید** | خیر؛ پیشنهاد ایجنت تأیید شد |

---

## ۲. Q1 — Ledger انتخاب قطعی است؛ Counter متعلق به FlightFlow را کپی نکنید

**پاسخ:** پیشنهاد ایجنت تأیید می‌شود.

### مدل داده

همان `AncillaryReservationUnitAllocation` کوچک و Immutable تحت Unit:

| فیلد | نوع | قاعده |
|---|---|---|
| `Id` | `long` | کلید داخلی |
| `AncillaryReservationUnitId` | `long` | FK اجباری به Unit |
| `ResourceKind` | `InventoryResourceKind` | Enum موجود: `FlightCount=1`، `FlightWeight=2`، `AirportSlot=3` |
| `InventorySourceId` | `long` | **ID ردیف موجودی موجود**؛ نه `ResourceId` تجاری و نه DefinitionId |
| `Amount` | `decimal(19,3)` | مثبت؛ Count/Persons باید عدد صحیح باشند، Weight با Precision موجود validate شود |

**جدول یا فیلد اضافه ممنوع:** `HeldCount`, `ConfirmedCount`, `RemainingCount` قابل‌تغییر روی InventorySource، و `AllocationStatus`, `AllocationExpiresAt` روی هر Allocation. تخصیص از وضعیت Unit و تاریخ انقضای Reservation تعیین می‌شود.

قاعده دسترس‌پذیری:

```text
ActiveUsage(source, now) = SUM(Allocation.Amount)
  WHERE Unit.Status == Confirmed
     OR (Unit.Status == Held AND (Hold.ExpiresAt > now OR legacy ExpiresAt IS NULL))
Available = Source.Capacity - ActiveUsage
```

- Hold منقضی‌شده **قبل از اجرای Poller** دیگر در `ActiveUsage` محاسبه نمی‌شود؛ Confirm بعد از deadline ممنوع است.
- `Release` و `Cancel` با تغییر وضعیت Unit، تخصیص را از مصرف فعال خارج می‌کنند؛ **رکورد Allocation حذف یا دوباره «برگشت» داده نمی‌شود**.
- برای یک Unit با دو پرواز، منبع هر پرواز Allocation جدا دارد؛ Count+Weight نیز Allocationهای جدا و مشترک در یک تراکنش دارند.
- برای رکوردهای Legacy با تاریخ انقضای خالی در هر محیط، تا تعیین تکلیف محافظه‌کارانه عمل شود؛ هیچ داده‌ای به‌صورت خودکار صفرمصرف فرض نشود.
- `TotalCapacity`, `CapacityKg`, `CapacityPersons` در Hold کم یا زیاد نشوند. `AdjustTo` فقط تنظیم ظرفیت پایه است.

**علت انتخاب:** FlightFlow از Counterهای خود برای صندلی استفاده می‌کند؛ Ancillary از قبل سه منبع مستقل Count/Weight/Slot و یک چرخه Unit دارد. Counter کپی‌شده، دو منبع حقیقت و هماهنگی پیچیده با Expiry ایجاد می‌کند. Ledger یک جدول و یک Query مصرف فعال است؛ اگر روزی از نظر عملکرد مسئله واقعی ثبت شد، بعداً اندازه‌گیری و بهینه شود، نه الان.

### معیار پذیرش

`I06`، `I07`، `I10`، `I14`، `I15`، `I17`، `I21` تا `I25` و تست‌های Release/Cancel باید از **SQL واقعی** اثبات شوند. تست این‌که دو Order آخرین ظرفیت را همزمان می‌خواهند و فقط یکی موفق می‌شود، **غیرقابل حذف** است.

---

## ۳. Q2 — یک روش قفل، با Transaction واحد

**پاسخ:** `sp_getapplock` از SQL Server با `@LockOwner='Transaction'` تأیید می‌شود. RedLock چندکلیدی FlightFlow کپی نشود.

### قرارداد اجرای قفل

```text
Acquire existing Order lock (only for same Order's lifecycle)
BEGIN Ancillary CommandDb SQL transaction
  Resolve Active Policy and physical Source IDs
  Sort DISTINCT (ResourceKind, InventorySourceId)
  Acquire SQL Exclusive application locks in that exact order
  Re-read CURRENT capacity, source status and active usage AFTER locking
  Validate the SUM of all requested allocations per source
  Persist Hold + Units + Allocations in the SAME transaction
COMMIT
```

- از **همان Connection و Transaction** برای `sp_getapplock`، خواندن Consumption و `SaveChanges` استفاده شود.
- خواندن ActiveUsage باید پس از گرفتن قفل از **آخرین داده Commit‌شده** انجام شود؛ از Snapshot Transaction قدیمی که پیش از انتظار قفل شروع شده، برای تصمیم ظرفیت استفاده نکنید. روی SQL Server، `READ COMMITTED` با Query مناسب و تست هم‌زمانی کافی است؛ سطح Isolation یا Lock دوم بدون شواهد اضافه نکنید.
- قفل‌های منبع باید **نام استاندارد یکتا** داشته باشند، مثلاً `Ancillary.Inventory.{kind}.{sourceId}`؛ همه مسیرهای Hold و کاهش ظرفیتِ مدیریتی باید از همان کلید استفاده کنند.
- `@LockTimeout` مقدار محدود با تنظیم فعلی پروژه داشته باشد؛ کد بازگشتی `<0` از SQL بررسی و در خطا تراکنش Rollback شود. انتظار بی‌نهایت ممنوع است.
- قفل منبع فقط برای عملیاتی لازم است که بتواند **مصرف فعال را افزایش دهد یا ظرفیت/فروش‌پذیری منبع را در رقابت با Hold تغییر دهد**. برای Release/Cancel/Expire صرفاً به‌دلیل کم‌شدن مصرف، زیرساخت قفل منبع مضاعف نسازید؛ Order lock + Transaction فعلی و تست عدم Race کافی است.
- قفل `ancillary-hold:{orderId}` فعلی برای هماهنگی عملیات **همان Order** باقی می‌ماند، اما به‌تنهایی مانع Oversell بین Orderها نیست.
- `AirportSlotInventoryRepository.LockFacilityAsync` فعلی از قفل Session برای **ایجاد و جلوگیری از هم‌پوشانی تعریف بازه** استفاده می‌کند؛ این عملیات را با قفل Transaction مربوط به **مصرف یک Source موجود** اشتباه نگیرید. دو سیستم عمومی قفل نسازید.
- `AdjustFlightCountInventoryService`, `AdjustFlightWeightInventoryService`, `AdjustAirportSlotInventoryService` در صورت کاهش Capacity باید تحت **همان قفل منبع** و بعد از خواندن مصرف فعال، `newCapacity >= ActiveUsage` را تضمین کنند.
- Close/Suspend/Retire نباید یک Hold جدید را بعد از بسته‌شدن منبع به‌اشتباه تأیید کنند؛ زمان‌بندی رقابتی آن‌ها با Hold در همان SQL test بررسی شود. Allocation قدیمی با این عملیات حذف نمی‌شود.

**پیچیدگی ممنوع:** Generic Lock Framework، Distributed Transaction، Redis+SQL دوگانه برای Stock، RowVersion دوم برای Allocation، و Singleton Capacity Counter جدید.

**شاهد فنی خارجی:** Microsoft SQL Server تصریح می‌کند قفل Transaction-owned با Commit یا Rollback آزاد می‌شود و اگر `sp_getapplock` خطای Deadlock/Timeout بدهد، برنامه باید صریحاً Rollback کند. منبع: https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql

---

## ۴. Q3 — API جزئی روی عملیات گروهی باشد؛ دو Endpoint جدید Unit ممنوع

**پاسخ:** گزینه **(ب)** ایجنت تأیید می‌شود، اما با قرارداد دقیق زیر. **کپی‌کردن لفظی FlightFlow علت تصمیم نیست**؛ علت واقعی این است که Unit بخشی از Aggregate است و می‌توان چند Unit را با یک درخواست تغییر داد.

### مسیرهای نهایی (فقط Service-to-Service)

```http
POST   /Service/v1/Ancillaries/Service-Holds
GET    /Service/v1/Ancillaries/Service-Holds/{holdId}
POST   /Service/v1/Ancillaries/Service-Holds/{holdId}/Confirmations
DELETE /Service/v1/Ancillaries/Service-Holds/{holdId}
POST   /Service/v1/Ancillaries/Service-Holds/{holdId}/Cancellations
POST   /Service/v1/Ancillaries/SeatMaps/Holds
```

**Release کل Hold فعال:**

```http
DELETE /Service/v1/Ancillaries/Service-Holds/9001
```

**Release انتخابی (یک یا چند Unit) در همان مسیر:**

```http
DELETE /Service/v1/Ancillaries/Service-Holds/9001?unitIds=9102&unitIds=9104
```

`unitIds` از Query String به‌صورت `long[]?` خوانده شود، نه از JSON body در DELETE. نبودن پارامتر به معنی **همه Unitهای Held قابل‌آزادسازی** است؛ وجود آن به معنی **فقط همان Unitها**. `unitIds` تکراری، صفر یا خالیِ صریح، نامعتبر است؛ هیچ‌کدام به‌صورت ضمنی به Release کل تبدیل نشوند.

**Cancel کل Confirmedهای فعال:**

```http
POST /Service/v1/Ancillaries/Service-Holds/9001/Cancellations
Content-Type: application/json

{"reasonCode":"CUSTOMER_REQUEST"}
```

**Cancel انتخابی:**

```http
POST /Service/v1/Ancillaries/Service-Holds/9001/Cancellations
Content-Type: application/json

{"reasonCode":"CUSTOMER_REQUEST","unitIds":[9102,9104]}
```

| Field | Type | Rules |
|---|---|---|
| `reasonCode` | `string` | برای Cancel اجباری؛ همان حداکثر ۵۰ کاراکتر Domain موجود؛ دلیل قبلی overwrite نشود |
| `unitIds` | `long[]?` | اختیاری؛ missing/null = همه Unitهای واجد شرایط؛ present = غیرخالی، unique، positive و belonging to Hold |

**پاسخ هر دو عملیات** همان `AncillaryHoldResult` موجود است؛ DTO جدید نسازید.

### رفتار اتمیک و Idempotent

1. Aggregate Root یک بار Load شود؛ همه UnitIdهای انتخاب‌شده ابتدا از نظر تعلق، تکراری نبودن، وضعیت و عمل مجاز بررسی شوند؛ سپس **همه یا هیچ** تغییر کنند.
2. Release فقط `Held → Released`؛ Cancel فقط `Confirmed → Cancelled`. `Expired`، `Held` برای Cancel و `Confirmed` برای Release، انتقال مجاز ندارند.
3. Unitهایی که **از قبل Terminal** هستند با اجرای `Confirm/Release/Cancel` گروهی به وضعیت دیگری منتقل نشوند. `Confirm All` فقط Heldهای زنده را به Confirmed می‌برد و Unitهای Released را رها می‌کند.
4. Replay درخواست **انتخابی** روی همان Unitهای قبلاً Released یا Cancelled، بدون Write دوباره پاسخ بدهد؛ برای Cancel انتخابی، تکرار با `reasonCode` متفاوت روی همان Unit قبلاً Cancelled باید `409` بدهد.
5. در عملیات **کلی**، Unitهای قبلاً Released/Cancelled از هدف عملیات جدید کنار گذاشته می‌شوند؛ نباید Cancel یک Unit تاریخی مانع لغو بقیه Confirmedها به دلیل متفاوت شود. اگر هیچ Unit واجد شرایط نمانده، اجرای تکراری بدون تغییر پاسخ دهد. تاریخچه reasonهای قدیمی حفظ شود.
6. هیچ Unit یا Allocation فیزیکی حذف نمی‌شود؛ بعد از Partial Release، سایر Unitها و HoldId ثابت می‌مانند.
7. وضعیت Root مطابق Pack v10 **عملیاتی** است و فهرست Unitها حقیقت جزئی؛ `Mixed`, `PartiallyReleased`, `Pending` یا Enum دیگری نسازید.
8. مسیرهای `DELETE …/Units/{unitId}` و `POST …/Units/{unitId}/Cancellations` در Pack v10 **کنار گذاشته شوند** و تست‌ها برای همین مسیرهای گروهی به‌روزرسانی شوند. هیچ Route موازی/Legacy جدید برای این دو شکل نسازید؛ چون هنوز پیاده نشده‌اند.

**نکته مهم:** عملیات جزئی به معنای تغییر هم‌زمان چند Unit در یک Aggregate است، نه درخواست Reservation مستقل برای هر Unit. این مدل برای حذف یک Baggage از Hold چندخدمتی مناسب‌تر است.

---

## ۵. Q4 — Local را درست بسازید، اما اتصال Reference ساختگی نسازید

**پاسخ:** گزینه **(الف)** ایجنت پذیرفته می‌شود، با اصلاح صریح معیار پذیرش و وضعیت استقرار.

### آنچه اکنون سورس اثبات می‌کند

- در `Providers/DependencyInjection.cs`، دو Reference برای Inventory Resource و Airport Facility به `NotConnected...` متصل‌اند.
- بنا بر گزارش Gate 0 روی Dev: **۵۳ Policy فعال** (۳۳ Unlimited، ۲۰ Supplier)، **صفر Policy Local** و **صفر Source Count/Weight/Slot**.
- بنابراین Endpointهای Live در این محیط نمی‌توانند ایجاد و فعال‌سازی واقعی یک منبع Local را از ابتدا طی کنند.
- این مشکل مربوط به **اتصال ReferenceData/پیکربندی داده** است، نه دلیل حذف منطق تضمین ظرفیت از Reservation.

### قرارداد این فاز

1. Gate 2 باید **تمام الگوهای پشتیبانی‌شده Local** را در Domain/Persistence پیاده کند: `FlightCount`, `FlightWeight.FixedKgPerAcceptedUnit`, `FlightCountPlusWeight`, `AirportSlot`؛ به‌علاوه `Unlimited`.
2. اثبات محاسبه/عدم Oversell با **SQL Server واقعی و fixture با Reference تأییدشده در Test** باشد؛ Fake نباید در Production DI ثبت شود.
3. تا اتصال واقعی Source Referenceها، برای HTTP زنده Local وضعیت **GATED / NOT VERIFIED** ثبت شود، نه PASS و نه الزام بی‌نتیجه برای ساختن Adapter جدید به AirInfo/FlightFlow.
4. HTTP زنده Gate 4 برای `Unlimited`، Seat commercial و Lifecycle موجود **لازم** است؛ برای Local، SQL acceptance شامل concurrency (Count, Weight, Slot)، rollback، و Admin Adjust اجباری است.
5. اگر تست SQL Local موفق شود اما Host مستقر به دلیل Reference نتواند Policy/Source فعال کند، **Local Production-Ready یا E2E Live** اعلام نشود. این گپ در گزارش Closure روشن بماند.
6. هیچ محصول یا Policy فاقد منبع Local به‌شکل خودکار `Unlimited` تعبیر نشود. نبود Source، قطع Reference، Source غیرفعال یا ClosedForSale ⇒ Fail Closed.
7. هیچ اتصال API جدید به FlightFlow یا AirInfo را **حدس نزنید**. آن دو مالک فیزیکی Inventoryهای تعریف‌شده Ancillary نیستند مگر قرارداد واقعی موجود باشد.

### اصلاح مهم پیرامون فروش‌پذیری تا Hold

گزارش نشان می‌دهد ۲۰ Policy فعال با `InventoryAuthority.Supplier` وجود دارند. همه آن‌ها لزوماً در Price قابل‌خرید نیستند؛ باید **واقعاً تست شود** که کدام Candidate قابل‌انتخاب و Price می‌شود. اگر Candidateای در Shopping/Price پذیرفته شود ولی در Hold به‌علت نبود Adapter الزاماً رد شود، این یک گپ واقعی Flow است.

**حکم حداقلی برای چنین موردی:** پیش از فعال‌کردن Checkout، همان محصول از **صدور Selection قابل‌خرید و پذیرش Price** کنار گذاشته شود یا با وضعیت Unavailable موجود نمایش داده شود. این کار فقط با **یک Guard کوچک در مسیر فعلی عرضه/اعتبارسنجی** و تست مربوط انجام شود؛ Engine جدید، DTO جدید، Inventory Fake و Supplier Adapter جدید نسازید. این تنها **استثنای محدود و مستند** نسبت به Freeze مسیر Shopping/Price در Pack v10 است؛ فقط اگر تست اثبات کند موردی واقعاً از Price عبور می‌کند. در غیر این صورت هیچ تغییری در Shopping/Price مجاز نیست.

این مسئله را نباید با ساختن `PendingSupplierHold` یا بازکردن Supplier Framework حل کرد.

### اثر بر تست‌های موجود

- `R07` به دو مدرک جدا تقسیم شود: **HTTP واقعی Unlimited/Seat** و **SQL واقعی Local**؛ عبارت «Live HTTP برای هر Local Pattern در Dev فعلی» حذف گردد.
- تست‌های `I06` تا `I28` به‌ویژه `I21..I24` پابرجا هستند و SQL واقعی می‌خواهند.
- هیچ آزمون GATED به‌صورت PASS در مجموع Test Closure شمارش نشود.

---

## ۶. Q5 — از ساختن فیلد AcceptedWeight بدون مصرف‌کننده جلوگیری کنید

**پاسخ:** گزینه **(ب)** ایجنت تأیید شد.

### دلیل واقعی

در `AncillaryInventoryPolicy`، `FlightWeightConsumptionMode.AcceptedWeightKg` برای `PricingUnit.PerKilogram` تعریف شده است؛ ولی `SellableCandidate.Is` در Price محصول `PerKilogram` را در این فاز قبول نمی‌کند. پس از جریان تجاری تأییدشده **وزن کل پذیرفته‌شده‌ای برای Hold قابل اثبات نیست**. ایجاد DTO و ستون صرفاً برای دریافت مقدار دلخواه، به‌جای پیشرفت واقعی Flow، سطح خطا را بیشتر می‌کند.

### تصمیم اجرایی

- `inventoryFacts.acceptedWeightKgTotal`، `AcceptedWeightKgTotal` در `AncillaryHoldServiceInput`, `AncillaryReservationUnitArgs`, `AncillaryReservationUnit` و Migration **فعلاً ساخته نشوند**.
- در این فاز فقط مصرف وزن با **`FixedKgPerAcceptedUnit × Quantity`** مجاز است؛ مقدار از Policy فعال و Qty معتبر می‌آید.
- هر Policy با `ConsumptionMode=AcceptedWeightKg` در Hold فعلی **خطای Unsupported مشخص** می‌دهد؛ هیچ تبدیل تخمینی، `1kg` پیش‌فرض، یا پذیرش داده وزن از Client نداریم.
- `FlightWeightInventory` و `FlightCountPlusWeight` با fixed kg طبق Pack ساخته و تست می‌شوند؛ حذف این feature به معنی حذف رزرو وزنی **نیست**.
- اگر بعدها `PerKilogram` در Shopping/Price واقعاً پشتیبانی شد، کل Contract فروش و مقدار وزن Accepted بررسی و **در آن فاز** فیلد لازم افزوده می‌شود.

### اصلاح اسناد/تست‌ها

1. هر ذکر فیلد/ستون `acceptedWeightKgTotal` در `02`, `03`, `04`, `05`, `06`, `11`, `12` حذف یا با «Unsupported فعلی» جایگزین شود.
2. `I12` از «Hold AcceptedWeight با مقدار دقیق» به **«Policy AcceptedWeightKg به‌صورت صریح قبل از ایجاد Allocation رد شود»** تبدیل شود.
3. `I13` اثبات کند که فیلد ناشناخته AcceptedWeight در Payload قابل پذیرش نیست و **هیچ ستون/Allocation ساخته نمی‌شود**.
4. `L20` برای Idempotency به‌جای تغییر AcceptedWeight، از تغییر `timeWithOffset`، `guestCount`، `quantity` یا IDهای تجاری موجود استفاده کند.
5. `decimal(19,3)` Allocation عمومی بماند؛ Price/Unit metadata تازه برای وزن ساخته نشود.

---

## ۷. Q6 — Domain Event تازه لازم نیست

**پاسخ:** پیشنهاد ایجنت تأیید شد.

- در Reservation فعلی، عملیات GET از CommandDb می‌خواند و Consumer/Projection تازه‌ای لازم نیست.
- `AncillaryReservation` در این فاز `DomainEvent` جدید صادر نکند؛ Outbox، Event Consumer، QueryDb Projection، Message Broker یا Recovery Worker اضافه نشود.
- تمام تغییرات Hold/Confirm/Release/Expire/Cancel در همان CommandDb ذخیره و از همان مسیر Query فعلی خوانده می‌شوند.
- اگر بعدها مصرف‌کننده مشخصی نیاز واقعی به رویداد داشته باشد، با قرارداد همان مصرف‌کننده تصمیم گرفته می‌شود.

**بنچ‌مارک FlightFlow در اینجا فقط درس چرخه عمر و Idempotency است؛ Event-driven بودن آن الزام Ancillary نیست.**

---

## ۸. تأیید/اصلاح هشت نتیجه تکمیلی Source Audit

| مورد ممیزی | تصمیم ایجنت باید چنین باشد |
|---|---|
| ۱. پیدا کردن Source | Count: `(OwnerAirlineId, FlightId, ResourceId)`؛ Weight: `(OwnerAirlineId, FlightId, WeightResourceId)`؛ Slot: بازه معتبر Facility/Owner/Airport. Allocation همواره **Source row Id** ذخیره کند. تأیید. |
| ۲. Precision | Kg با دقت ۳ رقم اعشار و قواعد `InventoryRules.IsKg`؛ `Amount decimal(19,3)` برای Allocation. تأیید. |
| ۳. Slot | بازه `[StartUtc, EndUtc)`، پوشش پیوسته، Sourceهای Active و Open؛ چند Slice در یک Transaction. تأیید. |
| ۴. وزن پذیرفته‌شده | مطابق Q5 فعلاً GATED؛ ساخت فیلد یا تست موفقیت ساختگی ممنوع. |
| ۵. Ordering | وجود Provider مصرف‌کننده Ancillary در Ordering فعلاً اثبات نشده؛ در این فاز Ordering/FlightFlow تغییر نکنند. E01–E03 فقط Contract Tests و **NOT VERIFIED E2E**. |
| ۶. Admin Adjust / تست‌های Freeze | `AdjustTo` باید زیر قفل Stock و با مصرف فعال سازگار باشد؛ Pinهای قدیمی تنها به اندازه تغییر مجاز Reservation/Inventory update شوند. تست‌های قدیمی حذف/Bypass نشوند. |
| ۷. کد خطا | الگوی `ExceptionFactory` موجود حفظ شود. فقط وقتی واقعاً معادل صحیح نیست، خطای کوچک جدید تعریف شود: `16417` Stock shortage (409)، `16418` Unit خارج از Hold (404)، و در صورت نیاز `16419` Inventory Not Reservable (409). همان مفهوم را با کدهای متنوع دوباره نسازید. `16607/16608` برای Unsupported/Unresolved موجود ترجیح دارند. |
| ۸. Service Auth | سیاست فعلی Service-to-Service و کنترلرهای موجود حفظ شود؛ Endpoint Frontend عمومی جدید نسازید و پروژه را به‌خاطر مدل احراز هویت FlightFlow تغییر ندهید. الزامات شبکه/محیط فعلی مستقل از این Gate است. |

### جزئیات Legacy و Migration — بدون ساختن سیستم Reconciliation جدید

گزارش Dev می‌گوید Local Policy و Local Source صفر است و هشت Hold موجود هم Seat هستند. بنابراین برای Dev موجود **هیچ Backfill واقعی Local لازم نیست**. لزومی ندارد یک Background Reconciliation Engine برای فرض تاریخی مبهم ساخته شود.

اما این نتیجه را به محیط‌های دیگر تعمیم ندهید. قبل از اجرای Migration روی هر محیط، یک Query/گزارش Deploy بررسی کند آیا Local Hold/Confirmed قدیمی بدون Allocation وجود دارد یا خیر؛ اگر دارد، **همان محیط تا تعیین تکلیف تأییدشده برای منابع متأثر Fail Closed** بماند. اطلاعات تاریخی حذف نشود و Allocation ساختگی نسازید. این Gate اجرایی حداقلی، جایگزین سیستم پیچیده Runtime Reconciliation است.

### مرز SeatMap — بدون تغییر

- `POST /Service/v1/Ancillaries/SeatMaps/Holds` همان Facade موجود بماند؛ در این فاز فقط `holdId?` برای Append به Root موجود اضافه شود.
- Seat verifier تجاری فعلی (Definition/Provision/Pricing/قواعد مسافر و Flight) حفظ شود؛ **کنترل OccupiedSeat انجام نشود**.
- Seat Unit با همان `Quantity=1`, `CoverageScope=Sector`, `SeatMapSelectionId`, `SeatNumber`, `PricingId` ذخیره شود؛ `Allocations=[]`.
- در Mixed Hold، `holdId`, `orderId`, `reference`, `expiresAt` و Idempotency append بررسی شود و Parent جدید نسازیم.
- هیچ Hold فیزیکی یا Compensation در FlightFlow توسط Ancillary انجام نشود. Ordering مالک آن است.

---

## ۹. تغییرات دقیق نسبت به اسناد v10 (برای حذف مرجع‌های متناقض)

| فایل Pack v10 | اصلاح لازم |
|---|---|
| `00-START-HERE.md` | تصمیم‌های همین سند در ابتدا به‌عنوان Owner Gate0 Decision ثبت شوند؛ ترتیب authority صریح، فقط یک Master. |
| `01-MASTER-RESERVATION-CONTRACT-FA.md` | در §7 و §8 مسیر `/Units/{unitId}` با Release/Cancel گروهی و `unitIds[]` جایگزین؛ Q5 Unsupported و قید Local Gated افزوده شود. |
| `02-EXACT-ENDPOINTS-AND-DTOS.md` | `unitIds` اختیاری روی DELETE Query و Cancellation Body؛ مثال‌های کامل؛ حذف `acceptedWeightKgTotal`؛ no extra routes. |
| `03-DOMAIN-AND-LIFECYCLE.md` | حذف `AcceptedWeightKgTotal`، حفظ `AppointmentTimeWithOffset`, `GuestCount`, `AppendIdempotencyKey`, Allocation child؛ Domain methods برای گروهی/جزئی با فهرست UnitId. |
| `04-INVENTORY-ALLOCATION-ALGORITHM.md` | حذف مسیر Hold موفق برای `AcceptedWeightKg`، حفظ FixedKg؛ قفل SQL Transaction، ActiveUsage و منبع مشترک بدون Counter. |
| `05-SEATMAP-AND-ORDERING-HANDOFF.md` | هیچ تغییر مالکیت؛ نمونه Partial Cancel/Release به `unitIds[]` جدید تبدیل شود؛ وزن Accepted حذف شود. |
| `06-FAILURE-RETRY-EXPIRY-AND-MIGRATION.md` | انقضا/Idempotency گروهی؛ حذف accepted weight؛ شرط pre-deploy legacy audit بدون سیستم جدید. |
| `07-ACCEPTANCE-MATRIX.md` | `I12/I13/L20/R07` اصلاح؛ L04/L05/L12، E08 و سناریوهای Release/Cancel برای Query/Body `unitIds[]`؛ IDs موجود حذف نشوند. |
| `08/09` Benchmarks/Traceability | انتخاب Ledger به‌جای کپی Counter؛ قفل SQL؛ Source gating؛ اصلاح Route Register و شواهد. |
| `10-AGENT-GATE0-AUDIT.md` | Gate0 طبق گزارش موجود خاتمه‌یافته؛ پاسخ شش سؤال همین سند است؛ re-audit بی‌دلیل لازم نیست. |
| `11-AGENT-GATE1-DOMAIN-CONTRACTS.md` | Gate1 فقط DTO/Domain/Schema لازم؛ بدون AcceptedWeight؛ Partial Group Commands، بدون per-child route. |
| `12-AGENT-GATE2-LOCAL-INVENTORY.md` | فقط FixedKg؛ Local SQL tests mandatory، Live Local gated تا Reference واقعی. |
| `13-AGENT-GATE3-SEATMAP-AND-LIFECYCLE.md` | گروهی `unitIds[]` و Seat append در Root موجود؛ no FlightFlow call. |
| `14-AGENT-GATE4-E2E-CLOSURE.md` | HTTP Unlimited+Seat؛ Local SQL واقعی؛ Local HTTP GATED؛ Ordering E2E NOT VERIFIED؛ هیچ GATED به‌عنوان PASS. |

**معیار جلوگیری از دو مرجع:** ایجنت باید در انتهای به‌روزرسانی Pack، `grep`/یک کنترل متنی برای موارد `Units/{unitId}`, `acceptedWeightKgTotal`, `FlightFlow physical seat allocation`, و `Local HTTP PASS` اجرا کند و تمام ارجاعات منسوخ را در اسناد اجرایی برطرف کند. این سند نباید به یک Pack دوم با دستورالعمل‌های متضاد تبدیل شود.

---

## ۱۰. پرامپت اجرایی نهایی به Coding Agent — دقیق و محدود

**ROLE:** شما فقط پیاده‌ساز کد هستید؛ تغییر تصمیم Domain، ساخت Contract جدید، تغییر Repository دیگر یا افزودن قابلیت خارج Scope بدون دستور مالک ممنوع است.

**AUTHORITIES:**
1. تصمیم Owner در این سند برای Q1–Q6 و اصلاحات مشخص؛
2. Pack v10 پس از ادغام این تصمیم‌ها در Master و فایل‌های اجرایی همان Pack؛
3. سورس Ancillary و تست‌های واقعی در `feat/ancillary-offers-v9`؛
4. FlightFlow، Travelport و Duffel فقط **بنچ‌مارک رفتار** هستند، نه الگوی کپی کد.

**ابتدا** اسناد v10 را طبق جدول §9 یکپارچه کن؛ فایل گزارش `reports/V10-GATE0-OWNER-DECISIONS-APPLIED.md` حاوی Diff و تأیید حذف تضاد بساز. **این کار بازکردن Gate0 نیست؛ ادغام تصمیم مالک است.** سپس:

### Gate 1 — Domain/DTO/Migration

- Root و Unit موجود را تکمیل کن؛ `InventoryAllocation` کوچک با ۵ فیلد بساز.
- Partial Release/Cancel را به‌صورت **فهرست UnitId روی Endpoint گروهی** پیاده کن؛ از ساخت Route فرزند خودداری کن.
- `RequestedExpiresAt` برای Hold جدید الزامی؛ `AppendIdempotencyKey` و `AppointmentTimeWithOffset/GuestCount` فقط وقتی مطابق نمونه تجاری نیاز دارند.
- AcceptedWeight field/column نساختن؛ no new status, event, provider adapter, query projection.
- Idempotency Create/Append/Partial را با تست دقیق ثابت کن.
- Migration فقط CommandDb و additive؛ Test DB پاک‌سازی مجاز تستی فقط در محیط Test، نه Dev کاربر.

### Gate 2 — Local Inventory

- `Unlimited` و Count، FixedWeight، Count+FixedWeight و AirportSlot را با Sourceهای موجود پیاده کن.
- یک SQL transaction و یک مکانیسم قفل منبع با `sp_getapplock Transaction`؛ Lock ordering ثابت و بررسی Return Code.
- Unit+Allocationها یا همگی Commit شوند یا هیچ‌کدام.
- `AdjustTo` و تغییرات status منبع، با Hold ایمن شوند؛ بدون Counter جدید.
- SQL concurrency واقعی I21–I24، Rollback، multi-flight/slot، legacy gate و expiry logical الزامی.
- Local HTTP Live را بدون اتصال مرجع، GATED ثبت کن؛ منبع External اختراع نکن.

### Gate 3 — SeatMap/Partial Lifecycle

- Endpoint اختصاصی SeatMaps/Holds همان Root را ایجاد کند یا به `holdId` موجود Append کند.
- صفر تماس FlightFlow برای Seat physical؛ Side Effect فقط Commercial Hold در Ancillary.
- Confirm/Release/Cancel فقط Unitهای واجد شرایط را منتقل کنند و سایر Unitها دست‌نخورده بمانند.
- Replay key/Reason و Expire سابق بدون تغییر موجودی مضاعف باشند.

### Gate 4 — E2E/Closure

- HTTP واقعی برای Unlimited+Seat (Hold/Read/Partial Release/Confirm/Partial Cancel/Expire)؛ SQL واقعی برای Local و مسابقه Last Unit.
- اگر موردی واقعاً از Price قابل خرید است ولی به‌علت Supplier بدون Adapter در Hold هیچ‌گاه موفق نمی‌شود، طبق §5 با Guard کوچک در همان مسیر عرضه/Price ببند؛ بدون Engine جدید؛ تست منفی و گزارش Diff.
- همه تست‌های موجود M1/Seat/V9 و test count=discovered count سبز؛ هیچ skip/abort بدون گزارش.
- قرارداد E01–E03 برای Ordering را NOT VERIFIED E2E اعلام کن تا اتصال واقعی Ordering در فاز مستقل انجام شود.
- برای **هر Gate** گزارش Source Diff، Migration SQL، دستور واقعی Build/Test، تعداد Tests discovered/passed و محدودیت‌های واقعی بده.

### ممنوعیت‌ها

- **No commit / push / merge** بدون دستور جدید مالک.
- **No** copy exact FlightFlow API، Counter، RedLock framework، domain events یا Quartz.
- **No** new Aggregate per Variant، Saga، Supplier fake، Inventory cache/counter مستقل، ReservationPreparation، Queue/Outbox.
- **No** changes in Ordering/FlightFlow/Physical SeatMap inventory؛ **No** repricing in Hold.
- **No** GATED feature reported as PASS.
- **No** deleting existing data or weakening tests to make them green.

**تعریف پایان:** Gate0 سؤال باز از شش مورد ندارد. پیاده‌سازی وقتی موفق است که Hold واقعی برای Local تحت SQL تضمین‌شده، Seat تجاری و Unlimited قابل اجرا، عملیات Partial درست، و محدودیت Supplier/Reference بدون ادعای غیرواقعی گزارش شده باشد؛ نه اینکه فقط همه Methodها وجود داشته باشند.

---

## ۱۱. بنچ‌مارک و حدود اعتبار آن

### Travelport — الگوی تجاری، نه معماری داخلی

در مستند رسمی **Ancillary and EMD Guide**، خدمات بر حسب Passenger/Segment و Airline متفاوت‌اند؛ Shopping/Booking/Payment/EMD مراحل جداگانه‌اند و Cancel Ancillary بسته به GDS/NDC و نوع خدمت محدودیت متفاوت دارد. این شواهد، **مرجع جزء و امکان لغو انتخابی با وضعیت درست** را توجیه می‌کند؛ اما نمی‌گوید Ancillary باید Counter، جدول Ledger یا URLهای FlightFlow را تقلید کند.

- https://support.travelport.com/webhelp/JSONAPIs/Airv11/Content/Air11/Ancillaries/AncillaryGuide.htm
- https://developer.travelport.com/docs/flights/guides/seats-and-ancillaries/ancillaries-and-emd-guide

### Duffel — Selected Serviceها مستقل‌اند؛ API جهانی واحد وجود ندارد

Duffel در `add_services[]` هر خدمت را با شناسه و Quantity مستقل می‌پذیرد؛ در عین حال مستندات آن اضافه‌کردن خدمات به Order نوع `hold` را پشتیبانی‌نشده اعلام می‌کنند. بنابراین **حد و مرز هر API به سامانه بستگی دارد** و نباید مسیر مورد نیاز AeroTech را از API Duffel عیناً استنتاج کنیم.

- https://duffel.com/docs/api/orders
- https://duffel.com/docs/guides/adding-post-booking-bags

### FlightFlow — فقط چرخه عمر و تضمین Capacity

FlightFlow در `Seat-Holds` مرجع گروه و جزء، Idempotency، Confirm/Release/Cancel و ظرفیت هم‌زمان را دارد. **این اصول** مفیدند؛ ولی Counterهای `FlightCapacity`، RedLock، Domain Event و Quartz محصول طراحی داخلی FlightFlow هستند و برای Ancillary الزام نیستند.

- https://github.com/aliifarhadi/Aerotech.FlightFlow/blob/962f41880df0f10a1d105559c233af63ef9db968/AeroTech.FlightFlow/FlightFlow/src/AeroTech.FlightFlow.RestApi/V1/FlightAggregate/Controllers/ServiceController.cs

### SQL Server — مبنای روش Atomic Stock

- `sp_getapplock`: https://learn.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-getapplock-transact-sql
- Locking/Row Versioning: https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-transaction-locking-and-row-versioning-guide

**تمایز شواهد:** تمام راه‌حل‌های Ledger، Transaction-owned resource lock، بدون Domain Event و انتخاب `unitIds[]` در این سند، **تصمیم‌های طراحی AeroTech** هستند. هیچ‌کدام را به‌عنوان استاندارد اجباری Amadeus/Sabre/IATA/Travelport/Duffel معرفی نکنید.

---

## ۱۲. مراجع دقیق کد و گزارش برای ایجنت

- Gate0 questions: https://github.com/aliifarhadi/AeroTech.Ancillary/blob/debd0657bd68b2574ba4376fb5a232ce2742f921/reports/V10-GATE0-QUESTIONS-AND-CONFLICTS.md
- Gate0 audit: https://github.com/aliifarhadi/AeroTech.Ancillary/blob/debd0657bd68b2574ba4376fb5a232ce2742f921/reports/V10-GATE0-SOURCE-AUDIT.md
- Existing Reservation Root: https://github.com/aliifarhadi/AeroTech.Ancillary/blob/2025efd4fbdb4ddb9c72a1ac1eb5023c0b29a3c0/src/AeroTech.Ancillary.Domain/AncillaryReservationAggregate/AncillaryReservation.cs
- Existing Hold Service: https://github.com/aliifarhadi/AeroTech.Ancillary/blob/2025efd4fbdb4ddb9c72a1ac1eb5023c0b29a3c0/src/AeroTech.Ancillary.Application/AncillaryReservationAggregate/Commands/HoldAncillaryServices/HoldAncillaryServicesService.cs
- Existing Inventory Policy: https://github.com/aliifarhadi/AeroTech.Ancillary/blob/2025efd4fbdb4ddb9c72a1ac1eb5023c0b29a3c0/src/AeroTech.Ancillary.Domain/AncillaryInventoryPolicyAggregate/AncillaryInventoryPolicy.cs
- Existing Providers DI: https://github.com/aliifarhadi/AeroTech.Ancillary/blob/2025efd4fbdb4ddb9c72a1ac1eb5023c0b29a3c0/src/AeroTech.Ancillary.Providers/DependencyInjection.cs
- Current Price Sellability: https://github.com/aliifarhadi/AeroTech.Ancillary/blob/debd0657bd68b2574ba4376fb5a232ce2742f921/src/AeroTech.Ancillary.Shopping/Selections/SellableCandidate.cs
- Project guardrails: https://github.com/aliifarhadi/AeroTech.Ancillary/blob/debd0657bd68b2574ba4376fb5a232ce2742f921/CLAUDE.md

**پایان تصمیم:** Gate 0 بسته است. خروجی این مرحله فقط یک تصمیم‌نامه برای پیاده‌ساز است؛ هیچ تغییر کدی انجام نشده است.
