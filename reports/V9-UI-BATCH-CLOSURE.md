# V9 — UI Grouping + Options یکجا — گزارش بسته‌شدن

| | |
|---|---|
| مرجع | `docs/Shopping/FrontEnd/01..05` (v9)، تصمیم‌های مالک 2026-10-24 و `reports/V9-R1-QUESTIONS-AND-CONFLICTS.md` (2026-10-25) |
| Baseline | `feat/ancillary-offers-v8@324e32999bac6a3a9dfd69604dbe9e31272c43f0` |
| شاخه | `feat/ancillary-offers-v9`، **commit نشده** |

## 1. تصمیم‌های مالک

1. یک پیاده‌سازی برای Options (قاعده ۲ CLAUDE.md).
2. خطای هر عضو همان کد و status را دارد و پیش‌وند `references[i]: ` یا `selections[i]: ` می‌گیرد (`Query/Shopping/Common/SelectionBatch.ItemAsync`).
3. `optionsEndpoint` از پاسخ Shopping حذف شد.
4. R1 (2026-10-25): **یک** مسیر Forms و **یک** مسیر Submit برای همه خانواده‌ها:
   - Forms فقط `{offerId, references}` می‌گیرد و خانواده از خود Reference خوانده می‌شود.
   - هر خط Submit `{reference, baggage? | pet? | assistedTravel? | airportService? | priority? | connectivity?}` است و دقیقاً یکی از آن‌ها پر می‌شود.
   - همه مسیرهای تک‌موردی v8 و مسیرهای خانواده‌ای حذف شدند.

## 2. قرارداد

| Route | Body | پاسخ |
|---|---|---|
| `POST {Surface}/v1/AncillaryOffers` | `{offerId}` | هر خانواده `[{variantCode, title, products:[{productKey, serviceDefinitionRef, title, description?, details, units:[{selectionScope, unit, coverage, unitPrice, quantityPolicy, selectionId? \| quantityOptions? \| reference?}]}]}]` |
| `POST {Surface}/v1/AncillaryOffers/Options/Forms` | `{offerId, references[1..50]}` | `{forms:[{reference, family, variantCode, fields}]}`. در Backoffice، `family` به شکل `{value,name,title}` است |
| `POST {Surface}/v1/AncillaryOffers/Options` | `{offerId, selections[1..50]:[{reference, <family>:{…}}]}` | `{selections:[{reference, selectionId, expiresAtUtc}]}` |

نام عضوهای خط Submit همان نام `AncillaryProfile` است با حرف اول کوچک: `baggage`، `pet`، `assistedTravel`، `airportService`، `priority`، `connectivity`. نوع هر عضو همان record قبلی است (`BaggageSelectionOptions`، …). عضو ناشناخته در هر سطح رد می‌شود (`JsonUnmappedMemberHandling.Disallow`).

**لایه‌ها:**
- RestApi: `Requests/SelectionOptionsFormsRequest` و `Requests/SubmitSelectionOptionsRequest`. Controllerها فقط query یا command می‌سازند.
- Query: `GetSelectionOptions/` شامل `ISelectionOptionsQuery`، `GetSelectionOptionsService`، `SelectionOptionsMapper`، `Backoffice/BackofficeSelectionOptionsMapper` و سه query/handler سطح.
- Query: `VerifyTypedSelection/TypedSelectionVerifier`.
- Application: `SubmitSelectionOptions/` شامل `SelectionOptionsLine`، `ISubmitSelectionOptionsCommand`، `SubmitSelectionOptionsService`، Validator مشترک، `SelectionOptionsLineValidator`، `BaggageSelectionOptionsValidator`، `PetSelectionOptionsValidator`، `SelectionSnapshotResult` و `SelectionSnapshotsResult`، و سه command/handler/validator سطح.
- Application: پورت `ITypedSelectionVerifier` با `TypedSelectionVerification` و `TypedSelection`.
- Shopping: `TypedSelectionService.FormAsync` دیگر خانواده نمی‌گیرد. `ProveAsync` همچنان خانواده عضو پرشده را با Definition مقایسه می‌کند و در صورت اختلاف 16819 می‌دهد.

**ترتیب کار:**
- Forms: شکل لیست بررسی می‌شود (16801/422)، سپس امضا و انقضای همه Referenceها، سپس Offer **یک بار** بارگذاری می‌شود و Guard اجرا می‌شود، سپس فرم هر Reference ساخته می‌شود.
- Submit: Validator اجرا می‌شود (400 با مسیر فیلد، مثل `Selections[1].Options`)، سپس همه Referenceها خوانده می‌شوند، Offer یک بار بارگذاری می‌شود، Proof هر خط ساخته می‌شود، همه Snapshotها ساخته می‌شوند و **یک** `SaveChangesAsync` اجرا می‌شود. هر شکست پیش از ذخیره است و هیچ ردیفی نمی‌نویسد.

**حذف‌شده:** `GET {family}/{reference}/Options`، شش `POST <Family>/{reference}/Options`، `POST {family}/Options/Forms`، شش `POST <Family>/Options/Batch`، `OfferFamilyRoutes`، `AncillaryOfferDto` و `BackofficeAncillaryOfferDto`. Pin مسیرها: 170 در v8 ← 155.

## 3. شواهد (2026-10-25، روی کد نهایی)

| فرمان | نتیجه |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 خطا، 61 هشدار (برابر baseline). هیچ هشداری از فایل‌های تغییرکرده نیست |
| `dotnet test tests/AeroTech.Ancillary.Shopping.Tests` | 112 کشف‌شده، 112 قبول |
| `dotnet test tests/AeroTech.Ancillary.Domain.ConformanceTests` | 264 کشف‌شده، 264 قبول |
| `dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests` | 232 کشف‌شده، 232 قبول (Pin 155 مسیر) |
| Hash فایل‌های SeatMap | `cf4211…e39d`، برابر baseline |
| `git status` در SeatMap، Domain، Persistence، Migrations، ResolveGrouped، Reservation، Engine، Offers، Selections، Resolution | هیچ تغییری (در R1؛ R2 فایل Resolution را تغییر داد، بخش 6) |

**اجرای زنده:**
- 2026-10-24 روی Api و OtaPanel با همه مراحل قبول شد، ولی با مسیرهای خانواده‌ای که حالا حذف شده‌اند. آن نتیجه برای کارت‌بندی Shopping، همه‌یا‌هیچ و Resolve هنوز معتبر است.
- اجرای زنده مسیرهای جدید **انجام نشده**، چون توکن‌ها منقضی شده‌اند. اسکریپت آماده است: scratch `v9/live_flow_r1.py`، 17 مرحله، شامل Forms چندخانواده‌ای و بررسی 404/405 مسیرهای قدیمی.

## 4. ماتریس پذیرش (`04-ACCEPTANCE-MATRIX.md`)

| ID | وضعیت | شاهد |
|---|---|---|
| G01–G17 | PASS | `V9GroupingTests` (تغییر نکرده) |
| F01–F15 | PASS | `V9FormsTests`. F03: 50 Reference و یک Load. F08: یک درخواست برای سه خانواده (AssistedTravel، Baggage، Priority). F12: Bassinet با choices جدا. F15: Backoffice `family` را به شکل `EnumValueDto` می‌دهد |
| P01–P09، P11–P16، P18–P20 | PASS | `V9BatchTests`. یک درخواست با پنج خانواده (A03، A15، A20، A23، A24) و یک Load و یک Save. عضو خانواده دیگر ← 404 `selections[0]`. نبودِ عضو یا دو عضو هم‌زمان ← 400 `Selections[0].Options` |
| P10 Pets | GATED | Domain برای Pet تأیید تأمین‌کننده می‌خواهد (16820، مثل O04) |
| P17 | PASS (in-memory) | یک `SaveChangesAsync` روی `AncillaryDbContext` (یک تراکنش EF). تست SQL جدا ساخته نشد |
| E01–E05 | PASS در سطح تست | P20 (Cards ← Forms ← Submit ← Resolve). E05 حالا ۲ مسیر در هر سطح است، نه ۳+۱۸ |
| N01–N06 | PASS | بخش 3. N05: N درخواست GET و N درخواست POST به ۲ درخواست و ۲ بار بارگذاری Offer تبدیل شده |

## 5. نکته‌هایی برای بازبینی

- فایل 05 بسته (پرامپت Frontend) هنوز مسیرهای خانواده‌ای (`/{Family}/Options/Forms` و `/{Family}/Options/Batch`) را شرح می‌دهد. این فایل مال شماست و تغییرش ندادم. Frontend باید دو مسیر بخش 2 را به کار ببرد.
- منبع Offer جعلی در تست‌ها مقدار `scope.Channel` و `default` را برای enum `Channel` به کار می‌برد. هیچ کد production از این enum استفاده تازه نمی‌کند. منتظر تأیید شماست.
- Frontend فعلی `AeroTech.Backoffice.V1` هنوز روی API گروه‌بندی قدیمیِ برگردانده‌شده است.
- Backoffice زنده اجرا نشده (توکن و Offer قیمت‌گذاری‌شده روی کانال Backoffice لازم است).

## 6. R2 (2026-10-26، پاسخ‌های `reports/V9-R2-QUESTIONS-AND-CONFLICTS.md`)

| سؤال | پاسخ مالک | انجام‌شده |
|---|---|---|
| Q1 Meal پیش از Order | الف + بررسی در Resolve | `FlightOfferShoppingContextMapper.Facts`: برای منبع Offer مقدار `ExistingServicesComplete = true` است، یعنی خدمات قبلی «تأییدشده و خالی». `SelectionResolver`: اگر دو انتخاب Meal با یک `ExclusiveMealFamilyCode` برای یک مسافر و یک unit (`TravellerRef` و `CoverageRef` یکسان) در یک درخواست باشند، کل درخواست رد می‌شود: 409/16820 با متن `selection …: another meal of family MAIN_MEAL is selected for the same traveller and unit` |
| Q2 Pet | GATED بماند | بدون تغییر |
| Q3 Priority | حذف `FareBenefitRef` از dev | SQL روی `Ancillary.PrioritySpecifications`: دو ردیف (`PRIORITY_BOARDING`، `PRIORITY_CHECKIN`) به NULL تغییر کردند. ReadModel این ستون را ندارد |
| Q4 Meal و Upgrade در `/Options` | اضافه نشوند | بدون تغییر |

**پیامدها:**
- قاعده excess-piece در Baggage (`SharedRules.cs:391`) هم روی منبع Offer اجرا می‌شود و تعداد خریده‌شده پیش از Order صفر حساب می‌شود.
- `SeatSelectionVerifier` مقدار خودش را دارد و تغییر نکرد (SeatMap ثابت).
- این تغییر بند N03 بسته («ResolveGrouped unchanged») را به دستور شما کنار می‌گذارد.
- کد خطای تازه ساخته نشد. همان 16820 موجود (انتخاب قابل فروش نیست) به کار رفت.

**تست‌ها:** `V9MealFamilyTests`:
- M01: Context منبع Offer، خدمات قبلی را کامل و مزایای fare را ناکامل می‌گذارد.
- M02: Meal دارای family روی Context منبع Offer امضا می‌شود و با خدمات قبلی تأییدنشده دیده نمی‌شود.
- M03: A11 و A12 (هر دو `MAIN_MEAL`) برای یک مسافر و یک پرواز ← 409/16820. A12 به‌تنهایی Resolve می‌شود.
- M04: یک family برای دو مسافر ← هر دو Resolve می‌شوند.

این تست‌ها روی کد قبلی اجرا نشده‌اند (red-first انجام نشد).

**شواهد (2026-10-26، روی کد نهایی):**

| فرمان | نتیجه |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 خطا. هشدارها همان هشدارهای build قبلی‌اند، به‌علاوه 58 هشدار MSB3061 به این دلیل که host شما (PID 37976) فایل‌های `ServiceHost/bin` را قفل کرده است |
| `dotnet test tests/AeroTech.Ancillary.Shopping.Tests` | 116 کشف‌شده، 116 قبول |
| `dotnet test tests/AeroTech.Ancillary.Domain.ConformanceTests` | 264 کشف‌شده، 264 قبول |
| `dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests` | 232 کشف‌شده، 232 قبول |
| Hash فایل‌های SeatMap | `cf4211…e39d`، بدون تغییر |
| خط‌پایان | همه فایل‌های تغییرکرده CRLF‌اند. فقط پنج فایل `docs/Shopping/FrontEnd` شما LF هستند و دست نخوردند |

اجرای زنده R2 **انجام نشده**، چون توکن‌ها منقضی شده‌اند.

## 7. R3 (2026-10-26، پاسخ‌های `reports/V9-R3-QUESTIONS-AND-CONFLICTS.md`)

این بخش جای قرارداد بخش 2 را می‌گیرد.

| Route | Body | پاسخ |
|---|---|---|
| `POST {Surface}/v1/AncillaryOffers` | `{offerId}` | همان شکل قبلی. هر unit که `reference` دارد، `options` را هم دارد: `{<family>: {<field>: {control, required, choices?, unit?, min?, exclusiveMin?, max?, fields?}}}` |
| `POST {Surface}/v1/AncillaryOffers/Options` | `{offerId, selections[1..50]: [{selectionId} \| {reference, <family>: {…}}]}` | `{selectionId: "s_…", expiresAtUtc}`، یک Id برای کل سبد |
| `GET {Surface}/v1/AncillaryOffers/Options/{selectionId}` | — | همان شکل `AncillaryOffers`. unit انتخاب‌شده `selected: {quantity, options?: {<family>: {…}}}` دارد |
| `POST Service/v1/Ancillaries/Selections/ResolveGrouped` | `{selectionId}` | همان شکل قبلی. `selectionId` هر آیتم، Id خط ذخیره‌شده است (`l_…`) |

**شکل گزینه‌ها:**
- نام عضو خانواده در `options` و `selected.options` همان نام عضو خط Submit است. نام هر فیلد هم همان نام عضو Submit است.
- کنترل `dimensions` زیرفیلدهای `lengthCm`، `widthCm` و `heightCm` را دارد (number، cm، `exclusiveMin` 0).
- Frontend مقدار هر فیلد را جای توصیف آن می‌گذارد و همان را با `reference` تازه همان unit می‌فرستد.

**رفتار:**
- Submit هر خط را جدا ثابت می‌کند:
  - خط Direct: امضا، انقضا، فروشنده (403)، Offer (409)، و فعال‌بودن نرخ، Provision و Definition. محصولی که گزینه می‌خواهد، خط Direct نمی‌گیرد.
  - خط گزینه‌دار: همان مسیر قبلی.
- قاعده‌های سبد در Shopping (`SelectionBasketRules`):
  - یک unit دو بار، یا دو Meal از یک family برای یک مسافر و یک unit، رد می‌شود.
  - در Submit با پیام `selections[i]: …` و در Resolve با `selection l_…: …`، هر دو 409/16820.
- Submit یک `SelectionSnapshot` با خطوط فرزند `SelectionSnapshotLine` ذخیره می‌کند، با یک `SaveChanges`. انقضای سبد زودترین انقضای خطوط است.
- GetBySubmitId:
  - Offer ذخیره‌شده را با دامنه فروش فراخواننده دوباره می‌سازد و همان listing را اجرا می‌کند.
  - فروشنده دیگر 403 می‌گیرد. Offer تغییرکرده 409 می‌گیرد. اگر یک خط دیگر عرضه نشود، کل پاسخ 409/16820 است.
  - Id ناشناخته 404 و Id منقضی 410 است.
- `Options/Forms` در هر سه سطح حذف شد.

**لایه‌ها و نام‌ها (همه به همان الگوی قبلی):**
- Domain:
  - `SelectionSnapshot` (پدر: `Id`، `Version`، `OfferId`، زمان ساخت، انقضا).
  - `Entities/SelectionSnapshotLine` و `Arguments/SelectionSnapshotLineArgs`.
- Persistence:
  - جدول `SelectionSnapshotLines` (FK، `Sequence`، ایندکس یکتا).
  - Migration `20261025214031_SelectionSnapshotLines` روی dev اعمال شد. 18 ردیف آزمایشی قبلی snapshot پیش از آن پاک شد.
  - طول `OfferId` 512 است، مثل `SourceOfferId` در Ordering.
- Shopping:
  - `SelectionProofService`، جای `TypedSelectionService`، چون حالا Direct را هم ثابت می‌کند.
  - `SelectionProof`، `SelectionBasketLine` و `SelectionBasketRules`.
  - Listing فرم و انتخاب را روی row می‌گذارد. Resolver حالا `OpenAsync` و `ResolveAsync(selectionId)` دارد.
- Application:
  - `ISelectionVerifier`، `SelectionVerification` و `VerifiedSelection`.
  - `SubmittedSelection` با دو نوع `DirectSelection` و `TypedSelection`.
  - `SelectionOptionsLine` عضو `SelectionId` گرفت. خروجی Submit `SelectionSnapshotResult(SelectionId, ExpiresAtUtc)` است.
- Query:
  - `VerifySelections/SelectionVerifier`.
  - `GetSelectedAncillaryOffers/` با سه query/handler سطح.
  - `GetAncillaryOffersService` یک overload با snapshot گرفت، تا listing یک پیاده‌سازی بماند.
  - `SelectionOptionsMapper`.
  - DTOهای `SelectionOptionsDto<TValue>` و `SelectedUnitDto`. `SelectionFieldDto` عضو `fields` گرفت.
- Pinها:
  - مسیرها: سه `Options/Forms` جای خود را به سه `GET Options/{selectionId}` دادند. تعداد 155 ماند.
  - Pin نام Domain دو نوع تازه را استثنا کرد، مثل `SelectionSnapshot` در v8.

**تصمیم‌هایی که خودم گرفتم و پرسیده نشد:**
- نام کلاس‌ها و فایل‌های بالا.
- 16820 برای دو قاعده سبد.
- انقضای سبد برابر زودترین خط.
- ستون `Sequence`.
- طول 512.

**تست‌ها:**
- `V9InlineOptionsTests` (6 تست):
  - فرم زیر عضو خانواده می‌آید.
  - ابعاد زیرفیلد دارد.
  - عضوهای `options` همان عضوهای خط Submit‌اند.
  - هر فیلد فرم در 12 variant عضو record همان خانواده است.
  - choices هر unit جدا است.
  - Backoffice همان فرم را دارد.
- `V9SelectedOffersTests` (6 تست):
  - `selected` درست می‌آید.
  - پاسخ بدون `selected` دقیقاً برابر پاسخ `AncillaryOffers` است.
  - Backoffice همان انتخاب را نشان می‌دهد.
  - 409، 403، 404 و 410.
- `V9BatchTests`: یک Id برای خطوط Direct و گزینه‌دار، رد یک unit دو بار، P01–P20.
- `V9MealFamilyTests`: M03 در Submit، M05 در Resolve.
- `V8ResolveTests`، `V8SecurityTests` و `V8OptionsTests` روی جریان تازه نوشته شدند.
- `V9FormsTests` حذف شد، چون مسیر آن حذف شد.

**شواهد (2026-10-26، روی کد نهایی):**

| فرمان | نتیجه |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 خطا، 61 هشدار، همان هشدارهای baseline |
| `dotnet test tests/AeroTech.Ancillary.Shopping.Tests` | 120 کشف‌شده، 120 قبول |
| `dotnet test tests/AeroTech.Ancillary.Domain.ConformanceTests` | 264 کشف‌شده، 264 قبول |
| `dotnet test tests/AeroTech.Ancillary.Application.AcceptanceTests` | 232 کشف‌شده، 232 قبول |
| `ef migrations has-pending-model-changes` (AncillaryDbContext) | No changes |
| Hash فایل‌های SeatMap | `cf4211…e39d`، بدون تغییر. پوشه‌های Reservation دست نخوردند |
| خط‌پایان و قاعده‌ها | همه فایل‌ها CRLF‌اند، جز پنج فایل `docs/Shopping/FrontEnd` شما. کامنت ندارند. `Channel` فقط در fake تست قبلی است |

**هنوز انجام نشده:**
- اجرای زنده R3، چون توکن معتبر ندارم.
- فایل 05 بسته (پرامپت Frontend) هنوز `Options/Forms` را شرح می‌دهد. فایل شماست و تغییرش ندادم.

## 8. R4 (2026-10-26، `reports/V9-R4-QUESTIONS-AND-CONFLICTS.md`، مالک: «اجرا کن»)

**تغییرات:**
- هر unit فقط یک `selectionId` دارد. `quantityOptions` حذف شد.
- `quantityPolicy {selectable, min, max}` بازه واقعی را پس از محدودیت موجودی نشان می‌دهد.
- Token `d_` به جای `q`، بازه را امضا می‌کند (`qn`، `qx`).
- خط Direct در Submit: `{selectionId, quantity?}`.
  - اگر `quantity` نیاید، min حساب می‌شود.
  - تعداد بیرون از بازه ← `selections[i]: …` با 422/16821.
  - `quantity` بدون `selectionId` یا کوچک‌تر از 1 ← 400.
- Resolve و GetBySubmitId تغییری نکردند.

**شواهد (2026-10-26، روی کد نهایی):**

| فرمان | نتیجه |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 خطا. هشدارها همان مجموعه baseline‌اند |
| Shopping / Domain / Application | 122/122، 264/264، 232/232 (برابر تعداد کشف‌شده) |
| Hash فایل‌های SeatMap | `cf4211…e39d`، بدون تغییر |

**اجرای زنده:**
- سطح OtaPanel، host خودم روی 4146، با Token شما و Offer `1|1|1;…24283…;3|1551571415767973888||70|1792915276`. خروجی‌ها در scratch `v9/live-r4-otapanel` است.
- همه 11 مرحله همان نتیجه‌ای را دادند که انتظار می‌رفت:

| مرحله | نتیجه |
|---|---|
| Shopping | 200. 52 unit `selectionId` دارند و 38 unit `options`. `quantityOptions` در پاسخ نیست. Meals: A11 و A12 با 40 unit. Priority: A23 با 10 unit |
| Submit با تعداد بیرون از بازه | 422/16821: `quantity must be between 1 and 3` |
| Submit با quantity کنار reference، یا یک Id دو بار | 400، بدون نوشتن ردیف |
| Submit درست | 200 `{selectionId, expiresAtUtc}`. یک ردیف snapshot و 6 خط ثبت شد |
| GetBySubmitId | 200. هر 6 unit انتخاب‌شده `selected` دارند |
| GetBySubmitId با Id ناشناخته | 404/16819 |
| مسیر قدیم `Options/Forms` | 405 |
| Resolve | 200 با 6 آیتم |
| Resolve با شکل قدیم `{ids}` | 400 |
| Submit بدون Token | 401 |

- host خودم پس از اجرا متوقف شد و پورت 4146 آزاد است.

## 9. R5 (2026-10-26، `reports/V9-R5-QUESTIONS-AND-CONFLICTS.md`، مالک: «با پیشنهادها موافقم»)

| Route | Body | پاسخ |
|---|---|---|
| `POST {Surface}/v1/AncillaryOffers` | `{offerId}` | هر unit: `offerItemId` (`d_` یا `r_`) و، اگر گزینه لازم باشد، `options: {<family>: {<field>: descriptor}}` |
| `POST {Surface}/v1/AncillaryOffers/Price` | `{offerId, selections: [{offerItemId, quantity?, options?: {<family>: {…}}}]}` | `{selectionId: "s_…", expiresAtUtc}` |
| `GET {Surface}/v1/AncillaryOffers/Price/{selectionId}` | — | همان پاسخ `AncillaryOffers`، با `selected: {quantity, options?}` روی unitهای انتخاب‌شده |
| `POST Service/v1/AncillaryOffers/Resolve` | `{selectionId}` | همان شکل قبلی. `offerItemId` هر آیتم همان Idی است که در Price فرستاده شده. `selected` همان شکل GetBySubmitId را دارد |

- **نام‌ها:**
  - `selectionId` فقط Id سبد است. `offerItemId` نام هر قلم است در هر سه مرحله (NDC: `OfferItemID`؛ Duffel: `available_services[].id`).
  - خط Price دقیقاً همان نام‌های unit را دارد: `offerItemId` و `options`.
- **نام مسیرها:** از الگوی هم‌خانواده AirAvail گرفته شد:
  - AirAvail: `POST {S}/v1/FlightOffers` ← `POST {S}/v1/FlightOffers/Price`. مسیر Service به دستور مالک `Resolve` نام گرفت.
  - همین با Amadeus «Flight Offers Price» و NDC `OfferPrice` هم‌خوان است.
  - مسیرهای `Options` و `Ancillaries/Selections/ResolveGrouped` حذف شدند. Pin مسیرها به‌روز شد و تعداد 155 ماند.
- **ذخیره:**
  - هر خط `OfferItemId` فرستاده‌شده را نگه می‌دارد. کلید خط `(SelectionSnapshotId, Sequence)` است و Id داخلی `l_` حذف شد.
  - Migration تأییدنشده R3 حذف و یکجا دوباره ساخته شد: `20261025231956_SelectionSnapshotLines`. روی dev اعمال شد و ردیف‌های آزمایشی snapshot پیش از آن پاک شد.
- **پیام‌های Resolve** حالا مثل Price با `selections[i]: …` می‌آیند.
- **نام‌های داخلی** به دستور مالک هم‌نام endpointها شدند. DTOهای خروجی همان نام قبلی را دارند:
  - `SubmitSelectionOptions*` ← `PriceAncillaryOffers*`، مثل `OtaPriceFlightOffersRequest` در AirAvail.
  - `SelectionOptionsLine` ← `AncillarySelectionLine`.
  - `ResolveGroupedSelections*` ← `ResolveAncillarySelection*`.
  - `GetSelectedAncillaryOffers*` ← `GetPricedAncillaryOffers*`.

**شواهد (2026-10-26، روی کد نهایی):**

| فرمان | نتیجه |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | 0 خطا. هشدارها همان مجموعه baseline‌اند |
| Shopping / Domain / Application | 122/122، 264/264، 232/232 |
| `ef migrations has-pending-model-changes` | No changes |
| Hash فایل‌های SeatMap | `cf4211…e39d`، بدون تغییر |
| زنده OtaPanel (host خودم 4146، Token تازه شما) | همه 11 مرحله همان نتیجه مورد انتظار را دادند. Price یک Id و 6 خط ثبت کرد. GetBySubmitId روی 6 unit `selected` نشان داد. `offerItemId`های پاسخ Resolve دقیقاً همان‌هایی‌اند که فرستاده شدند. مسیرهای قدیم (`Options`، `Details`) 404 می‌دهند. host پس از اجرا متوقف شد |
