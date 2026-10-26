# Reservation v10 — Gate 2: موجودی Local و جلوگیری از Oversell

**تاریخ:** 2026-10-26 · **مبنا:** `feat/ancillary-offers-v9@824d88b`؛ Gate 1 و Gate 2 هر دو commit نشده‌اند
**مرجع:** Pack v10 ادغام‌شده، فایل‌های `04`، `06`، `07` و `12`؛ تصمیم‌های Owner در `AeroTech-Ancillary-v10-Gate0-Owner-Decisions-FA.md`

## 1. وضعیت
Gate 2 ساخته و روی SQL واقعی آزموده شد. Hold یک سرویس غیر Seat حالا ظرفیت Local را در یک تراکنش و زیر قفل منبع کسر می‌کند و در هر کمبود کل درخواست را رد می‌کند. Local live HTTP همچنان **GATED** است، چون هیچ منبع مرجع واقعی برای Resource و Facility وصل نیست. چیزی مستقر نشده است.

## 2. مسیر Hold
همان `HoldAncillaryServicesService` گسترش یافت؛ Orchestrator دوم ساخته نشد.

1. Replay با همان کلید: نتیجه‌ی ذخیره‌شده برمی‌گردد. Allocation و قفل تازه‌ای ساخته نمی‌شود.
2. قفل `ancillary-hold:{orderId}` مثل قبل.
3. `AncillaryHoldAllocator` برای هر Unit:
   - Definition، Provision و Supplier بارگذاری و `AncillaryHoldChecks.Accept` اجرا می‌شود. Definition و Provision باید Active باشند. `SubjectToConfirmation` رد می‌شود (16419).
   - Unit Seat بدون Policy و بدون Allocation عبور می‌کند.
   - Policy با `(OwnerAirlineId, ServiceDefinitionRef)` و فقط در وضعیت Active خوانده و با `EnsureReservable` سنجیده می‌شود. نبود Policy، Draft، Suspended، Retired، Supplier، Usage Limit، `AcceptedWeightKg` و Local بی‌Binding همه fail-closed هستند.
   - شناسه‌ی منبع از Policy و مالک Definition به‌دست می‌آید و هرگز از ورودی Client خوانده نمی‌شود.
     - Count: به‌ازای هر پرواز پوشش‌داده‌شده، `RequiredUnits(qty)`.
     - Weight: `FixedKgPerUnit × qty`.
     - Slot: زمان قرار تا `+OccupancyMinutes`، پوشش پیوسته‌ی sliceها، و برای هر slice مقدار `PeoplePerAcceptedUnit × (qty + guests)`.
4. `InventoryLedger.OpenAsync(sources)`:
   - یک تراکنش READ COMMITTED روی همان `AncillaryDbContext` باز می‌شود.
   - برای هر منبع، به ترتیب (نوع، شناسه): `sp_getapplock @LockOwner='Transaction'`، با کلید `Ancillary.Inventory.{kind}.{sourceId}` و `@LockTimeout = 20 s`. خروجی منفی یعنی rollback و خطای 16622.
5. بعد از قفل، ظرفیت، وضعیت و `ClosedForSale` منبع و مصرف فعال دوباره خوانده می‌شوند. مصرف فعال = Confirmed، به‌علاوه‌ی Held با `ExpiresAt` خالی یا بزرگ‌تر از اکنون. سپس `EnsureAvailable` اجرا می‌شود: جمع مصرف هر منبع روی همه‌ی Unitها حساب می‌شود؛ منبع غایب، غیر Active یا بسته 16419 می‌دهد و کمبود 16417.
6. بررسی OrderService زنده (16406)، ساخت `AncillaryReservation.Hold` با Allocationها، `SaveChanges` و `Commit`. هر خطا قبل از Commit همه‌چیز را rollback می‌کند.

Admin: `Adjust`، `Close`، `Suspend` و `Retire` در سه نوع منبع (12 سرویس) قبل از بارگذاری منبع همان قفل را می‌گیرند. `AdjustTo(newTotal, activeUsage, …)` در Domain مقدار کمتر از مصرف فعال را با 16621 رد می‌کند. `Open` و `Activate` قفل نمی‌گیرند، چون تصمیم فقط چهار عملیات را نام برده است. `TotalCapacity` هیچ‌وقت در Hold تغییر نمی‌کند و شمارنده‌ی کش‌شده‌ای وجود ندارد.

## 3. خطاهای تازه

| کد | HTTP | معنا |
|---|---|---|
| 16417 | 409 | ظرفیت باقیمانده‌ی منبع برای این Hold کافی نیست |
| 16419 | 409 | Hold شدنی نیست؛ دلیل در پیام آمده است |
| 16621 | 409 | ظرفیت جدید کمتر از مصرف فعال منبع است |
| 16622 | 409 | قفل منبع در 20 ثانیه گرفته نشد |

16104 (Supplier خارجی بی‌Adapter)، 16402، 16403، 16406 و 16607 بدون تغییر بازاستفاده شدند.

## 4. آزمون SQL
- **فرمان:** `dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests --no-build -nodeReuse:false`
- **جداسازی:** `TestDatabase` در هر اجرا پایگاه تازه‌ی `DotAirAncillary_Tests_{guid}` را روی SQL Express می‌سازد و Migrationهای Command، Query و Reference را اجرا می‌کند. dev لمس نمی‌شود.
- **Fixture:**
  - `InventoryHarness` و `InventoryScope` برای Policy و منبع، با `InventoryFixture` متصل.
  - `AncillaryScope` برای Hold، با Allocator و `InventoryLedger` واقعی.
  - هر تلاش هم‌زمان در Scope و اتصال جداگانه‌ی خودش اجرا می‌شود.
- **کلاس جدید:** `Holds/V10InventoryAllocationAcceptanceTests` با 10 تست.

## 5. نتیجه‌ها (همه در همین نوبت اجرا شدند)

| کنترل | نتیجه |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 خطا، 61 هشدار (همان تعداد Gate 1) |
| Domain conformance | 286 از 286 (discovered 286) |
| Shopping | 123 موفق + 1 skip از 124 (discovered 124)، بدون تغییر |
| Application acceptance (SQL) | 245 از 245 (discovered 245)، بدون abort، 1.7 دقیقه |
| `ef migrations has-pending-model-changes` | No changes (Gate 2 ستون تازه‌ای ندارد) |
| CRLF / Comment | همه‌ی فایل‌ها CRLF هستند؛ کامنت تازه‌ای نوشته نشده است |

## 6. پوشش ماتریس

| شناسه | تست | نتیجه‌ی اثبات‌شده |
|---|---|---|
| I01 | SQL `V10_I01_…` | Unlimited: Held با 0 Allocation |
| I02, I03 | SQL `V10_I01_…` + Domain | Policy غایب، Draft (DailyCount) و Suspended → 16419؛ بدون Reservation و Unit |
| I04 | SQL + Domain | Supplier خارجی → 16104 پیش از Policy؛ شاخه‌ی Policy از نوع Supplier در Domain → 16419 |
| I05 | SQL | فعال‌سازی DailyCount → 16607؛ Policy در Draft می‌ماند و Hold → 16419 |
| I06, I08, I09, I25 | SQL `V10_I06_…` | qty2×3 = 6.000؛ Held و Confirmed شمرده می‌شوند و کمبود 16417 است؛ `TotalCapacity` ثابت |
| I07 | SQL `V10_I07_I10_…` | دو محصول روی یک منبع، یک ledger مشترک |
| I10 | SQL `V10_I07_I10_…` | 31 دقیقه بعد و بدون poller، ظرفیت آزاد است؛ ردیف‌های قدیمی هنوز Held هستند |
| I11, I14, I15 | SQL `V10_I11_…` | Count+Weight با هم ثبت می‌شوند؛ کمبود Weight هیچ Allocationی باقی نمی‌گذارد |
| I12 | SQL + Domain | Policy فعال `AcceptedWeightKg` روی محصول PerKilogram → 16419 و مصرف منبع Weight صفر |
| I13 | `V10_A01_…_I13` (Gate 1) | `acceptedWeightKgTotal` به‌عنوان عضو ناشناخته رد می‌شود؛ ستونی ساخته نشده است (pin `V121_M05`) |
| I16, I28 | SQL `V10_I16_I28_…` | Portion دو پروازه با منبع غایب: هیچ؛ منبع ایرلاین دیگر هرگز استفاده نمی‌شود |
| I17, I18, I19, I20 | SQL `V10_I17_…` | دو slice هر کدام 3 نفر (qty1 + 2 مهمان)؛ gap → 16419؛ slice بسته → 16419؛ Release پس از Close مجاز است |
| L19, L20 | SQL `V10_I17_…` | replay همان HoldId بدون Allocation تازه؛ تغییر guestCount → 16403؛ slot بی‌زمان → 16402 |
| I21, I22, I23 | SQL `V10_I21_…` | 4 Order هم‌زمان روی آخرین واحد، آخرین 12.5 kg و آخرین نفر: دقیقاً یک موفق و بقیه 16417 |
| I24 | SQL `V10_I24_…` + Domain | Adjust کمتر از مصرف → 16621؛ رقابت Adjust با Hold فقط یکی از دو نتیجه‌ی سازگار را می‌دهد |
| I19 (Close/Suspend/Retire) | SQL `V10_I19_close_…` | رقابت Hold با هر سه عملیات؛ پس از هر کدام Hold تازه → 16419 |
| I26 | SQL `V10_I26_…` | UnitOfWork پس از نوشتن Command شکست می‌خورد: 0 Reservation و 0 Allocation؛ تلاش دوباره موفق است |
| I27 | `V10_I27_…` (Gate 1) + Domain | `decimal(19,3)` در SQL رفت‌وبرگشت دارد؛ Count/Slot کسری رد می‌شود |
| I29, I32 | Query پیش از استقرار (بند 9) | طبق تصمیم Owner: بدون گیت runtime و بدون reconciliation |
| Release/Cancel | SQL `V10_I06_…`، `V10_I17_…` | Release ظرفیت Held و Cancel ظرفیت Confirmed را آزاد می‌کند |

## 7. ردیف‌های ظرفیت و مصرف (مقادیر assert‌شده)
قالب ظرفیت `Capacity|Version` است. مصرف از همان predicate دفتر کل خوانده می‌شود.

| سناریو | قبل | بعد |
|---|---|---|
| Count، ظرفیت 10، 3 در هر واحد | `10\|2`، مصرف 0.000 | Confirmed 6 + Held 3 → 9.000، ظرفیت `10\|2`. پس از Release 3 و Hold دوباره: 9.000. پس از Cancel 6: 3.000 |
| Count مشترک بین دو محصول، ظرفیت 2 | 0.000 | 2.000؛ +31 دقیقه بدون poller: Hold تازه → 1.000 |
| Count+Weight: 5 واحد و 30 kg، هر واحد 12.5 kg | 0.000 / 0.000 | qty2: 2.000 / 25.000. qty1 بعدی رد شد؛ ظرفیت `5\|2` و `30.000\|2` ثابت |
| Slot: دو slice با ظرفیت 4 | 0.000 / 0.000 | 3.000 / 3.000. پس از Close و Release: 0.000 / 0.000، ظرفیت `4\|3` و `4\|2` |
| رقابت روی آخرین واحد | 0 | Count 1.000، Weight 12.500، Slot 1.000 (برابر ظرفیت) |
| Adjust، ظرفیت 3 و مصرف 2 | `3\|2` | Adjust به 1 رد و بی‌اثر ماند. رقابت Adjust→2 با Hold: یا `3\|2` با 3.000 (16621)، یا `2\|3` با 2.000 (16417) |
| شکست Save | 0.000 | `0\|0` Reservation/Unit و مصرف 0.000؛ تلاش دوباره → 1.000 |

## 8. منبع تکی، منبع مشترک، دقت و سرریز
- **منبع تکی:** هر محصول Policy خودش را دارد. ظرفیت از ردیف منبع و مصرف از Allocationها خوانده می‌شود.
- **منبع مشترک:** دو Policy روی یک (Flight، Resource) به یک ردیف منبع می‌رسند و مصرف هر دو یک‌بار از همان ledger جمع می‌شود (I07). جمع مصرف در یک Hold چندواحدی هم یک‌جا محاسبه می‌شود.
- **دقت:** Weight سه رقم اعشار دارد (`0.001 × 3 = 0.003`، `12.5 × 3 = 37.5`)؛ Count و Slot عدد صحیح‌اند و `Amount` کسری با 16402 رد می‌شود.
- **سرریز:** `RequiredUnits`، `RequiredPersons` و `RequiredKg` حاصل‌ضرب را در `long` یا با سقف `MaxKg` می‌سنجند و سرریز را با 16602 رد می‌کنند (تست Domain).

## 9. dev (فقط خواندنی، `localhost\SQLEXPRESS / DotAirAncillary`)
- Query پیش از استقرار برای Unitهای زنده‌ی Local بدون Allocation: **0**. Reservation 0، Unit 0، Allocation 0، Policy Local 0، منبع 0. آخرین Migration: `20261026114827_V10ReservationAllocations`.
- محصولات Active غیر Seat: 33 با Policy از نوع Unlimited (Hold شدنی)، و 20 با Policy از نوع Supplier که از این Gate به بعد با 16419 رد می‌شوند. 6 محصول Seat بدون Policy هستند و مسیر Seat Policy را نمی‌خواند. محصول غیر Seat بدون Policy وجود ندارد.

## 10. تغییر Pinها و تست‌های موجود
- **Frozen hash:** 81 فایل (78 + `IAncillaryHoldAllocator`، `AncillaryHoldAllocator` و `InventoryLedger`)، hash `C56875A793756539ADAC845CE835617D04864B99973A0926F004648294130ABB` در `P2_X07`، `V12_C04` و `X20`.
- **`P2_X01` (Count) تغییر نام و انتظار داد:** نام جدید `…a_save_that_fails_after_the_command_write_rolls_back_under_the_source_lock_and_the_replay_adjusts_once`. با قفل، نوشتن Command و Commit در یک تراکنش‌اند، پس شکست بعد از Command همه‌چیز را rollback می‌کند: ردیف `8|1|0|8|1|0` و بدون اختلاف Projection. replay همان Adjust را یک‌بار انجام می‌دهد (`5|2|1|5|2|1`).
- **`TestDatabase`:** `MaxPoolSize = 250`. سه رقابت 100تایی Admin (`P2_X01` Count، `P2_X01` Slot و `X09`) با قفل جدید بیش از 100 اتصال هم‌زمان می‌خواهند (بند 12-2).
- **L19/L20:** از `V10ReservationAcceptanceTests` به کلاس Gate 2 منتقل شد، چون Fact حالا Policy از نوع Slot لازم دارد.
- **Catalog تست:** `M1` و `V10` Policy از نوع Unlimited فعال می‌گیرند (`InventoryHarness.ActivePolicyAsync`). Fixtureهای Domain با `ActiveLoungeDefinition` و `ActiveLoungeProvision` کار می‌کنند. 27 فراخوانی `AdjustTo` در تست‌های P2 مقدار `activeUsage = 0` گرفتند.

## 11. فایل‌های تغییرکرده در Gate 2

| لایه | فایل |
|---|---|
| Domain (جدید) | `_Shared/Contracts/InventorySourceKey`، `InventorySourceBalance`، `IInventoryLedger`، `IInventoryLedgerSession` |
| Domain | `AncillaryHoldChecks` (`Accept`، `EnsureReservable`، `EnsureCovered`، `EnsureAvailable`)؛ `AdjustTo` در `FlightCountInventory`، `FlightWeightInventory` و `AirportSlotInventory`؛ `AirportSlotConsumption.RequiredPersons`؛ پورت‌های `FindActiveAsync`، `FindCurrentIdAsync` (Count، Weight) و `ListCurrentAsync`؛ `ExceptionFactory` و `ExceptionMessages` |
| Application | `Services/IAncillaryHoldAllocator` و `AncillaryHoldAllocator` (جدید)؛ `HoldAncillaryServicesService`؛ 12 سرویس Adjust/Close/Suspend/Retire؛ `DependencyInjection` |
| Persistence | `AncillaryReservationAggregate/InventoryLedger` (جدید)؛ چهار Repository؛ `DependencyInjection` |
| Tests | جدید: `Holds/V10InventoryAllocationAcceptanceTests`، `V10InventoryAllocationConformanceTests`. تغییرکرده: `AncillaryScope`، `InventoryScope`، `InventoryHarness`، `TestDatabase`، `M1Fixtures`، `M1ReservationConformanceTests`، `M1ReservationAcceptanceTests`، `V10ReservationAcceptanceTests`، `P2CapacitySourceConformanceTests`، `P2FlightCapacityAcceptanceTests` و سه Pin |

## 12. نکته‌های باز برای Owner
1. **Retire منبعی که Allocation زنده دارد.** Index یکتای منبع روی ردیف‌های غیر Retired فیلتر شده است. بعد از Retire می‌توان برای همان (Flight، Resource) منبع تازه تعریف کرد که با مصرف صفر شروع می‌کند، در حالی که Allocationهای زنده روی منبع قدیمی می‌مانند. بند 55 فایل `04` می‌گوید Retire «نباید مصرف تاریخی را مخفی کند». الان Retire زیر قفل اجرا می‌شود و چیزی را حذف نمی‌کند، اما این حالت را نمی‌بندد.
   - **پیشنهاد:** Retire تا وقتی مصرف فعال بزرگ‌تر از صفر است با 16621 رد شود.
   - تا تصمیم شما کدی برای این مورد نوشته نشد.
2. **ظرفیت Pool اتصال.** `sp_getapplock` در حالت Transaction یعنی هر درخواست منتظر، تا 20 ثانیه یک اتصال Pool را نگه می‌دارد. Command و Query در dev یک Connection String و یک Pool دارند (پیش‌فرض 100). Hold اتصال دوم نمی‌خواهد. اما Admin که قفل را دارد برای ذخیره‌ی Read Model اتصال دوم می‌خواهد. پس بیش از حدود 100 نویسنده‌ی هم‌زمان روی یک منبع، Pool را خالی می‌کند. نتیجه fail-closed است (timeout)، نه oversell.
   - **پیشنهاد:** در محیط‌های واقعی `Max Pool Size` متناسب با هم‌زمانی تنظیم شود.
   - در تست فقط Pool پایگاه موقت بزرگ‌تر شد؛ کد Production تغییری نکرد.
3. Guard محصولات Supplier در مسیر Offer/Price طبق فایل `14` بند 6a در Gate 4 است و در این Gate انجام نشد.

## 13. قدم بعد
Gate 3 شروع نشده است. منتظر بازبینی و حکم شما درباره‌ی بندهای 12-1 و 12-2 هستم. Commit و Push انجام نشده است.
