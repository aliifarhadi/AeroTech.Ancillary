# V9-R3 — Options داخل Shopping، یک Id برای Submit، و GetBySubmitId — برداشت، نظر و سؤال‌ها

| | |
|---|---|
| شاخه | `feat/ancillary-offers-v9` روی `324e329`، R1 و R2 commit نشده‌اند |
| درخواست مالک (2026-10-26) | دو پیشنهاد. هیچ کدی نوشته نشده است |
| دامنه | فقط Shopping غیر Seat. SeatMap، مسیرها، شناسه‌ها و Hold آن دست نمی‌خورند |

## 1. برداشت من

1. `POST {S}/v1/AncillaryOffers`: هر unit که انتخاب لازم دارد (امروز `reference`)، فرم گزینه‌هایش را هم در خود دارد. `POST .../Options/Forms` حذف می‌شود.
2. `POST {S}/v1/AncillaryOffers/Options` (Submit): گزینه‌ها را **با همان نام‌ها و همان ساختار** فرمِ پاسخ Shopping می‌گیرد و برای **کل سبد فقط یک Id** برمی‌گرداند.
3. `GetBySubmitId` (جدید): Id را می‌گیرد و پاسخی **هم‌شکل AncillaryOffers** می‌دهد که انتخاب‌های ثبت‌شده را هم دارد. فقط برای وقتی است که صفحه Frontend رفرش شود.
4. `POST Service/v1/Ancillaries/Selections/ResolveGrouped`: فقط همان یک Id را می‌گیرد. **خروجی تغییر نمی‌کند.**

نتیجه: اگر Resolve فقط یک Id بگیرد، Submit باید **همه** انتخاب‌های سبد را بگیرد، نه فقط انتخاب‌های گزینه‌دار. یعنی Direct (`selectionId`) و انتخاب تعداد (`quantityOptions[].selectionId`) هم در Submit می‌آیند (Q1).

## 2. الگوی موجود (شواهد از کد)

| موضوع | امروز | شاهد |
|---|---|---|
| الگوی هم‌خانواده | AirAvail: `POST {Api\|OtaPanel}/v1/FlightOffers` فهرست می‌دهد. `POST .../FlightOffers/Price` انتخاب را می‌گیرد و **یک** offerId قیمت‌گذاری‌شده برمی‌گرداند. `POST Service/v1/FlightOffers/Details` با همان یک Id کل Offer را دوباره می‌سازد. Ordering و Ancillary فقط همان Id را نگه می‌دارند. این دقیقاً همان الگوی پیشنهاد شماست: Submit مثل Price، GetBySubmitId مثل Details، Resolve فقط با Id | `AirAvail@590e21e`، `OtaPanelController.cs:27-38`، `Service2Service/ServiceController.cs:13` |
| فرم امروز | فقط از `Options/Forms`. نام فیلدها همین حالا camelCase نام اعضای Submit است (`bagRef`، `measuredWeightKg`، `assistanceSsrCode`، `carrierDimensions`، …) | `Shopping/Options/TypedSelectionForm.cs:16-52`، `Application/.../SubmitSelectionOptions/*SelectionOptions.cs` |
| جایی که کاربر باید حدس بزند | ۱) نام عضو خانواده در Submit (`baggage`، `assistedTravel`، …): فرم فقط `family` = `"AssistedTravel"` می‌دهد. ۲) ساختار داخل کنترل `dimensions`: فرم `{lengthCm, widthCm, heightCm}` را نمی‌گوید. ۳) جواب Shopping و فرم دو درخواست جدا هستند | `SelectionOptionsFormDto`، `SelectionControls.Dimensions`، `SelectedDimensionsInput.cs` |
| Id امروز | هر خط Submit یک `s_` جدا دارد (`SelectionSnapshots`، یک ردیف برای هر خط، `SelectedJson` اجباری، offerId ذخیره نمی‌شود). Direct یک `d_` امضاشده است که در DB ذخیره نمی‌شود. Resolve لیست `{ids}` از `s_` و `d_` می‌گیرد | `Domain/SelectionSnapshotAggregate/SelectionSnapshot.cs`، `SelectionSnapshotConfiguration.cs:11-21`، `Shopping/Resolution/SelectionResolver.cs` |
| حجم | در اجرای زنده OtaPanel (2026-10-25)، جواب Shopping 29,080 بایت بود و 28 unit گزینه‌دار داشت. همه فیلدهای فرم این 28 unit با هم 2,820 بایت‌اند (حدود ۱۰٪). بیشتر 14,199 بایتِ پاسخ Forms، تکرار خود `reference` بود (9,352 بایت) که در پیشنهاد ۱ تکرار نمی‌شود | scratch `v9/measure_forms.py` روی `v9/live-r1-otapanel` |
| مصرف‌کننده | Frontend Backoffice (`AeroTech.Backoffice.V1@47d6778`) هنوز fixture API گروه‌بندی قدیمیِ برگردانده‌شده را می‌خواند و هیچ مسیر v9 را صدا نمی‌زند. OtaPanel اصلاً ancillary ندارد. Ordering متوقف است. پس تغییر قرارداد امروز هیچ مصرف‌کننده‌ای را نمی‌شکند | `features/booking/ancillaries/api/ancillary-api.ts`، `server/services/booking/ancillary-shopping.ts` |
| SeatMap | Shopping همین حالا Seat را حذف می‌کند (`OfferFamilyFilters.Services`). Seat شناسه، Resolve و Hold خودش را دارد | `Shopping/Offers`، `Query/Shopping/SeatMaps/**` (hash `cf4211…e39d`) |

بنچ‌مارک، از حافظه و دوباره بررسی‌نشده: در NDC، `OfferPrice` (یا `ServiceList`) انتخاب‌ها را می‌گیرد و یک Offer با یک `OfferID` می‌دهد، و `OrderCreate` فقط به آن ارجاع می‌دهد. در Amadeus، `Flight Offers Price` با `include=bags,other-services` گزینه‌ها را کنار Offer می‌دهد، و Offer قیمت‌گذاری‌شده به‌صورت یک تکه به `Flight Create Orders` می‌رود.

## 3. نظر من

**با هر دو پیشنهاد موافقم.** دلیل‌ها:
- همان الگوی AirAvail است: انتخاب، یک Id، ساخت دوباره با همان Id.
- Frontend یک درخواست کمتر دارد و هیچ نگاشتی را حدس نمی‌زند.
- Resolve و Ordering فقط یک Id نگه می‌دارند.
- رفرش صفحه بدون ذخیره در مرورگر کار می‌کند.

هزینه‌ها (همه داخل همین repo):

| مورد | توضیح |
|---|---|
| Schema | جدول پدر تازه، یک ردیف برای هر Submit: Id، offerId، زمان ساخت، انقضا. ردیف‌های امروز `SelectionSnapshots` خطوط فرزند آن می‌شوند: FK به پدر، و `SelectedJson` برای خط Direct خالی می‌ماند. طبق قاعده ۳ CLAUDE.md، لیست در جدول فرزند است. یک migration لازم است و داده dev پاک می‌شود |
| مسیرها | در هر سطح، `Options/Forms` حذف و `GetBySubmitId` اضافه می‌شود. Pin مسیرها 155 می‌ماند |
| Shopping | Listing فرم هر unit گزینه‌دار را می‌سازد. Definition، Provision و مسافران همین حالا در دست‌اند (`TypedSelectionForm.Fields`)، پس بارگذاری اضافه‌ای ندارد |
| Submit | خطوط Direct هم وارد می‌شوند: امضا، انقضا و `ob` آن‌ها مثل امروزِ Resolve بررسی می‌شود. قاعده‌های سبد (یک unit دو بار، دو Meal از یک family که در R2 به Resolve اضافه شد) هنگام Submit اجرا می‌شوند، با همان قاعده Shopping که Resolve هم صدا می‌زند |
| GetBySubmitId | Offer را با offerId ذخیره‌شده دوباره بارگذاری می‌کند (یک فراخوانی AirAvail، مثل AncillaryOffers) و انتخاب‌ها را روی unitهای منطبق می‌گذارد |
| Resolve | ورودی عوض می‌شود. خروجی همان است |

## 4. سؤال‌ها

### Q1 — Submit همه سبد را می‌گیرد؟

- الف (پیشنهاد): بله. هر خط یکی از این دو است: `{ selectionId }` برای Direct یا یکی از `quantityOptions`، یا `{ reference, <گزینه‌ها> }` برای unit گزینه‌دار. Resolve دیگر `d_` یا `s_` تکی نمی‌گیرد.
- ب: Submit فقط خطوط گزینه‌دار را می‌گیرد و Resolve هم Id سبد و هم `d_`ها را می‌گیرد. این با «Resolve فقط Id مرحله Submit» نمی‌خواند.

Answer: الف (مالک، 2026-10-26: «با پیشنهادهایت موافقم»)

### Q2 — شکل گزینه‌ها در پاسخ Shopping و در Submit

هر دو گزینه کنترل `dimensions` را با زیرفیلدهای `lengthCm`، `widthCm` و `heightCm` توصیف می‌کنند تا ساختار داخلی هم حدس زده نشود.

- الف (پیشنهاد): ساختار آینه‌ای، با اعضای typed فعلی. پاسخ Shopping برای unit:
  ```json
  { "reference": "r_…", "options": { "assistedTravel": { "assistanceSsrCode": { "control": "select", "required": true, "choices": [{ "value": "WCHR", "label": "WCHR" }] } } } }
  ```
  خط Submit دقیقاً همان کلیدها را دارد و به جای هر توصیف، مقدار آن را:
  ```json
  { "reference": "r_…", "assistedTravel": { "assistanceSsrCode": "WCHR" } }
  ```
  - Recordهای typed، `Disallow` عضو ناشناخته، Validatorهای 400 و Swagger همه می‌مانند.
  - Frontend فقط `options` را کپی می‌کند و جای توصیف‌ها مقدار می‌گذارد.
- ب: یک شیء عمومی `options` که کلیدهایش نام فیلدهاست: `{ "reference": "r_…", "options": { "assistanceSsrCode": "WCHR" } }`. خانواده از خود Reference خوانده می‌شود.
  - برای Frontend کمی ساده‌تر است.
  - ولی در RestApi نوع ثابت ندارد: Swagger شکل را نشان نمی‌دهد. خطای شکل از 400 Validator به 422/16821 سرویس منتقل می‌شود، چون خانواده پیش از خواندن Reference معلوم نیست.

Answer: الف (مالک، 2026-10-26)

### Q3 — Id سبد و خروجی Resolve

خروجی Resolve برای هر خط فیلد `selectionId` دارد.

- الف (پیشنهاد):
  - Submit جواب `{ selectionId: "s_…", expiresAtUtc }` می‌دهد. این تنها Id است که Frontend نگه می‌دارد.
  - Resolve ورودی `{ selectionId }` را می‌گیرد، به جای `{ ids: [] }`.
  - هر خط ذخیره‌شده Id داخلی خودش را دارد: همان Id امروزِ ردیف، با پیشوند جدا مثل `l_`. این Id در `selectionId` هر آیتم خروجی Resolve می‌آید، تا Ordering برای هر خدمت یک کلید یکتا داشته باشد. شکل خروجی ثابت می‌ماند.
- ب: ورودی `{ ids: [] }` بماند و فقط یک Id سبد در آن بیاید. `selectionId` هر آیتم خروجی هم همان Id سبد باشد. در این حالت Ordering خطوط را فقط با unit از هم جدا می‌کند.

Answer: الف (مالک، 2026-10-26)

### Q4 — GetBySubmitId: مسیر و رفتار وقتی یک انتخاب دیگر عرضه نمی‌شود

- مسیر پیشنهادی: `GET {S}/v1/AncillaryOffers/Options/{selectionId}`. Id کوتاه است (24 کاراکتر) و offerId در URL نمی‌آید.
- رفتار پیشنهادی:
  - Offer با offerId ذخیره‌شده و دامنه فروش خود فراخواننده دوباره ساخته می‌شود.
  - unit منطبق با هر خط عضو `selected` می‌گیرد: مقدار گزینه‌ها و تعداد. انطباق با rate، scope، مسافر و coverage خط است.
  - تطبیق دامنه فروش (`sb`، 403) و انقضا (410) مثل امروز بررسی می‌شوند.
- اگر یک خط با هیچ unit عرضه‌شده‌ای منطبق نشود (قیمت یا قاعده تغییر کرده):
  - الف (پیشنهاد): کل درخواست 409/16820 می‌گیرد، مثل همه‌یا‌هیچِ Resolve. Frontend دوباره AncillaryOffers را صدا می‌زند و کاربر از نو انتخاب می‌کند.
  - ب: پاسخ 200 بماند. خطوط پیدا‌نشده در فهرست جدا `withdrawnSelections` بیایند.

Answer: مسیر پیشنهادی و الف (مالک، 2026-10-26)
