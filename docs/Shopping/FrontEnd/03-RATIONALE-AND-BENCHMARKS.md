# دلایل تصمیم و بنچ‌مارک — AeroTech Ancillary UI Grouping + Batch Options

**2026-10-24 | مکمل تحلیلی قرارداد v9**  
**توجه:** شواهد صنعتی زیر با منابع رسمی بررسی شده‌اند؛ آنچه درباره Card Grouping یا Batch Forms/Submit بیان می‌شود **پیشنهاد اختصاصی AeroTech** است، نه ادعای وجود همان Endpoint در Amadeus، Duffel یا Travelport.

## 1. صورت مسئله واقعی از دید کاربر

برنچ `feat/ancillary-offers-v8@324e3299` یک پیشنهاد فروش را به ازای Selection Unit تولید می‌کند. در مثال ۴ مسافر × ۲ پرواز و یک Variant تخصصی، Front هشت ردیف تقریباً یکسان می‌گیرد. اگر برای هر Reference یک GET Options و یک POST Options بزند، ۱۶ HTTP call مربوط به Options ایجاد می‌شود. علاوه بر این هر GET/POST کنونی Offer را مجدداً Load می‌کند. نتیجه:

- لیست کارت‌های UI تکراری و سخت برای خرید؛
- API call بیشتر و بارگذاری مکرر Offer؛
- Front مجبور به group کردن variant + product + unit با حدس درباره Provision/Definition می‌شود؛
- خطر اینکه Form متفاوت یک مسافر با Form مسافر دیگر اشتباه استفاده شود.

هدف محدود **UI ergonomics و کاهش round-trip** است، نه تغییر مدل انتخاب یا رزرو.

## 2. بنچ‌مارک اول: Duffel — هویت مستقل خدمت و Coverage

**منابع رسمی:**
- https://duffel.com/docs/api/v2/orders
- https://duffel.com/docs/guides/adding-post-booking-bags
- https://duffel.com/docs/guides/adding-extra-bags

**آنچه منبع واقعاً پشتیبانی می‌کند:**

- Available service یک **ID** دارد و با `passenger_ids`, `segment_ids` به کاربران و قطعات سفر وابسته است؛ Quantity سقف جدا دارد (`maximum_quantity`).
- خرید خدمات می‌تواند مجموعه‌ای از `id` همراه `quantity` باشد. نشان می‌دهد یک کارت نمایشی معادل یک واحد تجاری خرید نیست و یک Variant ممکن است چند انتخاب جدا داشته باشد.
- بنابراین هرچقدر UI فهرست را تجمیع کند، **انتخاب نهایی هر Traveller/Segment باید مستقل** بماند.

**آنچه Duffel اثبات نمی‌کند:**
- گروه‌بندی کارت به ازای VariantCode خاص AeroTech، ProductKey جدید یا این‌که GET/POST Options را Batch کنیم. این‌ها **تصمیمات محلی** هستند.

## 3. بنچ‌مارک دوم: Travelport — Purchase Scope مستقل از Category

**منابع رسمی:**
- https://support.travelport.com/webhelp/JSONAPIs/Airv11/Content/Air11/Ancillaries/APIRef_AncillaryShop.htm
- https://support.travelport.com/webhelp/JSONAPIs/Airv11/Content/Air11/Ancillaries/AncillaryGuide.htm
- https://support.travelport.com/webhelp/jsonapis/airv11/content/air11/APIReferences.htm

**آنچه منبع واقعاً پشتیبانی می‌کند:**

- مقادیر محدوده/Quantity شامل `PassengerSegment`, `Segment`, `Itinerary`, `Passenger`, `PassengerOD` هستند؛ یعنی تعداد انتخاب‌شده با واحد پوشش گره دارد و لزوماً تابع دسته Ancillary نیست.
- Search/Shop Ancillary از SeatMap مسیر جدا دارد؛ تغییر UI این پروژه **نباید SeatMap را تغییر دهد**.
- Ancillaryهای انتخاب‌شده توسط ایرلاین/مسافر/Segment متفاوت‌اند؛ یک Card می‌تواند تعداد زیادی Selection Unit در خود داشته باشد.

**نسبت با Domain موجود:**
- `PerTravellerPerFlight` تقریب `PassengerSegment`، `PerTravellerPerPortion` تقریب `PassengerOD` و `PerOrder` تقریب `Itinerary` است؛ این‌ها **هم‌ارزی کامل استاندارد نیستند**. به‌خصوص `PortionRef` در AeroTech با FlightRef یکی نیست.
- Travelport به تنهایی وجود `SelectionScope.PerFlight` در کد موجود را توجیه نمی‌کند؛ چنین enumی در v8 موجود نیست و این فاز اضافه نمی‌شود.

## 4. بنچ‌مارک سوم: تفکیک نمایش تجاری و واحد معامله

در سیستم‌های خدمات سفر معمول است کاربر یک «نوع سرویس» مانند Baggage یا Assisted Travel ببیند، سپس زیر آن مسافر، Segment و Quantity/Options را مشخص کند. این **برداشت UX** از ساختار سرویس‌های فوق است؛ ادعایی درباره Endpoint رسمی شرکت مشخصی نیست.

**نتیجه عملی:**

1. کارت نمایشی یک «Variant» است (A01، A15 و ...)، و **حامل پول/رزرو نیست**.
2. Product مشخص (DefinitionVersion+Provision) زیر آن قرار می‌گیرد تا دو گزینه 23kg و 32kg، یا دو Provision متفاوت یک Variant یکی نشوند.
3. هر Unit همان Reference/SelectionId/Price و Coverage قبلی را دارد.
4. هیچ عملیات انتخابی به کارت کلی گره نمی‌خورد؛ Front برای هر Unit تصمیم می‌گیرد.

مزیت نسبت به تجمیع مستقیم `variantCode`: حفظ قواعد قیمت مسافر، مسافت و POS، و جلوگیری از خرید دو Unit متفاوت تحت یک ID تصادفی.

## 5. چرا «فقط POST Batch» کافی نیست؟

در HEAD فعلی:

- `GetSelectionOptionsService.ExecuteAsync` برای هر GET یک بار `_source.LoadAsync(query.OfferId, scope)` می‌کند.
- `TypedSelectionVerifier.VerifyAsync` برای هر POST یک بار `_source.LoadAsync(verification.OfferId, scope)` می‌کند.
- `TypedSelectionService.FormAsync` و `ProveAsync` همان Context را دریافت می‌کنند و برای هر Reference جدا قابل اجرا هستند.

پس Batch POST تنها نیمی از round-trip و تکرار Offer Load را کم می‌کند. Batch Forms نیز ضروری است؛ ۱۰ انتخاب در یک خانواده از ۱۰ GET + ۱۰ POST به **۱ درخواست Forms + ۱ درخواست Submit** تبدیل می‌شود (تا حد ۵۰ واحد). برای چند خانواده تعداد تماس‌ها جمع Batchهای همان خانواده است.

**مبنای فنی:** اشتراک Offer در یک Batch از Business Scope ناشی می‌شود؛ درخواست می‌گوید `offerId` واحد و Referenceها باید به همان Offer/Sales Scope امضاشده متعلق باشند. این یک سناریوی واقعی است، نه cache اختراعی.

## 6. چرا یک Endpoint POST برای همه خانواده‌ها نداریم؟

کد فعلی شش Body تخصصی دارد:
`BaggageSelectionOptions`, `PetSelectionOptions`, `AssistedTravelSelectionOptions`, `AirportServiceSelectionOptions`, `PrioritySelectionOptions`, `ConnectivitySelectionOptions`.

هر Body فیلدهای قابل استفاده متفاوت دارد. یک endpoint با `object` یا یک DTO با ۳۰ فیلد Nullable دوباره نقطه ضعف قدیمی پروژه را ایجاد می‌کند؛ قوانین Type-specific در زمان Deserialize و Validation از بین می‌روند.

**انتخاب سبک:** GET Forms می‌تواند با یک Route پارامتری `/{family}` کار کند (چون Body فقط `references[]` دارد). POST Batch شش مسیر تخصصی دارد؛ فقط Envelope مشترک است. ۱۸ Route سطحی می‌ارزد چون هر کدام Controller Action خیلی نازک و همان MediatR/Application Service مشترک را دارند؛ هیچ Business Engine جدیدی اضافه نمی‌کنند.

## 7. چرا Batch All-or-nothing است؟

کاربر در یک صفحه ممکن است چند Unit را انتخاب کند و سپس «تأیید» بزند. در پاسخ Partial، Front باید برای ۶ موفق و ۲ شکست‌خورده چند وضعیت Snapshot متفاوت را نگه دارد و Retry/Expiry را مدیریت کند. این برای این Phase پیچیدگی اضافه است.

**گزینه سبک:**
- تمام آیتم‌ها در همان Offer Context validate شوند؛
- یک شکست ⇒ هیچ Snapshot جدید Persist نشود؛
- تمام موفق ⇒ یک EF SaveChanges و پاسخ ordered.

این یک **سیاست AeroTech برای سهولت UI** است و از Duffel/Travelport اقتباس صریح نشده است. Database Transaction فعلی کافی است؛ Saga/Outbox/Job جدید لازم نیست.

## 8. چرا Grouping فقط در Mapper؟

Engine فعلی قبلاً درست با واحدهای Sector/Portion/Journey/Order، قیمت، Quantity و Offer Eligibility کار می‌کند. اگر Engine برای ساخت Card تغییر کند، «Card» در مدل Domain گمراه‌کننده می‌شود، ریسک رگرسیون Scope و نرخ بالا می‌رود، و قیمت‌های per-traveller/per-flight ممکن است یکی شوند.

**مرز درست:**

`Engine → AncillaryOfferRow[]` (unchanged) → `Query Mapper → VariantGroup[] / ProductGroup[] / UnitOffer[]` (presentation only).

این تغییر ارزان‌تر، reversible و قابل تست با رابطه ۱:۱ بین Unitهای جدید و ردیف‌های قبلی است.

## 9. محدودیت‌های پذیرفته‌شده و دلایل عدم توسعه Scope

- **Scope:** `PerTravellerPerPortion` با `PerTravellerPerFlight` متفاوت است. UI می‌تواند هر Portion را با پروازهای پوشش‌یافته نشان دهد؛ API نباید به ازای هر Flight دوباره Charge کند.
- **Pricing:** هیچ Reprice/Snapshot تازه برای Direct و Quantity؛ `d_`ها عین قبل. `s_`ها فقط Typed، TTL همان قبلی.
- **Gating:** `A09`, `A17`, `A19`, Insurance مطابق تصمیم‌های قبلی همچنان خارج از فروش جاری‌اند.
- **SeatMap:** API و flow آن جداست؛ کارتهای Ancillary غیرSeatMap نباید به SeatMap متصل شوند.
- **Security:** Batch باید برای هر Reference همان HMAC/Expiry/Offer/Sales Binding/Eligibility تک‌موردی را بررسی کند؛ کاهش API call نباید کنترل امنیتی را حذف کند.
- **Performance:** یک بار Offer Load در هر Batch الزامی است؛ Cache عمومی یا batch کردن تمام SQL reads فعلاً ضروری نیست؛ فقط پس از اندازه‌گیری واقعی.
- **Compatibility:** خروجی Shopping طبق تصمیم Owner اصلاح می‌شود؛ GET/POST تک‌موردی Options حذف نمی‌شود.

## 10. شواهد پیاده‌سازی در GitHub

مرجع برای audit، بدون اتکا به حدس:
- `https://github.com/aliifarhadi/AeroTech.Ancillary/blob/324e32999bac6a3a9dfd69604dbe9e31272c43f0/src/AeroTech.Ancillary.Shopping/Offers/AncillaryOfferListing.cs`
- `https://github.com/aliifarhadi/AeroTech.Ancillary/blob/324e32999bac6a3a9dfd69604dbe9e31272c43f0/src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/GetAncillaryOffers/AncillaryOffersMapper.cs`
- `https://github.com/aliifarhadi/AeroTech.Ancillary/blob/324e32999bac6a3a9dfd69604dbe9e31272c43f0/src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/GetSelectionOptions/GetSelectionOptionsService.cs`
- `https://github.com/aliifarhadi/AeroTech.Ancillary/blob/324e32999bac6a3a9dfd69604dbe9e31272c43f0/src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/VerifyTypedSelection/TypedSelectionVerifier.cs`
- `https://github.com/aliifarhadi/AeroTech.Ancillary/blob/324e32999bac6a3a9dfd69604dbe9e31272c43f0/src/AeroTech.Ancillary.Application/SelectionSnapshotAggregate/Commands/SubmitSelectionOptions/SubmitSelectionOptionsService.cs`

## 11. تصمیم‌های مصوب نهایی برای این اصلاح

1. **UI Card** = یک Card برای هر Variant داخل Family، درون آن Products و Units جدا.
2. **Mapper Grouping** = `(DefinitionVersionId, ProvisionId)` در سطح Product و هر `AncillaryOfferRow` یک Unit، بدون collapse.
3. **Batch GET** = `POST /{Family}/Options/Forms`, یک Offer Load، پاسخ Form به ازای Reference.
4. **Batch POST** = شش Route تخصصی `/Options/Batch`, همان یک Offer Load و اثبات تک‌به‌تک، یک SaveChanges، همه یا هیچ.
5. **Single API/Resolve/SeatMap** = بدون تغییر.
6. **No overengineering** = نه سرویس جدید، نه دیتابیس اضافه، نه generic engine و نه rework Domain.

**این فایل «چرا» را توضیح می‌دهد؛ تنها قرارداد حاکم بر payloadهای پیاده‌سازی، فایل `01` همین Pack است.**
