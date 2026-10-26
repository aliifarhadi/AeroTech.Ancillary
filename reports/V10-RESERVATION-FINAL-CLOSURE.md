# Reservation v10 — Gate 4: بستن نهایی

**تاریخ:** 2026-10-26 · **شاخه:** `feat/ancillary-offers-v9` · **HEAD:** `824d88b` · **مبنا:** `2025efd4fbdb4ddb9c72a1ac1eb5023c0b29a3c0`
**وضعیت کد:** Gate 1 تا 4 همه commit نشده‌اند. هیچ deploy یا merge انجام نشده است.

## 1. زنجیره‌ی مرجع
1. `CLAUDE.md` ریشه (قواعد Owner).
2. `docs/AeroTech-Ancillary-Reservation-Pack-v10-FINAL/AeroTech-Ancillary-v10-Gate0-Owner-Decisions-FA.md` (commit `824d88b` «Answers»).
3. Pack v10 ادغام‌شده، فایل‌های `00` تا `14`؛ `01` Master است. `VERIFY-PACK.py` در همین نوبت: `PASS: 16 required files, 108 unique numbered acceptance tests`.

هیچ مرجع v12 یا v13 به‌کار نرفت. Commitهای بعد از مبنا: `debd065` (Pack و گزارش‌های Gate 0) و `824d88b` (تصمیم‌های Owner). کد Gate 1 تا 4 در درخت کاری است.

| Gate | گزارش | وضعیت |
|---|---|---|
| 0 | `V10-GATE0-SOURCE-AUDIT.md`، `V10-GATE0-QUESTIONS-AND-CONFLICTS.md`، `V10-GATE0-OWNER-DECISIONS-APPLIED.md` | پاسخ Owner اعمال شد |
| 1 | `V10-GATE1-DOMAIN-AND-SCHEMA.md` | commit نشده |
| 2 | `V10-GATE2-LOCAL-INVENTORY.md` | commit نشده |
| 3 | `V10-GATE3-SEAT-AND-LIFECYCLE.md` | commit نشده |
| 4 | همین گزارش | commit نشده |

**اندازه‌ی تغییر نسبت به مبنا:**
- فایل‌های tracked: 103 فایل، +3062 / −296 خط. از این میان `src` 65 فایل (+915 / −183)، `tests` 17 فایل (+191 / −113) و `docs/reports` 21 فایل.
- فایل‌های جدید untracked در `src` و `tests`: 22 فایل، 6354 خط (Migration و Designer آن هم در همین شمار است).

## 2. نتیجه‌ی اجراها (همه در همین نوبت)

| کنترل | نتیجه |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 خطا، 61 هشدار |
| همان Build روی مبنای `2025efd` (worktree موقت `E:\v10b`، بعداً حذف شد) | 0 خطا، 61 هشدار ⇒ **هیچ هشدار تازه‌ای نیست** |
| Domain conformance | 287 از 287 (discovered 287) |
| Shopping (شامل V9) | 125 موفق + 1 skip از 126 (discovered 126). Skip همان مورد قدیمی V9 است (`TwoProductsOfOneCountingFamily…`) |
| Application acceptance (SQL واقعی) | 254 از 254 (discovered 254)، بدون abort، 4.04 دقیقه |
| تست حذف‌شده یا غیرفعال | هیچ. 0 خط `[Fact]` یا `[Theory]` حذف شده؛ تعداد `Skip` در مبنا 1 و الان 1. تنها نام حذف‌شده تغییر نام `P2_X01` است (بند 9) |
| `ef migrations has-pending-model-changes` | No changes |
| رقابت‌های I21–I23، I24 و Close/Suspend/Retire، دو اجرای مستقل | هر بار 3 از 3. هر اجرا پایگاه تازه، Order تازه و اتصال جدا دارد |
| HTTP زنده (Host جانبی 5299، پایگاه dev، اجرای 2) | 24 از 24 گام، 18 از 18 کنترل |
| فراخوانی‌های خروجی Host در هر دو اجرای زنده | 4 درخواست، همه `GET AirInfo …/Aircrafts/1/DisplaySeatMap`. **FlightFlow: صفر** |
| BackgroundService انقضا | فقط یکی (`AncillaryHoldExpiryPoller`) و یک‌بار ثبت شده است. Health check فقط آمادگی دو DbContext را می‌سنجد |

## 3. Migration افزودنی (`20261026114827_V10ReservationAllocations`، خروجی `ef migrations script`)
```sql
ALTER TABLE [Ancillary].[AncillaryReservationUnits] ADD [AppendIdempotencyKey] nvarchar(128) NULL;
ALTER TABLE [Ancillary].[AncillaryReservationUnits] ADD [AppointmentTimeWithOffset] datetimeoffset NULL;
ALTER TABLE [Ancillary].[AncillaryReservationUnits] ADD [GuestCount] int NULL;
CREATE TABLE [Ancillary].[AncillaryReservationUnitAllocations] (
    [Id] bigint NOT NULL,
    [AncillaryReservationUnitId] bigint NOT NULL,
    [ResourceKind] int NOT NULL,
    [InventorySourceId] bigint NOT NULL,
    [Amount] decimal(19,3) NOT NULL,
    [LastUpdateTime] datetimeoffset NOT NULL,
    [LastUpdatedBy] bigint NULL,
    CONSTRAINT [PK_AncillaryReservationUnitAllocations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AncillaryReservationUnitAllocations_AncillaryReservationUnits_AncillaryReservationUnitId] FOREIGN KEY ([AncillaryReservationUnitId]) REFERENCES [Ancillary].[AncillaryReservationUnits] ([Id]) ON DELETE NO ACTION
);
CREATE UNIQUE INDEX [IX_AncillaryReservationUnitAllocations_AncillaryReservationUnitId_ResourceKind_InventorySourceId] ON [Ancillary].[AncillaryReservationUnitAllocations] ([AncillaryReservationUnitId], [ResourceKind], [InventorySourceId]);
CREATE INDEX [IX_AncillaryReservationUnitAllocations_ResourceKind_InventorySourceId] ON [Ancillary].[AncillaryReservationUnitAllocations] ([ResourceKind], [InventorySourceId]);
```
- فقط افزودنی است: سه ستون nullable، یک جدول، دو Index. هیچ ستونی حذف یا تغییر نمی‌کند.
- دقت `decimal(19,3)` با `InventoryRules.IsKg` یکی است و Pin `V121_M05` آن را تثبیت کرده است.
- dev این Migration را دارد (آخرین Migration: `20261026114827_V10ReservationAllocations`).

## 4. قرارداد، قبل و بعد (فقط Routeهای موجود؛ هیچ Route تازه‌ای نیست)

| Route | قبل (`2025efd`) | بعد |
|---|---|---|
| `POST Service-Holds` | `requestedExpiresAt?` اختیاری؛ هر service فیلدهای `seatMapSelectionId?`، `seatNumber?`، `pricingId?` داشت؛ JSON آزاد | `requestedExpiresAt` اجباری و آینده؛ فیلدهای Seat حذف شدند؛ `inventoryFacts? {timeWithOffset, guestCount}` فقط برای Slot؛ عضو ناشناخته = 400 |
| `DELETE Service-Holds/{holdId}` | بدون پارامتر (آزادسازی کل) | `?unitIds=..&unitIds=..` اختیاری؛ همه یا هیچ |
| `POST Service-Holds/{holdId}/Cancellations` | `{reasonCode}` | `{reasonCode, unitIds?}`؛ strict |
| `POST Service-Holds/{holdId}/Confirmations`، `GET Service-Holds/{holdId}` | — | بدون تغییر شکل؛ Unitهای Released یا Cancelled دست نمی‌خورند |
| `POST SeatMaps/Holds` | Header فعلی | فقط `holdId?` اضافه شد (append به همان Hold) |
| پاسخ (`AncillaryHoldResult`) | — | **بدون تغییر** |
| `SeatMaps/Resolve`، Offers، Price، Resolve | — | بدون تغییر، به‌جز Guard بند 7 |

کدهای خطای تازه:

| کد | HTTP | معنا |
|---|---|---|
| 16417 | 409 | کمبود ظرفیت |
| 16418 | 404 | Unit متعلق به این Hold نیست |
| 16419 | 409 | Hold شدنی نیست |
| 16621 | 409 | ظرفیت کمتر از مصرف فعال |
| 16622 | 409 | منبع مشغول است |

## 5. موجودی Local روی SQL (Count، Weight، Slot)
- **دو سفارش رقیب** (I21–I23، دو اجرای مستقل): در هر رقابت 4 سفارش هم‌زمان آخرین واحد را می‌خواهند. هر بار دقیقاً 1 موفق و 3 با 16417 رد می‌شوند. مصرف پایانی برابر ظرفیت است و بیشتر نمی‌شود: Count `1.000`، Weight `12.500`، Slot `1.000`.
- **Adjust هم‌زمان با Hold** (I24): فقط یکی از دو نتیجه‌ی سازگار رخ می‌دهد: (`3|2`، مصرف `3.000`، Adjust → 16621) یا (`2|3`، مصرف `2.000`، Hold → 16417).
- **Hold ترکیبی** (2 Bag با منبع Count ظرفیت 5، 1 Meal Unlimited، 1 Seat؛ Gate 3 E08):

  | مرحله | مصرف منبع Bag |
  |---|---|
  | بعد از Hold | 2.000 |
  | بعد از append صندلی | 2.000 (صندلی Allocation ندارد) |
  | بعد از Release وعده‌ی غذا | 2.000 |
  | بعد از Confirm | 2.000 |
  | بعد از Cancel یکی از Bagها | 1.000 |

  ظرفیت در همه‌ی مراحل `5|2` ماند.
- **Count+Weight اتمیک** (I14/I15)، **Portion بدون منبع یک پرواز** (I16)، **شکست Save** (I26): هیچ ردیف ناقصی باقی نمی‌ماند.

## 6. جدول وضعیت Unitها در Hold ترکیبی زنده (اجرای 2، Hold `1564289997664681984`)
Unitها به ترتیب: Bag کابین، Bag اضافه‌وزن، صندلی 14A، صندلی 14C.

| گام | HTTP | Hold | Unit 1 | Unit 2 | Unit 3 | Unit 4 |
|---|---|---|---|---|---|---|
| ساخت دو Bag | 201 | Held | Held | Held | — | — |
| Retry همان ساخت | 201 | همان HoldId و UnitIdها | | | | |
| همان کلید با تعداد دیگر | 409 / 16403 | بدون تغییر | | | | |
| append دو صندلی با `holdId` | 201 | Held | Held | Held | Held | Held |
| Retry همان append | 201 | همان UnitIdها | | | | |
| `DELETE ?unitIds=` فقط Unit 2 | 200 | Held | Held | **Released** | Held | Held |
| Confirm | 200 | Confirmed | Confirmed | Released | Confirmed | Confirmed |
| Confirm دوباره | 200 | بدون تغییر | | | | |
| Release یک Unit تأییدشده | 409 / 16411 | بدون تغییر | | | | |
| Cancel انتخابی Unit 3 | 200 | Confirmed | Confirmed | Released | **Cancelled** | Confirmed |
| همان Cancel با دلیل دیگر | 409 / 16413 | دلیل اول حفظ شد | | | | |
| Cancel همه | 200 | Cancelled | Cancelled | Released | Cancelled | Cancelled |

دلیل‌ها در SQL: `SCHEDULE_CHANGE, -, CUSTOMER_REQUEST, SCHEDULE_CHANGE`.

سایر گام‌های زنده:
- Hold فقط-Seat: 201 → Release کل 200 → Release دوباره 200 (no-op).
- `SeatMaps/Resolve`: 200.
- بدون `requestedExpiresAt` → 400 (Validator).
- عضو `acceptedWeightKgTotal` → 400 (JSON strict).
- محصول با Supplier خارجی → 422 / 16104.
- Hold با مهلت 70 ثانیه:
  - بعد از مهلت، GET وضعیت Expired را برمی‌گرداند؛ این وضعیت از ساعت محاسبه می‌شود، نه از ردیف ذخیره‌شده.
  - Confirm → 409 / 16407.
  - poller وضعیت را برای هر سفارش جدا و زیر قفل در SQL ذخیره کرد (`Status 4`؛ لاگ: `Ancillary hold expiry marked 1 hold(s) of order 9940012 as expired.`).
- **درخواست نامعتبر چیزی نمی‌نویسد:** 0 ردیف برای سفارش‌های 9940023 تا 9940025 (و 9940013 تا 9940015 در اجرای 1).

## 7. Guard فروش Supplier (بند 6a فایل 14، استثنای مستند Freeze)
- **شاهد واقعی شکاف:**
  - در اجرای زنده‌ی R8 روی dev، چهار محصول با Policy فعال Supplier قیمت گرفتند و Resolve شدند: `UPG_ECONOMY_TO_PREMIUM`، `MEAL_BEEF_FILLET`، `LOUNGE_IKA`، `FAST_TRACK_IKA` (scratch `v9/live-r8-otapanel/09-resolve.json`؛ `Authority = 3` در dev همین نوبت تأیید شد).
  - Hold همین محصولات همیشه رد می‌شود: 16104 برای Supplier خارجی و 16419 برای Policy از نوع Supplier.
  - دو تست جدید روی کد قبل از Guard **شکست خوردند**: محصول `Selectable` بود، `OfferItemId` امضاشده داشت و Price آن را پذیرفت.
- **Guard (یک شرط):** `SellableCandidate.Is` حالا `candidate.Availability.InventoryAuthority != InventoryAuthority.Supplier` را هم می‌سنجد. همین تابع هم امضای Selection قابل‌خرید و هم پذیرش Price را کنترل می‌کند. محصول Supplier مثل محصولات SubjectToConfirmation و PerKilogram از فهرست قابل‌خرید کنار می‌رود. Engine، DTO یا Adapter تازه‌ای ساخته نشد.
- **تست‌ها:** `V10SupplierSaleGuardTests`، دو تست:
  1. محصول Supplier فهرست نمی‌شود، ولی محصول Unlimited کنار آن فهرست می‌شود.
  2. Selectionی که پیش از رفتن موجودی به Supplier امضا شده، در Price با 16820 رد می‌شود.
  بقیه‌ی 123 تست Shopping بدون تغییر سبزند.
- Price زنده بعد از Guard اجرا نشد، چون Token سطوح فروش در دسترس نبود. اثبات این بخش تست واحد است.

## 8. SeatMap فقط تجاری
- Verifier صندلی فقط Pricing Engine، AirInfo و Reference Data را می‌خواند و هیچ وابستگی به `IOccupiedSeatProvider` یا FlightFlow ندارد. تست `V10_S10_S11_…` گراف سازنده‌ها را پیمایش می‌کند.
- در لاگ Host در دو اجرای Gate 3 و دو اجرای Gate 4 صفر درخواست به FlightFlow ثبت شد.
- Replay هیچ درخواست خروجی نمی‌فرستد.
- `SeatMaps/Resolve` بدون تغییر است: diff خالی و 200 زنده.

## 9. جدول شواهد بستن
SHA تغییر برای همه‌ی ردیف‌ها: **commit نشده روی `824d88b`**.

| ID | تست | فرمان یا HTTP | نتیجه | شاهد |
|---|---|---|---|---|
| I01–I05, I12 | `V10_I01_I02_I03_I04_I05_I12_…` | `dotnet test … AcceptanceTests` | PASS | Hold شکست‌خورده هیچ ردیفی نمی‌نویسد؛ 16419، 16104 و 16607 |
| I06–I11, I14–I20, I24–I28 | کلاس `V10InventoryAllocationAcceptanceTests` و Domain `V10InventoryAllocationConformanceTests` | همان | PASS | گزارش Gate 2 بند 6 و 7 |
| I21–I23 | `V10_I21_I22_I23_…` | دو اجرای مستقل با filter | PASS ×2 | 1 موفق از 4؛ مصرف برابر ظرفیت |
| I24 + Close/Suspend/Retire | `V10_I24_…`، `V10_I19_close_…` | دو اجرای مستقل | PASS ×2 | بند 5 |
| I13, I27 | `V10_A01_…_I13`، `V10_I27_…` | همان | PASS | JSON strict؛ `decimal(19,3)` |
| I29, I32 | Query پیش از استقرار روی dev | `sqlcmd` (Gate 2) | PASS (0 ردیف) | بدون گیت runtime، طبق تصمیم Owner |
| L01–L23 | Domain `V10_L…` + SQL `V10_L02_…`، `V10_L03_L06_L11_L16_…` + زنده | همان + گام‌های 1 تا 14 و 22 تا 24 | PASS | بند 6 |
| S01–S12, S14, S15 | کلاس `V10SeatAndLifecycleAcceptanceTests` + `SEAT_A08` + زنده | همان + گام‌های 5، 6، 15 تا 18 | PASS | گزارش Gate 3 بند 5 |
| S13 | — | — | مستند | Ancillary بین سفارش‌ها داوری صندلی نمی‌کند (مالکیت Ordering/FlightFlow) |
| E05, E07, E08 | `V10_E05_…`، `V10_S03_S04_L23_E07_…`، `V10_S02_…_E08_…`، `V10_I26_…` | همان | PASS | replay بدون Allocation دوم |
| E06 | — | — | **NOT VERIFIED** | جبران صندلی فیزیکی کار Ordering است |
| E01–E03 | — | — | **NOT VERIFIED** | Ordering.Final `cd50a2a` هیچ فراخوان `Service-Holds` یا `SeatMaps/Holds` ندارد؛ Adapter Ordering وجود ندارد |
| E09 | — | — | N/A | Service ناشناس است (حکم ثابت Owner)؛ Route عمومی رزرو ساخته نشد |
| E10, E11, E12 | SQL I10 + زنده گام 21 | همان | PASS | انقضای منطقی بدون poller؛ خطای صریح Supplier؛ Ancillary هیچ EMD یا پول ثبت نمی‌کند |
| R01–R04 | Shopping 126، Domain 287 | همان | PASS | فقط Pinهای مجاز تغییر کرد |
| R05 | `has-pending-model-changes` | migrate.sh | PASS | No changes |
| R06 | رقابت‌ها و rollback روی SQL واقعی | همان | PASS | بدون Mock |
| R07 (a) | HTTP زنده Unlimited + Seat | Host 5299 | PASS | 24 از 24 |
| R07 (b) | SQL Local | Acceptance | PASS | بند 5 |
| Local live HTTP | — | — | **GATED** | منبع مرجع Resource و Facility وصل نیست؛ dev هیچ Policy Local ندارد |
| R08 | HTTP زنده SeatOnly + Mixed Append | Host 5299 | PASS | FlightFlow: صفر |
| R09 | `git diff --stat 2025efd` | git | PASS | فقط Reservation، Inventory، یک شرط Shopping (بند 7)، تست‌ها، docs، reports و Migration |
| R10 | زنجیره‌ی مرجع | — | PASS | بند 1 |
| 6a | `V10SupplierSaleGuardTests` | Shopping | PASS | قبل از Guard شکست، بعد از Guard موفق |

**تغییر در تست‌های قبلی:**
- `P2_X01` (Count) تغییر نام داد و انتظار rollback زیر قفل منبع گرفت (Gate 2).
- Pinهای Frozen حالا 82 فایل و hash `758010CE0F03CA960ACECEF62843BED3B7F7FCA52929066C6C963396CB8EABDD` دارند.
- `TestDatabase` حالا `MaxPoolSize = 250` دارد.
- `SEAT_A05` حالا Expire را برای هر سفارش جدا صدا می‌زند.

## 10. ریسک‌های باقیمانده (بدون وعده‌ی کار پس‌زمینه)
1. **Retire منبع با مصرف زنده** (باز از Gate 2): پس از Retire، منبع تازه برای همان پرواز و Resource با مصرف صفر شروع می‌شود. پیشنهاد: Retire تا وقتی مصرف فعال بزرگ‌تر از صفر است با 16621 رد شود.
2. **سقف Pool اتصال** (باز از Gate 2): درخواست‌های منتظر قفل منبع، اتصال Pool را نگه می‌دارند و Admin برای Read Model اتصال دوم می‌خواهد. بیش از حدود 100 نویسنده‌ی هم‌زمان روی یک منبع به timeout می‌رسد؛ نتیجه fail-closed است، نه oversell.
3. **سایر شکاف‌های Price → Hold** (خارج از حکم 6a، فقط Supplier بسته شد): در این سه حالت Price می‌پذیرد ولی Hold با 16419 رد می‌کند:
   - محصول بدون Policy فعال (dev: صفر مورد)؛
   - Policy با Usage Limit (امروز قابل فعال‌سازی نیست، چون منبع Counting Family وصل نیست)؛
   - Local بدون منبع تعریف‌شده (Local live GATED است).
4. **Local live HTTP** تا وصل شدن منبع مرجع واقعی GATED می‌ماند. **Supplier** بدون Adapter صریحاً رد می‌شود. **یکپارچگی Ordering** (E01–E03، E06) تأیید نشده است. هیچ ادعای E2E برای EMD یا Ordering مطرح نیست.
5. **داده‌ی آزمایشی dev:** Holdهای اجراهای زنده (سفارش‌های 9930010 تا 9940022) باقی‌اند. فقط سفارش 9930010 هنوز Held است؛ مهلتش گذشته و اولین Hostی که poller دارد آن را Expired ذخیره می‌کند. بقیه Confirmed، Released، Expired یا Cancelled هستند.

## 11. گیت انتشار
همه‌ی تست‌های P0 موجودی Local (I06–I28، به‌ویژه I21–I24) روی SQL سبزند و SeatMap، Price و Resolve بازگشت (regression) ندارند. Guard Supplier عمداً فروش محصولات Supplier را می‌بندد. شرط بستن Pack برقرار است، اما commit، merge و deploy فقط با دستور Owner انجام می‌شود.
