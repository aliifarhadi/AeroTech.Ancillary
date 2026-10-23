# قرارداد تجاری و نمای کلی: Ancillary Offers → Options → Resolve

**AeroTech — v8 LEAN SIGNED — 2026-10-23**  
**وضعیت:** طرح مرجع برای تأیید/اجرای مرحله‌ای؛ هرجا شکاف تأییدشده سورس وجود دارد، آن محصول **غیرفعال** می‌ماند، نه اینکه ایجنت رفتار جدید اختراع کند.  
**این سند Overview مرجع Pack v8 است.** تمام جزییات Required/Optional/Mapping و قواعد آزمون در اسناد `02` تا `07` همین Pack آمده‌اند؛ هیچ فایل خارج از Pack مرجع موازی نیست.

## 1. مسئله و تجربه خرید مورد انتظار

سرویس Ancillary برای یک `offerId`، خدمات قابل فروش را پیشنهاد می‌دهد. UI می‌تواند مشابه Singapore Airlines دسته «Add-ons» داشته باشد: بیمه یک دکمه انتخاب، اضافه‌بار یک کارت با «View options»، و سپس صفحه تعداد چمدان برای هر مسافر در هر مسیر. **این فرم UX قرارداد Backend نیست**؛ ممکن است برای یک محصول Quantity-only، صفحه ثانویه در UI نمایش داده شود ولی Backend قبلاً شناسه‌های Qty=1/2 را داده باشد. تصاویر Owner: `addon-list.png` و `baggage-details.png` صرفاً مرجع UX هستند، نه مقادیر سقف یا قیمت Domain.

هدف ساده است:

```text
POST /{Surface}/v1/AncillaryOffers {"offerId":"..."}
 │
 ├─ بدون ورودی → d_ signed selectionId از Shopping ──────────────────┐
 │                                                                  │
 ├─ فقط تعداد محدود → quantityOptions[{quantity,selectionId,...}] ──┤
 │                                                                  │
 └─ ورودی تخصصی → signed reference → GET/POST {Family}/Options ──────┤
                                    │                              │
                                    └─ Snapshot تخصصی + s_ ID ─────┤
                                                                   v
              POST /Service/v1/Ancillaries/Selections/ResolveGrouped {"ids":[...]}
                                                                   │
                                                                   v
                   {baggages:[...],meals:[...],assistedTravel:[...],...}
                                                                   │
                                                                   v
                           Ordering: مطابقت و ثبت خدمات در سفارش
```

**حدود فاز:** فقط PreOrder Shopping از OfferId، Options، Resolve. نه Hold، Confirm، Cancel، Stock Commit، EMD، تغییر Ordering یا اتصال پس از Order. هیچ Session/Reservation Aggregate جدیدی لازم نیست. SeatMap در این فاز تغییر نمی‌کند.

## 2. چرا برش بر اساس Scope است، نه Quantity؟

تعیین ردیف Shopping بر عهده **Selection Unit** است. صرف تعداد یا نوع فرم، ردیف اضافه تولید نمی‌کند.

| محدوده انتخاب | مرجع Domain فعلی | چند ردیف برای دو مسافر، سه Flight، دو Portion؟ | نمونه |
|---|---|---|---|
| `PerOrder` | Order × no traveller | 1 به‌ازای هر محصول/Rate | خدمت واقعاً Order-scoped؛ بیمه صرفاً مثال UX و در Domain فعلی وجود ندارد |
| `PerTraveller` | Journey × traveller | 2 | یک خدمت برای سفر هر نفر |
| `PerTravellerPerPortion` | Portion × traveller | 4 | اضافه‌بار برای رفت/برگشت؛ ممکن است هر Portion بیش از یک Flight داشته باشد |
| `PerTravellerPerFlight` | Sector × traveller | 6 | ویلچر یا خدمات هر مسافر/Flight |
| `PerFlight` | Sector بدون traveller | 3 *فقط پس از پشتیبانی واقعی Engine* | نوع احتمالی Flight-scoped |
| `PerPortion` | Portion بدون traveller | 2 *فعلاً unsupported* | نوع احتمالی Portion-scoped |
| `PerJourney` | Journey بدون traveller | 1 *فعلاً unsupported* | نوع احتمالی Journey-scoped |

در `CoveragePlanner` فعلی، چهار حالت اول پشتیبانی دارند. **سه حالت آخر را صرفاً برای جلوگیری از سوءفهم نام برده‌ایم؛ ساخت پشتیبانی برای آن‌ها مجاز نیست** مگر تصمیم مستقل Owner.

`unit` = هویت دقیق یک تصمیم خرید؛ `coverage` = مناطقی که خدمت روی آن‌ها اعمال می‌شود. برای Portion ممکن است ردیف `ADT-1 + B1` روی دو Flight باشد؛ یک قیمت و یک SelectionID دارد، نه دو قیمت. شناسه Unit با `pricingRateId` ترکیب می‌شود؛ دو مسافر یا دو Flight/Portion مختلف باید شناسه متفاوت داشته باشند، ولو قیمتشان یکی باشد.

## 3. سه رفتار UI مستقل از Scope

**Direct:** هیچ Quantity متغیر یا فیلد تخصصی وجود ندارد؛ Shopping مستقیم ID `d_` امضاشده صادر می‌کند. مقدار واقعی 1 در Resolution است و Rate واقعی شامل Free=0 است.

**Quantity:** فیلد تخصصی ندارد و فقط Quantity صحیح و قابل enumerate تغییر می‌کند؛ در همان **یک ردیف Unit**، `quantityOptions[]` صادر می‌شود. هر Quantity انتخاب‌شده `d_` **مستقل و امضاشده** دارد. Quantity صفر یعنی عدم انتخاب و ID ندارد. نمونه هواپیمایی ۰/۱/۲ چمدان، **سقف عمومی نیست**. مقدار min/max واقعی از Provision و Quantity Assessment می‌آید.

**Typed:** یک یا چند فیلد تخصصی دارد (مثلاً SSR، اندازه حیوان یا وقت خدمات فرودگاهی)؛ Shopping فقط `reference` امضاشده می‌دهد. `GET Options` فرم اختصاصی همان Variant را نشان می‌دهد. `POST Options` با Request تخصصی مقدارها را کنترل می‌کند، Snapshot موقت ذخیره می‌کند و شناسه کوتاه تصادفی `s_` می‌دهد. اگر Typed هم Quantity دارد، Quantity داخل همان POST و Snapshot است.

### رفتار Qtyهای بسیار زیاد یا اندازه پیوسته

Qtyهای صحیح طبق Range واقعی Provision enumerate می‌شوند؛ هیچ سقف ۲ یا ۶ دلخواه در Domain نگذارید. برای مقدار پیوسته مثل وزن اندازه‌گیری‌شده (`A03`) Typed Options استفاده می‌شود؛ چنین مقادیری نباید با enumeration مصنوعی تولید شوند. هرگاه محدودیت Response/Page اندازه عملی را غیرقابل اجرا کند، **قبل از کد فهرست شمارا و وضعیت محصول را با Owner روشن کنید**؛ بدون اختراع سقف جدید.

## 4. زنجیره اعتبار: امضا و Eligibility کجا رخ می‌دهد؟

در این قرارداد، Hash یا Base64URL ساده برای اثبات صدور کافی نیست؛ دو شناسه Stateless فقط با HMAC صادر می‌شوند:

- `r_`: مرجع جهت Options؛ صادرشده **فقط** پس از Shopping واجد شرایط و دارای نرخ کامل. دارای `pricingRateId`, `unit`, `offerBinding`, `salesBinding`, `expiresAt`, `purpose`؛ دارای امضای HMAC-SHA256.
- `d_`: انتخاب نهایی Direct یا Quantity؛ همان تعهدها به‌علاوه مقدار Quantity؛ صادرشده فقط در Shopping واجد شرایط. **بدون جدول و Snapshot**.
- `s_`: شناسه کوتاه تصادفی Snapshot ثبت‌شده برای **Typed**؛ قابل حدس نیست و از طریق DB و انقضا خوانده می‌شود، نه از طریق Payload آزاد Client.

**HMAC اثبات اصالت صدور است؛ نه اجازه خرید دائمی.** در Shopping باید Eligibility (نوع مسافر، مسیر/Bound/Flight، POS واقعی، Rate و Currency، شرایط فروش، قوانین Provision)، قیمت قطعی، Quote/Approval و فعال‌بودن Rate/Definition/Provision ارزیابی شود. Token فقط برای Candidate واقعاً مجاز صادر می‌شود؛ RateId از کاربر گرفته نمی‌شود.

`POST Options` باید HMAC و TTL مرجع را بررسی کند، User/Channel مجاز را با context امضاشده تطبیق دهد، Type/Unit/Rate و گزینه‌های انتخاب‌شده را با **Domain معتبر خود Ancillary** تطبیق دهد و در صورت لزوم از مسیر ارزیابی موجود برای حصول اطمینان از شرط انتخاب استفاده کند. **نباید به‌صورت صوری بگوید «امضا درست است پس هر انتخابی درست است».** اگر شرطی بدون source مورد اعتماد قابل بررسی نباشد، **Snapshot صادر نکنید**. `OfferId`/`POS`/`Country` وارد Body این Endpoint نمی‌شوند.

`Resolve` فقط دقت `ids[]` و اصالت/انقضا یا Snapshot را بررسی کرده و Definition/Provision/PricingRate واقعی داخلی را می‌خواند. **نه Shopping مجدد، نه Eligibility تجاری، نه سرویس خارجی و نه Reprice**. در مرحله Ordering، تطبیق `offerBinding` و `salesBinding` با Offer و Actor واقعی سفارش انجام می‌شود؛ Service Caller احراز هویت‌شده است. POS Country به Body Resolve اضافه نمی‌شود.

**محدودیت صریح:** چون Resolve به Offer Service وصل نمی‌شود، HMAC به‌تنهایی نمی‌داند Offer بعد از صدور عوض شده یا نه. TTL کوتاه و تطبیق Ordering این شکاف را کنترل می‌کنند؛ اگر یک قانون فروش ابطال فوری بخواهد، این فاز بدون تصمیم جدید آن ادعا را ندارد.

## 5. Pricing و Free با حداقل پیچیدگی

هر انتخاب قابل فروش، **PricingRateId معتبر** دارد حتی مبلغ صفر. `PricingRateId` شناسه نرخ است؛ `PricingId` شناسه Aggregate دارای آن Rate، نه هم‌معنا. برای Free، مطابق تصمیم Owner، Authoring از مسیر **Paid + Filed با Rate واقعی صفر** استفاده می‌کند؛ اثر آن بر disposition/document باید در Gate0 با سورس بررسی شود؛ از `ProvisionId` برای جعل RateId استفاده نمی‌کنیم و `PriceAssessor.Free` را به مسیر ساخت ID ساختگی تبدیل نمی‌کنیم.

**D1 مصوب:** در این فاز فقط خدماتی عرضه شوند که مبلغ نهایی بدون FX تغییر‌یافته، Quote، Fee سطح سفارش یا وزن/Tax محاسبه‌نشده قابل استخراج باشد. همه پول‌ها از نرخ DB و تعدادی که ID مشخص می‌کند ساخته می‌شوند؛ قیمت از Client پذیرفته نمی‌شود.

مثال EUR در این پروژه `currencyId:47` است (بر اساس اعلام Owner؛ Gate0 باید از Catalog تأیید کند). عدد 978 شناسه داخلی ارز نیست.

## 6. قرارداد ساده Shopping — سه Surface

```
POST /Backoffice/v1/AncillaryOffers
POST /OtaPanel/v1/AncillaryOffers
POST /Api/v1/AncillaryOffers
{ "offerId": "OFFER-EXAMPLE" }
```

یک Engine مشترک داریم؛ خروجی هر Surface می‌تواند عناوین/خلاصه متفاوتی داشته باشد اما **خانواده‌ها، اسامی نوعی و معنای ID/Scope/Price یکی‌اند**. هیچ `OrderId`, `ExpectedVersion`, `PointOfSale` یا `Country` از درخواست لازم نیست.

Envelope مورد انتظار:

```json
{
  "offerId":"OFFER-EXAMPLE", "currencyId":47,
  "baggages":[],"meals":[],"pets":[],"assistedTravel":[],
  "airportServices":[],"priorityServices":[],"connectivity":[],
  "upgrades":[],"extraSeats":[],"travelInsurance":[]
}
```

### 6.1 نمونه JSON یک پاسخ Shopping: A01 با ۲ مسافر × یک Portion (داده‌های مثال، نه نرخ واقعی)

```json
{
  "offerId":"OFFER-EXAMPLE", "currencyId":47,
  "baggages":[
    {"variantCode":"A01","title":"Additional checked bag","selectionScope":"PerTravellerPerPortion","unit":{"travellerRef":"ADT-1","portionRef":"B1"},"coverage":{"travellerRefs":["ADT-1"],"portionRefs":["B1"],"flightRefs":["B1:F1","B1:F2"]},"quantityPolicy":{"selectable":true,"min":1,"max":2,"unit":"Piece"},"unitPrice":{"currencyId":47,"amount":106.88},"details":{"chargeKind":"ExtraPiece","maxKgPerPiece":23},"quantityOptions":[{"quantity":1,"selectionId":"d_<signed-q1-pax1>","totalAmount":106.88},{"quantity":2,"selectionId":"d_<signed-q2-pax1>","totalAmount":213.76}]},
    {"variantCode":"A01","title":"Additional checked bag","selectionScope":"PerTravellerPerPortion","unit":{"travellerRef":"ADT-2","portionRef":"B1"},"coverage":{"travellerRefs":["ADT-2"],"portionRefs":["B1"],"flightRefs":["B1:F1","B1:F2"]},"quantityPolicy":{"selectable":true,"min":1,"max":2,"unit":"Piece"},"unitPrice":{"currencyId":47,"amount":106.88},"details":{"chargeKind":"ExtraPiece","maxKgPerPiece":23},"quantityOptions":[{"quantity":1,"selectionId":"d_<signed-q1-pax2>","totalAmount":106.88},{"quantity":2,"selectionId":"d_<signed-q2-pax2>","totalAmount":213.76}]}
  ],"meals":[],"pets":[],"assistedTravel":[],"airportServices":[],"priorityServices":[],"connectivity":[],"upgrades":[],"extraSeats":[],"travelInsurance":[]
}
```

این مثال مقدار ۲۳kg و €106.88 را **به‌عنوان fixture** از حال‌وهوای تصویر نمایش می‌دهد؛ هیچ قانون سراسری نداریم. یک ردیف برای هر `ADT × Portion` است، نه برای هر Leg آن Portion. با تغییر `coverageScope=Sector` محصول به ردیف مجزا برای هر Flight تبدیل می‌شود.

### 6.2 نمونه Order-scoped بدون Quantity

اگر محصول واقعی Order-scoped با نرخ صفر یا مبلغ ثابت وجود داشته باشد، دقیقاً یک ردیف با `unit:{"scope":"Order"}` و `selectionId:"d_..."` برمی‌گردد. مثال بیمه در تصویر **شاهد UX است، نه مجوز افزودن Insurance Domain**؛ بنابراین `travelInsurance:[]` تا تصویب تعریف واقعی باقی می‌ماند.

### 6.3 نمونه یک ردیف Meal رایگان با PricingRate واقعی (فقط Item، نه Envelope)

```json
{"variantCode":"A11","title":"Vegetarian special meal","selectionScope":"PerTravellerPerFlight","unit":{"travellerRef":"ADT-1","flightRef":"B1:F1"},"coverage":{"travellerRefs":["ADT-1"],"portionRefs":["B1"],"flightRefs":["B1:F1"]},"quantityPolicy":{"selectable":false,"min":1,"max":1,"unit":"Each"},"unitPrice":{"currencyId":47,"amount":0},"details":{"mealKind":"SpecialRequest","mealCode":"VGML","cateringLeadTimeMinutes":0},"selectionId":"d_<hmac-bound-free-rate>"}
```

`VGML` انتخاب خودکار تنها وقتی مجاز است که همان Product واقعاً MealCode ثابت داشته باشد؛ در غیر این صورت گزینه تخصصی باید پرسیده شود.

### 6.4 نمونه یک بخش Wheelchair تخصصی (فقط آرایه assistedTravel، نه Envelope)

Shopping:

```json
{"assistedTravel":[{"variantCode":"A15","title":"Wheelchair assistance","selectionScope":"PerTravellerPerFlight","unit":{"travellerRef":"ADT-1","flightRef":"B1:F1"},"coverage":{"travellerRefs":["ADT-1"],"portionRefs":["B1"],"flightRefs":["B1:F1"]},"quantityPolicy":{"selectable":false,"min":1,"max":1,"unit":"Each"},"unitPrice":{"currencyId":47,"amount":0},"details":{"assistanceKind":"Wheelchair","wheelchair":{"allowedSsrCodes":["WCHR","WCHS"],"leadTimeMinutes":120}},"reference":"r_<signed-wheelchair>","optionsEndpoint":"/OtaPanel/v1/AncillaryOffers/AssistedTravel/{reference}/Options"}]}
```

`GET Options` (گزینه‌ها و اعداد نمونه، فقط از Domain واقعی قابل استفاده):

```json
{"variantCode":"A15","fields":{"assistanceSsrCode":{"control":"select","required":true,"choices":[{"value":"WCHR","label":"Ramp assistance"},{"value":"WCHS","label":"Steps assistance"},{"value":"WCHC","label":"Cabin assistance"}]}}}
```

`POST Options`:

```json
{"assistanceSsrCode":"WCHS"}
```

Response:

```json
{"selectionId":"s_<opaque-random>","expiresAtUtc":"2026-10-23T16:30:00Z"}
```

گزینه‌ها از `AllowedSsrCodes` واقعی Definition می‌آیند؛ بودن هر سه WCHR/WCHS/WCHC در تمام ایرلاین‌ها فرض نمی‌شود. Snapshot آن کد، Unit و Rate معتبر را در Ancillary نگه می‌دارد.

### 6.5 نمونه Pet و AirportService

Pet: `r_` + فرم فقط `animalType,combinedWeightKg,carrierDimensions` و فیلدهای شرطی همان Variant، پس از POST یک `s_`. AirportService: اگر `airportId/facilityId` ثابت و نوبت لازم نباشد Direct `d_` است؛ اگر `timeWithOffset` برای Appointment لازم باشد `r_` و POST آن فیلد، `s_`. هیچ DTO با فیلدهای نامرتبط ارائه نمی‌شود.

## 7. قرارداد Options: تخصصی، ساده، بدون افزودن پارامتر فروش

```
GET  /{Surface}/v1/AncillaryOffers/{Family}/{reference}/Options
POST /{Surface}/v1/AncillaryOffers/{Family}/{reference}/Options
```

فقط برای Typed. برای Quantity-only **نه GET نه POST لازم است**؛ UI از `quantityOptions[]` انتخاب می‌کند. GET فقط تعریف فرم و گزینه‌های واقعی را برمی‌گرداند. POST فقط DTO همان Variant را می‌گیرد؛ نه OfferId/OrderId/POS/RateId/Price و نه یک `AncillarySelection` مشترک ۳۰ فیلدی.

حداقل Result POST: `selectionId`, `expiresAtUtc`؛ قیمت دقیق را Resolve از Rate فعال و Quantity ذخیره‌شده بازسازی می‌کند؛ POST Response **فقط** همین دو فیلد اعلام‌شده را دارد و فیلد اختیاری جدید بدون تصمیم Owner اضافه نمی‌شود. طرح مرجع فیلدبه‌فیلد در سند `02` آمده است.

## 8. ResolveGrouped: فقط IDs و نتیجه تخصصی

```
POST /Service/v1/Ancillaries/Selections/ResolveGrouped
{"ids":["d_<signed-baggage-q2>","d_<signed-meal-free>","s_<typed-wheelchair>"]}
```

Response (خلاصه ساختاری؛ جزئیات همه فیلدها در `02/03`):

```json
{
  "baggages":[{"selectionId":"d_<signed-baggage-q2>","variantCode":"A01","unit":{"travellerRef":"ADT-1","portionRef":"B1"},"definition":{"definitionVersionId":100,"serviceDefinitionRef":"BAG23"},"provision":{"provisionId":200,"coverageScope":"Portion"},"pricing":{"pricingId":300,"pricingRateId":400,"currencyId":47,"quantity":2,"unitAmount":106.88,"totalAmount":213.76},"details":{"chargeKind":"ExtraPiece","maxKgPerPiece":23}}],
  "meals":[{"selectionId":"d_<signed-meal-free>","variantCode":"A11","pricing":{"pricingRateId":401,"currencyId":47,"quantity":1,"unitAmount":0,"totalAmount":0},"details":{"mealCode":"VGML"}}],
  "assistedTravel":[{"selectionId":"s_<typed-wheelchair>","variantCode":"A15","unit":{"travellerRef":"ADT-1","flightRef":"B1:F1"},"pricing":{"pricingRateId":402,"currencyId":47,"quantity":1,"unitAmount":0,"totalAmount":0},"details":{"assistanceKind":"Wheelchair"},"selected":{"assistanceSsrCode":"WCHS"}}],
  "pets":[],"airportServices":[],"priorityServices":[],"connectivity":[],"upgrades":[],"extraSeats":[],"travelInsurance":[]
}
```

**نمونه‌های بالا schematic و عمداً کوتاه‌اند**؛ DTO نهایی برای هر ردیف شامل `sourceBinding:{offerHash,salesHash}` و همه فیلدهای تجاری لازم Ordering است (قرارداد فیلدبه‌فیلد 02). Body Resolve هیچ اطلاعات دیگری نمی‌پذیرد. Resolve باید Retry-safe باشد و Snapshot را consume/delete نکند.

## 9. Handoff: چه چیزی با چه کسی است؟

- Ancillary: می‌گوید این شناسه در Shopping واجد شرایط صادر شده، امضا و TTL معتبر است، Rate/Provision/Definition داخلی دارد و کاربر چه گزینه‌ای را ثبت کرده است.
- Ordering: `offerBinding` خروجی Resolve را با **Offer سفارش خودش** تطبیق می‌دهد؛ Unit/Beneficiary و `travellerRef` را با Travellerهای همان Offer/Order می‌سنجد؛ ارتباط `portionRef` با همه Flightهای Order را از دیتا خودش می‌سازد؛ اجازه افزودن تکراری همان Selection را نمی‌دهد؛ Price Snapshot نهایی پذیرفته‌شده در Order را ثبت می‌کند.
- نه پاسخ Resolve به‌معنای Hold است، نه HMAC به معنی صادرشدن Ticket/EMD. در فاز رزرو بعدی، هر Service جداگانه طبق Provider policy رزرو می‌شود.

## 10. انقضا، امنیت، و «یک‌بارمصرف»

- `r_` و `d_` در Payload امضاشده دارای Expiry هستند؛ بدون Expiry پذیرش ندارند.
- `s_` یک مقدار تصادفی قابل حدس‌نبودن است که در جدول دارای ExpiresAt ذخیره می‌شود. **هیچ اطلاعات پزشکی/PII در URL یا شناسه نرود.**
- Resolve Read-only است و Retry با همان ID تا قبل TTL جواب معتبر می‌دهد. **یکبارمصرف = یک تصمیم مشخص**، نه یک بار فقط قابل خواندن.
- Purge فیزیکی Snapshots با Job جدا در فاز بعدی است؛ Expiry **همین فاز** اعمال می‌شود. مقدار دقیقه TTL از Config/سیاست Owner می‌آید؛ `expiresAt` هرگز از اعتبار Offer بیشتر نیست.
- **Pricing Retired/Suspended، Provision غیرفعال، Definition غیرفعال یا نرخ فاقد والد Active نمی‌تواند شناسه جدید بگیرد؛ Resolve نیز انتخابی را که اکنون این وضعیت را دارد رد می‌کند، حتی اگر HMAC/TTL هنوز معتبر باشد. هیچ استثنای Read-Retired برای خرید جدید وجود ندارد.** این قرارداد درباره مشاهده تاریخچه Order قبلاً ثبت‌شده تصمیمی نمی‌گیرد.
- A17 تا حفاظت محرمانگی Snapshot تأییدنشده، GATED است. A18 اگر منبع معتبر لیست Infant/Guardian در Options وجود ندارد GATED است. A09 تا FlightFlow proof، A19 تا رفع mismatch، و Insurance تا Domain authoring واقعی.

## 11. مواردی که عمداً اجرا نمی‌شوند

- هیچ engine تازه‌ای، general DynamicForm engine یا Rule DSL تازه‌ای، Provider-specific fake eligibility، Order Reader، HMAC revocation registry، token persistence برای d_/r_، جدول QtySnapshot، Event، Saga، Reservation یا Payment logic.
- هیچ تاریخ انقضای غیرقابل توضیح یا TTL عددی ثابت از مثال‌ها وارد Domain نشود.
- هیچ product/Variant جدید از مثال UX ساخته نشود؛ TravelInsurance فقط آرایه رزرو‌شده خالی است.
- هیچ ادعای اینکه «هر مسافر در هر Flight حتماً با 23kg و Qty2» یا «هر خدمات سلامت قابل فروش فوری» نداریم.

## 12. معیار Done از دید محصول

۱) Shopping هر سه Surface با OfferId واقعی و ردیف درست Scope/Quantity؛ ۲) هیچ RateId/Unit دست‌کاری‌شده شناسه معتبر نمی‌سازد؛ ۳) Quantity از Shopping بدون POST و بدون snapshot کار می‌کند؛ ۴) POST Typed، داده صحیح را می‌سنجد و Snapshot می‌نویسد؛ ۵) Resolve مختلط Direct/Qty/Typed تنها با ids[] خروجی تخصصی مورد نیاز Ordering را می‌دهد؛ ۶) Free صفر Rate واقعی؛ ۷) expired/retired/mismatched tenant/CHD/ADT رد؛ ۸) SeatMap test/hash بدون تغییر؛ ۹) endpointهای legacy حساس خارج از محدوده حفظ می‌شوند؛ ۱۰) صدور یا مصرف Snapshot به معنی Hold نیست.

**اسناد تکمیلی:** `02` همه DTO و خطاها؛ `03` نگاشت همه ۲۴ Variant؛ `04` فرمت ID و Snapshot؛ `05` Handoff و Acceptance؛ `06` بنچ‌مارک؛ `10..15` مراحل اجبار ایجنت.
