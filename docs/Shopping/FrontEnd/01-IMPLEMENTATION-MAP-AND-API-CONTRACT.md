# AeroTech Ancillary — UI Grouping + Batch Options

**نسخه:** v9.0 / 2026-10-24 / مصوب از نظر هدف و حدود توسط مالک  
**مرجع کد ثابت برای این اصلاح:** `aliifarhadi/AeroTech.Ancillary`, branch `feat/ancillary-offers-v8`, commit `324e32999bac6a3a9dfd69604dbe9e31272c43f0`.  
**مرجع اجرایی برای تغییر حاضر:** **همین فایل**. اسناد 02 و 03 فقط دستور اجرا/دلیل تصمیم و فایل 04 تست پذیرش همین قراردادند. هیچ مراجعه‌ای به Pack v6/v7/v8 برای پرکردن ابهام مجاز نیست؛ کد HEAD بالا مرجع بخش‌های *بدون تغییر* است.

## 1. هدف و مرز

دو تغییر؛ نه بیشتر:
1. Response `POST /{Surface}/v1/AncillaryOffers` به شکلی باشد که **هر Variant یک کارت** و زیر آن **Products و Selection Units مستقل** قرار گیرند؛ هر Unit همان Price، Coverage، Direct ID / Quantity Options / Typed Reference قبلی را حفظ کند.
2. برای شش خانواده‌ای که اکنون Options تخصصی دارند، **Batch Forms** و **Batch Submit** افزوده شود تا هر گروه چند مسافر/پرواز با یک بار بارگذاری Offer پردازش شود.

**ممنوع:** تغییر Shopping Engine، Evaluation، Quantity logic، Rule/Eligibility، Pricing/FX، HMAC/JWS و signed ID format، Snapshot schema یا TTL، ResolveGrouped، Order integration، Reservation/Hold/Confirm، SeatMap (به‌جز تغییر کاملاً ناگزیر برای کامپایل که در این Scope انتظار نمی‌رود)، ساخت `DynamicFormEngine` جدید، جدول/Cache/Queue/Service جدید، ایجاد Variant جدید، تغییر عملکرد gated A09/A17/A19/Insurance.

**بازار هدف:** سه Surface موجود `Backoffice`، `OtaPanel` و `Api` با Auth/Enum representation موجود؛ نه Surface چهارم.

## 2. شواهد سورس و تطبیق تغییر

| قابلیت | فایل/کلاس واقعی در HEAD | کار موردنیاز |
|---|---|---|
| ردیف‌های فروش | `Shopping/Offers/AncillaryOfferRow.cs`, `AncillaryOfferListing.cs` | **بدون تغییر**؛ برای هر Candidate/Unit ردیف باقی بماند |
| Context و Engine | `Query/.../GetAncillaryOffers/GetAncillaryOffersService.cs` | بدون تغییر |
| Projection مشتری | `Query/.../GetAncillaryOffers/AncillaryOffersMapper.cs` | گروه‌بندی Variant → Product → Units |
| Backoffice Projection | `.../Backoffice/BackofficeAncillaryOffersMapper.cs` | گروه‌بندی مشابه، حفظ `EnumValueDto` |
| Response اصلی | `Dto/AncillaryOffersDto.cs`, `Dto/Backoffice/BackofficeAncillaryOffersDto.cs` | تغییر نوع اعضای آرایه‌های خانواده به VariantGroup |
| Form موجود | `Shopping/Options/TypedSelectionService.FormAsync`, `TypedSelectionForm.Fields` | استفاده مجدد؛ بدون فرم‌ساز تازه |
| GET تک‌موردی | `Query/.../GetSelectionOptions/GetSelectionOptionsService.cs` | حفظ API؛ استخراج تابع مشترک و افزودن Batch Forms |
| POST تک‌موردی | `Application/.../SubmitSelectionOptions/SubmitSelectionOptionsService.cs` | حفظ API؛ Batch جدا با reuse همان Proof و Create |
| Proof/Source | `Query/.../VerifyTypedSelection/TypedSelectionVerifier.cs` | بارگذاری یک‌باره Offer، سپس اثبات هر Reference |
| Snapshot | `Domain/SelectionSnapshotAggregate/SelectionSnapshot.cs` | فقط Create برای هر عضو موفق؛ schema تغییر نکند |
| Endpoints | `RestApi/V1/AncillaryOffers/Controllers/{Backoffice,OtaPanel,Ota}Controller.cs` | افزودن Routes، حفظ تک‌موردی‌ها |
| Resolve | `.../ResolveGroupedSelections/*` | بدون تغییر |

## 3. قرارداد پاسخ Shopping — از Flat به Variant/Card

### 3.1 Route و ورودی بدون تغییر

```http
POST /Backoffice/v1/AncillaryOffers
POST /OtaPanel/v1/AncillaryOffers
POST /Api/v1/AncillaryOffers
Content-Type: application/json

{"offerId":"OFFER-123"}
```

Response همچنان Envelope دارد: `offerId: string`, `currencyId: int`, و آرایه‌های `baggages`, `meals`, `pets`, `assistedTravel`, `airportServices`, `priorityServices`, `connectivity`, `upgrades`, `extraSeats`, `travelInsurance`. دو خانواده آخر تا تصمیم قبلی `[]` می‌مانند. `currencyId` شناسه داخلی پلتفرم است، نه کد ISO؛ قیمت و currency هر Row عین سورس موجود بماند.

### 3.2 Shape دقیق هر خانواده

`baggages[]: VariantGroup<BaggageDetailsDto>[]` و برای خانواده‌های دیگر Generic تخصصی متناظر. فیلدهای هر سطح:

| مسیر | نوع | الزام | تعریف |
|---|---|---|---|
| `variantCode` | string | R | مانند `A01`؛ کلید کارت در *همان خانواده* |
| `title` | string | R | نام Variant از Registry موجود `AncillaryVariant.Find(code).Name`؛ نه عنوان تصادفی اولین Candidate |
| `products` | array of `ProductGroup<TDetails>` | R | حداقل ۱ محصول برای Variant نمایان؛ آرایه خالی اصلاً کارت ایجاد نکند |
| `products[].productKey` | string | R | presentation key پایدار در یک پاسخ، فرمت `${DefinitionVersionId}:${ProvisionId}`؛ **نه ID قابل‌خرید/امنیتی** |
| `products[].serviceDefinitionRef` | string | R | مقدار موجود `row.Candidate.ServiceDefinitionRef` |
| `products[].title` | string | R | `row.Candidate.CommercialName`، عنوان محصول واقعی |
| `products[].description` | string? | O | `row.Candidate.Description`؛ null از JSON حذف شود |
| `products[].details` | `TDetails` | R | DTO تخصصی **فعلی** همان خانواده، بر اساس `row.Definition` و `row.Provision` |
| `products[].units` | `UnitOffer[]` | R | برای هر `AncillaryOfferRow` دقیقاً یک Unit؛ **هیچ Row حذف نشود** |
| `units[].selectionScope` | `SelectionScope` / `EnumValueDto` | R | مقدار فعلی آن Row؛ Backoffice همان enum wrapper فعلی |
| `units[].unit` | `OfferUnitDto` | R | یکی از چهار record موجود، بدون تغییر فیلدها |
| `units[].coverage` | `OfferCoverageDto` | R | `travellerRefs:string[]`, `portionRefs:string[]`, `flightRefs:string[]`؛ دقیقاً مثل Row فعلی |
| `units[].unitPrice` | `OfferMoneyDto` | R | `currencyId:int`, `amount:decimal` |
| `units[].quantityPolicy` | `OfferQuantityPolicyDto` / Backoffice equivalent | R | `selectable:bool`, `min:int`, `max:int`, `unit:AncillaryQuantityUnit` |
| `units[].selectionId` | string? | O | `d_` برای Direct؛ در دو مدل دیگر حذف |
| `units[].quantityOptions` | `OfferQuantityOptionDto[]?` | O | برای Quantity؛ هر `quantity:int`, `selectionId:string`, `totalAmount:decimal`؛ بدون Snapshot |
| `units[].reference` | string? | O | `r_` برای Typed؛ در دو مدل دیگر حذف |

**قاعده کلیدی:** یک `variantCode` ممکن است چند Definition یا Provision متفاوت داشته باشد؛ بنابراین تجمیع فقط بر اساس `variantCode` در سطح کارت است، ولی در سطح Product، دقیقاً `(DefinitionVersionId, ProvisionId)` گروه‌بندی شود. هیچ Price/Quantity/Scope/Unit ادغام، متوسط‌گیری، انتخاب اولین ردیف یا حذف Duplicate انجام نشود. اگر دو Row روی یک Unit ولی POS متفاوت باشد هر دو **به‌شکل دو UnitOffer مستقل** باقی بمانند. Frontend قیمت‌ها را بدون انتخاب Product/Unit با هم جمع نزند.

برای جلوگیری از افزایش Payload، `details` فقط در Product و نه در هر Unit تکرار شود. `optionsEndpoint` قدیمی که فقط Template است از **UnitOffer گروه‌بندی‌شده** حذف شود؛ مسیرهای Batch ثابت‌اند و Frontend از نام `Family` در Envelope می‌سازد. Routes تک‌موردی موجود و پاسخ آن‌ها دست‌نخورده می‌مانند. **برای فروش نیازمند Options، presenceِ `reference` معیار است**؛ هیچ فیلد جدید `interaction` لازم نیست.

### 3.3 ترتیب ثابت و قابل‌تست

- ترتیب خانواده‌ها = Envelope فعلی.
- ترتیب `VariantGroup` = اولین ظهور Variant در `snapshot.Rows`، بدون sort جدید تجاری.
- ترتیب `ProductGroup` = اولین ظهور `(DefinitionVersionId,ProvisionId)` در همان Variant.
- ترتیب `units` = ترتیب ردیف‌های اصلی حفظ شود.
- عدم وجود Row یک Variant ⇒ عدم وجود کارت؛ خانواده خالی `[]`.
- `productKey` فقط برای render و diff Frontend است؛ هرگز در GET/POST Options و Resolve ارسال نمی‌شود.

### 3.4 نمونه فروش، دو مسافر و یک Portion (فقط مثال؛ IDها نمادین)

```json
{
  "offerId": "OFFER-123",
  "currencyId": 47,
  "baggages": [
    {
      "variantCode": "A01",
      "title": "Extra checked bag",
      "products": [
        {
          "productKey": "2001:3001",
          "serviceDefinitionRef": "BAG-23KG",
          "title": "Extra 23kg bag",
          "details": {
            "chargeKind": "ExtraPiece",
            "allowanceConcept": null,
            "packageWeightKg": null,
            "maxKgPerPiece": 23,
            "weightFromExclusiveKg": null,
            "weightToInclusiveKg": null,
            "maxSizeCm": null,
            "maxLinearSumCm": null,
            "equipmentKind": null,
            "chargeCombination": null
          },
          "units": [
            {
              "selectionScope": "PerTravellerPerPortion",
              "unit": {"travellerRef": "ADT-1", "portionRef": "BOUND-1"},
              "coverage": {"travellerRefs":["ADT-1"], "portionRefs":["BOUND-1"], "flightRefs":["BOUND-1:F1", "BOUND-1:F2"]},
              "unitPrice": {"currencyId":47, "amount":35},
              "quantityPolicy": {"selectable":true, "min":1, "max":2, "unit":"Piece"},
              "quantityOptions": [{"quantity":1,"selectionId":"d_EXAMPLE_1","totalAmount":35},{"quantity":2,"selectionId":"d_EXAMPLE_2","totalAmount":70}]
            },
            {
              "selectionScope": "PerTravellerPerPortion",
              "unit": {"travellerRef": "ADT-2", "portionRef": "BOUND-1"},
              "coverage": {"travellerRefs":["ADT-2"], "portionRefs":["BOUND-1"], "flightRefs":["BOUND-1:F1", "BOUND-1:F2"]},
              "unitPrice": {"currencyId":47, "amount":35},
              "quantityPolicy": {"selectable":true, "min":1, "max":1, "unit":"Piece"},
              "selectionId":"d_EXAMPLE_3"
            }
          ]
        }
      ]
    }
  ],
  "meals": [], "pets": [], "assistedTravel": [], "airportServices": [],
  "priorityServices": [], "connectivity": [], "upgrades": [], "extraSeats": [], "travelInsurance": []
}
```

**تذکر مثال:** نمونه `details` برای توضیح شکل است؛ serializer واقعی همچنان همان `BaggageDetailsDto` کد را استفاده می‌کند. `currencyId:47` در داده‌های dev برای EUR گزارش شده؛ مقدار واقعی را از Offer/Source خوانده و هیچ‌وقت ثابت نکنید. اگر QuantityPolicy در Row `selectable:false` است، آن Row دقیقاً طبق کد فعلی `selectionId` دارد، نه `quantityOptions`. `d_EXAMPLE_*` صرفاً placeholder هستند و امضای معتبر ندارند.

## 4. Batch Forms — یک Offer Load برای چند Reference

### 4.1 Route

```http
POST /{Surface}/v1/AncillaryOffers/{Family}/Options/Forms
```

`Surface ∈ {Backoffice,OtaPanel,Api}`. `Family ∈ {Baggages,Pets,AssistedTravel,AirportServices,PriorityServices,Connectivity}` فقط؛ Route غیرمرتبط (Meals/Upgrades/ExtraSeats) ایجاد نکنید. Route تک‌موردی `GET /{Family}/{reference}/Options?offerId=...` همچنان معتبر است.

### 4.2 Request — دقیق

```json
{
  "offerId": "OFFER-123",
  "references": ["r_EXAMPLE_ADT1", "r_EXAMPLE_ADT2"]
}
```

| Field | Type | Rule |
|---|---|---|
| `offerId` | string | nonblank؛ همان Offer اصلی Shopping؛ Body فقط یک بار |
| `references` | string[] | ۱ تا ۵۰؛ هر عنصر nonblank، <= 1024؛ case-sensitive unique؛ ترتیب معتبر |

Unknown JSON members و Family نامعتبر ⇒ 400/404 مطابق الگوی Errors موجود. Repeated reference ⇒ Validation Error 422 (از Error factory موجود، کد جدید نسازید).

### 4.3 Response — دقیق

```json
{
  "forms": [
    {
      "reference": "r_EXAMPLE_ADT1",
      "variantCode": "A15",
      "fields": {
        "assistanceSsrCode": {
          "control": "select",
          "required": true,
          "choices": [{"value":"WCHR","label":"WCHR"},{"value":"WCHS","label":"WCHS"}]
        }
      }
    },
    {
      "reference": "r_EXAMPLE_ADT2",
      "variantCode": "A15",
      "fields": {
        "assistanceSsrCode": {
          "control": "select",
          "required": true,
          "choices": [{"value":"WCHC","label":"WCHC"}]
        }
      }
    }
  ]
}
```

- `forms[]` دقیقاً **یک Form برای هر Reference**، به ترتیب Request؛ نام `reference` برای اتصال UI به Unit لازم است.
- `fields` همان `SelectionOptionsDto.Fields` فعلی (key=field name) و `SelectionFieldDto`: `control:string`, `required:bool`, optional `choices:[{value,label}]`, `unit:string?`, `min:decimal?`, `exclusiveMin:decimal?`, `max:decimal?`. بدون داده جدید/نقشه فرم عمومی.
- برای هیچ فیلدی Choices ساختگی ایجاد نشود؛ مثال بالا فقط گویای متفاوت‌بودن فرم‌هاست، نه اینکه A15 dev چنین مقادیری دارد.
- `control` فقط هفت مقدار موجود: `select`, `multi-select`, `text`, `number`, `checkbox`, `date-time`, `dimensions`.
- GET/Forms هیچ Snapshot یا write انجام نمی‌دهد.
- **All-or-nothing در پاسخ:** اگر یک Reference نامعتبر/Expired/Family-foreign/Offer-binding-mismatched بود، همان خطای موجود برگردد و Forms ناقص ارسال نشود. خطا باید index (صفرمبنا) و field path مثل `references[1]` را در پیام قابل‌عیب‌یابی نشان دهد، بدون تغییر فرم کلی Error response.

### 4.4 الگوریتم مشخص

1. Surface Sales Scope فعلی یک بار Resolve شود.
2. `_source.LoadAsync(offerId, scope)` **دقیقاً یک بار برای یک Batch**؛ سپس `PricedOfferScopeGuard.EnsureSellable` یک بار.
3. `shopping.PointsOfSale.Select(...Context)` یک بار ساخته شود.
4. به ترتیب Referenceها: `ReadReferenceAsync` از JWS موجود، سپس `TypedSelectionService.FormAsync` با همین Context مشترک. تمام checks قبلی (MAC, expiry, offer/sales binding, active chain, family) دقیقاً پابرجا بمانند.
5. `SelectionOptionsMapper.Map(form)` موجود استفاده شود. تنها `reference` به هر پاسخ افزوده شود.
6. `200 {forms:[...]}`؛ نه فراخوانی داخلی `GetSelectionOptionsService.ExecuteAsync` در loop (چون باعث بارگذاری مکرر Offer می‌شود).

## 5. Batch Submit — ورودی تخصصی هر خانواده، یک Transaction

### 5.1 Routes و Request type

```http
POST /{Surface}/v1/AncillaryOffers/Baggages/Options/Batch
POST /{Surface}/v1/AncillaryOffers/Pets/Options/Batch
POST /{Surface}/v1/AncillaryOffers/AssistedTravel/Options/Batch
POST /{Surface}/v1/AncillaryOffers/AirportServices/Options/Batch
POST /{Surface}/v1/AncillaryOffers/PriorityServices/Options/Batch
POST /{Surface}/v1/AncillaryOffers/Connectivity/Options/Batch
```

برای هر سه Surface شش Action تخصصی موجود به روش Batch تکمیل می‌شود (۱۸ Route جدید). هیچ endpoint generic با `object`, `JsonElement` یا dictionary-of-object برای input ساخته نشود.

### 5.2 Request generic envelope ولی Options تخصصی

`SubmitBatchRequest<TFamilyOptions>` فقط Envelope مشترک است: `offerId:string`, `selections: [{reference:string, options:TFamilyOptions}]`؛ `TFamilyOptions` دقیقاً یکی از recordهای **موجود**: `BaggageSelectionOptions`, `PetSelectionOptions`, `AssistedTravelSelectionOptions`, `AirportServiceSelectionOptions`, `PrioritySelectionOptions`, `ConnectivitySelectionOptions`.

نمونه واقعی قرارداد خانواده AssistedTravel:

```json
{
  "offerId": "OFFER-123",
  "selections": [
    {"reference":"r_EXAMPLE_ADT1", "options":{"assistanceSsrCode":"WCHR"}},
    {"reference":"r_EXAMPLE_ADT2", "options":{"assistanceSsrCode":"WCHS"}}
  ]
}
```

نمونه Pet (مقادیر فقط illustration):

```json
{
  "offerId": "OFFER-123",
  "selections": [
    {"reference":"r_EXAMPLE_P1", "options":{"animalType":"Dog","combinedWeightKg":6.2,"carrierDimensions":{"lengthCm":45,"widthCm":30,"heightCm":25}}}
  ]
}
```

**مقدار واقعی `animalType` مطابق JSON Enum converter فعلی است؛ هیچ تغییر serializer در این Scope مجاز نیست.**

### 5.3 فیلدهای مشترک و تخصصی POST

| Field | Type / Rule |
|---|---|
| `offerId` | `string`, nonblank, فقط یک بار |
| `selections` | ۱ تا ۵۰ عنصر، ترتیب حفظ شود؛ Reference غیرتهی، غیرتکراری، <=1024 |
| `selections[].reference` | `string` امضاشده `r_` مربوط به همان Family/Offer/Scope |
| `selections[].options` | `TFamilyOptions` مطابق جدول پایین؛ nonnull؛ **unknown member رد شود** |

| Family | فیلدهای مجاز options (nullable بودن مطابق record فعلی؛ Required بودن به فرم همان Variant وابسته است) |
|---|---|
| Baggages | `bagRef:string?`, `measuredWeightKg:decimal?`, `dimensions:{lengthCm,widthCm,heightCm}?`, `weightKg:decimal?` |
| Pets | `animalType:PetAnimalType?`, `otherAnimalCode:string?`, `animalAgeWeeks:int?`, `combinedWeightKg:decimal?`, `carrierDimensions:{lengthCm,widthCm,heightCm}?`, `documentAcknowledgements:string[]?`, `sizeBracket:string?` |
| AssistedTravel | `assistanceSsrCode:string?`, `infantRef:string?`, `guardianRef:string?` |
| AirportServices | `timeWithOffset:DateTimeOffset?`, `guestCount:int?` |
| PriorityServices | `optIn:bool?` |
| Connectivity | `deviceCount:int?` |

Required/constraints از `TypedSelectionForm` و Engine فعلی خوانده می‌شود، نه از این جدول nullable به عنوان اجازه گذشتن از validation. هیچ `quantity` در Batch POST تخصصی جدید لازم نیست؛ Quantityهای فعلی Shopping خودشان `quantityOptions[]` می‌دهند.

### 5.4 Response — یک شناسه برای هر مورد

```json
{
  "selections": [
    {"reference":"r_EXAMPLE_ADT1", "selectionId":"s_EXAMPLE_1", "expiresAtUtc":"2026-10-24T12:00:00Z"},
    {"reference":"r_EXAMPLE_ADT2", "selectionId":"s_EXAMPLE_2", "expiresAtUtc":"2026-10-24T12:00:00Z"}
  ]
}
```

هر `selectionId` همان Snapshot ID کوتاه `s_` در سرویس فعلی است؛ `reference` عین ورودی و خروجی به همان ترتیب. از POST پاسخ `selected`, `price`, `snapshot`, `form` یا `product` اضافی برنگردد؛ UI فرم محلی را دارد و بعداً Resolve فقط ID می‌گیرد.

### 5.5 رفتار کامل Transaction و خطا

1. قبل از ساخت هر Snapshot، تمام ۱..۵۰ ورودی در همان **یک Offer context** بررسی شوند: سطح دسترسی/Offer guard → `r_` HMAC/TTL → Binding → family → active pricing/provision/definition → `TypedSelectionService.ProveAsync` → Engine Rule و rate consistency.
2. خطای هر عنصر ⇒ **هیچ Snapshot جدید در آن Batch Persist نشود**. برای تشخیص `selections[i].options.<field>` همان Error factory/status فعلی را به همراه index در متن خطا استفاده کنید؛ Response خطای جدید / Partial Success نسازید.
3. فقط پس از موفقیت اثبات همه، `SelectionSnapshot.Create` موجود را به‌ازای هر کدام اجرا، `ISelectionSnapshotRepository.AddAsync` موجود را برای همه صدا بزنید، **فقط یک** `IUnitOfWork.SaveChangesAsync` اجرا کنید. EF یک Transaction برای SaveChanges؛ در صورت هر شکست، هیچ بخشی Commit نشود.
4. مدت اعتبار هر Snapshot عین فرمول فعلی `min(reference expiry, now + configured MaxAge)`؛ بی‌نیاز از cron/new TTL config.
5. Post تکراری می‌تواند Snapshot IDهای جدید صادر کند (رفتار فعلی)؛ **Idempotency جدید نسازید**. Resolve از همین `s_` مانند قبل چندبار خواندنی است.
6. به‌هیچ‌وجه Verifier فعلی را در حلقه صدا نزنید اگر موجب `_source.LoadAsync` برای هر عضو می‌شود. یک `VerifyBatchAsync` روی همان port موجود `ITypedSelectionVerifier` اضافه کنید (یا نام هم‌معنای هماهنگ با repo)، با یک Scope/Offer load و حلقه Proof؛ متد `VerifyAsync` قدیمی از همان منطق مشترک برای یک عضو استفاده کند.
7. APIهای تک‌موردی GET/POST فعلی **حفظ و تست** شوند؛ Batch جایگزین اجباری برای Clientهای موجود نیست.

## 6. پیشنهاد UI/UX که این API پشتیبانی می‌کند (بدون کدنویسی Frontend در این Scope)

1. Front از آرایه هر خانواده Card می‌سازد؛ کلید کارت `family + variantCode`.
2. زیر Card، `products[]` را نشان می‌دهد؛ دو محصول با یک Variant و قیمت/وزن متفاوت جدا باقی می‌مانند.
3. برای هر Product `units[]` را به تفکیک مسافر/پرواز/Portion/Order رندر می‌کند؛ `quantityOptions[]` نیازی به HTTP اضافی ندارد. مقدار ۰ = عدم انتخاب و عدم افزودن ID، نه انتخاب یک ID با quantity=0.
4. وقتی Card تخصصی باز شد، `reference`های مطلوب **فقط در همان Family** در یک `Options/Forms` ارسال شوند. هر Form با Reference خودش به Unit متصل شود؛ فرض یکسان‌بودن Choices همه مسافران ممنوع.
5. Renderer فقط همان ۷ کنترل موجود؛ `dimensions` = سه ورودی عددی cm، `date-time` = مقدار دارای Offset؛ Dynamic Form Framework تازه لازم نیست.
6. پس از تأیید کاربر، فقط Unitهای واقعاً انتخاب‌شده در یک `Options/Batch` با typed `options` مخصوص همان Family ارسال شوند. هیچ Snapshot برای Unitهای بدون انتخاب ساخته نشود.
7. شناسه‌های `d_` از Direct و Quantity و `s_` از Batch در سبد جمع و به `POST /Service/v1/Ancillaries/Selections/ResolveGrouped` با `{ids:[...]}` داده شوند؛ Resolver بدون تغییر است.
8. برای Familyهای مختلف حداکثر یک Forms و یک Batch در هر Family در هر اقدام؛ نیازی به Batch سراسری همه انواع نیست. بیش از ۵۰ مورد ⇒ Front آن خانواده را به chunkهای حداکثر ۵۰ تقسیم کند.

## 7. سیاست تغییر و Compatibility

- تغییر Shape Shopping **Breaking** برای Consumerهای قبلی است، اما **طبق دستور Owner اصلاح همان API فعلی** است؛ Version یا endpoint تازه برای Shopping اختراع نشود.
- هیچ حذف Route تک‌موردی Options؛ فقط Additive Batch Routes.
- Backoffice بر خلاف Api/OtaPanel باید Enum Value wrapper موجود را در `selectionScope`، `quantityPolicy.unit` و DTO تخصصی حفظ کند. هیچ normalize کردن enum به string با شکستن Backoffice مجاز نیست.
- همه `AncillaryOfferRow`های قابل‌فروش فعلی باید بدون تغییر `selectionId/reference/quantityOptions` و قیمت به بخش `units` منتقل شوند.
- کد عملیات Service Resolve، SeatMap و Reservation **حتی برای refactor زیبایی** دست‌نخورده بماند.
- `A09`, `A17`, `A19`, `TravelInsurance` همان gated بودن قبلی را حفظ کنند.

## 8. معیار بسته‌شدن تغییر

فقط وقتی Stage تمام است که: (۱) سه Surface خروجی درست Card/Product/Unit بدهند، (۲) GET/POST Batch برای هر شش Family و هر سه Surface route و typed validation داشته باشد، (۳) برای N Reference فقط یک بار Offer load شود، (۴) POST Batch با یک `SaveChanges` اتمیک باشد، (۵) SelectionIdهای نهایی دقیقاً با Resolve فعلی کار کنند، (۶) رفتار Single Options/SeatMap/Reservation تغییر نکند، (۷) تست‌های `04-ACCEPTANCE-MATRIX.md` پاس شوند، (۸) Agent گزارش Diff، Build/Test و نمونه HTTP واقعی یا تست یکپارچه معتبر بدهد.

اگر داده Dev برای یک Variant وجود ندارد، به‌جای جعل Product، تست با داده Fixture واقعی Domain بنویسید و محدودیت Live را گزارش کنید. برای سه Surface فقط یک DTO Generic عمومی در Query کافی نیست اگر Backoffice serialization از بین برود؛ Projection جدا حفظ شود. **هیچ Batch مخصوص SeatMap ایجاد نشود.**
