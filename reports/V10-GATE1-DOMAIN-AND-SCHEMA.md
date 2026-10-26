# Reservation v10 — Gate 1: Domain، قرارداد و Schema

**تاریخ:** 2026-10-26 · **مبنا:** `feat/ancillary-offers-v9@debd065`؛ commit نشده
**مرجع:** Pack v10 ادغام‌شده (`reports/V10-GATE0-OWNER-DECISIONS-APPLIED.md`)، فایل `11`

## 1. وضعیت
Gate 1 ساخته و آزموده شد. Hold هنوز ظرفیت را تضمین نمی‌کند، چون تخصیص موجودی و fail-closed بودن Policy در Gate 2 است. چیزی مستقر نشده است.

## 2. چه ساخته شد

### Domain (`AncillaryReservationAggregate`)

| نوع | تغییر |
|---|---|
| `AncillaryReservation` | `Hold` حالا `requestedExpiresAt` غیرخالی و آینده می‌خواهد. guard قدیمی «Definition + Traveller + Flight» حذف شد؛ `OrderServiceId` تکراری و Seat تکراری در یک Hold همچنان رد می‌شوند. |
| | `AppendSeats(appendKey, orderId, reference, expiresAt, seats)`: فقط Seat، روی همان Root. ابتدا replay بررسی می‌شود، سپس برابری Order/Reference/Expiry و `Held` بودن Root. |
| | `Release(unitIds?)` و `Cancel(reason, unitIds?)`: بدون فهرست = عملیات کلی؛ با فهرست = همه یا هیچ. |
| | `Confirm`، `Release`، `Cancel` و `Expire` فقط Unitهای واجد شرایط را تغییر می‌دهند. |
| | وضعیت Root از وضعیت Unitها مشتق می‌شود. ترکیب هم‌زمان Held و Confirmed خطای 16408 می‌دهد. |
| | `EnsureSameContent` فقط Unitهای ساخته‌شده در Create را مقایسه می‌کند، نه Unitهای append. |
| `AncillaryReservationUnit` | فیلدهای `AppointmentTimeWithOffset`، `GuestCount`، `AppendIdempotencyKey` و `Allocations`. |
| | قواعد Seat: تعداد 1، Sector، بدون Fact و بدون Allocation. `GuestCount` فقط همراه زمان قرار و ≥0. |
| | Replay Factها را با لحظه UTC مقایسه می‌کند. |
| `AncillaryReservationUnitAllocation` (جدید) | Entity فرزند با 5 فیلد: `Id`، `AncillaryReservationUnitId`، `ResourceKind`، `InventorySourceId`، `Amount`. |
| | Count و Slot عدد صحیح؛ Weight طبق `InventoryRules.IsKg` (3 رقم اعشار). یک Allocation برای هر (نوع، منبع). |
| `AncillaryReservationAllocationArgs` (جدید) | ورودی Allocation. Gate 2 آن را از Policy پر می‌کند. |
| خطا | 16418 `AncillaryHoldUnitNotFound` (404): Unit متعلق به این Hold نیست. 16417 برای کمبود ظرفیت در Gate 2 کنار گذاشته شد. |

### Application
- `AncillaryHoldServiceInput`: فیلدهای Seat حذف شدند و `InventoryFacts` (`InventoryHoldFactsInput(TimeWithOffset, GuestCount)`) اضافه شد. هر دو type با `JsonUnmappedMemberHandling.Disallow`.
- `RequestedExpiresAt` در command عمومی غیرخالی است؛ Validator مقدار پیش‌فرض را رد می‌کند (در facade Seat هم).
- `IHoldAncillaryServicesService` یک مسیر نوشتن واحد دارد: `HoldAsync(AncillaryHoldHeader, units)`. ورودی عمومی و facade Seat هر دو از همین مسیر عبور می‌کنند؛ کلاس داخلی `SeatHold` حذف شد.
- `UnitIds` اختیاری به Release و Cancel اضافه شد. Validator فقط شکل فهرست را می‌سنجد: غیرخالی، یکتا، مثبت.

### Persistence و REST
- Unit سه ستون nullable گرفت. جدول `AncillaryReservationUnitAllocations` با FK از نوع Restrict ساخته شد.
- Repository، Unitها و Allocationهایشان را با هم load می‌کند.
- `DELETE Service-Holds/{holdId}` پارامتر `[FromQuery] long[]? unitIds` گرفت؛ فهرست خالی یا نبودن آن یعنی کل Hold.
- `CancelAncillaryHoldRequest(reasonCode, unitIds?)` اکنون strict است؛ بدنه Hold هم strict است.
- هیچ Route تازه‌ای ساخته نشد.

## 3. Migration (فقط CommandDb و افزودنی)
`20261026114827_V10ReservationAllocations`، تولیدشده با `ef migrations script`:

```text
ALTER TABLE [Ancillary].[AncillaryReservationUnits] ADD [AppendIdempotencyKey] nvarchar(128) NULL;
ALTER TABLE [Ancillary].[AncillaryReservationUnits] ADD [AppointmentTimeWithOffset] datetimeoffset NULL;
ALTER TABLE [Ancillary].[AncillaryReservationUnits] ADD [GuestCount] int NULL;
CREATE TABLE [Ancillary].[AncillaryReservationUnitAllocations] (
    [Id] bigint NOT NULL, [AncillaryReservationUnitId] bigint NOT NULL, [ResourceKind] int NOT NULL,
    [InventorySourceId] bigint NOT NULL, [Amount] decimal(19,3) NOT NULL,
    [LastUpdateTime] datetimeoffset NOT NULL, [LastUpdatedBy] bigint NULL,
    CONSTRAINT [PK_AncillaryReservationUnitAllocations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_..._AncillaryReservationUnitId] FOREIGN KEY ([AncillaryReservationUnitId]) REFERENCES [Ancillary].[AncillaryReservationUnits] ([Id]) ON DELETE NO ACTION);
CREATE UNIQUE INDEX [IX_..._AncillaryReservationUnitId_ResourceKind_InventorySourceId] ...;
CREATE INDEX [IX_..._ResourceKind_InventorySourceId] ...;
```

| کنترل | نتیجه |
|---|---|
| اعمال روی dev (`DotAirAncillary`) | آخرین migration: `20261026114827_V10ReservationAllocations` |
| `has-pending-model-changes` | `No changes have been made to the model since the last migration.` |
| Down و Up روی dev (dev هیچ Reservation ندارد) | بعد از Down: 0 شیء v10. بعد از Up: 10 شیء (3 ستون + 7 ستون جدول) |
| QueryDb | بدون تغییر |

## 4. اثبات

| دستور | نتیجه |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 خطا، 61 هشدار (همان تعداد مبنا) |
| Domain (`dotnet test` + `--list-tests`) | 277 کشف‌شده؛ 277 قبول |
| Shopping | 124 کشف‌شده؛ 123 قبول و 1 Skip (تست Counting Family از R8) |
| Application (SQL واقعی، migration کامل در DB آزمون) | 236 کشف‌شده؛ 236 قبول |
| کامنت در فایل‌های تغییرکرده | ندارد |
| پایان خط | CRLF |

### تست‌های تازه
- `V10ReservationLifecycleConformanceTests` (Domain، 13 تست): L01، L04، L06–L22، A14، I27، S02–S09، و فهرست نامعتبر `unitIds`.
- `V10ReservationAcceptanceTests` (SQL، 4 تست):
  - E08، L02، L08، L10، L12، L14: Release جزئی، Confirm و Cancel جزئی، سپس خواندن دوباره.
  - L19 و L20: Factها با offset ذخیره می‌شوند؛ replay با Fact متفاوت 16403 می‌دهد.
  - I27: Allocation با سه رقم اعشار در SQL ذخیره و خوانده می‌شود.
  - A01، A02، A16، I13: قرارداد JSON فیلد ناشناخته را رد می‌کند (وزن پذیرفته‌شده، `seatNumber` در ورودی عمومی، `currencyId`) و Validatorها فهرست `unitIds` نامعتبر را رد می‌کنند.

### Idempotency
- Create replay همان Hold را برمی‌گرداند. تغییر `guestCount`، لحظه قرار یا `quantity` خطای 16403 می‌دهد. همان لحظه با offset متفاوت، replay معتبر است.
- Append با همان کلید و همان Seat تغییری ایجاد نمی‌کند. همان کلید با Seat دیگر 16403 می‌دهد.
- Release یا Cancel انتخابیِ تکراری بدون نوشتن پاسخ می‌دهد. Cancel انتخابی با دلیل دیگر 16413 می‌دهد و دلیل قبلی حفظ می‌شود.

## 5. تست‌های قدیمی به‌روزشده (حذف نشدند)
- `M1ReservationConformanceTests`:
  - Hold بدون انقضا اکنون 16402 است (قبلاً «هرگز منقضی نمی‌شود»).
  - دو خدمت از یک Definition برای یک مسافر و پرواز اکنون مجاز است (A14؛ قبلاً 16402).
- `M1ReservationAcceptanceTests` و `M1Commands`: همه Holdها انقضا دارند. Commandهای آزمون Release و Cancel `UnitIds` اختیاری گرفتند.
- `SeatReservationConformanceTests`: امضای Release و Cancel تغییر کرد؛ رفتار همان ماند.
- Pinها، فقط به اندازه دامنه مصوب:
  - hash فایل‌های منجمد Reservation: از 72 فایل و `BCB3AE2E…` به 78 فایل و `BF569942…`، در سه تست (`P2_X07`، `V12_C04`، `X20`). اسکریپت hash روی `HEAD` همان مقدار قبلی را دوباره ساخت.
  - `P2_X07` فایل‌های پوشه Reservation را دیگر جزو منبع inventory حساب نمی‌کند.
  - `V121_M05` ستون `AncillaryReservationUnitAllocations.Amount decimal(19,3)` را می‌شناسد.

## 6. Breaking changes
1. `requestedExpiresAt` در `POST Service-Holds` الزامی است.
2. `seatMapSelectionId`، `seatNumber` و `pricingId` از ورودی عمومی Hold حذف شد؛ فیلد ناشناخته 400 می‌گیرد.
3. Confirm، Release و Cancel دیگر Unit آزادشده یا لغوشده را دست نمی‌زنند.

## 7. هنوز باز و خارج از Gate 1
- Gate 2: Policy فعال، رد Supplier/تأیید/A09/الگوی پشتیبانی‌نشده، تخصیص Local با قفل `sp_getapplock`، کد 16417، و قفل منبع در Adjust/Close/Suspend/Retire.
- Gate 3: `holdId` روی `SeatMaps/Holds` (متد Domain آماده است) و قفل سفارش برای Confirm/Release/Cancel/Expire.
- HTTP زنده در این Gate اجرا نشد؛ رفتار binding پارامتر `unitIds` از Query در Gate 4 با HTTP واقعی ثابت می‌شود.
- تعارض تازه‌ای با Pack یا سورس پیدا نشد.
