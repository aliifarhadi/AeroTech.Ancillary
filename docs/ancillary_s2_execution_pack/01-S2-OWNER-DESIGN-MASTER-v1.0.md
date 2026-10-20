# AeroTech.Ancillary — Stage 2 End-to-End Design, API Contract, Seat Resolution & Acceptance Master

**نسخه سند:** `S2-OWNER-REVIEW-v1.0` — ۲۰ اکتبر ۲۰۲۶  
**وضعیت:** **پیشنهاد طراحی کامل برای تصمیم مالک؛ هنوز مجوز کدنویسی یا تأیید پایان S2 نیست.**  
**مخزن قابل تغییر در اجرای بعدی:** فقط `aliifarhadi/AeroTech.Ancillary`؛ سایر مخازن فقط خواندنی و موضوع قرارداد اتصال‌اند.  
**کد بررسی‌شده:** `AeroTech.Ancillary/k8s-stg@7efee95cbd01b5ff30bd4c8c36326debc5020574` در قیاس با `S2-01@6853e83e316ec6d6bc2d3d9610e0f099bebdc9b1`.  
**مراجع بالادستی فعلی:** `CLAUDE.md` و `docs/AeroTech-Ancillary-Master-Pack-v13.2-FINAL/`، به‌ویژه فصل‌های ۰۶ تا ۰۹، ۱۴، ۱۶ و ۲۱. **تا اعلام موافقت مالک، بندهای جدید این سند جایگزین Authority نیستند.** در صورت موافقت، اصلاح فصل ۰۸ درباره ذخیره Quote و فصل ۰۷ درباره Order-based shopping باید با ADR صریح و یک مرجع معتبر انجام شود، نه دستورات متناقض و موازی.  
**نوع خروجی:** مشخصات محصول، تصمیم‌های معماری، قرارداد فیلدبه‌فیلد، سناریوهای قابل اجرا، قیود امنیت/قیمت/Seat، معیار پذیرش و پرامپت ادامه چت؛ **نه کد و نه گزارش تست سبز**.  
**نکته منبع:** سورس Ancillary در SHA فوق مستقیم خوانده شده است؛ سورس Ordering/FlightFlow/AirAvail/AirPrice در این نوبت مجدداً قابل دریافت نبود. اطلاعات دقیق آنها فقط در حد ممیزی بین‌مخزنی ۱۹ اکتبر ۲۰۲۶، Pack فعلی و فایل‌های قبلی است و در این سند با برچسب `[PRIOR-AUDIT]` آمده است. Endpoint جدید آنها «موجود» تلقی نمی‌شود.

---

**راهنمای سریع مطالعه / جست‌وجو در فایل**

| پرسش اصلی | محل پاسخ |
|---|---|
| تصمیم نهایی معماری چیست و با Pack کجا تعارض دارد؟ | §00–01، §17.1 |
| چه بنچ‌مارکی از Offer/Order/Seat و Shop/Price/Book داریم؟ | §02، §19.2 |
| دو جریان PreIssue/PostTicketed دقیقاً چه توالی دارند؟ | §03 |
| کدام Endpoint موجود است و هر Request چه فیلدهایی دارد؟ | §04 |
| Shopping Context از Order/Offer دقیقاً چه Factهایی می‌خواهد؟ | §05 |
| List/Details/سه Surface چه DTO و Optionهایی می‌دهند؟ | §06 |
| Batch Evaluate دقیقاً چه input/output/selection fields دارد؟ | §07 |
| قیمت، تعداد، مالیات، Fee مشترک و FX چگونه محاسبه می‌شوند؟ | §08 |
| SeatMap ID فعلی چگونه بدون تغییر DTO در Evaluate/Resolve کار می‌کند؟ | §09 |
| Stateless token چه claimها و قواعد امنیتی دارد؟ | §10 |
| Resolve چه چیزی به Ordering تحویل می‌دهد و Stage3 چه نقشی دارد؟ | §11 |
| هر یک از ۲۴ Variant چه سناریو و محدودیت دارد؟ | §12 |
| مجموعه تست‌ها، Edge Caseها و شروط Closure چیست؟ | §13، §17.4 |
| JSON نمونه و وایرفریم خرید را از کجا ببینیم؟ | §14–15 |
| بازنویسی/rollback/تغییر کد چگونه کمینه می‌شود؟ | §16 |
| اتصال بیرونی چه موارد باز و قابل اثباتی دارد؟ | §17.2–17.3 |
| چگونه چت جدید را بدون گم‌شدن Context شروع کنیم؟ | §18 |
| لینک سند/کد مرجع و درجه اطمینان از کجا معلوم می‌شود؟ | §19 |
| عدد Enumها و ترتیب نهایی تطبیق Authority چیست؟ | §20 |

---

## 00. قرارداد معنایی و نحوه خواندن

- `[CURRENT]`: مسیر/فیلد/رفتار در Ancillary HEAD فوق دیده شده است.
- `[TARGET]`: تصمیم پیشنهادی مطلوب Stage 2، **هنوز ساخته نشده**.
- `[FROZEN-WIRE]`: JSON، نام/نوع/تو‌در‌تویی و مسیر موجود نباید به‌صورت Breaking تغییر کند.
- `[FIX-REQUIRED]`: نقص قابل مشاهده در سورس فعلی که باید رفع شود؛ حتی اگر به منطق داخلی SeatMap مربوط باشد.
- `[EXTERNAL-CONTRACT]`: وابسته به Owner سرویس دیگر، نیازمند قرارداد واقعی و آزمون Integration.
- `[BLOCKED-BY-SOURCE]`: اطلاعات لازم از منبع معتبری ارائه نمی‌شود؛ نتیجه درست Fail-closed، NeedsQuote یا NeedsVerification است.
- «Candidate» خدمتِ قابل معرفی برای Passenger/Flight/Scope است؛ «Option» مقدار مجاز یکی از ورودی‌های آن؛ «Priced Selection» **انتخاب نهایی مشخص مشتری با قیمت و هویت تأییدشده** است؛ «OrderService» موجودیت ثبت‌شده در Ordering است؛ «Hold» و «EMD» هیچ‌یک خروجی S2 نیستند.
- همه JSONهای مثال `TARGET` هستند، مگر با `[CURRENT]` علامت خورده باشند. شناسه‌ها و مبالغ مثال، داده واقعی Staging نیستند. هر JSON نمایشی فقط فیلدهای لازم سناریو را نشان می‌دهد؛ **جداول فیلدی این سند شکل قرارداد کامل را تعریف می‌کنند**.

## 01. تصمیم اجرایی-تجاری یک‌صفحه‌ای

**حکم طراحی:** یک **Shopping Engine** برای `Offer` و `Order`؛ دو Source Adapter؛ سه Surface مستقل؛ **List سبک ولی قابل انتخاب برای محصولات ساده**؛ Details on demand؛ Batch Evaluate برای همه خدمات شامل SeatMap ID؛ هر Selection پذیرفته‌شده یک Token کوتاه‌عمر **خودتوصیف‌گر، اصالت‌سنجی‌شده و Order-bound** می‌گیرد؛ Ordering آن را از طریق Resolve داخلی دوباره کنترل می‌کند؛ هیچ جدول `PricedSelections`، Aggregate جدید، SeatMap DTO جدید یا Booking در S2 ساخته نمی‌شود.

### 01.1 سه زمان واقعی خرید

| زمان | منبع درست | Stage موتور | ادامه تراکنش |
|---|---|---|---|
| هنوز Order نداریم | priced `OfferId` از AirAvail | `PreOrder` | نمایش/پیش‌قیمت؛ برای الحاق نهایی، پس از ایجاد Order با آن Order تجدید ارزیابی شود |
| Order ساخته شده ولی Issue نشده | `OrderId` از Ordering **ترجیح اصلی** | `PreOrder` | Add Ancillary به Order پیش از پرداخت/Issue؛ Ordering مالک Commit است |
| Order Issue شده و کاربر بعداً برمی‌گردد | `OrderId` از Ordering | `PostTicketed` | OrderChange + پرداخت افزایشی + Reserve/Confirm/EMD متناسب با سرویس؛ S2 فقط مقدمات و Resolve |

**چرا پس از ساخت Order، `OrderId`؟** چون Order مالک مسافران، مبلغ پذیرفته‌شده، وضعیت رزرو/کوپن، تغییرات و خدمات پیشین است. این مانع استفاده از Offer قدیمی برای سفارش تغییرکرده می‌شود. مسیر Offer-based همچنان برای حالت قبل از ساخت Order و سازگاری کلاینت فعلی می‌ماند. هیچ «بازگشت خاموش» از OrderId به OfferId در خطای Ordering مجاز نیست.

### 01.2 تفکیک دقیق مسئولیت

| مالک | Facts/Actions |
|---|---|
| AirAvail/AirPrice | priced Flight Offer، Fare/FareBasis/RBD، allowance، نرخ‌ها و FX؛ جزئیات S2S تا حد Contract واقعی |
| Ordering | Order، مسافر معتبر، OrderService، PNR، OrderCommercialVersion، کوپن‌های صادر/مصرف/Refund، خدمات موجود، Payment/OrderChange/Ticketing orchestration |
| Ancillary S1 | Definition، Provision، Pricing، گزینه‌های Author شده، BookingMethod، Document Routing، محدودیت‌های فروش |
| Ancillary S2 | Context composition، Eligibility، Display Offer، Price/Option/Quote Evaluation، SeatMap facade، توکن انتخاب و Resolve مجدد |
| Ancillary S3 | Hold/Confirm/Cancel و رفرنس Provider یا Local per unit، با سازگاری OrderService؛ نه Payment/EMD generation |
| FlightFlow | physical seat occupancy/layout consistency و Seat Hold؛ Ancillary حق اعلام صندلی خالی از پاسخ ناقص ندارد |
| Payment/Ticketing | دریافت وجه، EMD-A/EMD-S، Exchange، Void و Refund اسناد؛ نه در S2 |

### 01.3 بخش‌هایی که عمداً **نمی‌سازیم**

۱. `PricedSelectionAggregate`, `PricedSelections` و child tables/cleanup/migration مربوط به Quote کوتاه‌عمر. ۲. موتور Shopping دوم برای PostTicketed. ۳. Endpoint عمومی جدا برای تک‌تک A01–A24. ۴. کاتالوگ جعلی Supplier Menu/Appointment/Upgrade. ۵. DTO SeatMap جدید در مسیرهای فعلی. ۶. رزرو صندلی و تضمین ظرفیت حین Shopping. ۷. نوشتن Order در Ancillary یا ویرایش مخزن Ordering توسط ایجنت S2.

### 01.4 مرز بین «خدمت قابل نمایش»، «انتخاب کامل» و «مجاز برای Commit»

- `visible`: قابل نمایش در لیست، حتی اگر هنوز نیاز به Input/Quote/Verification باشد.
- `selectable`: UI می‌تواند گزینه و فرم بدهد؛ **هرگز به‌تنهایی به معنی مبلغ نهایی/امکان خرید نیست**.
- `priceComplete`: مبلغ دقیق *همین Selection* با tax/fee/FX معتبر و بدون ابهام محاسبه شده؛ Order-level shared fees به‌درستی تفکیک شده‌اند.
- `quoteReady`: فیلدها + Commercial Rule + قیمت کامل + Auth + TTL معتبر؛ توکن می‌تواند صادر شود. ممکن است `requiresCheckAtReserve=true` باشد؛ ظرفیت تضمین نشده است.
- `commitReady`: پس از Resolve مجدد و کنترل نسخه Order، Mapping و Acceptance قیمت؛ سپس فقط **Ordering** می‌تواند OrderService را ثبت کند.
- `confirmed/issued`: صرفاً بعد از Stage3/Payment/Ticketing؛ Resolve هرگز این وضعیت را جعل نمی‌کند.

---

## 02. Benchmark مستند و محدودیت استنتاج

### 02.1 شواهد مستقیم

| مرجع و لینک | مشاهده مستند | تصمیم برای AeroTech | چیزی که منبع ثابت نمی‌کند |
|---|---|---|---|
| [Breeze JSON offer ancillaries](https://pro-docs.flybreeze.com/api/json/endpoints/offer-ancillaries) | `POST /offers/{offerId}/services` و `/seats` برای Source نوع Offer، per pax و OfferItem ID | دو منبع مستقل، Seat جدا | ساختار داخلی دیتابیس، Stateless بودن شناسه |
| [Breeze JSON order ancillaries](https://pro-docs.flybreeze.com/api/json/endpoints/order-ancillaries) | `POST /orders/{orderId}/services` و `/seats` برای Order جاری | پس از Order از حقیقت Order بخوان | پشتیبانی همه ۲۴ Variant |
| [Breeze E2E bags & extras](https://pro-docs.flybreeze.com/api/json/sample-flows/add-bags-and-extras) | ServiceList شامل Wi-Fi، bag، priority، SSR؛ `offerItemId` به یک مسافر/سفر مقید؛ Add از طریق AcceptOffer و Order change؛ CB2 بدون CB1 ممکن است رد شود | حفظ per-pax/per-coverage و ExistingServiceFacts؛ Idها هنگام Order commit چک شوند | معنای دقیق Internal ServiceDefinitionRef در AeroTech |
| [Duffel offer available_services](https://duffel.com/docs/api/v2/offers) | قیمت و `passenger_ids`, `segment_ids`, `maximum_quantity`, Service ID در Offer؛ Get Offer با available_services اختیاری | برای ساده‌ها List بدون Details کافی است | تمام محصولات Duffel یا قابلیت PostOrder Seat |
| [Duffel postbooking services](https://duffel.com/docs/api/v2/orders) | `GET /orders/{id}/available_services` و `POST /orders/{id}/services`؛ محدودیت‌های Hold و Supplier | PostOrder Source و Checkout از هم جدا | پشتیبانی postbooking برای همه سفارش‌ها |
| [Duffel Seat Maps](https://duffel.com/docs/api/seat-maps/get-seat-maps) | SeatMap per segment، قیمت/شناسه صندلی، قانون یک صندلی در هر Segment/مسافر و عدم انتخاب یک Seat توسط دو Pax | SeatMap جدا؛ uniqueness در سبد | رزرو صندلی پس از Order در Duffel؛ مستندات آن محدودیت دارد |
| [Travelport Ancillary Shop](https://support.travelport.com/webhelp/JSONAPIs/Airv11/Content/Air11/Ancillaries/APIRef_AncillaryShop.htm) | Shop برای priced Offer، new workbench، existing reservation؛ payloadهای متفاوت، خروجی مفهومی مشابه | دو Adapter و Canonical Context | یکسان‌بودن همه Channel/Providerها |
| [Travelport Ancillary Price](https://support.travelport.com/webhelp/jsonapis/airv11/content/air11/ancillaries/APIRef_AncillaryPrice.htm) | Price پیش از Book برای NDC الزامی؛ Traveler, Product, Quantity و Quote refs؛ حتی تفسیر Qty در kg بین خطوط هوایی متفاوت | Evaluate اجباری، PricingUnit و EffectiveQuantity بر اساس Source | لزوم جدول Quote در Ancillary ما |
| [Travelport Ancillary Book](https://support.travelport.com/webhelp/jsonapis/airv11/content/air11/Ancillaries/APIRef_AncillaryBook.htm) | Book پس از Shop/Price، Seat Book جدا، قابلیت‌های NDC/GDS متفاوت | S2 را با Hold اشتباه نگیریم | تضمین EMD همه Variantها |
| [Breeze NDC E2E extras](https://pro-docs.flybreeze.com/api/ndc-xml/sample-flows/add-bags-and-extras) | ServiceList با OrderRequest یا OfferRequest؛ PaxRefID تا Create/Change ثابت و مرتبط | PNR/Order/Pax identity mapping الزامی | لزوماً یک مدل داخلی مشترک در پیاده‌سازی آنها |
| [Sabre NDC ServiceList](https://developer.sabre.com/rest-api/ndc-it-servicelist/21.3.4/index.html) | وجود API مستقل ServiceList در NDC IT | نقش واضح Discover | پوشش تمامی ۲۴ نوع و قیمت‌گذاری در همه Carrierها |
| [Amadeus developer FAQ](https://admin.developers.amadeus.com/self-service/apis-docs/guides/developer-guides/faq/) | محدودیت و تفکیک flight pricing، baggage و seat در Self-Service | Source capability per provider؛ هیچ ادعای universal | داخلی بودن Altea/PSS و ساختار microservice |

**استنتاج معماری، نه نقل قول Benchmark:** `Stateless pricedSelectionRef`، `Batch Evaluate`، تقسیم UI `Inline/OnDemand` و payload استاندارد AeroTech انتخاب ماست. هیچ سند عمومی نشان نمی‌دهد که Duffel/Travelport/Breeze حتماً Quote را Decode می‌کنند یا در پایگاه‌داده ذخیره نمی‌کنند.

### 02.2 Trade-off اجباری Details

- **۲۰ محصول همگی Details:** ۱ List + ۲۰ Details + ۱ Evaluate + ۱ AddToOrder = ۲۳ تماس client-facing در سناریوی فرضی؛ پرهزینه و بی‌دلیل برای A01/A02/A24 ساده.
- **تمام Detail داخل List:** حجیم‌شدن Responses، materialization پیچیده ۲۰ محصول، مالیات/قرارداد/گزینه پویا و نشست‌های Supplier؛ مخصوصاً API عمومی نامناسب.
- **انتخاب نهایی:** **Progressive Disclosure**؛ List شامل نیازمندی‌های انتخاب و گزینه‌های کم‌حجم/ثابت؛ Detailed Rules/Options با یک Call **هنگام بازکردن** محصول پیچیده. Browser/API هرگز ۲۰ Details خودکار نفرستد. یک Batch Evaluate پس از انتخاب چند کالا؛ Error به تفکیک Line.
- Inline eligibility **وابسته به داده واقعی Candidate** است، نه `if VariantCode in [...]`: همان Lounge گاهی `inline` و گاهی `detailsRequired` می‌شود. محدودیت انتشار گزینه در List یک Budget صریح presentation است، نه Policy دامنه؛ پیشنهاد اولیه `maxInlineChoices=12` و `maxInlineOptionPayloadBytes=4096` (Configurable، نه عدد Benchmarked). اگر بیشتر شد فقط `detailsRequired=true`, `choices` truncated **نباید معنای "همه گزینه‌ها" بدهد**.

### 02.3 کنش‌های UI

- **List**: تنها Caller Source (`offerId` یا `orderId`) + محدوده اختیاری احرازشده؛ تمام گزینه‌های Inline و مسیر ادامه نمایش داده می‌شوند.
- **Details**: فقط با کلیک/بازکردن محصول نیازمند جزئیات، و تنها همان Candidate؛ Cache در Client با `sourceVersion`/Expiry تا تغییر Source.
- **Evaluate**: Batch همه انتخاب‌ها بعد از پایان سبد، با امکان Re-Evaluate یک Line هنگام تغییر؛ قیمت کل Basket فقط اگر shared fees reconciliation کامل است.
- **Ordering Add**: فقط پس از تأیید خلاصه قیمت از سوی مشتری؛ Resolve داخلی بلافاصله و سپس ثبت OrderChange/OrderService.

---

## 03. جریان‌های E2E — Trace دقیق فراخوانی

### 03.1 جریان A: Order ساخته شده، هنوز Issue نشده

```mermaid
sequenceDiagram
    autonumber
    actor C as Customer/UI
    participant O as Ordering
    participant A as Ancillary S2
    participant P as Pricing/Definition
    participant F as FlightFlow
    C->>O: CreateOrderFromOffer(offerId,travellers,contacts)
    O-->>C: orderId + orderVersion + stage=PreOrder
    C->>A: POST /{S}/v1/Orders/{orderId}/AncillaryOffers
    A->>O: S2S GetOrderAncillaryContext(orderId)
    O-->>A: verified Order/Pax/Flight/Fare/SalesScope
    A->>P: evaluate active definitions/provisions/prices
    P-->>A: candidates + terms + options
    A-->>C: List (inline requirements/price preview/candidate refs)
    opt Product needs detail
       C->>A: POST /{S}/v1/AncillaryOffers/Details {orderId,offerItemRef}
       A-->>C: full rule/terms/options
    end
    opt User chooses seats
       C->>A: POST /{S}/v1/Orders/{orderId}/SeatMaps
       A->>F: trusted occupied seats
       F-->>A: complete occupancy state
       A-->>C: FROZEN SeatMapsDto with travelerPricing[].id
    end
    C->>A: POST /{S}/v1/AncillaryOffers/Selections/Evaluate {orderId,selections[]}
    A-->>C: line results + signed pricedSelectionRefs + amount
    C->>O: AddAncillaries(orderId,pricedSelectionRefs,idempotencyKey)
    O->>A: POST /Service/v1/Ancillaries/Selections/Resolve {orderId,refs,version}
    A->>O: Revalidate Order Context + price + seat; Valid/Stale/etc
    O-->>C: accepted OrderChange/OrderServices OR reprice/error
    Note over O,F: Reservation/Payment/Issue outside S2
```

**ترتیب:** Order ابتدا ایجاد می‌شود، اما صرف ایجاد Order به معنی Flight Issue، Ancillary Hold یا پرداخت نیست. `Order` ممکن است با عملیات دیگری تغییر کند؛ Resolve باید `orderCommercialVersion` فعلی را بررسی کند. Mapping `AirOfferTravellerRef ↔ OrderTravellerId` از Ordering معتبر دریافت شود، نه از نام مسافر یا ترتیب تصادفی.

### 03.2 جریان B: PostTicketed

```mermaid
sequenceDiagram
    autonumber
    actor C as Customer/UI
    participant A as Ancillary S2
    participant O as Ordering
    participant F as FlightFlow
    C->>A: POST /{S}/v1/Orders/{orderId}/AncillaryOffers
    A->>O: S2S GetOrderAncillaryContext(orderId)
    O-->>A: ticket/coupon status + existing services + versions
    A-->>C: per-Pax services eligible for PostTicketed
    opt Seat requested
        C->>A: POST /{S}/v1/Orders/{orderId}/SeatMaps
        A->>F: occupancy / layout
        A-->>C: SeatMapsDto
    end
    C->>A: Batch Evaluate(orderId,selections)
    A-->>C: accepted refs / rejected / quote-required
    C->>O: POST AddAncillaries(orderId,refs)
    O->>A: S2S Resolve refs + orderId + orderVersion
    A-->>O: Valid / Stale / NotEligible / NeedsQuote
    O-->>C: New OrderChange/Services, financial workflow starts
    Note over O: Capture/refund/EMD-A/EMD-S/exchange depend on service; outside S2
```

**PostTicketed فقط عوض‌شدن URL نیست.** Used/Flown/Refunded/Void coupon، پنجره زمانی خرید، منشأ Refund/EMD قبلی، محاسبه موجودی مصرف‌شده و پذیرش Supplier متفاوت‌اند. A10 ممکن است به Ticket Exchange نیاز داشته باشد؛ A20 appointment پس از عبور زمان پذیرش ندارد؛ یک خدمت قبلاً صادرشده نباید دوباره با ID تازه شارژ شود.

### 03.3 جریان C: هنوز Order نداریم (Preview/Prebook)

`POST /{S}/v1/AncillaryOffers {offerId}` و SeatMap موجود قابل استفاده‌اند. مشتری می‌تواند پیش‌نمایش خدمات/قیمت ببیند و حتی Evaluate کند؛ اما برای **Add به Order در مدل منتخب** بعد از CreateOrder باید دوباره با `orderId` Evaluate انجام دهد و Token **Order-bound** دریافت کند. این یک محدودیت عمدی برای جلوگیری از replay یک Token آزاد به چند Order است. اگر محصول اجازه نمی‌دهد Order بدون پرداخت ساخته شود، این **وابستگی طراحی به Ordering** است و باید Owner در قرارداد CreateOrder/State مشخص کند؛ نباید در Ancillary خودمان یک OrderPreparation اختراع کنیم.

### 03.4 حالات خطا/توقف Flow

- تغییر قیمت/FX/Revision/Offer/Order/coupon/seat => Resolve `Stale`, UI به Re-Evaluate و بازتأیید مبلغ برمی‌گردد.
- Supplier Quote ناموجود => `NeedsQuote` و no pricedSelectionRef؛ نه Price=0.
- seat `OCCUPIED`/source incomplete => No Token، حتی اگر UI قبلاً آن را آزاد دیده بود.
- یکی از چند Selection نامعتبر => Batch Evaluate نتایج تفکیکی؛ **هیچ Add خودکار subset**؛ اصلاح/حذف Line و تأیید مجدد Basket لازم است.
- Order تغییر کرده => تمام Tokenهای Order-bound به نسخه قبلی Stale؛ هیچ Snapshot دوباره استفاده نمی‌شود.
- یک Request Add دوبار تکرار شود => Ordering idempotency و dedup transaction؛ S2 token به‌تنهایی exactly-once را تضمین نمی‌کند.

## 04. Inventory دقیق Endpointها: موجود، توسعه و جدید

`{S} ∈ {Backoffice,Api,OtaPanel}`. تمام Public routes با Authorization متناسب Surface و بدون امکان جعل Agency/POS در body. نام‌های جدید در این فصل «پیشنهادی TARGET» هستند؛ بعد از تأیید مالک باید یک‌بار Contract Freeze شوند.

| عملیات | Method + Path | حالت | Request/Response اصلی |
|---|---|---|---|
| Shop with Offer | `POST /{S}/v1/AncillaryOffers` | CURRENT + additive response | `{offerId, options?}` → `SurfaceAncillaryOffersDto` |
| Shop with Order | `POST /{S}/v1/Orders/{orderId}/AncillaryOffers` | **NEW TARGET** | `{options?}` → همان Surface DTO با `source.type=Order` |
| Details | `POST /{S}/v1/AncillaryOffers/Details` | CURRENT + Order source | `{offerId XOR orderId,offerItemRef,options?}` → Surface Detail DTO |
| Evaluate single | `POST /{S}/v1/AncillaryOffers/Selections/Evaluate` | CURRENT legacy | `{offerId,offerItemRef,selection,options?}` → Single Surface DTO |
| Evaluate batch | **همان** `POST /{S}/v1/AncillaryOffers/Selections/Evaluate` | **NEW TARGET additive** | `{offerId XOR orderId,selections:[...]}` → `SurfaceBatchEvaluationDto` |
| SeatMap with Offer | `POST /{S}/v1/SeatMaps` | CURRENT **FROZEN-WIRE** | `{offerId,options?}` → همان `SeatMapsDto` |
| SeatMap with Order | `POST /{S}/v1/Orders/{orderId}/SeatMaps` | **NEW TARGET (conditional)** | `{options?}` → همان `SeatMapsDto`، نه تغییر در شکل فعلی |
| Resolve | `POST /Service/v1/Ancillaries/Selections/Resolve` | **NEW TARGET S2S** | `{orderId,expectedOrderCommercialVersion,pricedSelectionRefs[],correlationId}` → per-line resolved services |
| Order context | `POST /Service/v1/Orders/AncillaryShoppingContext` | **EXTERNAL CONTRACT only** | `{orderId}` → `OrderAncillaryContext` |
| Create initial Order | `POST /{S}/v1/Orders/FlightOffers` برای Backoffice؛ سایر مسیرهای واقعی طبق Controllerهای Ordering | **PRIOR-AUDIT** | current CreateOrder از Offer بدون Ancillary list |
| Add to Order | `POST /{S}/v1/Orders/{orderId}/Ancillaries` | **EXTERNAL TARGET only** | `{pricedSelectionRefs[],expectedOrderCommercialVersion,idempotencyKey}` |
| AirAvail offer details | `POST /Service/v1/FlightOffers/Details` | **PRIOR-AUDIT** | `{offerId}` → priced air offer details |
| Stage3 ancillary hold | `POST /Service/v1/Ancillaries/Service-Holds` | CURRENT؛ خارج S2 | `HoldAncillaryServicesRequest` از Ordering |

**چرا دو URL برای Source List، ولی فقط یک Details/Evaluate؟** دو مبنای امنیتی مستقل و واضح برای پیدا کردن منبع (Breeze/Travelport) در برابر یک use case واحد Configure/Price. ایجاد `/ByOrderId` سوم یا موتور جدا بی‌دلیل است. قرارداد Order-aware برای `Details/Evaluate` با `offerId XOR orderId` و همان Path انجام می‌شود. چنانچه route جدید Order-based در host با path Ordering مشابه باشد، این دو سرویس host/base address جدا دارند؛ reverse proxy باید صریحاً map کند، نه به حدس.

### 04.1 Shared request envelope و قواعد Wire

| JSON field | Type | Null/Required | قانون |
|---|---|---|---|
| `offerId` | `string` | فقط در Offer source الزامی | همان priced OfferId صادرشده توسط AirAvail، نه search OfferId |
| `orderId` | `int64` به‌صورت string یا عدد طبق converter سرویس | فقط در Order source الزامی | >0؛ Caller مجاز، Owner از Ordering |
| `offerItemRef` | string | Details و Evaluate single الزامی | شناسه opaque حاصل List همان Scope |
| `options` | `BackofficeAncillaryOfferOptions` | Backoffice اختیاری؛ API/OtaPanel ممنوع | `countryId?:long`, `pointOfSaleId?:long`؛ باید مجاز/سازگار با Source باشد |
| `selections` | array | Batch الزامی، حداقل ۱ | سقف تعداد Configurable؛ پیشنهاد ۱–۵۰ Line، Payload limit صریح |
| `clientSelectionId` | string | هر Batch Line الزامی | Unique per request، حداکثر ۶۴ char، echo برای نگاشت UI؛ **شناسه OrderService نیست** |
| `kind` | `"ancillary"|"seatMap"` | هر Line الزامی | A07/A08 فقط seatMap؛ A09 generic ancillary |
| `selection` | typed object | برای kind=ancillary الزامی | فیلدهای مطابق `selectionContract` تعریف‌شده؛ unknown fields رد شوند |
| `seatMapSelectionId` | string | فقط kind=seatMap الزامی | همان `travelerPricing[].id` ۴۳کاراکتری، نباید به Route/DTO SeatMap دست زد |
| `selectedOptionRefs` | map<string,string> | اختیاری؛ وقتی option enumerate شده باید match کند | کلید نام فیلد JSON؛ مقدار OptionRef صادرشده؛ عدم تطبیق 422 |
| `expectedOrderCommercialVersion` | string | در Add و Resolve الزامی | نسخه واقعی از Ordering؛ نسخه Client تنها concurrency hint است |
| `correlationId` | string | برای داخلی الزامی | UUID/trace compatible، برای observability نه Idempotency business |
| `idempotencyKey` | string | برای Ordering Add الزامی | در سطح `(OrderId,Operation,PayloadHash)` در Ordering guard می‌شود |

Request با هر دو `offerId` و `orderId` یا هیچ‌کدام 422؛ برای مسیر `Orders/{id}`، `orderId` از URL استخراج می‌شود و تکرار نامنطبق آن در Body باید 422 باشد. Api/OtaPanel حق ارسال `agencyId`, `buyerId`, `ownerAirlineId`, `pointOfSaleId`, `fulfillmentProviderKey`, `currencyOverride` در Body ندارد. Frontend ممکن است برای نمایش Currency درخواست دیگری بدهد، اما Quote به Currency تأییدشده Source/Policy تعلق دارد.

---

## 05. Canonical Context — مهم‌ترین قرارداد درونی S2

### 05.1 Adapterها و صحت منبع

- `AirOfferShoppingContextReader` `[CURRENT]`: FlightOffer Details + fares/baggage/FlightFacts/ROE؛ منبع OfferId، SourceKind=`Offer=1`, `ShoppingStage=PreOrder`.
- `OrderShoppingContextReader` `[TARGET]`: `IOrderCommercialContextReader` از Ordering؛ SourceKind=`Order=2`; ShoppingStage از وضعیت **تأییدشده** Order، نه نوع ورودی و نه متن Caller.
- **یک `CanonicalAncillaryShoppingContext`** که پس از تبدیل، موتور Eligibility/Quantity/Pricing/Inventory هر دو منبع را یکسان می‌بیند. تفاوت Fact completeness حفظ می‌شود؛ پرکردن Fake DTO برای رسیدن به یک interface ممنوع است.

### 05.2 Field dictionary `CanonicalAncillaryShoppingContext` [TARGET]

| Field canonical | Type | Owner | اعتبارسنجی/مصرف |
|---|---|---|---|
| `source.kind` | `Offer|Order` | source adapter | enum موجود `ShoppingSourceKind {Offer=1,Order=2,Direct=3}`؛ Direct در این فلو ممنوع |
| `source.reference` | string | AirAvail/Ordering | OfferId یا OrderId، canonical بدون حدس |
| `source.orderId` | long? | Ordering | فقط Order type؛ >0 |
| `source.originalPricedOfferId` | string? | Ordering | Link واقعی Order←Offer؛ نبود آن نباید با ساخته‌شده جایگزین شود |
| `source.commercialVersion` | string? | Ordering | نسخه معتبر وضعیت Order؛ در Resolve Order لازم، عدم وجود = fail closed برای Commit |
| `source.fingerprint` | string | Adapter | hash deterministic از همه Facts مؤثر بر eligibility/pricing/coverage/seat |
| `source.evaluatedAtUtc` | DateTimeOffset | سرور Ancillary | UTC؛ SalesDate/`PricedAt` ≠ زمان quote issuance |
| `source.sourceValidUntilUtc` | DateTimeOffset? | source | اگر publisher اعلام نکرد، `null`؛ زمان اعتبار مصنوعی به Provider نسبت داده نشود |
| `source.quoteValidUntilUtc` | DateTimeOffset | Ancillary | min(local short TTL, existing upstream expires, provider expires), never > any valid source |
| `source.issuedAtUtc` | DateTimeOffset? | Ordering | صرفاً postticketed؛ رکورد واقعی ticket/document |
| `ownerAirlineId` | int | Auth/Ordering | برابر Owner فروش و Catalogue |
| `channel` | authorized enum | Auth/Ordering | از JWT/Trusted Order، نام‌مبنا بین AirAvail و Ancillary؛ raw numeric Producer را یکسان فرض نکن |
| `customerId` | long? | Auth/Ordering | mismatch رد؛ null Caller نباید Customer مشخص Offer دیگری را به‌طور ضمنی مجاز کند |
| `travelAgencyId` | long? | Auth/Ordering | لازم در Agency surfaces؛ bind refs/token |
| `airlineOfficeId` | long? | Auth/Ordering | Backoffice; bind refs/token |
| `sellingOfficeId` / `pcc` | long?/string? | Ordering/Auth | فقط internal و هنگام تایید؛ در API عمومی نشت نکند |
| `pointOfSaleId` | long | Auth/ReferenceData | هر Candidate به یک POS مشخص؛ multiple eligible POS بدون انتخاب مجاز نباید random-first شود |
| `countryId` | long? | Auth/Ordering | currency, sales restriction; verified or NeedsVerification |
| `currencyId`, `currencyCode` | int/string? | AirAvail/Ordering | quote currency؛ ROE verified |
| `travellers[]` | collection | AirAvail/Ordering | stable refs، PTC، age/DOB فقط اگر verified، infant-adult mapping، OrderTravellerId |
| `bounds[]` | collection | source | boundRef, origin/destination, ordered flights |
| `flights[]` | collection | source+FlightFlow | flightRef, actual FlightId, **FlightCapacityId**, FlightVersion, aircraft, operating/marketing airline, cabin/RBD, timestamps/timezones |
| `fareFacts[]` | collection | AirPrice via source | by Traveller×Flight/Coupon: AirFareId, FareBasis, FareFamilyId if verified, RBD, coupon IsUpgradable |
| `allowanceFacts[]` | collection | AirAvail/Ordering | checked/cabin baggage included per pax/flight, sourceComplete |
| `ticketCouponFacts[]` | collection | Ordering postorder | ETicket/EMD coupon state `Open/Used/Flown/Refunded/Voided/...` source status; only eligible future coverage |
| `existingServices[]` | collection | Ordering | OrderServiceId, variant, beneficiary, scope, quantities/counting family, status, EMD/document consumed, seat assignments |
| `usageEvidence[]` | collection | Ordering + Inventory authority | verified remaining/used under configured family/period; unknown must not become 0 |
| `fareEntitlements[]` | collection | validated Fare source | verified included benefits; if missing then `FareBenefitsComplete=false` |
| `seatFacts[]` | collection | FlightFlow/AirInfo | occupancy/layout/seat characteristics, evidence freshness; only SeatMap/Evaluate-seat context |
| `catalogSourceVersion` | string? | Ancillary S1 | actual Definition/Provision/Pricing revision/version, not only dates |
| `completeness` | typed evidence flags | Adapter | `TravellersComplete,ItineraryComplete,FareFactsComplete,BaggageFactsComplete,FareBenefitsComplete,ExistingServicesComplete,CouponsComplete,PriceComplete,SeatOccupancyComplete` — never force true |

**مرز Mapping هویت:** در source Offer، `TravellerRef` می‌تواند `ADT-1` یا رفرنس AirAvail باشد؛ در Order، ID پایدار `OrderTravellerId` دارد. Ordering باید جدول نگاشت معتبر `OrderTravellerId ↔ OfferTravellerRef` فراهم کند. از index مسافر، Passport/Name/DOB یا شباهت PTC هویت نسازید. برای Airline multiple segments، `FlightId` (نمونه عملیاتی)، `FlightCapacityId` (موجودی/hold)، `FlightRef` (wire coverage)، `SegmentId` (سطح API) و `BoundRef` **یک چیز نیستند**؛ تبدیل باید explicit باشد.

### 05.3 قرارداد موردنیاز Ordering برای `GetOrderAncillaryContext` [EXTERNAL-CONTRACT]

**نام پیشنهادی:** `POST /Service/v1/Orders/AncillaryShoppingContext`; Body `{ "orderId": 12345 }`. **هیچ ادعایی درباره وجود کنونی این route نداریم.** این S2S API فقط به Ancillary با Service Identity مجاز پاسخ می‌دهد؛ امکان دسترسی مستقیم Agency با OrderId تصادفی وجود ندارد.

```json
{
  "orderId": 12345,
  "orderCommercialVersion": "V17",
  "status": "Created",
  "purchaseStage": "PreOrder",
  "originPricedOfferId": "<original-offer-id>",
  "ownerAirlineId": 100,
  "sales": {
    "channel": "AgencyPanel",
    "customerId": 501,
    "travelAgencyId": 900,
    "airlineOfficeId": null,
    "pointOfSaleId": 7001,
    "countryId": 98,
    "currencyId": 70
  },
  "travellers": [{"orderTravellerId": 11, "sourceTravellerRef": "ADT-1", "ptc": "ADT", "dateOfBirthVerified": false, "associatedAdultRef": null}],
  "portions": [{"portionRef": "B1", "flightRefs": ["B1:F1"]}],
  "flights": [{"flightRef": "B1:F1", "flightId": 1001, "flightCapacityId": 2001, "flightVersion": 3, "cabinClassId": 1, "departureAt": "2027-03-01T08:00:00Z", "arrivalAt": "2027-03-01T10:00:00Z"}],
  "coupons": [{"orderTravellerId": 11, "flightRef": "B1:F1", "couponStatus": "Open", "fareBasis": "Y123", "isUpgradable": false}],
  "allowances": [{"orderTravellerId": 11, "flightRef": "B1:F1", "checkedPieces": 1, "checkedWeightKg": 23}],
  "existingServices": [],
  "fareBenefits": [],
  "completeness": {
    "travellers": true,
    "flights": true,
    "coupons": true,
    "existingServices": true,
    "fareBenefits": false,
    "usageEvidence": false
  }
}
```

**تمام فیلدهای بالا پیشنهادی‌اند؛** Ordering نباید JSON بالا را بدون Map به Entites واقعی v1 کورکورانه اجرا کند. `status=Created` و `purchaseStage=PreOrder` فقط وقتی قابل قبول است که در State Machine واقعی Ordering معادل معتبر داشته باشد. برای سفارش Partial-ticketed، enum دوبخشی کافی نیست: سرور باید Scope/coupon-specific Eligibility را تأمین کند و پاسخ خرید در حالت مخلوط تا وجود Facts کافی `NeedsVerification` باشد.

### 05.4 موانع حقیقی در Source فعلی و سیاست برخورد

1. [CURRENT] `FlightOfferShoppingSource.LoadAsync` زمان Source TTL را از `serverNow + SourceContextTtl` می‌سازد؛ این ضمانت AirAvail برای دوام Fare نیست. Validate again در Resolve الزامی است.
2. [CURRENT] `FlightOfferFingerprint.Of` coupon allowances/flags/FX را وارد کرده ولی `FareComponent.CabinClassId`, `RbdId`, `BookingClass`، بعضی sales scope facts، ticket identity details و version coverage را کامل fingerprint نمی‌کند. اثر هر فیلد eligibility/price/seat بر fingerprint باید تست شود.
3. [CURRENT] `PricedOfferScopeGuard` وقتی `scope.CustomerId` null باشد CustomerId مشخص Offer را الزاماً رد نمی‌کند؛ سیاست Channel/Customer/Order identity باید source-grounded باشد و fail closed.
4. [PRIOR-AUDIT] raw Channel values AirAvail و Ancillary هم‌نام ولی عددهای متفاوت دارند؛ OtaPanel AirAvail ممکن است Channel.API حمل کند. تا Contract روشن نشود نگاشت بر پایه **نام و semantics مسیر actual producer** باشد، نه enum cast و نه تغییر Channel برای دورزدن mismatch.
5. [BLOCKED-BY-SOURCE] AirPrice FareFamily free-seat/lounges flags به معنی entitlement verification کامل نیست؛ تا evidence معتبر نداریم `FareBenefitsComplete=false` بماند.
6. [EXTERNAL-CONTRACT] Ordering `OrderCommercialVersion`، SourceTravellerRef mapping، Coupon status و valid Source Sale Scope را باید واقعی بدهد؛ صرف `orderId` و `OfferId` در Request کافی نیست.

---

## 06. List و Details — قرارداد نمایشی بدون N×Details

### 06.1 DTO مرکزی Candidate (`CanonicalShoppingOfferItem`) [TARGET]

| Field | Type | Required | معنی/قاعده |
|---|---|---|---|
| `offerItemRef` | string | Y | SHA opaque فعلی؛ bind به SourceVersion/Owner/POS/Agency/office/Scope/Definition/Provision/Pricing؛ **Decode نمی‌شود** |
| `serviceDefinitionRef` | string | Y | SKU داخلی منتشرشده؛ A02 package و A24 plan هر کدام یک Definition مستقل |
| `variantCode` | string `A01..A24` | Y | Registry موجود؛ A07/A08 از generic List حذف، A09 باقی |
| `profile` | string/enum | Y | ۹ Profile موجود؛ Surface آن را مستقل map کند |
| `name`,`description` | string,string? | Y,N | Commercial title، short description بدون private provider metadata |
| `eligibility` | `{state,reasons[],missingFacts[]}` | Y | Eligible/NotEligible/InsufficientEvidence، جدا از CanBuy |
| `coverage` | `{scope,travellerRefs[],flightRefs[],portionRefs[]}` | Y | sorted, unique; Order-scope فقط A22/A24 مطابق Provider policy |
| `selectionContract` | `{kind,fields[]}` | Y | فقط CustomerInputهای Domain؛ Coverage refs در canonical item نه input آزاد |
| `quantity` | `{unit,min,max,step,required,effectiveQuantityRule}` | Y | `step=1` برای count، weight decimals rule جدا؛ دسترسی مستقیم UI |
| `displayOptions[]` | `{fieldName,sourceState,totalChoiceCount?,isComplete,choices[]}` | Y | فقط known authored short options داخل List؛ omission به معنی Unknown نیست |
| `interaction` | `{mode,requiresDetails,detailsReason?,requiresSeatMap}` | Y | `Inline|DetailsOnDemand|SeatMap|RequiresExternalQuote|Unavailable` presentation-only |
| `pricePreview` | `{currencyId,currencyCode?,pricingUnit,unitAmount?,amountKind,isComplete,taxSummary?,feeSummary?}` | Y | indicative/completely-known **unit**؛ totalQty غالباً پس از Evaluate |
| `availability` | `{authority?,state,isGuaranteed,requiresCheckAtReserve}` | Y | `AVAILABLE` seat معنای خودش؛ ancillary availability هیچگاه guaranteed unless evidence |
| `readiness` | string | Y | فعلی `NotEligible,NeedsSelection,NeedsQuote,NeedsVerification,Selectable,Unavailable` |
| `canSelect` | bool | Y | می‌توان فرم/گزینه را انتخاب کرد، حتی وقتی Qty هنوز تعیین نشده |
| `canPurchase` | bool | Y | **فقط** انتخاب کاملاً مشخص و priced، نه generic candidate با missing required input |
| `reasonCodes[]` | string[] | Y | public safe یا full diagnostics بسته به Surface |
| `commercialSummary` | `{purchaseStage,confirmationRequirement,refundability?,documentType?,cancellationSummary?,cutoffAt?}` | Y | مختصر؛ عدم وجود filed cancellation rule مساوی penalty=0 نیست |
| `sourceVersion`,`validUntilUtc` | string?,DateTimeOffset? | Y | زمان ارائه/آخرین اعتبار known، نه تعهد capacity |

**List گزینه‌ها را چگونه حمل کند؟** برای فیلد required با انتخاب از کاتالوگ verified و `totalChoices<=12` و payload<=4KB، `displayOptions.choices[]` شامل `{optionRef,value,label,sourceVersion,availability,applicability,pricePreview?}` و `isComplete=true` است. وقتی بزرگ‌تر شد، `choices=[]`, `isComplete=false`, `requiresDetails=true`; نه اعلام «موردی موجود نیست». وقتی upstream option منبع ندارد `sourceState=Unknown`, `isComplete=false`, reason code و no purchasable token.

### 06.2 Details [TARGET additive]

Details برای Candidate واحد، تمام fields بالا را با شرح بیشتر می‌آورد: `fullSpecification`, `includedAllowance[]`، `baggageRule`, `seatRule`, `restrictions` تاریخ/blackout/daytime/seasonality، `commercialTerms` دارای `purchaseStage,disposition,reissueRefund,formOfRefund,confirmationRequirement,bookingMethod,documentRouting,documentType,rfic,rfisc,cancellationRules[]`، `publishedRates[]` (Backoffice)، `optionGroups[]` با کامل‌ترین Choiceها، `priceCompleteness`, `availability`, `requiresSelectionInputs[]`، `evidenceMissing[]`. **Details نباید به‌تنهایی Quote خارجی را جعل کند**؛ dynamic provider lookup اگر موجود باشد باید SourceVersion/TTL/Signature معتبر ثبت کند، وگرنه `NeedsQuote`.

`optionGroups[].choices[]` بر اساس Source واقعاً مجاز؛ برای `measuredWeightKg`, `dimensions`, `dateWithOffset`, `animalAgeWeeks` که ورودی تایپی‌اند OptionRef مصنوعی نسازید. `A02.PackageProductRef` و `A24.PlanCode` برابر DefinitionRef موجود، A12.MenuItemRef فقط authored، A18/19 TravellerRefs فقط از Source واقعی، A20/A22 Facility فقط authored، `A07/A08` اصلاً generic Details محصول نیستند.

### 06.3 اختلاف Surface و Compatibility

| Surface | CURRENT envelope / row | TARGET افزایشی | فیلد ممنوع برای مشتری |
|---|---|---|---|
| **Backoffice** | `BackofficeAncillaryOffersDto`: `offerId,evaluatedAtUtc,currency,flightOffer,scope,journey,travellers,categories,offers[],blockedOffers[],diagnostics[],ratesOfExchange[],completeness`. Row=`offerItemRef,pointOfSaleId,candidateIdentity,service,coverage,eligibility,selection,quantity,pricing,availability,confirmation,document,offerReadiness,displayState,canPurchase,reasonCodes` | `source`, `interaction`,`displayOptions`, `commercialSummary` و `canSelect` فقط additive؛ diagnostics با دسترسی Role مجاز | اطلاعات PII غیرمرتبط و Provider credential |
| **Api** | `{meta,data:{type,flightOfferId,travelers[],segments[]},included:{bags:{ref:ApiBagDto},"other-services":{ref:ApiOtherServiceDto}}}`؛ هر مورد `ancillary:{service,pricing,quantity,selectionRequirements,availability,confirmationRequirement,readiness,canPurchase,reasons,selectionKind,coverageScope}` | حفظ دقیق envelope برای Offer-based؛ هر ancillary: `interaction,displayOptions,commercialSummary,canSelect`, top-level `source:{type,id,version?}`، Order-based `flightOfferId?=null` در صورت نبود origin واقعی؛ **نه مقدار جعلی** | `ProvisionId`, internal `SupplierId`, `QuoteProviderKey`, Airline Office/PCC، private rule diagnostics |
| **OtaPanel / Website-facing** | `{offerId,currencyId,currencyCode,travellerGroups:[{flightGroups:[{services:[]}]}],blockedCount,ratesOfExchange,orderScopedServices[]}`؛ row `offerItemRef,category,variantCode,title,description,coverageScope,coveredFlightRefs,priceStatus,priceUnit,unitPrice,rateOfExchangeTableId,orderLevelFees,quantityUnit,minQuantity,maxQuantity,requiredInputs,displayStatus,requiresRecheck,canPurchase,selectionKind,availabilityState,offerReadiness,reasonCodes,taxes,fees` | row: `displayOptions,interaction,selectionFields,commercialSummary,canSelect`، top-level `source`; Order scope once only؛ preserve older fields for Offer-based | Supplier secrets/Office internals |

**Wire compatibility rule:** خروجی Offer-based موجود از نظر نام/نوع و مقدار فیلدها شکسته نشود، مگر Bugfix مستند `canPurchase`/Price/Seat statuses. اگر `flightOfferId` فعلی برای Order-based قابل تولید نیست، فیلد نباید `""`/OrderId ساختگی بگیرد: target Order response با `source` معتبر و `flightOfferId:null` فقط در مسیر جدید/حالت جدید منتشر شود؛ برای کلاینتی که non-null بودن را شرط گرفته، نیاز به versioned documentation و contract test است. راه ساده‌تر در OtaPanel نیز همین افزودن `source` و حفظ `offerId` به‌صورت nullable **فقط** در Source جدید است؛ برای Offer source همان مقدار موجود بماند.

### 06.4 Human-readable Partner API، بدون تحمیل UI داخلی

JSON خارجی باید علاوه بر shape فعلی شامل نام human readable، `category`, `code`, `applicability:{passengerIds[],segmentIds[]}`, `price:{currency,totalAmount?,amountType}`, `selection:{kind,requiredFields[],options[]}`, `availability:{status,checkedAt?,requiresRecheck}`, `conditions` مختصر، `errors/reasons` string-based و `links` یا `requiresDetails` باشد. `meta` و `included.bags/other-services` فعلی تغییر نکند؛ Properties **اضافه** شوند. نام‌های دامنه `FareFamily`, `SSR`, `RFIC/RFISC` فقط در صورت نیاز متخصص API و با تعریف/توضیح قابل فهم در docs بیایند. اختلاف Case enumهای داخلی به API Public منتقل نشود؛ public codes versioned و stable باشند.

### 06.5 کنترل اندازه پاسخ/Performance بدون اختراع cache مالی

- برای هر Source یک‌بار Flight/Order Context read و یک‌بار Source Catalog Snapshot؛ N+1 DB برای هر row ممنوع. Query active Definitions/Provisions/Pricing batch.
- Pricing read برای ۲۰ row باید grouped/batched باشد؛ هر candidate نیازمند Supplier dynamic options نشود مگر کاربر وارد Details شده.
- Client با ۲۰ row، **یک** List؛ کلیک تنها یک Details برای همان row؛ چند Selection یک Batch Evaluate. Query cache در UI فقط تا TTL/sourceVersion، و re-evaluation همیشه server authoritative.
- `displayOptions` کافی نیست؟ UI دکمه «جزئیات» نشان دهد. اگر List جواب `NeedsQuote` دارد قیمت نهایی نمایش ندهد؛ فقط indicative/readiness.
- Failure یکی از Supplierها نباید ۲۰ Candidate سالم را غیب کند، مگر Cross-source قاعده global (Customer/Order security) شکسته باشد؛ partial completeness با diagnostics و fail-closed برای Candidateهای متاثر.

---

## 07. `Selections/Evaluate` — قرارداد فیلدبه‌فیلد خرید واقعی

### 07.1 Current single request و قانون سازگاری

[CURRENT] در هر سه Surface: `{offerId:string?, offerItemRef:string?, selection:...}` و برای Backoffice `options?`. [CURRENT] پاسخ Single ساختار Surface-specific دارد و `pricedSelectionRef`/`validUntilUtc` همیشه null هستند. **نه قرارداد ورودی Multi-Selection و نه SeatMap ID در Evaluate فعلی وجود ندارد.**

[TARGET] همان Endpoint در صورت وجود `selections`، Batch DTO را مصرف می‌کند. **Request تک‌انتخاب قدیمی** همچنان همان Single Response قبلی را می‌گیرد (با فیلد Token به‌صورت additive)؛ دو شکل request هم‌زمان غیرمجاز و 422. Batch فقط یک Source (`offerId XOR orderId`) و یک SalesScope دارد؛ Split by Source ممنوع.

### 07.2 Batch request — نوع فیلدها [TARGET]

```json
{
  "orderId": 12345,
  "selections": [
    {
      "clientSelectionId": "L1",
      "kind": "ancillary",
      "offerItemRef": "<A01-for-ADT1-B1>",
      "selection": {"quantity": 2},
      "selectedOptionRefs": {}
    },
    {
      "clientSelectionId": "L2",
      "kind": "ancillary",
      "offerItemRef": "<A01-for-ADT2-B1>",
      "selection": {"quantity": 1},
      "selectedOptionRefs": {}
    },
    {
      "clientSelectionId": "L3",
      "kind": "seatMap",
      "seatMapSelectionId": "<travelerPricing.id-issued-by-SeatMaps>"
    }
  ]
}
```

| Batch line Field | Type | Required/Default | Guard |
|---|---|---|---|
| `clientSelectionId` | string | Required | unique 1..64، echo، هیچ مجوزی نمی‌دهد |
| `kind` | `ancillary` or `seatMap` | Required | دقیقا یک discriminator |
| `offerItemRef` | string? | Ancillary only | مال همان Source/Scope/POS و فعال |
| `selection` | `AncillarySelectionInput`? | Ancillary only | همه required fields مندرج در Definition Contract؛ no arbitrary fields |
| `selectedOptionRefs` | Dictionary<string,string>? | When chosen enumerated option | reference و selected value باید به یک Choice با SourceVersion یکسان اشاره کنند |
| `seatMapSelectionId` | string? | Seat only | ۴۳کاراکتر URL-safe hash CURRENT؛ no decoded user claims؛ match از map موجود بازسازی‌شده |
| `travellerId`/`flightId`/`seatNumber` | — | **Not accepted as authority** | Server از reference معتبر استخراج/بازسازی می‌کند؛ اگر UI برای نمایش بفرستد تنها hint و cross-check است |

**Count semantics:** برای دو مسافر با همان Baggage SKU، دو line مستقل؛ برای دو Piece همان Passenger/Coverage/Definition، یک line با `quantity=2` و یک token. اگر چند line درباره همان محصول/مسافر/محدوده بیایند، یا canonical merge با محاسبه واقعی cumulative limits انجام شود یا 422 `DuplicateSelectionScope`؛ **بی‌صدا دو OrderService نسازید**. هر دو روش باید یک‌بار در Owner ADR ثابت شوند؛ انتخاب این سند: `duplicate exact candidate within one basket` **422**، UI باید Quantity را اصلاح کند. دو Seat برای همان Pax×Flight یا همان Seat برای دو Pax **422/Conflict**.

### 07.3 `AncillarySelectionInput` کامل [CURRENT fields + TARGET correction]

| JSON Field | Type | Relevant Variants | Required under domain rule | Semantics / safeguard |
|---|---|---|---|---|
| `quantity` | int? | A01,A02,A05,A12؛ گاهی دیگر به‌تناسب Contract | Required for those | >0، min/max/cumulative، effectiveQty؛ `0` در سبد به معنی حذف Line، نه خرید صفر |
| `packageProductRef` | string? | A02 | Y | EXACT `ServiceDefinitionRef` definition فعال، one SKU one weight package |
| `bagRef` | string? | A03 | Y | identifier real checked bag/permitted extra item؛ بدون source معتبر block |
| `measuredWeightKg` | decimal? | A03 | Y | >0؛ با `ChargeableWeightBasis` فرق دارد؛ conversion/allowance معتبر |
| `dimensions` | `{lengthCm,widthCm,heightCm}`? | A04,A06 | A04 Y; A06 conditional | positive decimal و max length/linear size همان filed rule |
| `equipmentKind` | string? | A06 | Y | authored kind، no arbitrary value |
| `weightKg` | decimal? | A06 | conditional | source/origin weight، bounds |
| `seatNumber` | string? | A09 (generic); A07/A08 via SeatMap ID | A09 Y | normalization case/trim، adjacent seat physical policy در S3 |
| `exitRowTermsAccepted` | bool? | Seat/ExtraSeat if applicable | conditional | Only explicit true when required؛ false block |
| `purpose` | code? `ExtraSeatPurpose` | A09 | Y if purpose authored | e.g EXST reason exact current enum; not generic paid seat |
| `targetCabinId` | int? | A10 | Y | selected verified target, coupon upgradability شرط لازم |
| `quoteRef` | string? | A10 dynamic | conditionally Y | verified supplier quote + expiry; arbitrary text fail |
| `mealCode` | string? | A11 | Y | SSR/meal code authored؛ free only with explicit PriceOrigin.Free |
| `menuItemRef` | string? | A12 | Y | authored catalog item or verified dynamic provider item، no fake options |
| `animalType` | enum `PetAnimalType`? | A13/A14 | Y | allowed type per Definition |
| `otherAnimalCode` | string? | A13/A14 | only if `RegisteredOther` | matched approved code |
| `animalAgeWeeks` | int? | A13/A14 | conditional | ≥0 and min age; unverifiable age -> NeedsVerification |
| `combinedWeightKg` | decimal? | A13/A14 | Y | animal+carrier combined against supplier constraints |
| `carrierDimensions` | `{lengthCm,widthCm,heightCm}`? | A13/A14 | Y | positive and capacity size validation |
| `documentAcknowledgements` | string[]? | A13/A14 | if RequiredDocumentCodes | acknowledgements ≠ verified uploaded documents |
| `sizeBracket` | string? | A14 | if authored | approved code per hold size |
| `assistanceSsrCode` | string? | A15/A16 | Y | authored WCHR/WCHS/WCHC etc; none inferred from disability |
| `equipmentCode` | string? | A17 | Y | code from actual MedicalEquipmentSpec |
| `evidenceDocumentRefs` | string[]? | A17 | if medical evidence required | authorized doc refs; no raw document/PII in token |
| `oxygenUnits` | decimal? | A17 Oxygen | conditional | bounded, source approval/capacity |
| `infantRef` | string? | A18 | Y | verified infant who is in selected Source/order |
| `guardianRef` | string? | A18 | Y | actual associated ADT, relationship/age/seat must verify |
| `childRef` | string? | A19 | Y | selected verified CHD/UMNR and route |
| `guardianHandoffContactProvided` | bool | A19 (CURRENT) | **INSUFFICIENT** | boolean cannot establish actual contact identity |
| `guardianPickupContactProvided` | bool | A19 (CURRENT) | **INSUFFICIENT** | boolean cannot establish actual contact identity |
| **`guardianHandoffContactRef`** | string? | A19 (TARGET minimal correction) | Y for true Quote | authoritative contact record/ref in Ordering, belongs to this Order |
| **`guardianPickupContactRef`** | string? | A19 (TARGET minimal correction) | Y for true Quote | other authorized contact; identity validation per policy |
| `airportId` | int? | A20–A22 | Y | actual itinerary airport/station and travel direction |
| `facilityRef` | long? | A20–A22 | Y if authored Facility | verified FacilityRef scoped to Airport/provider |
| `timeWithOffset` | DateTimeOffset? | A20–A22 | if appointment required | timezone/cutoff/time-window; Supplier slot availability separate |
| `guestCount` | int? | A20/A22 | optional/conditional | ≥0 <= filed max; charges for guest must be priced |
| `optIn` | bool? | A23 | Y | explicit true; benefit duplicate if verified included fare |
| `planCode` | string? | A24 | Y | **exact** `ServiceDefinitionRef` selected A24; provider activation later |
| `deviceCount` | int? | A24 | conditionally required by provider/policy | ≤MaxDevices, no order-scoped forced fake pax |

**A19 تصمیم تمیز:** Domain `[CURRENT]` `CustomerSelectionContract.For(A19)` فیلدهای متنی `GuardianHandoffContact` و `GuardianPickupContact` می‌خواهد، ولی `AncillarySelection` فعلی صرفاً boolean `...Provided` دارد. برای خرید واقعی، ContactRefهایی که Ordering نگهداری و اعتبارسنجی می‌کند بر bool برتری دارند. **این یک اصلاح محدود به S1 Domain Contract مورد نیاز S2 است**؛ اگر Owner عمداً Stage1 را کاملاً قفل بداند، تا تأیید قرارداد مشترک گزینه درست `[BLOCKED-BY-SOURCE]` و عدم تولید Token A19 است، نه ادعای خرید. تغییر دو field باید با Authority Check، اثر Migration (معمولاً DTO/VO، نه لزوماً DB)، Test و توافق Owner همراه شود. Raw PII تماس در token ممنوع.

### 07.4 Batch response — Field dictionary [TARGET]

| Field | Type | Source / interpretation |
|---|---|---|
| `source` | `{type:"offer"|"order",id:string,commercialVersion?:string}` | resolved authoritative source |
| `evaluatedAtUtc` | DateTimeOffset | Clock سرور |
| `currencyId` | int | currency quote server |
| `status` | `Ready|Incomplete|Rejected|NeedsQuote|NeedsVerification|Stale` | aggregate basket projection |
| `allReady` | bool | true فقط همه lineها quoteReady و cross-item guards pass |
| `items[]` | item result[] | همان ترتیب clientSelectionId؛ no silent drop |
| `items[].clientSelectionId` | string | echo |
| `items[].kind` | string | ancillary/seatMap |
| `items[].status` | `Accepted|Incomplete|Rejected` | سازگاری با existing `SelectionEvaluationStatus`; richer reason in readiness |
| `items[].readiness` | string | `OfferReadiness` موجود؛ seat readiness mapping محفوظ |
| `items[].offerItemRef` | string? | applicable ancillary only |
| `items[].seatMapSelectionId` | string? | seat only; exact 43-char input |
| `items[].pricedSelectionRef` | string? | فقط Accepted + full money + trusted source + Order-bound؛ null otherwise |
| `items[].validUntilUtc` | DateTimeOffset? | قیمت/توکن TTL؛ null when no quote |
| `items[].service` | `{serviceDefinitionRef,variantCode,profile,serviceSubCode,documentRouting}` | projected public-safe terms، no internal provider key |
| `items[].beneficiary` | `{travellerRefs[]}` | Order scope [] واقعی، نه dummy |
| `items[].coverage` | `{scope,flightRefs[],portionRefs[]}` | source verified؛ flight refs mapped to real Flight/Capacity |
| `items[].selectedOptions[]` | `{fieldName,value,optionRef?}` | exact canonical selected fields |
| `items[].exactPrice` | `ExactPrice` §08 | strings/decimals with formatting per surface |
| `items[].requiresCheckAtReserve` | bool | inventory unconfirmed; not a guarantee |
| `items[].reasonCodes[]` | string[] | stable human-doc codes, no private data |
| `items[].fieldIssues[]` | `{field,code,message?}[]` | specific form error |
| `basketPricing` | `{lineTotal?,sharedFees[],sharedTaxes[],total?,currencyId,isComplete,reasonCodes[]}` | shared fees once; if impossible total null, `allReady=false` for final financial acceptance |
| `basketConflictCodes[]` | string[] | duplicate seat, double family, incompatible options, provider mixed restriction |

**بهترین قاعده ۲۰ محصول:** Basket Evaluate در یک request همه انتخاب‌ها را price می‌کند. `allReady=false` یعنی **هیچ ref نباید به Ordering برای Commit ارسال شود**؛ می‌توان refهای individual valid را با TTL صادر کرد، ولی UI باید کل Basket را دوباره بررسی/approve کند. برای سخت‌گیری، اگر Fee مشترک ناشناخته است ref فردی `null` و کل Basket `Incomplete` باشد؛ هیچ Quote با قیمت ناقص به مرحله پرداخت نرود.

### 07.5 نمونه response — انتخاب دو مسافر + Seat

```json
{
  "source": {"type":"order", "id":"12345", "commercialVersion":"V17"},
  "evaluatedAtUtc": "2026-10-20T00:00:00Z",
  "currencyId": 70,
  "status": "Ready",
  "allReady": true,
  "items": [
    {
      "clientSelectionId":"L1", "kind":"ancillary", "status":"Accepted", "readiness":"Selectable",
      "pricedSelectionRef":"<secured-token-1>", "validUntilUtc":"2026-10-20T00:03:00Z",
      "service":{"serviceDefinitionRef":"BAG23", "variantCode":"A01","profile":"Baggage","serviceSubCode":"<sourced-code>","documentRouting":"Emd"},
      "beneficiary":{"travellerRefs":["ADT-1"]},
      "coverage":{"scope":"Portion","flightRefs":["B1:F1"],"portionRefs":["B1"]},
      "selectedOptions":[{"fieldName":"quantity","value":"2"}],
      "exactPrice":{"currencyId":70,"quantity":2,"baseAmount":1000000,"unitTotal":1090000,"lineTotal":2180000,"taxes":[{"category":"Added","code":"VAT","amount":90000}],"unitFees":[],"sharedFees":[],"isTotalComplete":true},
      "requiresCheckAtReserve":true,"reasonCodes":[],"fieldIssues":[]
    },
    {
      "clientSelectionId":"L2", "kind":"ancillary", "status":"Accepted", "readiness":"Selectable",
      "pricedSelectionRef":"<secured-token-2>","validUntilUtc":"2026-10-20T00:03:00Z",
      "service":{"serviceDefinitionRef":"BAG23","variantCode":"A01","profile":"Baggage","serviceSubCode":"<sourced-code>","documentRouting":"Emd"},
      "beneficiary":{"travellerRefs":["ADT-2"]},
      "coverage":{"scope":"Portion","flightRefs":["B1:F1"],"portionRefs":["B1"]},
      "selectedOptions":[{"fieldName":"quantity","value":"1"}],
      "exactPrice":{"currencyId":70,"quantity":1,"baseAmount":1000000,"unitTotal":1090000,"lineTotal":1090000,"taxes":[{"category":"Added","code":"VAT","amount":90000}],"unitFees":[],"sharedFees":[],"isTotalComplete":true},
      "requiresCheckAtReserve":true,"reasonCodes":[],"fieldIssues":[]
    },
    {
      "clientSelectionId":"L3","kind":"seatMap","status":"Accepted","readiness":"Selectable",
      "seatMapSelectionId":"<original-seatmap-43-char-id>","pricedSelectionRef":"<secured-seat-token>","validUntilUtc":"2026-10-20T00:03:00Z",
      "service":{"serviceDefinitionRef":"<verified-seat-definition>","variantCode":"A07","profile":"Seat","serviceSubCode":"<sourced-code>","documentRouting":"Emd"},
      "beneficiary":{"travellerRefs":["ADT-1"]},
      "coverage":{"scope":"Sector","flightRefs":["B1:F1"],"portionRefs":["B1"]},
      "selectedOptions":[{"fieldName":"seatNumber","value":"12A"}],
      "exactPrice":{"currencyId":70,"quantity":1,"unitTotal":500000,"lineTotal":500000,"isTotalComplete":true},
      "requiresCheckAtReserve":true,"reasonCodes":[],"fieldIssues":[]
    }
  ],
  "basketPricing":{"currencyId":70,"lineTotal":3770000,"sharedFees":[],"sharedTaxes":[],"total":3770000,"isComplete":true,"reasonCodes":[]},
  "basketConflictCodes":[]
}
```

مبالغ و نرخ مالیات در مثال کاملاً فرضی‌اند، اما رابطه حسابداری **Base 1,000,000 + VAT اضافه‌شونده 90,000 = UnitTotal 1,090,000** درون مثال سازگار است. این داده‌ها نباید به قیمت‌گذاری واقعی تبدیل شوند؛ تست واقعی باید از `Pricing` و `TaxTreatment` Author شده بخواند. `documentRouting=Emd` نیز فقط نمونه کالای فرضی است؛ برای هر Definition از Source واقعی گرفته می‌شود، نه از VariantCode استنباط.

---

## 08. حسابداری Pricing / Quantity / Fee / FX — بدون shortcut

### 08.1 الگوریتم canonical exact price

1. `Source` و `commercialVersion`, `SellerScope`, `OptionSourceVersion`, `Definition/Provision/PricingRevision` را تأیید کن.
2. `SelectionContract` را چک کن: نوع، required، allowed known option، ارز، beneficiary/coverage و عدم Duplicate/Conflict بین Lines.
3. EffectiveQuantity را از `PricingUnit` و **معنای تجاری** محصول بساز؛ اجباری‌کردن `quantity=1` در Public Request برای Variantی که Quantity نمی‌گیرد ممنوع.
4. نرخ Filed را با PTC/Age/Fare/TravelDate/POS/SourceCurrency پیدا کن؛ اگر مبهم یا Source ناقص `Incomplete/NeedsQuote`.
5. تبدیل ارز فقط با ROE verified و precision محفوظ؛ **ROE حداقل decimal(19,9)** در مدل/انتقال اگر DB نیاز داشت، نه silently truncated به 2 رقم.
6. Base و tax/fee lineها را طبق Treatment و FeeApplicationUnit بر مبنای واحد فروش محاسبه کن. شامل مالیات Included در Base را دوباره جمع نکن.
7. Costهای PerItem/PerPiece/PerPassenger/PerSeat/PerKilogram بر واحد درست multiply شوند؛ مشترک `PerTicket`/`PerOrder` در Basket **فقط یک‌بار** طبق scope واقعی و family charge key محاسبه شوند. اگر ApplicationUnit ناشناخته `isTotalComplete=false`.
8. خطاهای `CurrencyScaleUnknown`,`TaxTreatmentUnknown`,`FeeUnitNotSupported`,`ChargeableWeightNotDetermined`,`RateAmbiguous`,`FareBenefitsNotVerified` مانع صدور PricedSelectionRef معتبرند؛ صفر default ممنوع.
9. `ValidUntil = min(localTTL, upstreamSourceExpiry if known, providerQuoteExpiry if any, product expiry)`؛ Clock سرور، و `LastTicketingDate` به اشتباه TTL قیمت Ancillary نیست.
10. خروجی دارای `exactPrice` و `basketPricing` قابل reconciliation است؛ Resolve بازسازی می‌کند، mismatch حتی ۱ واحد minor Currency -> Stale/PriceChanged + قبول مجدد.

### 08.2 EffectiveQuantity و Unit — قرارداد حالت‌ها

| Variant/PricingUnit | Effective quantity | Guard | نمونه خطا |
|---|---|---|---|
| A01,A05 `PerPiece` | `selection.quantity:int` | مثبت، محدودیت per pax/coverage | Qty ۰/۱۰۰ → Invalid |
| A02,A12 `PerItem` | `selection.quantity:int` | package/menu Source-valid، cumulative family | دو SKU وزن مختلف دو Candidate، با یک Qty یکسان قیمت متفاوت |
| A03 `PerKilogram` | `chargeableWeightKg:decimal` محاسبه‌شده از **basis معتبر** | `measuredWeightKg` به‌تنهایی مبنای کل kg قابل شارژ را اثبات نمی‌کند؛ airline rule/allowance باید verified باشد | no verified basis → `ChargeableWeightNotDetermined` |
| A04,A06 `PerPiece|PerItem` | معمولاً `1` per selected typed object | تعداد implicit از typing تنها وقتی Definition unit=one verified؛ اگر چند item مجاز است جدا انتخاب شود | قیمت نمایش ضرب در Dimensions ممنوع |
| A07/A08,A09 `PerSeat` | `1` برای هر Pax×Flight×Seat selection | seat number + scope + exact seat quote | seat unknown → No Quote |
| A10,A11,A15,A16,A18,A19,A20,A21,A23 `PerPassenger` | ۱ به ازای همین beneficiary/source scope | یک beneficiary واقعی؛ Order-scoped برای اینها منع | NullQty نباید باعث null total شود |
| A13/A14,A17,A22,A24 `PerItem` | ۱ مگر معتبر بودن Multiple units تأیید شده | source-authored quantity rule؛ A24 DeviceCount **ضریب تعداد Plan نیست** مگر published price explicitly per device | deviceCount=3 نباید ناخواسته سه بار plan charge شود |
| A22/A24 `CoverageScope.Order` | یک Order unit؛ Quantity واقعی Definition | no forced Traveller; applicable flight set | یک Plan group به ۲ Pax دو بار Charge نشود |
| `PriceOrigin.Free` | Total صفر با Currency و Tax/fee evidence و effectiveQty | Free metadata معتبر، امکان Required SSR/Confirmation | nullable Currency با صفر ساختگی ممنوع |
| `PriceOrigin.ExternalQuote` | فقط پس از supplier quote با amount/terms/TTL | QuoteRef معتبر و Quote Owner verified | symbolic QuoteRef ≠ price quote |

**Bug CURRENT:** `PriceAssessor.Priced(..., int? quantity)` تنها وقتی `quantity > 0` باشد `RequestedQuantityTotal` می‌دهد؛ به همین دلیل برای Serviceهای `PerPassenger`, Seat و typed forms مبلغ نهایی ممکن است null باشد. این باید **در S2 Price Evaluator** اصلاح شود؛ از فرانت انتظار `quantity=1` برای همه Variantها نداشته باشید.

### 08.3 Reconciliation و Shared Fees

نمونه ۲ مسافر و ۳ سرویس: ۱ Fee per-ticket مشترک بین دو Service یک مسافر نباید دوبار کسر شود. Rule:

- `priceLineKey` = `ownerRuleRef + feeCode + feeApplicationUnit + currency + applicableTicket/OrderRef + commercialSourceVersion`؛ فقط اگر source شناسه واقعی Rule دارد این کلید authoritative است؛ بدون key قابل اعتماد، Basket total **Incomplete**.
- `LineTotal` هر انتخاب فقط آن دسته Feeهای خودِ Unit را دارد؛ Fee مشترک در `basketPricing.sharedFees` یک بار ثبت، نه در LineTotal دوباره.
- نمونه: Lines ۲٬۰۰۰٬۰۰۰ + ۱٬۰۰۰٬۰۰۰؛ SharedPerTicketFee ۵۰٬۰۰۰ فقط یک‌بار => Basket ۳٬۰۵۰٬۰۰۰؛ نه ۳٬۱۰۰٬۰۰۰.
- Concurrency: اگر یکی از Lines حذف یا Qty تغییر کند، entire Basket Fees و shared eligibility مجدد محاسبه شود.
- اگر Fee دقیق فقط با Payment Method در Ordering معلوم می‌شود (۳DS/agency commission/payment surcharge)، S2 مقدار ناشناخته را صفر اعلام نمی‌کند. `basketPricing.isComplete=false` یا قرارداد مشخص `supplierAncillaryTotal` جدا از payment processing fees و `finalPayable` در Ordering تعریف می‌شود. S2 نباید ناخواسته total کل Order شامل airfare را ادعا کند.

### 08.4 منبع Facts پس از Issue

- `used/flown/refunded/voided` coupon => service coverage eligibility per flight/coupon.
- Existing bag quantities و family counting => no duplicate/free entitlements؛ خرید پله‌ای supplier در صورت filed rule (مانند Breeze CB1/CB2)؛ بدون Source evidence، ترتیب را حدس نزن.
- `alreadyIssuedEMD` یا active OrderService => جلوگیری از double sale/charge، حتی اگر Definition جدید همان OfferItemRef را بدهد.
- A10 بعد ticket issue ممکن است exchange/reshop مستقل لازم داشته باشد؛ اگر Ordering integration آن موجود نیست Stage2 `NeedsQuote/NotSupportedBySource`، نه پذیرش token self-sufficient.
- بیش از یک Currency در سبد یا تغییر FX بین List/Evaluate => یا همه Quotes در یک target Currency معتبر convert شود یا Batch `Incomplete`؛ silent mix currencies ممنوع.

## 09. SeatMap end-to-end — شرط صریح Owner: ID صادرشده باید تا Evaluate و Order قابل استفاده باشد

### 09.1 **قرارداد جاری غیرقابل Breaking** [FROZEN-WIRE]

سه route قدیمی:

```text
POST /Backoffice/v1/SeatMaps
POST /Api/v1/SeatMaps
POST /OtaPanel/v1/SeatMaps
```

`BackofficeGetSeatMapQuery { offerId:string?, options?:BackofficeAncillaryOfferOptions }`; `ApiGetSeatMapQuery { offerId:string? }`; `OtaPanelGetSeatMapQuery { offerId:string? }`. پاسخ **دقیق** از `src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Dto/SeatMapsDto.cs`:

```text
SeatMapsDto:
  meta: {count:int}
  data: SeatMapDto[]:
    type:string, flightOfferId:string, segmentId:string,
    departure:{iataCode:string?, terminal:string?, at:DateTimeOffset},
    arrival:{iataCode:string?, terminal:string?, at:DateTimeOffset},
    carrierCode:string?, number:string?, operating:{carrierCode:string?},
    aircraftId:int, class:string?, cabinClassId:int?,
    decks: [{
      deckType:string?,
      deckConfiguration:{width:int,length:int,startSeatRow:int,endSeatRow:int,
        startWingsX:int,endWingsX:int,startWingsRow:int,endWingsRow:int,exitRowsX:int[]},
      cabins:[{cabinClass:string?,cabinClassId:int?,cabinStartSeatRow:int,cabinEndSeatRow:int,
        items:[{number:string?,code:string?,column:int,row:int,title:string?,
          characteristicsCodes:string[],conditions:string[],coordinates:{x:int,y:int,length:int},
          travelerPricing:[{id:string?,travelerId:string,seatAvailabilityStatus:string,
            price:{currency:string,base:string,total:string,
              taxes:[{amount:string,code:string?}],fees:[{amount:string,type:string?}]}?}]
        }]}]
    }],
    availableSeatsCounters:[{travelerId:string,value:int}]
  dictionaries:{facilities:map<string,string>,seatCharacteristics:map<string,string>}
```

**هیچ فیلد، type، nullability، nesting، string status یا route قدیمی تغییر نکند.** `travelerPricing[].id` فقط هنگامی غیر-null که همان Seat برای **همان مسافر و Segment و SalesScope** قابل Selection و قیمت معتبر دارد؛ `seatAvailabilityStatus` فعلی دقیقاً `AVAILABLE|BLOCKED|OCCUPIED`.

### 09.2 شناسه ۴۳کاراکتری CURRENT قابل Decode نیست؛ استراتژی تمیز Resolve

کد موجود `OfferItemRef.OfSeat(...)` از `(OfferId,Channel,OwnerAirline,POS,TravellerRef,FlightRef,SeatNumber,CandidateIdentity)` SHA-256 Base64URL می‌سازد. بنابراین **از ۴۳-character hash نمی‌توان SeatNumber یا قیمت را درآورد.** وعده Decode کردن Hash برای S2 غلط است. راه درست Stateless با قرارداد فعلی:

1. UI `travelerPricing[].id` را **بدون تبدیل** همراه `kind:"seatMap"` به `Selections/Evaluate` می‌فرستد.
2. Service منبع `Order` یا `Offer` و Scope معتبر را می‌خواند، **همان Composition/SeatPricing موجود** را به‌روزرسانی می‌کند و Seat IDهای موجود را در یک `Dictionary<Id,SeatCandidate>` برای یک منبع و POS بازتولید می‌کند (یا با کد فعلی SeatMap snapshot+mapper Match می‌زند). **ساخت SeatPricing Engine دوم ممنوع**.
3. Exactly one match: نوع A07/A08، stable traveller mapping، `FlightRef`, `FlightId`, `FlightCapacityId`, version, aircraft/layout, seatNumber, commercial Definition/Provision/Rate، source/agency/office verified؛ اگر `0` یا `>1` match => 422/NotFound، نه حدس.
4. physical availability از FlightFlow با **کامل‌بودن presence همه FlightIdها** بررسی شود؛ missing flight row ≠ empty occupied seats. Supplier/FlightFlow outage => no token.
5. قیمت همان Seat برای همان Pax/Flight/Seat از engine و Source current recalculated؛ مقدار `SeatMapPrice.Total` برای نمایش، نه اعتماد به client. `PerSeat effective quantity = 1`. همه Tax/Fee/FX و free entitlement authoritative.
6. اگر کامل و status AVAILABLE، Generate **`pricedSelectionRef` جدیدِ self-contained** با `kind=seat`, `seatMapSelectionId` original, seatNumber/Traveller/Flight/Pricing/Price و Order binding. اگر fail => `Rejected/NeedsVerification/Stale` و بدون Token.
7. Ordering تنها `pricedSelectionRef` دریافتی Evaluate را Add می‌کند؛ Resolve آن را decode و **همان Seat ID را دوباره regenerate** و live occupancy/repricing را بررسی می‌کند. سپس data شامل SeatNumber/FlightCapacityId/TravellerId/Price را به Ordering بازمی‌گرداند؛ physical Seat Hold در Stage3 توسط FlightFlow، جداگانه.
8. دو متقاضی ممکن است در فاصله Evaluate تا Hold همان Seat را AVAILABLE ببینند؛ **فقط FlightFlow Hold** جلوی double assignment نهایی را می‌گیرد. Token هیچ Seat lock نیست.

**سازگاری IDهای قبلی:** اگر SeatMap قبلاً از OfferId نمایش داده شده و سپس Order ساخته شده است، Evaluate با `orderId` می‌تواند با `originPricedOfferId` معتبر Order یک‌بار legacy Offer Seat IDs را regenerate و به Pax/Flightهای Order map کند؛ فقط در صورت انطباق تمامی Source/Price/Order.Version معتبر می‌شود. اگر Offer قدیمی دیگر قابل بازیابی نیست، ID قدیمی Stale و UI باید از Order-based SeatMap تازه شناسه بگیرد. این شرط مانع Reset بی‌دلیل صفحه کاربر و مانع پذیرش SeatID نامربوط است.

### 09.3 تضادهای موجود در سورس که صرف فریز DTO حلشان نمی‌کند [FIX-REQUIRED]

- [CURRENT] `AncillaryShoppingEngine.SeatMaps.cs` وقتی `category is null` است هم ممکن است SeatPricing را `Available` با `Candidate=null` برگرداند. **در طراحی هدف ممنوع**: صندلی بدون commercial proof یا verified free entitlement، `BLOCKED` و `Price=null` و `Id=null`.
- [CURRENT] `SeatMapMapper.Price` اگر `PriceAssessment=null` شود fallback `base="0"/total="0"` دارد؛ و برای `BaseAmount/CompleteUnitTotal` nullable هم از `??0` استفاده می‌کند. **این قیمت ساختگی ممنوع است**؛ `AVAILABLE` فقط با Money کامل و مثبت یا صفر با `PriceOrigin.Free` تأییدشده (Currency معتبر) ممکن است.
- [CURRENT] `GetSeatMapService` از `occupied.GetValueOrDefault(flightId)` استفاده می‌کند و پاسخ ناقص occupancy ممکن است معادل empty فرض شود. **باید presence و completeness هر FlightId** و out-of-order/duplicate/unknown responses را بررسی کند.
- [CURRENT] `OfferItemRef.OfSeat` به AgencyId/OfficeId و SourceCommercialVersion کامل bind نشده، هرچند SalesScope در Hash سایر offers بهبود یافته. **همه SeatIDs در Evaluate/Resolve باید از Context احرازشده و Order mapping بازتولید شوند**؛ برای نسل جدید ID بدون تغییر shape ۴۳کاراکتری، افزودن claimهای Scope به Hash مجاز است ولی اثر compatibility باید version policy و regression test داشته باشد. Legacy IDs را بدون تضمین match رد کنید، نه کورکورانه قبول.
- [CURRENT] `GetSeatMapService` بر هر POS ممکن است candidate همان Seat را با first-available نگه دارد. چند POS معتبر اما متفاوت اگر Quote provenance مشخص نیست => Ambiguous، نباید first random انتخاب شود.
- غیبت verify layout/aircraft version بعد swap و missing FlightFlow source => fail closed. `AvailableSeatsCounters` باید فقط واقعاً AVAILABLE با id/price معتبر بشمارد.

**نکته مهم درباره «فریز»:** در Pack قدیمی بعضی Implementation files نیز به‌طور مطلق فریز شده بودند؛ **دستور جدید Owner که Seat ID باید Evaluate/Resolve و Order usable شود اصلاح رفتار داخلی را ضرورت می‌دهد**. پیشنهاد این سند: فقط JSON/Route/حالت‌های معتبر seat فریز باشند و internal code تحت تست Regression قابل اصلاح باشد؛ این **تغییر Authority** نیازمند تأیید Owner است. بدون آن SeatMap جریان انتها به انتها را به‌غلط سبز اعلام نکنید.

### 09.4 Order-based SeatMap [TARGET] و محدودیت wire

`POST /{S}/v1/Orders/{orderId}/SeatMaps` با **همان SeatMapsDto** هدف است و `travelerPricing[].id` همچنان ۴۳-character opaque می‌ماند؛ token جدید به `orderId+orderCommercialVersion` bind می‌شود. در DTO فریز `flightOfferId:string` وجود دارد. بنابراین پاسخ Order-based فقط وقتی با همین DTO **semantically valid** است که Ordering مقدار معتبر `originPricedOfferId` را فراهم کرده باشد. برای Order تاریخی فاقد این Value نباید `orderId` را در `flightOfferId` جا زد؛ `OrderSeatSourceIncomplete` و عدم صدور شناسه و سند `[EXTERNAL_CONTRACT]` داده شود. پوشش تمام Orderهای Historical بدون origin Offer نیازمند **تصمیم جداگانه DTO نسخه‌دار** است؛ جزو تغییر بی‌صدا در S2 نیست.

### 09.5 ممنوعیت‌های Seat / EXST / Free

- A07/A08 فقط SeatMap؛ A09 ExtraSeat (EXST) در generic AncillaryOffers و مدلی مستقل از Seat Number معمولی است؛ seat adjacent/extra physical capacity در S3 و Ticketing هماهنگ می‌شود.
- Infant بدون Seat entitlement مستقل، cabin mismatch، exit row child/ineligible، occupied، unknown/partial source => no payable seat ref.
- zero-price فقط `explicit free` provision یا evidence معتبر fare seat entitlement؛ `Price=null`، `Definition=null` یا `Coupon benefit unknown` را free حساب نکن.
- Paid seat after ticket: document change/EMD or ticket exchange طبق policy واقعی، نه فرض یک نوع EMD برای همه.

### 09.6 تست Seat/Order trace الزام‌آور

`S2-SEAT-01`: SeatMaps id=43char، status AVAILABLE، Price صالح؛ `S2-SEAT-02`: همان ID → Evaluate seat → token مثبت؛ `S2-SEAT-03`: Token → Resolve with OrderId → same Seat/Traveller/Flight/Amount and original SeatID؛ `S2-SEAT-04`: passenger swapped ID => reject؛ `S2-SEAT-05`: same seat two Pax in one basket => conflict؛ `S2-SEAT-06`: same Pax two seats same Segment => conflict؛ `S2-SEAT-07`: occupied since List => Stale؛ `S2-SEAT-08`: missing FlightFlow flight result => unavailable؛ `S2-SEAT-09`: aircraft swap/version changed => Stale؛ `S2-SEAT-10`: unknown Price/Commercial candidate => BLOCKED/null/null؛ `S2-SEAT-11`: verified free => Price 0/id valid؛ `S2-SEAT-12`: same seat different POS price refs never conflated؛ `S2-SEAT-13`: fake 43-character id fail； `S2-SEAT-14`: post-ticket order seat map requires valid original OfferId; `S2-SEAT-15`: old Offer ID can be matched only when origin Order linkage/current terms verified؛ `S2-SEAT-16`: existing ۳ route و تمام nested JSON property names/statuses unchanged.

---

## 10. Stateless `pricedSelectionRef` — Wire, Security, Expiry و تغییرات

### 10.1 تفاوت سه ID (اشتباه نگیر)

| ID | Existing | Encode/Decode | آیا Order آن را مستقیم Commit می‌کند؟ |
|---|---|---|---|
| `offerItemRef` | CURRENT | ۴۳-character SHA-256؛ **غیرقابل Decode** | خیر؛ هنوز Selection/Quantity ندارد |
| `SeatMaps.travelerPricing[].id` | CURRENT | ۴۳-character SHA-256؛ **غیرقابل Decode، قابل بازتولید** | خیر؛ ابتدا Evaluate seat |
| `optionRef` | CURRENT Details | deterministic hash شناسه Choice؛ **غیرقابل Decode، قابل بازتولید** | خیر |
| `pricedSelectionRef` | TARGET | protected self-contained token قابل Verify/Decrypt | فقط بعد Resolve، Auth، current-context & current-price confirmation |
| `OrderServiceId` | Ordering | پایدار/DB Owner | پس از Add توسط Ordering |

### 10.2 تولید Token بدون جدول Quote

**گزینه انتخاب‌شده:** `v=1` encrypted/authenticated AEAD با `keyId` و payload Canonical؛ می‌توان از استاندارد پذیرفته‌شده JWE/PASETO یا ASP.NET authenticated Data Protection با Key Ring پایدار استفاده کرد. هیچ Base64 ساده، SHA تنها، unsigned JSON، رمز اختصاصی یا `alg:none` مجاز نیست. مستند مرجع [RFC 7519](https://www.rfc-editor.org/rfc/rfc7519.html) تفاوت Signature/Encryption و Trust context را تصریح می‌کند. **انتخاب کتابخانه مطابق پترن Security sibling و CLAUDE rule1 قبل از کدنویسی**؛ این سند فقط خواص امنیتی قابل تست را freeze می‌کند.

**Token internal claims (هر خط دارای Version و Serializable contract مشخص؛ تمام قیمت‌ها Decimal invariant string، نه floating point):**

| Field | Type | Required | منبع/نگهبان |
|---|---|---|---|
| `version` | int | Y | version=1; unknown reject |
| `issuer`,`audience` | constrained strings | Y | Ancillary quotation / Ordering resolve only |
| `keyId` | string (header) | Y | key rotation، public client نباید کلید بگیرد |
| `issuedAtUtc`,`expiresAtUtc` | epoch/DateTimeOffset | Y | Clock UTC، short TTL، exp/nbf validated with tight skew |
| `nonce` | cryptographically random bytes | Y | prevent deterministic exposing equal selections، **not** replay prevention alone |
| `sourceKind` | Offer/Order | Y | Order برای Add الزامی؛ Offer-only preview token Commit نمی‌شود |
| `sourceOrderId` | long? | Y for committing | همان OrderId verified |
| `sourceOriginOfferId` | string? | if exists | link provenance؛ null در Order که origin معلوم نیست، no fake |
| `sourceCommercialVersion` | string | Y for Order | match current Order; stale on change |
| `sourceFingerprint` | hash string | Y | current trusted commercial facts digest |
| `ownerAirlineId`,`salesChannel`,`pointOfSaleId` | typed | Y | bind to actual authorized commercial context |
| `travelAgencyId`,`airlineOfficeId`,`customerId` | optional IDs | context required | only relevant scope; revalidated independently |
| `kind` | `ancillary|seatMap` | Y | exactly one |
| `offerItemRef` | string? | generic required | selected Candidate identity |
| `seatMapSelectionId` | string? | seat required | 43-character original map id, must be current reconstructed |
| `definitionVersionId`,`provisionId`,`pricingRevisionId`,`pricingRateId` | long/long? | Y where filed | source matching on Resolve |
| `serviceDefinitionRef`,`variantCode`,`serviceSubCode` | string | Y | cross-check active Definition/Product |
| `travellerRefs[]`,`orderTravellerIds[]` | ID arrays | Y if pax-specific | explicit verified mapping; Order scope [] only where allowed |
| `flightRefs[]`,`flightIds[]`,`flightCapacityIds[]`,`portionRefs[]` | canonical arrays | Y based on coverage | avoid Bound-vs-Sector mismatch and physical hold wrong FlightId |
| `selection` | canonical typed fields | Y | actual selected quantity/options/weight/SeatRef/Refs, **not PII** |
| `selectedOptionEvidence[]` | `{field,value,sourceVersion,optionRef}`[] | if enumerated | match available choices source proof |
| `effectiveQuantity` | decimal+unit | Y | pricing quantity, PerKg decimal separate from count |
| `price` | Canonical exact price | Y | currency, base/unit/line, tax/fee/FX, shared fee refs, total completeness |
| `providerQuoteRef` | string? | if dynamic trusted | source owner and expiry checked |
| `requiresCheckAtReserve` | bool | Y | remains true after token; no hold claim |
| `commercialDigest` | hash string | Y | full rules+price option applicability and SalesScope canonical |

**Token shape** می‌تواند ~۱–۲ KB شود؛ آن را QueryString URL قرار ندهید، فقط JSON body/HTTPS. اگر `selectedOptionEvidence`, tax components و scope بزرگ شدند، compressed authenticated payload فقط با استاندارد امن و limits مجاز است؛ **نباید بدون مجوز به DB migration دست زد**. `maxRefLength` و `maxBatchBytes` مشخص، reject oversized input برای DoS.

### 10.3 Replay / cross-order / stale / revoked key

1. **هر Order-bound Token فقط روی همان `orderId` و `orderCommercialVersion` قابل Resolve است**؛ Offer-only token برای Add رد شود. ۲. `clientSelectionId` و `idempotencyKey` به تنهایی سند هویت نیستند. ۳. Authorization داخلی Caller Ordering: `pss.service`, `context_type=Service`, `principal_type=internal_service` + allowlisted verified client identity؛ missing config => fail closed. ۴. Encrypted/authenticated token را قبل از parsing claims قیمت/موارد دیگر verify کن؛ invalid/unknown key/expired = reject، no fallback. ۵. بعد از Verify هنوز Order read جاری، Catalogue active, OptionSource, FX, Seat availability و price rules دوباره چک شود. ۶. key rotation باید old current tokens تا TTL قبول و بعد invalidate کند؛ revoke emergency بدون crash. ۷. Cross agency/POS/office/customer/owner mismatch => 403 یا not-found safe response، no leakage. ۸. Replay Token به همان Order در Request جدید بدون Idempotency جلوگیری نمی‌شود؛ **Ordering** باید Exactly-once Add Operation guard، unique service conflict و event dedup داشته باشد. ۹. رمزکردن اطلاعات شخصی/اسناد کافی نیست؛ **اساساً PII را وارد Token نکنید**. ۱۰. Token type confusion Seat vs Baggage، بایت‌های تغییرکرده، JSON duplicate keys، غیرقابل‌خواندن، زمان آینده/گذشته در تست امنیتی رد شوند.

### 10.4 قیمت تغییر کرده ولی Token هنوز Expire نشده است

معتبر بودن امضا/TTL **فقط شرط اول** است. اگر OrderVersion/FX/fare/coupon/capacity/Definition/Rate تغییر کرده:

- `status=Stale` و `reasonCodes=[PriceChanged|SourceChanged|CommercialRevisionChanged|...]`، `currentPrice?` فقط اگر دقیق و Source-authenticated باشد.
- Ordering هیچ Amount جدید را خودکار اعمال نکند. UI «قیمت تغییر کرده» نشان دهد، دوباره Batch Evaluate و confirmation؛ تازه بعد از آن Add.
- اگر Source Provider outage => `NeedsVerification`, no guessing and no Commit.

---

## 11. `Resolve` داخلی و قرارداد تحویل به Ordering

### 11.1 Request [TARGET]

```json
{
  "orderId": 12345,
  "expectedOrderCommercialVersion": "V17",
  "pricedSelectionRefs": ["<token-T1>", "<token-T2>", "<token-seat>"],
  "correlationId": "e7934519-8e56-47f4-a532-d7f077899e7b"
}
```

| Field | Type | Required | Validity |
|---|---|---|---|
| `orderId` | long | Y | >0 و Order واقعی متعلق به Verified Owner/Caller |
| `expectedOrderCommercialVersion` | string | Y | exact current; mismatch entire batch stale |
| `pricedSelectionRefs` | nonempty `string[]` | Y | ۱..max configured، duplicate exact token در same request 422 |
| `correlationId` | string | Y | trace/audit؛ idempotency اصلی در Ordering |

این Endpoint تحت `[Authorize]` واقعی مخصوص service-to-service است. صرف این‌که Controller قدیمی S3 route `Service/...` دارد Auth تضمین نمی‌کند؛ Policy باید صریحاً روی Resolve نصب و در Integration Test کنترل شود.

### 11.2 Response [TARGET]

```json
{
  "orderId": 12345,
  "orderCommercialVersion": "V17",
  "allValid": true,
  "evaluatedAtUtc": "2026-10-20T00:01:00Z",
  "items": [
    {
      "status": "Valid",
      "reasonCodes": [],
      "selectionKind": "ancillary",
      "sourceSelectionRefDigest": "<redacted-sha256-token-digest>",
      "offerItemRef": "<revalidated-current-candidate>",
      "service": {
        "serviceDefinitionRef": "BAG23",
        "definitionVersionId": 101,
        "provisionId": 201,
        "pricingRevisionId": 301,
        "variantCode": "A01",
        "profile": "Baggage",
        "serviceTypeCode": "BG",
        "serviceSubCode": "<actual-filed-subcode>",
        "bookingMethod": "Ssr",
        "ssrCode": null,
        "documentRouting": "Emd",
        "documentType": "EmdAssociated",
        "rfic": null,
        "rfisc": null,
        "fulfillmentProviderKey": "<server-trusted-provider-key>",
        "confirmationRequirement": "Immediate"
      },
      "beneficiaries": [{"orderTravellerId": 11,"sourceTravellerRef":"ADT-1"}],
      "coverage": {
        "scope": "Portion", "portionRefs": ["B1"], "flightRefs": ["B1:F1"],
        "flightIds": [1001], "flightCapacityIds": [2001]
      },
      "selection": {"quantity":2},
      "price": {
        "currencyId":70,"quantity":2,"pricingUnit":"PerPiece",
        "baseAmount":1000000,"unitTotal":1090000,"lineTotal":2180000,
        "includedTaxes":[],"addedTaxes":[{"code":"VAT","amount":90000,"currencyId":70}],"unitFees":[],"sharedFees":[],
        "filedCurrencyId":70,"fxEvidence":null,"isTotalComplete":true
      },
      "availability": {"requiresCheckAtReserve":true,"isGuaranteed":false},
      "validUntilUtc": "2026-10-20T00:03:00Z"
    }
  ],
  "basketPricing": {"currencyId":70,"lineTotal":2180000,"sharedFees":[],"total":2180000,"isComplete":true}
}
```

**JSON بالا exemplar است و تمام مقادیر مالی/Provider code فرضی‌اند**. در قرارداد نهایی برای `sourceSelectionRefDigest` فقط hash جهت Audit، نه Token حساس، نگهداری شود. `fulfillmentProviderKey` فقط S2S و با guard real; Ordering باید service owner/Provider assignment immutable را حفظ کند. `DocumentRouting` حق صدور EMD نیست؛ فقط راهنمای downstream Stage3/Ticketing.

### 11.3 Field matrix `ResolvedSelection` [TARGET]

| Field | Type | Required | Trace into Ordering |
|---|---|---|---|
| `status` | `Valid|Stale|Expired|NotAuthorized|NeedsQuote|NeedsVerification|Unavailable|Rejected` | Y | `Valid` تنها مجوز ایجاد OrderService قابل پرداخت؛ 200 batch می‌تواند items invalid داشته باشد |
| `reasonCodes[]` | string[] | Y | no private Provider error details |
| `selectionKind` | `ancillary|seatMap` | Y | Seat maps to `OrderServiceType.SeatAssignment`; no double counting |
| `sourceSelectionRefDigest` | string | Y | technical trace + idempotency audit |
| `offerItemRef`,`seatMapSelectionId` | string? | kind-specific | provenance؛ raw seat id persists as ref if useful |
| `definitionVersionId`,`provisionId`,`pricingRevisionId`,`pricingRateId` | long / long? | per Source | validated internal mapping for S3 input |
| `serviceDefinitionRef`,`variantCode`,`profile`,`serviceTypeCode`,`serviceSubCode` | identifiers | Y | Order item/service commercial snapshot |
| `bookingMethod`,`ssrCode`,`documentRouting`,`documentType`,`rfic`,`rfisc` | enum/codes | source conditional | fulfillment and EMD route, no invented static by Variant |
| `fulfillmentProviderKey` | string | Y when required | actual immutable assignment on OrderService; no public leak |
| `beneficiaries[].orderTravellerId` | long | Pax-scoped required | matches actual OrderTraveller; 0/null never fake |
| `coverage.scope` | Sector/Portion/Journey/Order | Y | exact order scope |
| `coverage.portionRefs[]`,`flightRefs[]`,`flightIds[]`,`flightCapacityIds[]` | ordered source ids | Y when applicable | no confusion FlightId with FlightCapacityId |
| `selection` | exact normalized inputs | Y | selected Quantity/Weight/Seat/etc as Order snapshot; contactRefs not raw PII |
| `price` | complete precision-aware breakdown | Y valid | source of accepted Ancillary commercial amount; order-level charge rule once |
| `availability.requiresCheckAtReserve` | bool | Y | future S3 physical supplier/local availability check |
| `validUntilUtc`, `sourceVersion`, `orderCommercialVersion` | typed | Y | expire/stale/version safety |
| `currentPrice?`, `changedFields[]?` | advisory | if stale | UI reprice prompt only; not automatic commit |

### 11.4 مقایسه با Ordering واقعی / Stage3 existing boundary

- [PRIOR-AUDIT] سه Route فعلی CreateOrderFromOffer در Ordering بر اساس controllerها `POST /Backoffice/v1/Orders/FlightOffers`, `POST /Api/v1/Bookings/FlightOffers`, `POST /OtaPanel/v1/Bookings/FlightOffers` هستند؛ **نه** `POST /{S}/v1/Orders` فرضی. در این نوبت مجدداً قابل خواندن نبودند. Add Ancillaries route فعلاً صرفاً Target و Consumer Team must approve.
- [PRIOR-AUDIT] `CreateOrderArgs` هنوز Ancillary selections ندارد و `OrderService.TravellerId` non-nullable `long` است. در نتیجه `CoverageScope.Order` برای A22/A24 با OrderService نیازمند تغییر/سازگاری در Ordering است؛ **Ancillary نباید TravelerId ساختگی بسازد**. اگر Consumer هنوز آن را پشتیبانی نمی‌کند، Order-scoped quote قابل‌نمایش اما `EXTERNAL_PENDING` Commit است.
- [CURRENT] Stage3 Hold API `POST /Service/v1/Ancillaries/Service-Holds` ورودی `IdempotencyKey:string,OrderId:long,Reference:string,RequestedExpiresAt:DateTimeOffset?,Services[]` می‌گیرد. هر `Services[]` فعلی: `OrderServiceId:long,ServiceDefinitionId:long,ProvisionId:long,TravellerId:long?,CoverageScope:enum,CoveredFlightIds:long[],Quantity:int`. این ورودی مستقیماً token S2 نیست؛ **Ordering بعد از ایجاد OrderService و Resolve mapping آن را صدا می‌کند**. ورودی qty برای خدمات وزن‌محور یا seat special هنوز ممکن است به توسعه واقعی در S3 نیاز داشته باشد؛ در S2 با cast کردن `decimal weight` به int دروغ تولید نکن.
- PostTicketed: Ordering باید OrderChange/Payment/Document orchestration و history را حفظ کند؛ S2 Resolve به معنی EMD/PNR updated نیست.

### 11.5 اتمیک‌بودن Basket و نتیجه جزئی

- **پیش‌فرض طراحی:** Add Ancillaries به Order یک **درخواست تجاری all-or-none تا مرز ثبت OrderChange** است. Resolve نتایج جزئی می‌دهد تا UI دلیل خطا را بداند؛ اما Ordering **قبل از ثبت** اگر `allValid=false` هیچ انتخابی از همان درخواست را silent persist/charge نکند.
- پس از ورود به Stage3 و تماس‌های Supplier ممکن است partial operational success رخ دهد؛ این موضوع با partial validation یکی نیست. Ordering باید per-unit Pending/Confirmed/Failed و compensation workflow داشته باشد؛ S2 پیش از آن Hold نمی‌کند.
- اگر concurrent change Order version در میان Resolve و Persist رخ داد، Ordering باید optimistic concurrency guard/retry با revalidation؛ تکیه بر Resolve بدون guard کافی نیست.

## 12. ماتریس الزام‌آور تمام ۲۴ Variant — منبع، Input، Pricing، Coverage، Outcome و Handoff

**قانون کلی:** این ۲۴ سناریو «فهرست فعال‌کردن خودکار همه خدمات» نیستند. هر Variant تنها اگر Definition/Provision/Pricing/Supplier capability/Fact evidence معتبر دارد Candidate می‌سازد؛ **نبود منبع ≠ صفر/رایگان/قابل خرید.** کدها/نام‌ها و PricingUnitهای مجاز دقیقاً با `AncillaryVariant.cs` و `CustomerSelectionContract.cs` در SHA فوق تطبیق داده شده‌اند. هر سناریو باید حداقل یک تست مثبت *اگر منبع متناسب وجود دارد* و یک یا چند تست Fail-closed داشته باشد. اگر سناریوی مثبت مستلزم منبع خارجیِ موجودنبودنی است، به‌صورت `EXTERNAL_PENDING` با دقیق‌ترین contract gap ثبت و شبیه‌سازی فریبنده ممنوع است.

### 12.1 خلاصه ۲۴ ردیف اجرایی

| Code | Definition / pricing allowed | Beneficiary/coverage | Required customer input | Option source | Outcome مطلوب |
|---|---|---|---|---|---|
| A01 | Extra checked bag / `PerPiece` | Pax×Portion | `quantity` | typed qty | unit × qty؛ inventory/allowance verified |
| A02 | Extra weight package / `PerItem` | Pax×Portion | `packageProductRef`, `quantity` | **same DefinitionRef** | each 5kg/10kg own SKU/rate |
| A03 | Overweight / `PerPiece|PerKilogram` | Pax×Portion | `bagRef`,`measuredWeightKg` | typed measured weight + source chargeable basis | no exact price without chargeable basis |
| A04 | Oversize / `PerPiece` | Pax×Portion | `dimensions` | typed dimensions | linear-size / overlap rules |
| A05 | Cabin bag / `PerPiece` | Pax×Portion | `quantity` | typed qty | cabin allowance/capacity source |
| A06 | Sports equipment / `PerItem|PerPiece` | Pax×Portion | `equipmentKind`, dimensions/weight conditional | authored kind | special supplier approval |
| A07 | Standard seat / `PerSeat` | Pax×Sector | **SeatMap id only** | SeatMaps actual Seat | verified seat price, FlightFlow later holds |
| A08 | Preferred seat / `PerSeat` | Pax×Sector | **SeatMap id only** | SeatMaps preferred characteristic | no exit-row for ineligible pax |
| A09 | Extra seat / `PerSeat` | Pax×Sector | `seatNumber`, `purpose`, terms conditional | generic offers, not SeatMaps generic A07/A08 | coordinated physical+ticket later |
| A10 | Cabin upgrade / `PerPassenger|PerSeat` | Pax×Flight | `targetCabinId`, dynamic `quoteRef` | authored target + supplier quoted | false CouponUpgradable=>reject; true insufficient |
| A11 | Free special meal / `PerPassenger|PerItem` | Pax×Flight | `mealCode` | authored meal code | explicit Free, SSR/lead-time approval |
| A12 | Paid meal / `PerItem|PerPassenger` | Pax×Flight | `menuItemRef`, `quantity` | authored real menu only | subtotal/multiple, no fake free |
| A13 | Pet in cabin / `PerItem` | Pax×Portion | animal,weight,dimensions,docs conditional | authored list | pet/cabin quota, provider approval |
| A14 | Pet in hold / `PerItem` | Pax×Portion | A13 + `sizeBracket` if authored | authored size bracket | hold capability+connection rules |
| A15 | Wheelchair / `PerPassenger` | Pax×Flight | `assistanceSsrCode` | allowed SSR codes | no automated confirmation without source |
| A16 | Disability assistance / `PerPassenger` | Pax×Flight | `assistanceSsrCode` | allowed SSR codes | eligibility/approval |
| A17 | Medical equipment / `PerPassenger|PerItem` | Pax×Flight | `equipmentCode`, evidence,oxygenUnits conditional | authored equipment | medical approval/oxygen capacity |
| A18 | Bassinet / `PerPassenger|PerItem` | Infant + Guardian / Flight | `infantRef`,`guardianRef` | actual travellers | age/association/space verified |
| A19 | UMNR / `PerPassenger` | Child×Bound | `childRef`, verified handoff/pickup contacts | Ordering contact records | no boolean-as-proof; connection/country constraints |
| A20 | Lounge / `PerPassenger` | Pax×Airport | `airportId`, facility/time/guests conditional | authored facility, supplier slot if available | time-window and guest pricing |
| A21 | Fast Track / `PerPassenger` | Pax×Airport | `airportId`,facility/time conditional | authored airport/terminal | cutoff/availability source |
| A22 | CIP / `PerPassenger|PerItem` | Pax or Order if truly allowed | `airportId`, facility, appointment/guests conditional | authored package/facility | grouped components one charge; provider |
| A23 | Priority boarding/checkin / `PerPassenger` | Pax×Flight | `optIn:true` | typed boolean | no duplicate verified fare benefit |
| A24 | Onboard Wi-Fi / `PerItem|PerPassenger` | Pax or Order if allowed | `planCode`,deviceCount conditional | **same DefinitionRef** | provider activation later, one charge for Order scope |

### 12.2 تست مثبت/منفی و داده Source برای هر Variant

#### A01 — Extra checked bag

- **Input:** `{quantity:2}` برای `offerItemRef` خاص `ADT-1,B1`. `quantity=1` همان Candidate ولی Token متفاوت؛ `ADT-2` Token دیگر.
- **Source:** checked baggage allowance همان Coupon؛ filed increment limit؛ active rate PerPiece/ROE/Tax؛ remaining cumulative count اگر policy می‌خواهد.
- **List:** Inline، `min/max`, `unitPrice`, `coverage`, `selectedCount=0` پیش از انتخاب؛ `canSelect=true`, `canPurchase=false` تا Evaluate.
- **Negative:** max violation؛ duplicate same counting-family؛ Allowance unknown؛ `A01` Order-scope؛ ticket Used postIssue. **Handoff:** `quantity:2, travellerId, coverage=Portion, flightIds, serviceSubCode, exact tax-fee lines`.

#### A02 — Fixed weight package

- **Input:** `{packageProductRef:"BAG_WEIGHT_10",quantity:1}` فقط اگر همان **active Definition.ServiceDefinitionRef**؛ با وجود دو Package، دو OfferItemRef مستقل؛ تغییر SKU از 5 به 10kg نیازمند Candidate دیگر.
- **Source:** `BaggageSpecification.PackageWeightKg` برای هر Definition؛ published A02 Rate `PerItem`؛ exclude double charge/use included kilos.
- **List:** بسته ۵ و ۱۰kg دو ردیف یا grouped list با ۲ distinct candidate IDs در Presentation؛ **در Domain یک Definition چند option invented نسازید**.
- **Negative:** مقدار نامعتبر `<PACKAGE-DIFFERENT>` یا `PlanKind` به‌جای SKU، یک Package متعلق به Definition دیگر، یا Quantity بالاتر از سقف => `SelectedOptionNotOffered`/Invalid. **Handoff:** selected packageRef، وزن تعریف‌شده و quantity.

#### A03 — Overweight

- **Input:** `{bagRef:"<actual-bag-ref>",measuredWeightKg:27.5}`. `MeasuredWeightKg` به تنهایی کافی نیست؛ `chargeableWeightKg` باید تحت Rule معتبر محاسبه شود (مثل فقط وزن مازاد یا کل وزن نرخ‌گذاری‌شده). انتخاب یکی از این دو بدون Source Rule غلط است.
- **Pricing:** PerPiece = یک واحد overweight طبق Rule؛ PerKilogram = `verifiedChargeableKg × verifiedRatePerKg` با rounding/step واقعی Supplier.
- **Negative:** bagRef نامعتبر، وزن نامعتبر، allowance unknown، chargeable basis absent => `NeedsVerification`/no token. **Handoff:** measured weight/chargeable weight و evidence، نه فقط Quantity:int.

#### A04 — Oversize

- **Input:** `{dimensions:{lengthCm:...,widthCm:...,heightCm:...}}`؛ هر عدد >۰ و constraints واقعی.
- **Pricing:** PerPiece؛ اگر A03 هم‌زمان برای همان BagRef باشد، `ChargeCombination` باید از Rule واقعی معلوم باشد.
- **Negative:** size missing/oversize excluded، overlap double fee without verified rule. **Handoff:** Dimensions و coverage، approval Provider بعداً.

#### A05 — Cabin bag

- **Input:** `{quantity:1}` با cabin allowance per Pax/Coupon و operating aircraft restrictions.
- **Pricing:** PerPiece؛ ExistingServices و ticket stage برای تعداد مجاز مؤثر است.
- **Negative:** unknown cabin allowance، no capacity، count violation، Order scope. **Handoff:** cab-baggage kind distinct from checked bag.

#### A06 — Sports/special equipment

- **Input:** `{equipmentKind:"<authored-kind>",weightKg:12,dimensions:{...}}` فقط فیلدهای required فعلی Definition.
- **Pricing:** PerItem/PerPiece؛ نوع و محدودیت‌های اندازه/وزن از Authoring واقعی.
- **Negative:** arbitrary kind، embargo، oversized/overweight، supplier approval pending. **Handoff:** Specialized SSR/provider requirements.

#### A07 — Standard seat

- **Input:** `{kind:"seatMap",seatMapSelectionId:"<43-char-id>"}`؛ Source regenerated SeatMaps، نه decode SHA.
- **Source:** FlightFlow occupancy کامل، AirInfo layout matching، paid/free seat Product با نرخ معتبر.
- **Negative:** occupied، forged id، aircraft swap، missing commercial candidate/price، same Pax two seats. **Handoff:** SeatNumber, OrderTravellerId, FlightCapacityId, exact money, SeatHold later.

#### A08 — Preferred/exit-row seat

- **Input:** همان A07؛ برای exit row `exitRowTermsAccepted:true` در **فیلد افزایشی Evaluate Seat line**، نه در SeatMapsDto.
- **Source:** preferred characteristics و eligibility verified، cabin/child/infant/adult restrictions.
- **Negative:** child exit-row، false consent، cabin mismatch، source unknown. **Handoff:** seat category/accepted terms evidence.

#### A09 — ExtraSeat (EXST)

- **Input:** `{seatNumber:"12B",purpose:"<authored-purpose>"}` در generic offers؛ A07/A08-style generic SeatMap ID برای EXST فرض نکنید.
- **Source:** adjacent seat/capacity rules، BookingMethod، Special Ticket treatment.
- **Negative:** unavailable adjacency، false capacity، insufficient provider support. **Handoff:** specialized EXST physical/ticket obligation، not regular seat.

#### A10 — Cabin upgrade

- **Input:** `{targetCabinId:2,quoteRef:"<valid-provider-quote>"}` فقط در Quote-required Source؛ Fixed tariff مطابق authored rule.
- **Source:** `CouponUpgradable=false` => reject؛ true تنها شرط لازم، FareFamily/RBD/target cabin/exchange policy.
- **Negative:** wrong target، expired supplier quote، coupon partly unupgradeable، postissue exchange not supported. **Handoff:** target/coupon/ticket exchange routing، نه EMD arbitrary.

#### A11 — Free special meal

- **Input:** `{mealCode:"<authored-SSR-code>"}`، نه quantity؛ Free فقط با explicit filed PriceOrigin.Free یا verified fare rule.
- **Source:** Passenger/Meal restrictions، lead time، SSR.
- **Negative:** wrong meal code، catering cutoff، paid meal inferred free، same meal family. **Handoff:** zero-money evidence و confirmation requirements.

#### A12 — Paid preorder meal

- **Input:** `{menuItemRef:"MENU_PASTA_01",quantity:2}` تنها وقتی actual authored MenuRef.
- **Source:** real menu item option، catering stock/cutoff/price.
- **Negative:** arbitrary MenuItemRef، Qty too large، no trusted menu option، duplicate meal family. **Handoff:** item/qty/Pax/flight.

#### A13 — Pet in cabin

- **Input:** `{animalType:"<authored>",combinedWeightKg:7.1,carrierDimensions:{...},documentAcknowledgements:[...]}` طبق Definition.
- **Source:** allowed type, age, weight, operating carrier PETC constraints, quota and approval.
- **Negative:** young/unallowed animal، missing docs evidence، source capacity unknown. **Handoff:** specific Pet service, not baggage piece.

#### A14 — Pet in hold

- **Input:** A13 plus `{sizeBracket:"<authored-bracket>"}` conditional.
- **Source:** cargo hold aircraft/connection restrictions، bracket policy.
- **Negative:** size bracket mismatch، no hold provider، unsupported connection. **Handoff:** hold-type pet BookingMethod and Provider.

#### A15 — Wheelchair

- **Input:** `{assistanceSsrCode:"WCHR"}` فقط وقتی WCHR در AllowedSsrCodes است.
- **Source:** passenger/airport lead time and supplier ack.
- **Negative:** invalid SSR، airport cutoff، pending approval treated as confirmed. **Handoff:** SSR + not-yet-confirmed semantics.

#### A16 — Disability assistance

- **Input:** `{assistanceSsrCode:"<authored-code>"}`.
- **Source:** approved SSR and Airport/Carrier conditions؛ no guessed disability/medical facts.
- **Negative:** unknown SSR، outside window، unsupported Provider. **Handoff:** assistance booking requirements.

#### A17 — Medical equipment/Oxygen

- **Input:** `{equipmentCode:"<authored-code>",evidenceDocumentRefs:["<authorized-ref>"],oxygenUnits:1}` if oxygen.
- **Source:** verified approval/documents, oxygen volume/provider stock.
- **Negative:** missing document، expired approval، units overmax، source outage. **Handoff:** evidenceRefs only, no personal health content in token.

#### A18 — Bassinet

- **Input:** `{infantRef:"INF-1",guardianRef:"ADT-1"}`.
- **Source:** infant and adult association, age, cabin/layout eligibility, bassinet availability.
- **Negative:** forged guardian، infant too old، ineligible seat/cabin، source capacity absent. **Handoff:** linked infant/adult refs.

#### A19 — UMNR

- **Input (TARGET):** `{childRef:"CHD-1",guardianHandoffContactRef:"<verified-order-contact>",guardianPickupContactRef:"<verified-order-contact>"}`.
- **Source:** real Order contact records/permission, passenger age, connection/minor policy.
- **Negative:** only booleans true without contacts، invalid contacts، connection forbidden، no supplier confirmation. **Handoff:** verified contactRefs only; Stage1-contract compatibility task required.

#### A20 — Airport Lounge

- **Input:** `{airportId:...,facilityRef:...,timeWithOffset:"...",guestCount:0}` as required by spec.
- **Source:** actual station/terminal/timezone, provider appointment availability, guests pricing.
- **Negative:** wrong airport، invalid DST، guests fee unknown، source slot absent. **Handoff:** appointment/facility/scope.

#### A21 — Fast Track

- **Input:** `{airportId:...,facilityRef:...? ,timeWithOffset:...?}` conditional.
- **Source:** departure/arrival airport rule, time window/terminal, provider.
- **Negative:** invalid terminal، past window، no provider. **Handoff:** station/time/service method.

#### A22 — CIP Meet & Assist

- **Input:** `{airportId:...,facilityRef:...,timeWithOffset:"...",guestCount:...}` conditional.
- **Coverage:** Pax default؛ Order فقط با verified Definition/Provision/Supplier and one-fee rule.
- **Source:** bundled components, appointment, guest fee and provider support.
- **Negative:** Order-scope with per-pax cost without eligible rule، duplicate component charge، no slot. **Handoff:** one order-scoped service / no dummy Pax.

#### A23 — Priority boarding/check-in

- **Input:** `{optIn:true}` per Pax×Flight.
- **Source:** validated fare benefits and current existing services.
- **Negative:** false optIn، benefit already included، fare-benefits source missing => NeedsVerification. **Handoff:** SSR/priority as approved.

#### A24 — Wi-Fi

- **Input:** `{planCode:"WIFI_FULL_FLIGHT",deviceCount:1}`; PlanCode EXACT active DefinitionRef; `PlanKind` only descriptive.
- **Coverage:** Pax or true Order-scoped Plan; charge exactly once for Order unit.
- **Source:** actual aircraft eligibility, device cap, provider activation path.
- **Negative:** caller sends PlanKind instead of DefinitionRef، devices over max، provider unsupported، wrong Order coverage. **Handoff:** internal plan SKU, device count, actual Provider key internally.

### 12.3 منشأ Option و نحوه نمایش

| Variant | Verified option source / typed only | Default UI classification |
|---|---|---|
| A01,A05 | quantity typed bounded | Inline |
| A02 | DefinitionRef + PackageWeightKg، ۱ Choice per Definition | Inline |
| A03,A04 | bagRef/weight/dimensions typed، chargeable basis external | DetailsOnDemand یا NeedsVerification |
| A06 | authored equipment kind singleton، typed dimensions/weight | DetailsOnDemand |
| A07,A08 | SeatMap pricing by Pax/Flight | SeatMap |
| A09 | authored purpose + seat typed | DetailsOnDemand |
| A10 | authored target cabin + verified dynamic quote | RequiresExternalQuote |
| A11 | authored MealCode | Inline when facts complete |
| A12 | authored MenuItemRef singleton + qty | Inline or Details |
| A13,A14 | authored AnimalType/SizeBracket، typed dimensions | DetailsOnDemand |
| A15,A16 | authored SSR code list | Inline or Details according to approval |
| A17 | authored MedicalEquipment, verified docs | DetailsOnDemand |
| A18,A19 | verified TravellerRef/OrderContactRef | DetailsOnDemand |
| A20,A21,A22 | authored Airport/Facility, time typed, provider slot verified separately | DetailsOnDemand/NeedsQuote |
| A23 | optIn typed, fare benefit verified | Inline if evidence |
| A24 | PlanCode=DefinitionRef، device count | Inline if source provider supports |

منبع Unknown → صفر Choice معلوم، `sourceState=Unknown` و no purchasable ref؛ List نباید گزینه‌های فرضی FullFlight/VG/5kg ایجاد کند. بزرگ بودن گزینه‌ها → `isComplete=false` و `requiresDetails=true` نه «گزینه‌ای وجود ندارد».

## 13. ماتریس سناریوهای Acceptance و Anti-Regression — قبل از پیاده‌سازی نوشته شود

**معیار تست:** هر مورد ورودی، منبع حقیقت، انتظار HTTP/Readiness، اثر State و اثبات عدم فروش نامعتبر دارد. برای Code-only Closure، همه مواردی که Source/Ordering واقعی هنوز آماده نیستند باید با **Contract Fake خوانا** و تست Host/Mapper معتبر شوند؛ در گزارش با `[EXTERNAL_PENDING]` از اجرای Integration واقعی جدا شوند. نتایج CI/`dotnet test` بدون command، count و SHA ثبت‌شده معتبر نیستند.

### 13.1 Scenario suite تجارت دو جریان

| ID | Given / When | Expected (exact observable invariant) | Owner |
|---|---|---|---|
| FLOW-01 | Valid priced Offer, no Order, List | works under `SourceKind.Offer`, no invented OrderId | Ancillary/AirAvail |
| FLOW-02 | Same Offer, client creates Order, List with OrderId | SourceKind.Order, `PreOrder`, facts originate from Ordering | Ordering adapter |
| FLOW-03 | Order Issued, Order List | SourceKind.Order, `PostTicketed`, coupon/service facts read | Ordering adapter |
| FLOW-04 | Order from Offer changed afterwards | original Offer-based pricing not blindly accepted | Ordering adapter |
| FLOW-05 | 2 adults × 2 bounds × 2 flights | stable Pax×Scope candidates, no product cross-assignment | engine |
| FLOW-06 | Infant without seat entitlement | no paid/occupied seat invented for lap infant | seat engine |
| FLOW-07 | Order scope A22/A24 | single offer item/price; `travellerRefs=[]`, no fake Traveller | engine/Ordering contract |
| FLOW-08 | Order scope A01/A12/A13/A23 | no Candidate or hard NotEligible; forgery Evaluation rejected | engine |
| FLOW-09 | Order change to Coupon Used | corresponding post-ticketed candidate removed or NotEligible | adapter+engine |
| FLOW-10 | One Pax coupon Open, other Used | allowed only for Open Pax/Flight unit | adapter+engine |
| FLOW-11 | Order Refunded/Cancelled | no new sellable quote for affected services | adapter |
| FLOW-12 | Original Offer expired but Order valid | OrderShopping still works from Order facts; no stale AirOffer fallback | adapter |
| FLOW-13 | Order source API outage | explicit unavailable/error, no fabricated blank order | adapter |
| FLOW-14 | OrderCommercialVersion changes mid checkout | Resolve Stale, no Add success | Ordering/Resolve |
| FLOW-15 | Same `OfferId` and `OrderId` body | 422, no Source selected | public API |
| FLOW-16 | API caller for foreign orderId | 403/404, no order details leak | Auth |

### 13.2 List/Details/UI/Surface suite

| ID | Given / When | Expected | Owner |
|---|---|---|---|
| VIEW-01 | ۲۰ configured candidate rows | ۱ List response، no implicit ۲۰ Details | Query |
| VIEW-02 | A01 simple fixed quantity | List includes SelectionField/Qty/price preview; user can choose inline | mapper |
| VIEW-03 | A02 5kg/10kg independent Definitions | ۲ candidates/SKUs، selectable without fabricated options | mapper |
| VIEW-04 | A24 `ServiceDefinitionRef` plan | exact valid choice from authored spec | mapper |
| VIEW-05 | A13 with many forms | `DetailsOnDemand`, List not gigantic | mapper |
| VIEW-06 | Dynamic supplier menu offline | `optionSourceState=Unknown`, no fake VG/CH menu | Query |
| VIEW-07 | ۳۰ valid choices > inline budget | `isComplete=false,requiresDetails=true`, Details full list | mapper |
| VIEW-08 | One Details clicked | only candidate selected retrieved; no ۱۹ sibling Calls | host/front contract |
| VIEW-09 | Existing Offer-based Backoffice golden | all previously published wire paths/types kept | Backoffice mapper |
| VIEW-10 | Existing Offer-based API golden | `meta,data,included.bags,other-services` all intact | Api mapper |
| VIEW-11 | Existing OtaPanel golden | `travellerGroups`/`orderScopedServices` intact | Ota mapper |
| VIEW-12 | Agency API response | no `pointOfSaleId`,`internal SupplierId`,`quoteProviderKey`,`airlineOfficeId`,`PCC` | security |
| VIEW-13 | Public API 24 types | human-readable names/types/constraints/error codes per contract | Api mapper |
| VIEW-14 | Backoffice diagnostics | operator can see rule/Revision details under role | Backoffice mapper |
| VIEW-15 | Reusing cached Details after sourceVersion changes | re-fetch/invalidated; optionRefs stale | client/Query |
| VIEW-16 | ۲ eligible POS price differently | explicit provenance/no arbitrary first POS | Query |

### 13.3 Exact selection, Price, Basket, Money

| ID | Given / When | Expected | Owner |
|---|---|---|---|
| PRICE-01 | A01 Qty1 vs Qty2 same item | ۲ priced tokens different and totals 1× vs 2× | Pricing |
| PRICE-02 | same A01 for T1 and T2 | per Pax quote refs and OrderTravellerId distinct | Pricing |
| PRICE-03 | A24 `PerItem` with deviceCount=3 | price not accidentally tripled unless authored per-device | Pricing |
| PRICE-04 | A15 `PerPassenger`, no quantity field | effective quantity 1, exact line total | Pricing |
| PRICE-05 | A10 fixed upgrade `PerPassenger`, Qty absent | full quote if all rules valid (not null just for missing Quantity) | Pricing |
| PRICE-06 | A03 `PerKilogram`, chargeable basis source absent | Incomplete + no token, reason `ChargeableWeightNotDetermined` | Pricing |
| PRICE-07 | A03 source knows only measured weight | no invented excess kg by subtracting allowance without rule | Pricing |
| PRICE-08 | A03 trusted chargeable basis supplied | exact decimal weight×rate, negative/step reject | Pricing external contract |
| PRICE-09 | Tax Added vs Included | exact arithmetic once each, Type/Country/Station trace | Pricing |
| PRICE-10 | Fee per Order and ۳ selection lines | one shared Fee in basket only, no double charge | Pricing |
| PRICE-11 | PerTicket shared fee ۲ lines same ticket | once by real fee key and ticket ref | Pricing |
| PRICE-12 | Shared fee authority/amount unknown | basket total incomplete, no purchasable token for final charge | Pricing |
| PRICE-13 | Multi-currency with verified ROE | precision and rounding per published conversion | Pricing |
| PRICE-14 | ROE 9 decimal places | no 18,2 truncation or binary float | Pricing |
| PRICE-15 | zero price with explicit Free | price zero **and** isComplete true, valid currency evidence | Pricing |
| PRICE-16 | Null Price and unknown benefit | never `0.00` or `canPurchase=true` | Pricing |
| PRICE-17 | A10 CouponUpgradable=false | NotEligible + `CouponNotUpgradable` | Domain engine |
| PRICE-18 | CouponUpgradable=true, provider quote unavailable | NeedsQuote, no ref | Domain engine |
| PRICE-19 | One Line Rejected in ۳-Line basket | allReady=false, whole basket not committed | Evaluate/Ordering |
| PRICE-20 | duplicate exact candidate same Basket | 422 `DuplicateSelectionScope` (chosen rule) | Evaluate |
| PRICE-21 | same SeatID twice | reject duplicate | Evaluate |
| PRICE-22 | Qty=0 explicit | omit/remove line in UI, do not generate zero purchase | Evaluate |
| PRICE-23 | exact Price changes 1 minor unit | Resolve Stale and returns new price advisory | Resolve |
| PRICE-24 | Multiple same family services exceeds cumulative max | Basket conflict; no extra charge | engine |

### 13.4 Seat regression suite (complete)

باید تمام ۱۶ تست `S2-SEAT-01..16` فصل ۰۹ اجرا شوند. **افزون بر آنها:**

| ID | Given / When | Expected |
|---|---|---|
| SEAT-17 | Old seatMap OfferId available before Order; create Order | Source→Order mapping validated; seat id accepted only if same valid snapshot |
| SEAT-18 | Order-based SeatMap with missing `originPricedOfferId` | explicit incompatibility; no fabricated `flightOfferId` |
| SEAT-19 | Two seats same flight, same passenger; different Cabin | conflict before Add |
| SEAT-20 | Seat per flight in connection with two legs | distinct `FlightRef`/capacity; BoundId not mistaken for a physical Flight |
| SEAT-21 | Missing one FlightFlow FlightId in OccupiedSeats response | entire affected map/unpriced seat unavailable, not `AVAILABLE` |
| SEAT-22 | Seatmap and Ancillary generic joint Batch | mixed seat and baggage exact totals, two provider owners, Order handoff correct |
| SEAT-23 | Seat status AVAILABLE but candidate.Price null | fails after internal fix, null Id+Price and BLOCKED |
| SEAT-24 | Seat free with verified explicit entitlement | Price.Total `0` true and Id valid; not fake zero |
| SEAT-25 | Same 43-char id from foreign agency | 403/404 even with shared POS |

### 13.5 Token/Resolve/Auth/Concurrency suite

| ID | Given / When | Expected |
|---|---|---|
| SEC-01 | Alter a byte of encrypted Token | integrity fail; no price exposed |
| SEC-02 | Unknown `keyId`, key revoked | no acceptance; typed reason |
| SEC-03 | Expired Token | `Expired` not auto renew |
| SEC-04 | Token `nbf` future, clock skew | reject according to configured safety window |
| SEC-05 | Token Source=Offer used in Add Order | reject; must re-Evaluate order-scoped |
| SEC-06 | Token orderId=11 used on OrderId=12 | NotAuthorized/NotFound (safe), no price leak |
| SEC-07 | AgencyA token used by AgencyB same POS | reject |
| SEC-08 | Same agency, other Office without selling permission | reject |
| SEC-09 | Valid Token with OrderVersion old | Stale even before TTL |
| SEC-10 | Token PriceRevision old | Stale, no silent reprice |
| SEC-11 | Option removed after Evaluate | Stale, no arbitrary fallback |
| SEC-12 | Different quantity same OfferItemRef | different token/evaluation, exact price |
| SEC-13 | Different A12 MenuItem same Definition not authored | 422 |
| SEC-14 | Order-scoped A24 with Pax pseudo-ref | reject; genuine empty beneficiaries only |
| SEC-15 | Resolve endpoint without JWT | HTTP 401 |
| SEC-16 | JWT has user/API scope but not internal service scope | HTTP 403 |
| SEC-17 | JWT internal_service but non-allowlisted client | HTTP 403 |
| SEC-18 | Missing Allowlist config at startup | fail-closed access, no public route |
| SEC-19 | Same Ordering Add `idempotencyKey` + same payload | same result, exactly one OrderChange |
| SEC-20 | Same key + different payload | conflict, no partial duplicate |
| SEC-21 | Concurrent Order Change between Resolve and Persist | Optimistic Concurrency conflict; no stale OrderService |
| SEC-22 | Client sends supplierKey/currencyOverride | rejected/ignored under explicit schema, no injection |
| SEC-23 | Client sends raw selected price | trusted server ignores; no underpayment |
| SEC-24 | Very large Token/JSON duplicate fields | constrained validation, no DoS/ambiguous parse |
| SEC-25 | One invalid token in many | Resolve returns all statuses, `allValid=false`, Ordering no silent subset |

### 13.6 PostTicketed and S3 handoff

| ID | Given / When | Expected |
|---|---|---|
| POST-01 | Ticketed Order with fresh valid flight coupon | Eligible service appears only if PurchaseStage permits |
| POST-02 | Used/Flown segment | no add-on on consumed flight unless explicit supplier exception |
| POST-03 | Already issued A01 bag EMD | duplicate counting family blocked |
| POST-04 | Refund service but coupon Open | policy/waiver/source decides re-purchase; never assume yes |
| POST-05 | Priority already included fare | no paid duplicate quote |
| POST-06 | A10 ticket exchange needed | needs verified exchange/reshop support; no fake EMD route |
| POST-07 | CIP guest count fee unknown | no false basket total |
| POST-08 | Supplier hold fails after valid S2 Resolve | Stage3 status pending/failed truthfully; S2 Token not a Hold guarantee |
| POST-09 | Partial provider confirm | Order per-unit status and compensation belong Stage3/Ordering |
| POST-10 | EMD-A Bag, EMD-S standalone Lounge | metadata routed correctly, only Ordering/Ticketing issues actual doc |
| POST-11 | Wallet/Card/Agency credit/cash settlement | Pricing Shopping unaffected; Payment ownership in Ordering/JetPay |
| POST-12 | Ticketing/Payment call times out | no claim of issued/charged state from S2 |
| POST-13 | Order scoped A24 in Ordering TravellerId long NOT nullable | explicit `EXTERNAL_PENDING` until Consumer supports cardinality; no dummy ID |
| POST-14 | Only Order context endpoint unavailable | do not claim real PostOrder integration complete |

### 13.7 Coverage × Surface × Source combinatorial gate

- **For every A01–A24:** at least ۱ positive/eligible or fully documented external-block fixture، ۱ missing-field، ۱ wrong-option، ۱ source-unknown case where relevant. All ۲۴ have a traceable `VariantCode` and `CustomerSelectionContract` validation.
- **For each public surface** Backoffice/Api/OtaPanel: test Offer-based List, Order-based List, Details only-needed, Single Evaluate legacy, Batch Evaluate, SeatMap Offer legacy, SeatMap Order (if required context), Auth fail. PII/Provider secrets not leaked in Api/Ota.
- **All source outcomes** Accepted, Incomplete, Rejected, NeedsQuote, NeedsVerification, Stale, Expired, NotAuthorized, Unavailable, ConflictingOrderVersion. HTTP body status and HTTP code mapping must be snapshot-tested.
- **Golden comparison:** ۹ old S2-01 fixtures *semantic/value compare* for existing members؛ not merely path subset (current `S2GoldenContractTests` only checks property paths). Values intentionally fixed by Price/Seat Bugfix get an explicit expected change review.
- **Build and full discovered tests:** list assemblies, per-assembly discovered count/executed pass/fail/skipped, no abort، same SHA, CI link. A log stating "all tests pass" without counts/command/source commit not valid.

---

## 14. Wire Examples واقعی‌تر برای بررسی فرانت و API Consumer

### 14.1 List با `OrderId` در Agency/OtaPanel [TARGET]

Request: `POST /OtaPanel/v1/Orders/12345/AncillaryOffers` with `{}`. فروشنده احراز شده است؛ نه `agencyId` و نه POS از UI قابل Override نیستند.

```json
{
  "source": {"type":"order","id":"12345","commercialVersion":"V17","purchaseStage":"PreOrder"},
  "currencyId":70,
  "currencyCode":"IRR",
  "travellerGroups":[
    {
      "travellerRef":"ADT-1",
      "passengerTypeCode":"ADT",
      "flightGroups":[
        {
          "flightRef":"B1:F1",
          "services":[
            {
              "offerItemRef":"<opaque-A01-ADT1-B1>",
              "category":"Baggage",
              "variantCode":"A01",
              "title":"Extra checked bag 23 kg",
              "coverageScope":"Portion",
              "coveredFlightRefs":["B1:F1"],
              "quantityUnit":"Piece","minQuantity":1,"maxQuantity":3,
              "unitPrice":{"amount":1000000,"currencyId":70},
              "priceStatus":"Complete","selectionKind":"QuantityChoice",
              "displayOptions":[],
              "selectionFields":[{"name":"quantity","type":"integer","required":true,"min":1,"max":3,"step":1}],
              "interaction":{"mode":"Inline","requiresDetails":false},
              "canSelect":true,"canPurchase":false,
              "offerReadiness":"NeedsSelection","requiresRecheck":true,
              "reasonCodes":[]
            }
          ]
        }
      ]
    }
  ],
  "orderScopedServices":[
    {
      "offerItemRef":"<opaque-A24-order>","category":"Connectivity","variantCode":"A24",
      "title":"Full flight Wi-Fi plan","coverageScope":"Order",
      "displayOptions":[{"fieldName":"planCode","sourceState":"Known","isComplete":true,
        "choices":[{"optionRef":"<optionref>","value":"WIFI_FULL_FLIGHT","label":"Full flight Wi-Fi"}]}],
      "interaction":{"mode":"Inline","requiresDetails":false},
      "canSelect":true,"canPurchase":false
    }
  ],
  "blockedCount":0,
  "ratesOfExchange":[]
}
```

**این یک نمونه فشرده قراردادی است و سایر فیلدهای CURRENT OtaPanel مانند `orderLevelFees,taxes,fees,availabilityState` حذف‌شدنی نیستند.** برای فاز PostIssue تنها `source.purchaseStage` و Facts تغییر می‌کنند، نه Envelope اصلی و Engine. `offerId` قدیمی در Order response تنها وقتی origin Offer معتبر دارد پر شود؛ اگر origin موجود نیست null در Source جدید و خروجی source به عنوان مرجع اصلی ارائه گردد.

### 14.2 Partner API — حفظ `data/included` موجود، افزودن human-readable choice [TARGET]

```json
{
  "meta":{"count":1,"withheldCount":0,"status":"NeedsSelectionOrRecheck"},
  "source":{"type":"order","id":"12345","commercialVersion":"V17"},
  "data":{"type":"ancillary-offers","flightOfferId":null,
    "travelers":[{"id":"1","travelerType":"ADULT"}],
    "segments":[{"id":"1"}]},
  "included":{
    "bags":{
      "<candidate-ref>":{
        "name":"EXTRA_CHECKED_BAG",
        "segmentIds":["1"],"travelerIds":["1"],
        "ancillary":{
          "type":"CHECKED_BAGS",
          "selectionKind":"QuantityChoice","coverageScope":"Portion",
          "canSelect":true,"canPurchase":false,
          "interaction":{"mode":"Inline","requiresDetails":false},
          "selectionContract":{"kind":"QuantityChoice","fields":[{"name":"quantity","type":"Integer","required":true}]},
          "quantity":{"unit":"Piece","min":1,"max":3,"step":1},
          "displayOptions":[],
          "pricePreview":{"currencyCode":"IRR","unitAmount":"1000000","amountKind":"Unit","isComplete":true},
          "readiness":"NeedsSelection","reasons":[]
        }
      }
    },
    "other-services":{}
  }
}
```

**احتیاط:** current `ApiBagDto` و nested `ApiAncillaryDto` اعضای بیشتری دارند؛ JSON نمونه *بخشی از شکل* برای نمایش additions است و نمونه یک‌به‌یک DTO current نیست. در پیاده‌سازی، هیچ Property موجود از `ApiBagDto`, `ApiOtherServiceDto`, `ApiAncillaryDto` حذف یا rename نشود. برای Order source نام `flightOfferId` فعلی هنگام نبود original priced offer نباید با OrderId پر شود؛ امکان nullable در DTO جدید و JSON contract version بررسی و تست شود.

### 14.3 Details فرم پیچیده A13 [TARGET]

Request: `POST /Api/v1/AncillaryOffers/Details`

```json
{"orderId":12345,"offerItemRef":"<A13-candidate-ref>"}
```

Response در فیلد `data` سطح API (خلاصه، همراه سایر existing fields):

```json
{
  "data":{
    "id":"<A13-candidate-ref>",
    "selectionContract":{"kind":"TypedForm","fields":[
      {"name":"animalType","type":"Code","required":true},
      {"name":"combinedWeightKg","type":"Decimal","required":true},
      {"name":"carrierDimensions","type":"DimensionsCm","required":true},
      {"name":"documentAcknowledgements","type":"CodeSet","required":true}
    ]},
    "optionGroups":[{"fieldName":"animalType","sourceState":"Known",
      "choices":[{"optionRef":"<option-ref>","value":"<authored-animal-type>","label":"<label>","availability":"NotChecked"}]}],
    "optionSourceState":"Known",
    "priceCompleteness":{"isComplete":false,"reasonCodes":["SelectionRequired"],"requiredInputs":["animalType","combinedWeightKg","carrierDimensions","documentAcknowledgements"]},
    "commercialTerms":{"confirmationRequirement":"SubjectToConfirmation","cancellationRules":[]}
  }
}
```

**نکته:** `commercialTerms` در DTO واقعی current فیلدهای بیشتری دارد که باید حفظ شوند؛ `SubjectToConfirmation` تنها نمونه است، نه مقدار همه Petها. Required بودن مدارک فقط وقتی Definition چنین چیزی author کرده است.

### 14.4 Resolve خطای قیمت/Source [TARGET]

```json
{
  "orderId":12345,
  "orderCommercialVersion":"V18",
  "allValid":false,
  "items":[
    {"status":"Stale","reasonCodes":["OrderCommercialVersionChanged"],
      "sourceSelectionRefDigest":"<sha256-digest>",
      "currentPrice":{"currencyId":70,"lineTotal":1200000,"isTotalComplete":true}},
    {"status":"NotAuthorized","reasonCodes":["SelectionScopeMismatch"],
      "sourceSelectionRefDigest":"<sha256-digest>"}
  ],
  "basketPricing":{"isComplete":false,"total":null,"reasonCodes":["AtLeastOneSelectionInvalid"]}
}
```

No raw token/logs/secret in response. Client must confirm new amount and re-Evaluate; Ordering must not insert any of the two lines.

---

## 15. Error Contract، Source Fallback، UX و Telemetry

### 15.1 Codes و HTTP mapping

| Case | API HTTP | Stable reason/status | UX Action | Effect |
|---|---|---|---|---|
| request malformed/XOR fails | `400/422` طبق Framework فعلی | `InvalidShoppingSource` | صحیح‌کردن request | no engine run |
| unauthenticated | `401` | `Unauthenticated` | login | no leakage |
| wrong authorized buyer/channel/POS | `403` or safe `404` | `ShoppingCallerNotAuthorized` | عدم نمایش | no leakage |
| candidate or seat ID from another source | `404/422` | `OfferItemWasNotFound`/`InvalidSeatSelection` | fresh List/SeatMap | no token |
| wrong field/type/unknown option | `422` | `SelectionFieldRequired`/`SelectedOptionNotOffered` | فرم را اصلاح کند | no token |
| Order version changed | `409` or `200` per-line Stale for Resolve batch | `OrderCommercialVersionChanged` | re-fetch Order/List | no commit |
| Price/Source/Option revision changed | `200` per-item Stale / `409` on all-source version | `PriceChanged`/`SourceChanged` | reprice + consent | no commit |
| Quote missing or supplier unavailable | `200` with NeedsQuote/NeedsVerification, or 503 for whole upstream outage | `QuoteRequired`/`SourceUnavailable` | later/retry/manual | no fake 0 |
| Token invalid/expired | `422/410` per policy | `InvalidSelectionRef`/`SelectionExpired` | re-Evaluate | no fallback |
| Occupied Seat or complete seat source missing | `200` blocked state / 503 source failure | `SeatUnavailable`/`SeatSourceIncomplete` | refresh SeatMap | no fake id |
| Duplicate Cart / same pax seat | `422` | `DuplicateSelectionScope`/`SeatSelectionConflict` | change basket | no partial commit |
| Order service unsupported | `200` NeedsVerification or dedicated business 422 | `ExternalOrderContractPending` | disclose feature unavailable | no phantom OrderService |

**Error-code governance:** این جدول نام‌های **قراردادی پیشنهادی** است؛ قبل از اضافه‌کردن ExceptionFactory Numeric جدید، نگاشت با کدهای موجود (از جمله 16802،16809،16811) انجام شود. هیچ کد Numeric جدید اختراع یا enum موجود renumber نشود. API عمومی `code` پایدار، `message` human-readable و `fieldIssues[]` بدون افشای Provider secret بدهد. خطای امنیت Scope نباید تفاوت وجود/عدم وجود Order مشتری دیگر را آشکار کند.

### 15.2 Behavior هنگام خالی‌بودن نتایج

- `0 candidates` با Source معتبر/Completed eligibility: پاسخ موفق با `count=0`, reason `NoApplicableOffer` و `source.completeness` معلوم.
- `0 candidates` به‌دلیل outage/unverified catalog: **همان ۰ بدون توضیح نیست**؛ باید status unavailable/partial با diagnostics و no purchasable path باشد.
- Order با یک Coupon Open و دیگری Used: Candidateهای eligible تنها برای Coupon Open و Coverage مناسب ظاهر می‌شوند؛ groupهای unaffected سالم برمی‌گردند.
- `OfferReadiness.NeedsQuote` به معنی «فروش حتماً ممکن خواهد بود» نیست؛ button «استعلام قیمت» اگر Integration واقعی پشتیبانی کند، در غیر این صورت feature unavailable.

### 15.3 Frontend interaction / وایرفریم رفتاری

```text
[Order #12345 — مرحله: آماده پرداخت / یا صادرشده]
[پرواز رفت: B1:F1] [مسافر: ADT-1 / ADT-2] [پرواز برگشت: B2:F1]

بسته‌های ساده (Inline):
  [ADT-1] اضافه‌بار چمدان ۲۳ کیلو | قیمت واحد ... | [-] 2 [+]
  [ADT-2] اضافه‌بار چمدان ۲۳ کیلو | قیمت واحد ... | [-] 1 [+]
  [Order]  طرح Wi-Fi طول پرواز | [Plan SKU] [DeviceCount] 

خدمات فرم‌دار:
  حیوان در کابین [جزئیات و مدارک] -> onClick GET/POST Details همان مورد
  CIP [فرم/رزرو زمان] -> onClick Details + provider availability if real

صندلی:
  انتخاب صندلی [بازکردن SeatMap] -> SeatMapsDto -> id انتخابی

سبد:
  [T1: Qty2] [T2: Qty1] [Seat T1/12A]
  [بررسی انتخاب‌ها و قیمت نهایی] -> ONE Batch Evaluate
  [مبلغ: کاملاً محاسبه‌شده؟] [تغییر قیمت؟] [دکمه تأیید افزودن]
  -> Ordering Add -> Resolve -> Status/OrderChange/Payment next steps
```

**Edge UX:** یک خط NeedsQuote کل پرداخت سبد را خودکار انجام نمی‌دهد؛ می‌توان آن خط را حذف و با تأیید مجدد کاربر Basket باقی‌مانده را Evaluate کرد. اگر Qty change شد token قبلی آن Line معتبر نیست. وقتی Seat انتخابی Occupied شد، UI row دیگر را مجبور به پرداخت مجدد نمی‌کند بلکه conflict را نشان می‌دهد و re-Evaluate Basket الزامی است. Seller API به جای `detailsRequired` UI button، فیلد machine-readable `interaction` و schema/links می‌گیرد.

### 15.4 Metrics و observability

- counters: requests per `sourceKind,surface,variant`, `DetailsOnDemand` rate, options source unknown, Evaluate acceptance vs incomplete, Token expiry/stale, SeatMap occupancy failures, Order version mismatch, Resolve latency, Price revision changes, SupplierUnknown.
- هر request: `correlationId`, `sourceKind`, hashed/opaque Source ID, `orderId` فقط internal log guarded, **بدون Token payload/raw, PII, provider secret**.
- UI performance test برای ۲۰ Candidate: تعداد Calls List/Details/Evaluate و تعداد DB queries اثبات شود؛ N+1 قطعی ممنوع، latency budgets فقط بر اساس baseline اندازه‌گیری واقعی و Owner performance SLO تعریف گردد (در این سند latency ms ساختگی قرار داده نشده).

---

## 16. Code delta واقعی، انتخاب Baseline و عدم تولید بی‌رویه کد

### 16.1 پیش‌شرط‌ها در Ancillary HEAD `7efee95`

**حفظ کنید:** Authoring Stage1 و ۹ Profile/۲۴ Variant؛ S2-01 canonical engine/Offer Context/Quantity/Eligibility; `OptionCatalog` و Details additions S2-02; A02/A24 DefinitionRef source binding; OrderScope allowlist A22/A24; OtaPanel `orderScopedServices`؛ S2-02 source fingerprint expansion (با تکمیل‌اش)؛ test goldens و negative rules مفید.

**جایگزین/تکمیل کنید:**

| Delta | کمترین تغییر معقول | Domain/DB Migration؟ |
|---|---|---|
| `OrderShoppingContextReader` | Query/Application Port + trusted Ordering Adapter + mapper/context tests | نه در Ancillary؛ external Ordering S2S endpoint لازم |
| Order-based List route | typed routing/handlers برای سه Surface به همان Engine | نه |
| Order-based SeatMap route | facade جدید reuse existing SeatMap engine+DTO | نه؛ اگر OriginOffer unavailable external gap |
| List Inline data | DTO additive + existing `OptionCatalog`/Commerce terms projected, no per-item business code | نه |
| Existing Details for `orderId` | Source XOR request handling، همان OptionBuilder | نه |
| Batch Evaluate | shared evaluator pipeline + line conflicts & quote aggregation; legacy single preserved | معمولاً نه؛ **A19 minimal ContactRef Domain correction** نیازمند Owner approval |
| SeatMap ID evaluation | regenerate/map existing 43char ID to trusted Seat Candidate, exact price | اصلاح internal SeatMap/Engine/Query، **wire frozen**؛ migration نه |
| Fix seat free/null/status/source | internal guards/fixed price math/occupied presence | نه، مگر داده موجود Source needs change |
| Fix Pricing quantities/fees | reuse engine existing price assessment, effective quantity and Basket fees, no arbitrary client fields | نه به صورت پیش‌فرض |
| `pricedSelectionRef` | secure token protector + claims/TTL/auth tests | **نه table نه Aggregate** |
| `Resolve` S2S | dedicated controller + internal auth + source/price/seat revalidation | نه |
| Ordering handoff | standalone interface DTO JSON schema + example/contract tests in Ancillary | **نه در Ordering code توسط Ancillary agent** |
| Seat/Shopping golden/Scenario tests | regression + full 24 variants, invalid cases | نه |

### 16.2 آیا Rollback لازم است؟

**Recommendation:** به‌صورت پیش‌فرض **rollback نکنیم**؛ HEAD `7efee95` بخش‌های درست و تست‌های موجود ارزشمند دارد. تغییرات شامل جداکردن DTO files/Golden fixtures نیز عمدتاً سازمانی‌اند. ابتدا یک `diff` با `6853e83` و unit coverage گزارش شود. اگر تجربه ایجنت نشان داد مسیر current DTO split به‌طور واقعی Compatibility را خراب کرده، revert **انتخابی همان فایل‌ها** با اثبات؛ نه Reset کورکورانه S1/S2-01. بازگشت کامل فقط وقتی Test/Security evidence ثابت کند incremental repair از baseline clean پرهزینه‌تر است؛ تصمیم Owner. Owner ممکن است Rollback را اجازه دهد اما آن اجازه به معنی توصیه فنی خودکار نیست.

### 16.3 اصل یک‌بار کدنویسی و یک مرجع

پیش از هر coding: (۱) `CLAUDE.md`؛ (۲) Source SHA ثابت؛ (۳) External contract mapping؛ (۴) همین سند approved؛ (۵) no-new-concepts guard؛ (۶) Test matrix اول؛ (۷) Implementation. اگر Pack v13.2 فصل ۰۸ با Stateless conflict دارد، **یک ADR اصلاحی پس از تأیید Owner** در همان Pack/Canonical Authority ثبت شود؛ اجازه ندهید ایجنت به طور هم‌زمان دو روش «جدول Quote» و «signed token» را بسازد.

**Governance:** ایجنت فقط Ancillary را تغییر دهد، سورس Ordering/FlightFlow/AirAvail/AirPrice را نخوانده/حدس‌زده جایگزین نکند؛ برای external missing interface `EXTERNAL_PENDING`. بدون حکم جداگانه Owner commit/push/merge ممنوع. `dotnet` build و تست کامل در محیط Agent اجرا و log به SHA پیوست شود. نبود CI را سبز جا نزنید.

---

## 17. Decision Register، تقدم منابع، جدول Traceability و شرط نهایی پذیرش Owner

### 17.1 تصمیم‌های طراحی برای تأیید یک‌باره، با دلیل و پیامد دقیق

**اینها پیشنهاد نهایی تحلیل‌اند، نه ادعای تصویب قبلی.** پس از تأیید Owner باید در یک ADR واحد «S2-2026-10-20-Owner-Approved» ثبت شوند، تا Agent بین این سند و فصل ۰۸ Pack v13.2 معلق نماند.

| ID | تصمیم پیشنهادی | چرا این گزینه و نه گزینه دیگر؟ | پیامد/هزینه پذیرفتنی | مرجع Benchmark/کد |
|---|---|---|---|---|
| D01 | دو Source Context: Offer پیش از Order و Order از لحظه وجود Order؛ یک Engine | Order مرجع معتبر Pax/coupon/history است | Order S2S Context لازم؛ هیچ fallback به Offer قدیمی | Breeze/Duffel/Travelport §02 |
| D02 | List برای Inline کامل، Details on demand و نه اجباری | ۲۰ Ancillary نباید ۲۰ Roundtrip الزام کند | بازبینی DTO List و حجم گزینه‌ها | §06، Duffel Offer services |
| D03 | Details تنها وقتی `requiresDetails=true` یا Client صریحاً خواست | فرم‌های بزرگ و Dynamic source لزوماً در List مناسب نیستند | همان Details موجود با source Order نیز کار کند | §06 و §15 |
| D04 | سه Surface Mapper با یک Engine؛ API public اصطلاحات انسانی/مشخص | Backoffice، Agency UI و Partner API مصرف‌کننده یکسان نیستند | افزودن view DTO بدون شکستن قدیمی | §06 |
| D05 | Batch Evaluate شامل mixed `ancillary`+`seatMap`؛ تک‌انتخاب legacy حفظ | ۲ مسافر و ۳ خدمت یک قیمت‌گذاری سبد لازم دارند | Basket-level duplicate/shared-fee reconciliation | §07–08 |
| D06 | `pricedSelectionRef` محافظت‌شده، خودتوصیف‌گر و کوتاه‌عمر؛ **بدون DB quote** | از `PricedSelectionAggregate` و Migration اضافی پرهیز می‌کند؛ نیاز به Revalidation پابرجاست | key lifecycle + token versioning + size/privacy tests | §10؛ انتخاب داخلی، نه ادعای Benchmark |
| D07 | `offerItemRef` و `seatMapSelectionId` فعلی Decode نمی‌شوند؛ Resolve با recompute آنها را **match** می‌کند | Hash موجود یک‌طرفه است و SeatMap JSON فریز است | اجرای محدود Seat Pricing هنگام Evaluate/Resolve | §09 / Source code |
| D08 | SeatMap public DTO/routes byte-schema frozen، ولی Seat `AVAILABLE` بی‌Candidate/قیمت درونی اصلاح | تضمین فروش ساختگی ممنوع | آزمون فریز + قواعد availability fail-closed | Pack 09، §09 |
| D09 | Resolve S2S مستقل از Hold؛ Price/Scope/OrderVersion را revalidate می‌کند | صدور Token ضمانت Stock/Order نیست | دو check و امکان Stale/re-evaluate | §11 |
| D10 | Ordering بعد از ایجاد OrderService orchestrates Hold/Payment/EMD؛ Ancillary صرفاً قرارداد مصرف می‌دهد | مالکیت Order و Finance باید محفوظ بماند | Consumer تغییرات خارج Repo | Pack 14، §11 |
| D11 | A02 و A24 هر active `ServiceDefinitionRef` یک SKU/Plan واقعی داخلی | owner decision قبلی، بدون Supplier Catalog جعلی | برای دو SKU دو Definition لازم است | §12 |
| D12 | `Order` coverage فقط A22/A24 وقتی Provision/Provider اجازه دهد | سایر خدمات به Passenger نیاز دارند | Ordering باید واقعی `TravellerId=null` پشتیبانی کند؛ اکنون EXTERNAL_PENDING | Pack 06، §12 |
| D13 | A19 بدون verified guardian contacts پذیرش نمی‌شود | Boolean «تماس ثبت شد» داده واقعی نیست | اصلاح محدود Contract پس از مجوز Owner؛ در غیر این صورت Block | §07، §12 |
| D14 | Fee مشترک بر اساس Order/Ticket فقط یک بار؛ BasketTotal اگر Source ناقص null | جلوگیری از Double-charge | سبدبندی به جای جمع ساده LineTotals | §08 |
| D15 | PerKilogram A03 بدون Source قابل استناد chargeable weight fail-closed | وزن مازاد/کل وزن قابل حدس نیست | سناریوی مثبت معطل Source rule واقعی | §08، §12 |
| D16 | Supplier dependent/EMD status در S2 «قابل بررسی» است نه «رزرو/صدور شده» | Shopping با Fulfillment یکی نیست | `requiresCheckAtReserve=true` صادقانه | Pack 10–14 |
| D17 | API عمومی فعلی `{meta,data,included}` additive بماند؛ UI Panel DTO نمایش‌محور، BO DTO عملیاتی | شکست Client فعلی هزینه غیرضروری دارد | Canonical DTO محاسباتی جدا از UI | §06 |
| D18 | Default بدون Reset کامل HEAD؛ اگر rollback، فقط با diff/tests و تأیید Owner | S2-02 OptionCatalog و تست‌های مفید دارد | حذف selective code در صورت لزوم | §16 |

**نکته بحرانی D06 و فصل ۰۸ Pack:** Pack v13.2 Storage Snapshot را صراحتاً «روش منتخب» نوشته است؛ Stateless یک **پیشنهاد تغییر Authority** است، نه تفسیر بدون تعارض Pack. بعد از تأیید Owner دقیقاً یک راه بماند؛ Agent حق ساخت همزمان `PricedSelections` و Token Stateless ندارد.

### 17.2 نیازهای خارجی که نمی‌توان با Prompt داخلی ناپدید کرد

| Ref | مالک | Contract / Evidence لازم | رفتار Until Ready | وضعیت |
|---|---|---|---|---|
| EXT-01 | Ordering | `GetAncillaryShoppingContextAsync(orderId,callerScope)` برای PreIssue+PostTicketed، version، BuyerSeller/POS، Traveller/Flight/ExistingServices، Coupon/Document، Seat Assignment | Order Source در runtime unavailable؛ Block/EXTERNAL_PENDING، نه fallback خاموش | `[EXTERNAL-CONTRACT]` |
| EXT-02 | Ordering | Add Ancillaries Request با OrderId+`pricedSelectionRefs[]`+Idempotency+ExpectedOrderVersion و mapping `OrderService` | Ancillary فقط Resolve/Contract Tests؛ Order DB نباید از Ancillary نوشته شود | `[EXTERNAL-CONTRACT]` |
| EXT-03 | Ordering | نوع TravellerId nullable برای Order Scoped یا مدل مالکیت معادلِ واقعی، بدون dummy ID | Order-scoped candidate می‌تواند قابل نمایش باشد اما checkout تا Consumer آماده نیست | `[EXTERNAL-CONTRACT]` |
| EXT-04 | AirAvail | Flight offer production parity و Channel cross-surface و SalesScope mapping با enum واقعی producer، نه numeric حدسی | Offer Source mismatch => refusal؛ special OtaPanel mapping باید با Source واقعی منطبق شود | `[EXTERNAL-CONTRACT]` |
| EXT-05 | AirPrice/Offers | معتبر بودن FareIncluded benefits، tax/ROE/filed price و unknown Coupon flags | no phantom included/free benefit | `[EXTERNAL-CONTRACT]` |
| EXT-06 | FlightFlow | complete occupied/seat holds per FlightId، aircraft-version consistency، owner physical | missing response => Block/Unavailable | `[EXTERNAL-CONTRACT]` |
| EXT-07 | Supplier/Provider | A10 dynamic Quote, A20–22 appointment slot, A03 chargeable-weight rule و سایر unsupported capability | NeedsQuote/NeedsVerification؛ no purchased token | `[EXTERNAL-CONTRACT]` |
| EXT-08 | Order/Ticketing | Seat physical assignment + PNR/EMD-A/EMD-S/E-ticket routing and postpaid refund/changes | S2 metadata only؛ not issued claim | `[EXTERNAL-CONTRACT]` |

**درخواست تغییر کد محدود است:** اگر Owner فقط Ancillary را مجاز بداند، هیچ EXT ردیف در مخزن دیگری پیاده نمی‌شود. اما S2 «Contract Ready» می‌تواند با Fake فنی معتبر و Integration Contract Test بسته شود؛ **«Full Operational E2E» بدون Consumer/Source واقعی تأیید نمی‌شود**. گزارش Closure باید این دو سطح را تفکیک کند.

### 17.3 Read-after-Code Traceability — مشخصات به فایل‌های قابل مشاهده

| Requirement | Evidence فعلی Ancillary HEAD | تغییر هدف | تست Gate |
|---|---|---|---|
| Canonical A01–A24 | `Domain/AncillaryServiceDefinitionAggregate/AncillaryVariant.cs`; `CustomerSelectionContract.cs` | Registry بدون کد ساختگی؛ A19 gap | VAR-01..24 |
| Valid Offer context | `Query/Shopping/Common/FlightOffers/FlightOfferShoppingSource.cs`; `FlightOfferShoppingContextMapper.cs` | OrderSource adapter؛ complete Source revisions | FLOW-01.., SEC-05.. |
| Existing 9 public Non-Seat endpoints | `RestApi/V1/AncillaryOffers/Controllers/{BackofficeController,OtaController,OtaPanelController}.cs` | Order routes + Batch, additive | VIEW-01.., FLOW-... |
| List presentation | `Query/Shopping/AncillaryOffers/Queries/GetAncillaryOffers/{Backoffice,Api,OtaPanel}/` | inline schema/option subset + required constraints per Surface | VIEW-03..16 |
| Option provenance | `Query/Shopping/AncillaryOffers/Queries/GetAncillaryOfferDetail/OfferOptionCatalog.cs` | shared projection to List and Details, not double generator | VAR-A02,A12,A24 |
| Single Evaluate existing | `Query/Shopping/AncillaryOffers/Queries/EvaluateSelection/EvaluateSelectionService.cs` | batch with 1-item compatibility; mixed Seat | PRICE-01.., SEAT-... |
| Monetary engine | `Shopping/Engine/PriceAssessor.cs`; `Query/Shopping/Common/OfferPurchase.cs` | effective qty, shared fee, priceComplete/quoteReady distinct | PRICE-01..24 |
| Sale authority | `Query/Shopping/Common/SalesScope/*`; `Providers/Offer/Wire/PricedOfferId.cs` | trusted channel/customer/agency/order equality | SEC-05..08 |
| Seat public DTO | `Query/Shopping/SeatMaps/Dto/SeatMapsDto.cs` | **zero schema mutation** | S2-SEAT-01..16 |
| Seat availability/price | `Query/Shopping/SeatMaps/Queries/GetSeatMap/{GetSeatMapService,SeatMapMapper}.cs`; `Shopping/Engine/AncillaryShoppingEngine.SeatMaps.cs` | availability source complete; no fake available/free | SEAT-21..24 |
| Seat ID | `Query/Shopping/Common/OfferItemRef.cs` (`OfSeat`) | exact ref regeneration + seat user/flight/price map | S2-SEAT-01..16, SEAT-17..25 |
| Token | current selection mappers have nullable `PricedSelectionRef=null` | issuance/payload/keyring/protector, no snapshot DB | SEC-01..14 |
| S2S Resolve | `RestApi/V1/AncillaryReservationAggregate/Controllers/ServiceController.cs` has Hold/Get/Confirm, **no Resolve** | dedicated internal auth/Resolve | SEC-15..25 |
| S3 handoff | `Application/AncillaryReservationAggregate/Commands/HoldAncillaryServices/AncillaryHoldInputs.cs` | maintain explicit mapping to real `OrderServiceId` and typed qty | POST-... |
| S2 regression | `tests/AeroTech.Ancillary.Shopping.Tests/` | tests from §13, not merely 9 Golden path subsets | All acceptance |

در هر Audit آینده ادعاهای این جدول را با SHA واقعی مجدداً کنترل کنید؛ اگر Source دیگر تغییر کرده است، diff صرفاً برای تشخیص تغییر، **نه برای بازطراحی سناریو** استفاده می‌شود. کارهایی که در کد امروز نیستند نباید در گزارش «Done» شوند.

### 17.4 Definition of Done دو سطحی، بدون مفهوم مبهم «Stage 2 بسته شد»

**S2 Contract Complete** تنها اگر تمام شروط زیر برقرار است:

1. تمام Endpointهای Ancillary داخل Scope، field schema، Auth policy، خطا، Source XOR و backward compatibility با تست قرارداد verify شده‌اند؛ Status واقعی `[CURRENT]/[TARGET]` از روی SHA گزارش شده.
2. تمام Variantهای A01–A24 positive یا honest `[BLOCKED-BY-SOURCE]` و negative acceptance دارند؛ تک‌تک شرط‌ها در ماتریس §12 به Test ID §13 وصل شده‌اند. هیچ Price جعلی در رفتار مثبت «test green» نشده.
3. Offer-based و Order-based Context با contract/fixture اصل Product و verified source revision تولید می‌شوند؛ Owner واقعی `OrderContext` را تأیید کرده؛ آنچه نداریم `[EXTERNAL_PENDING]` است.
4. List single-call inline + Details on demand، حداقل یک Example Golden در هر ۳ Surface، و `20 items / 1 click` no N+1 query اجرا شده.
5. Batch Evaluate (mixed seat/ancillary)، idempotent/non-mutating Evaluation، total vs shared fee، pricing-unit quantity، و `pricedSelectionRef` کوتاه‌عمر برای تمام Accepted/fully-priced/OrderBound lineها، با Rejected/NeedsQuote truthful رفتار می‌کند.
6. همان `SeatMapsDto` فعلی و ۳ route پابرجاست؛ id فعلی SeatMap به Evaluate قابل مصرف است؛ Employee/Pax/Segment/Flight/Seat/Price mapping پس از recompute صحیح است؛ `AVAILABLE` بی‌قیمت معتبر و missing FlightFlow coverage غیرممکن شده.
7. Resolve فقط Ordering مجاز، Order-aware و fail-closed است؛ Token integrity/expiry, SourceVersion/OrderVersion, Price/Option/Fare/Seat changes و Foreign Agency/Office cover شده؛ هیچ Hold/EMD داخل S2 انجام نمی‌شود.
8. API Contract Handoff واقعی OrderService/Kafka/Payment boundary و `EXTERNAL` نقاط باز به‌وضوح در report آمده؛ Target Consumer خارج Repo به‌جای implemented معرفی نشده.
9. همه تست‌ها **واقعاً** با `dotnet test/build` روی commit ثابت اجرا شده‌اند: تعداد discovered و executed یکسان، failed=0، skipped/aborted مشخص؛ CI اگر وجود ندارد «سبز» فرض نمی‌شود.
10. Owner پس از دیدن test matrix، delta لیست فایل‌ها، source gaps و گزارش آخر به‌صورت صریح `S2_CONTRACT_CLOSED` اعلام می‌کند؛ Agent به‌تنهایی Stage را نمی‌بندد.

**Full Operational E2E Complete** شرط افزوده‌ای دارد: Ordering S2S واقعی Context/Add کار کند؛ AirAvail/FlightFlow real deployment طبق سطح ادعاشده متصل باشند؛ پرداخت/Issue/EMD را فقط تیم صاحب آن به‌طور جداگانه تأیید کند. **این سطح را با `S2_CONTRACT_CLOSED` یکی نگیرید.**

---
## 18. پرامپت مستقل ادامه ChatGPT در چت جدید — حفظ کامل مسیر Stage 2

**دستور استفاده:** تمام همین فایل Markdown را در چت جدید Attach کن یا مسیر آن را به‌عنوان فایل ضمیمه به ChatGPT بده و متن زیر را ارسال کن. **صرف کپی‌کردن متن زیر بدون خود فایل نمی‌تواند ۲۴ Variant و ۱۰۰+ Scenario را منتقل کند.** این متن آغاز یک گفت‌وگوی *طراحی/ممیزی* است، نه مجوز کدنویسی برای Coding Agent.

> من Owner پروژه `AeroTech.Ancillary` هستم. زبان پاسخ فارسی است. فایل پیوست `AeroTech-Ancillary-Stage2-E2E-Design-Contract-Scenario-Master-v1.0.md` قرارداد کامل Stage 2 End-to-End برای بررسی/تأیید من است؛ آن را از ابتدا تا انتها بخوان و بدون فرضیات خارج از آن ادامه بده. وضعیت فعلی هنوز S2 باز و سند `S2-OWNER-REVIEW-v1.0` **تأییدنشده** است. حق نداری Stage 2 را بسته، Agent را مجاز به Code، یا سیستم را Full E2E integrated اعلام کنی.
>
> **مرجع کار:** `CLAUDE.md` موجود Ancillary، Master Pack v13.2 chapters 00,06,07,08,09,14,16,21 و این سند پس از تصویب من. تعارض Storage Snapshot در Pack chapter 08 و Stateless token در سند، و SeatMap frozen source vs required internal fixes، به‌عنوان درخواست تصمیم Owner باید حل شوند؛ هرگز دو طراحی را در کد موازی پیاده نکن. مخزن مجاز برای تغییر فقط `aliifarhadi/AeroTech.Ancillary` است. Agent فقط کد می‌زند و بدون دستور مالک Commit/Push ندارد.
>
> آخرین HEAD ممیزی‌شده `k8s-stg@7efee95cbd01b5ff30bd4c8c36326debc5020574`؛ baseline قبل `S2-01@6853e83e316ec6d6bc2d3d9610e0f099bebdc9b1`. قبل از اظهار نظر درباره پیاده‌سازی جدید، HEAD را از طریق GitHub دوباره fetch و diff کن. Ordering/FlightFlow/AirAvail/AirPrice در نوبت تدوین این سند قابل Fetch نبودند؛ قراردادهایشان `[PRIOR-AUDIT]` یا `[EXTERNAL-CONTRACT]` است، نباید وجود endpoint را حدس بزنی. User تصمیم دارد اگر لازم شد rollback کند، ولی هدف حفظ کد درست و حذف بازنویسی غیرضروری است.
>
> **دو جریان:** پیش از Order با priced OfferId (برای preview)، بعد از ساخته‌شدن Order در هر دو وضعیت PreIssue یا PostTicketed با OrderId، از Ordering verified commercial snapshot. یک Canonical Shopping Engine، دو Source Adapter، سه Mapper مجزای Backoffice/API/OtaPanel. List برای محصول ساده باید inline انتخاب‌پذیر باشد؛ Details برای پیچیده فقط on demand. Batch Evaluate همه ancillary و SeatMap 43-character IDهای فعلی را می‌گیرد، per-line valid quote با قیمت کاملاً reconciliationشده صادر می‌کند؛ Token کوتاه‌عمر authenticated/Order-bound طبق طرح پیشنهادی، بدون جدول PricedSelections، **فقط پس از تصمیم من**. Ordering `orderId+pricedSelectionRefs[]` می‌گیرد و فوراً S2S `Selections/Resolve` صدا می‌زند؛ Resolve reprice/context/seat source/sales scope را چک و دیتا به OrderService می‌دهد. Stage3 physical Hold و Payment/EMD جدا هستند.
>
> **SeatMap:** سه Route و `SeatMapsDto` موجود Frozen. `travelerPricing[].id` فعلی SHA256 hash است و Decode نمی‌شود؛ re-enumerate/recompute همان candidate و exact match به id برای Evaluate و Resolve، سپس new pricedSelectionRef self-contained. حق نداریم id نامعتبر یا price null را AVAILABLE/zero کنیم. `A09 EXST` generic service نه A07/A08 seat cell. Ordered state و Coupon status در PostTicketed مهم‌اند.
>
> **Known gaps:** effective Quantity برای PerPassenger/PerItem بدون qty، A03 kg basis، shared fees per Ticket/Order، A19 دو Bool در API در برابر دو Contact Text در Domain، CouponUpgradable A10، fare benefits Unknown، OrderScoped TravellerId در Ordering، trusted post-order OrderContext، OtaPanel/AirAvail Channel mapping؛ همگی باید به‌صورت source-backed و Fail-closed بررسی شوند. تمام A01–A24 و test matrix §13 را مرور کن؛ از عبارت مبهم «تست کنید» بدون Given/Input/Expected استفاده نکن.
>
> **روش همکاری:** ابتدا اگر این قرارداد هنوز تصویب نشده است Decision review بده. وقتی من تصویب کردم، یک **و فقط یک** Coding Agent prompt خودبسنده، با Taskهای مرتب، scope دقیق، file/source traceability، DTO schema، negative/positive tests، frozen regression، reporting and gates تهیه کن. هیچ تغییر معماری جدید، Aggregate/DB جدید، fake supplier option، تغییر sibling repos و reset کورکورانه مجاز نیست. پایان S2 را تنها بعد از Source audit و test evidence تأیید من اعلام کن.

### 18.1 چک‌لیست چت جدید، قبل از هر پاسخ تحلیلی

1. **این فایل خوانده شده؟** از §00 تا §20، شامل همه Variant و Test matrix؛ بدون آن continuation ناتمام است.
2. **Status بالادستی چیست؟** `S2-OWNER-REVIEW` یا `S2-OWNER-APPROVED`؟ اگر هنوز owner review است، Agent prompt صادر نشود.
3. **HEAD و docs جدیدند؟** Pin قبلی را با latest real compare کنید؛ ادعای green بدون command ممنوع.
4. **Source Ordering حاضر است؟** در نبود آن URL یا Nullable fields واقعی حدس نشود؛ قرارداد هدف را دقیق نگه دارید.
5. **دو تصميم کلیدی:** snapshot storage vs stateless token؛ frozen SeatMap wire vs internal availability fixes؛ باید با یک ADR مالک بسته شوند.
6. **Seat ID behavior و Order-based SeatMap** یک E2E تست مثبت معتبر و negative test دارند یا فقط الگوریتم فرضی؟
7. **S2 Full Operational vs Contract Closed** اشتباه گرفته نشده‌اند؟

---

## 19. فهرست منابع، درجه اطمینان و ممیزی سازگاری خود سند

### 19.1 منبع داخلیِ مستقیم `[CURRENT]`

- Source SHA: [`AeroTech.Ancillary@7efee95`](https://github.com/aliifarhadi/AeroTech.Ancillary/tree/7efee95cbd01b5ff30bd4c8c36326debc5020574).
- [`CLAUDE.md`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/CLAUDE.md)؛ قانون تقدم و Scope.
- [`AncillaryVariant.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.Domain/AncillaryServiceDefinitionAggregate/AncillaryVariant.cs)؛ ۲۴ variant و pricing units.
- [`CustomerSelectionContract.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.Domain/AncillaryServiceDefinitionAggregate/CustomerSelectionContract.cs)؛ Required fields per Variant.
- [`AncillarySelection.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.Shopping/Selection/AncillarySelection.cs)؛ ورودی Evaluate واقعی.
- [`PriceAssessor.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.Shopping/Engine/PriceAssessor.cs)؛ محاسبه فعلی و Known Quantity gaps.
- [`OfferItemRef.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.Query/Shopping/Common/OfferItemRef.cs)؛ hashing Candidate/SeatMap.
- [`SeatMapsDto.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Dto/SeatMapsDto.cs)؛ Frozen wire.
- [`GetSeatMapService.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Queries/GetSeatMap/GetSeatMapService.cs)؛ source composition and gaps.
- [`AncillaryShoppingEngine.SeatMaps.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.Shopping/Engine/AncillaryShoppingEngine.SeatMaps.cs)؛ Seat availability behavior.
- [`SeatMapMapper.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Queries/GetSeatMap/SeatMapMapper.cs)؛ Price null→zero risk.
- [`OfferOptionCatalog.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/GetAncillaryOfferDetail/OfferOptionCatalog.cs)؛ current source-backed options.
- [`GetAncillaryOfferDetailService.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/GetAncillaryOfferDetail/GetAncillaryOfferDetailService.cs)؛ Details assembly.
- [`EvaluateSelectionService.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/EvaluateSelection/EvaluateSelectionService.cs)؛ current single-item Evaluate.
- [`ServiceController.cs`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/src/AeroTech.Ancillary.RestApi/V1/AncillaryReservationAggregate/Controllers/ServiceController.cs)؛ current Stage3 Hold/Get/Confirm only.
- Master Pack v13.2: [`06`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/docs/AeroTech-Ancillary-Master-Pack-v13.2-FINAL/06-A01-A24-BUSINESS-AND-SELECTION-MATRIX.md), [`07`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/docs/AeroTech-Ancillary-Master-Pack-v13.2-FINAL/07-S2-SHOPPING-EXACT-SURFACES-AND-OPTIONS.md), [`08`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/docs/AeroTech-Ancillary-Master-Pack-v13.2-FINAL/08-S2-PRICED-SELECTION-IDENTITY-AND-VALIDITY.md), [`09`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/docs/AeroTech-Ancillary-Master-Pack-v13.2-FINAL/09-S2-SEATMAP-FROZEN-AND-VERIFIED-BEHAVIOR.md), [`16`](https://github.com/aliifarhadi/AeroTech.Ancillary/blob/7efee95cbd01b5ff30bd4c8c36326debc5020574/docs/AeroTech-Ancillary-Master-Pack-v13.2-FINAL/16-END-TO-END-SCENARIO-COVERAGE-MATRIX.md).

### 19.2 منابع عمومی مستقل رسمی `[BENCHMARK]`

- Breeze Air JSON Offer/Order Ancillaries + bags/add-ons E2E: https://pro-docs.flybreeze.com/api/json/endpoints/offer-ancillaries ; https://pro-docs.flybreeze.com/api/json/endpoints/order-ancillaries ; https://pro-docs.flybreeze.com/api/json/sample-flows/add-bags-and-extras
- Breeze NDC ServiceList flow: https://pro-docs.flybreeze.com/api/ndc-xml/sample-flows/add-bags-and-extras
- Duffel Offer/Order/Seat APIs: https://duffel.com/docs/api/v2/offers ; https://duffel.com/docs/api/v2/orders ; https://duffel.com/docs/api/seat-maps/get-seat-maps
- Travelport Ancillary Shop/Price/Book: https://support.travelport.com/webhelp/JSONAPIs/Airv11/Content/Air11/Ancillaries/APIRef_AncillaryShop.htm ; https://support.travelport.com/webhelp/jsonapis/airv11/content/air11/ancillaries/APIRef_AncillaryPrice.htm ; https://support.travelport.com/webhelp/jsonapis/airv11/content/air11/Ancillaries/APIRef_AncillaryBook.htm
- ATPCO Optional Services / IATA NDC semantic: https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/Create_and_Update_Services.htm ; https://airtechzone.iata.org/docs/ImpForum/CN20/TravelNDC.pdf
- Sabre NDC: https://developer.sabre.com/rest-api/ndc-it-servicelist/21.3.4/index.html
- RFC7519 JWT: https://www.rfc-editor.org/rfc/rfc7519.html ؛ استفاده برای استانداردهای Token و ضرورت issuer/audience/expiry validation؛ پیاده‌سازی JWE/AEAD مطابق رمزنگاری استاندارد .NET/Platform و مدیریت Key واقعی، نه Homegrown encryption.

### 19.3 هشدار جامعیت و درجه اطمینان

| بخش | درجه اطمینان | تعبیر مجاز |
|---|---|---|
| Ancillary current ۱۲ public S2 routes and ۳ Service-Hold routes | بالا: مستقیم خوانده شده | `[CURRENT]` با SHA ثابت |
| ۲۴ Variant fields/rates, PriceAssessor and Seat hash | بالا: از Source مستقیم خوانده شده | Current facts، نه اثبات موفقیت سناریوها |
| Two-source UI, Batch Evaluate, Token protection and Resolve schemas | طراحی دقیق اما نساخته‌شده | `[TARGET]`، نیازمند Owner approval |
| Endpoint واقعی Context/Get در Ordering و Add Ancillaries | در این نوبت تأیید نشده | `[EXTERNAL-CONTRACT]`، مسیر پیشنهادی نه source fact |
| AirAvail channel semantics, FlightFlow runtime availability | prior review/Pack فقط | در runtime verify شود؛ Source conflict را رد نکنید |
| Official benchmark systems internal DB design | ناشناخته | Stateless AeroTech یک Design Decision مستقل است |
| اجرای واقعی کل ۲۴ variant و auth/integration | اثبات نشده | فقط Test Plan؛ نه green CI |

**Status پایانی این سند:** `S2_DESIGN_DOCUMENTED_FOR_OWNER_REVIEW`; هیچ `S2_CODE_CLOSED` یا `S2_OPERATIONAL_E2E_CLOSED` اعلام نشده است.

---

## 20. پیوست مستقل — Numeric Enum، Schema Guard و Source-to-Contract Freeze

برای اینکه چت جدید مجبور نشود معنی enumهای موجود یا DTOهای فعلی را دوباره حدس بزند، این Dictionary از **Master Pack v13.2 chapter 21** به‌عنوان مرجع خودبسنده نقل مفهومی شده است. **این مقادیر در این نوبت برای تک‌تک enumهای زیر مستقیماً از فایل C# کنترل مجدد نشده‌اند**؛ در اجرا همچنان `CLAUDE.md` و سورس واقعی بالاتر از Pack قرار دارند. عددها یا Labelهای فعلی را بدون تطبیق و Migration impact تغییر ندهید.

| Enum | تعریف عددی در Pack v13.2 | اثر روی قرارداد S2 |
|---|---|---|
| `AncillaryProfile` | `Baggage=1,Seat=2,Upgrade=3,Meal=4,Pet=5,AssistedTravel=6,AirportService=7,Priority=8,Connectivity=9` | UI grouping، نه دستکاری Domain code |
| `ServiceCoverageScope` | `Sector=1,Portion=2,Journey=3,Order=4` | Order scope only A22/A24 when source supports |
| `PricingUnit` | `PerPassenger=1,PerRoom=2,PerItem=3,PerVehicle=4,PerSeat=5,PerPiece=6,PerKilogram=7` | `PerRoom/PerVehicle` موجود در Enum ≠ Variant الزامی جدید |
| `AncillaryQuantityUnit` | `Each=1,Piece=2,Kilogram=3` | Count integer و weight Decimal/Scale |
| `PriceOrigin` | `Filed=1,ExternalQuote=2,Free=3,NotAvailable=4,LegacyUnspecified=5` | صفر فقط Free معتبر، Quote نیازمند Provider |
| `InventoryAuthority` | `Unlimited=1,Local=2,Supplier=3` | `FlightFlow` به‌عنوان InventoryAuthority جدید اضافه نشود |
| `InventoryCapacityReadState` | `NotConfigured=1,Unlimited=2,ConfiguredNotGuaranteed=3,ClosedForSale=4,DelegatedCheckRequired=5,Unknown=6,UnsupportedPattern=7,NotApplicable=8` | Missing capacity ≠ Guaranteed |
| `OfferReadiness` | `NotEligible=1,NeedsSelection=2,NeedsQuote=3,NeedsVerification=4,Selectable=5,Unavailable=6` | `Selectable` candidate ≠ `quoteReady` Basket |
| `PurchaseStage` | `PreOrder=1,PostTicketed=2,Both=3,LegacyUnspecified=4,OnBoard=5` | PostTicketed from Order status source |
| `ProvisionApplicationType` | `Standard=1,Baggage=2,Seat=3` | سه کاربرد، نه ۲۴ Aggregate |
| `SupplierFulfillmentKind` | `Local=1,External=2` | S2 projection only, Hold downstream |
| `ReissueRefundPolicy` | `Refundable=1,NonRefundable=2,NonRefundableReusable=3` | Refundability vs cancellation/penalty distinct |
| `FormOfRefund` | `OriginalPayment=1,EVoucher=2` | S2 terms only, Payment owner |
| `DocumentRouting` | `NoAncillaryDocument=1,Emd=2,TicketOrExchange=3` | `Emd` metadata does not mean issued |
| `AncillaryDocumentType` | `None=1,EmdAssociated=2,EmdStandalone=3` | EMD-A coupon link or EMD-S standalone downstream |
| `BookingMethod` | `Ssr=1,AuxiliarySegment=2,DisplayPriceContactCarrierForBooking=3,NoBookingProcessRequired=4,PerServiceRecord=5` | external quote/manual truth, never automatic confirmation |
| `SelectionKind` | `SimpleOptIn=1,QuantityChoice=2,TypedForm=3,SeatMapSelection=4,ExternalQuote=5` | `kind` in Batch is a transport discriminator; don't renumber Domain enum |
| `SelectionFieldType` | `Integer=1,Decimal=2,Text=3,Boolean=4,Code=5,CodeSet=6,Reference=7,DimensionsCm=8,DateTimeWithOffset=9` | Type-driven UI without ۲۴ custom API endpoints |
| `ShoppingSourceKind` | `Offer=1,Order=2,Direct=3` | `Direct` not authorized for two-flow Stage2 |

### 20.1 تفاوت نام و Shape سه سطح را با Enum equality اشتباه نگیرید

1. Backoffice برای `EnumValueDto` می‌تواند `{value,name,title}` منتشر کند؛ تغییر Numeric Domain بدون migration ممنوع.
2. Partner API باید Code قابل توضیح و Human Readable مثل `baggage`, `seat_map`, `requires_confirmation` بدهد. نگاشت Numeric internal به public string از **یک مرجع ثابت Mapper** انجام شود، بدون اینکه Source Enum معنای جدیدی پیدا کند.
3. OtaPanel/Website labels برای انسان و enumهای Source فقط ورودی Mapping هستند؛ Client نباید infer ProviderFulfillment از صرف Category بکند.
4. Producer AirAvail یک `Channel` numeric جدا دارد. انطباق نام‌مبنا و سیاست احراز هویت واقعی اولویت دارد؛ raw enum cast بین Microserviceها ممنوع، مخصوصاً Backoffice/IBE/OtaPanel.
5. JSON `int64` برای APIهایی که `LongToStringJsonConverter` دارند string است؛ `OrderTravellerId`, `OrderId`, `FlightId` نباید با cast به `int` یا sequence index جایگزین شود.

### 20.2 ترتیب تطبیق Contract تا کد و Closure

```text
Owner mandates and CLAUDE.md existing repo rules (highest authority)
    -> One Owner-approved Canonical business design / ADR (only AFTER explicit approval)
    -> Master Pack v13.2 chapters 06/07/08/09/14/16/21 reconciled to that ADR
    -> Real current HEAD code and true sibling request/response contracts
    -> Exact wire compatibility (three surfaces + SeatMap frozen)
    -> Domain data source proof / no arbitrary enum or supplier option
    -> full negative acceptance and leak/security tests
    -> actual dotnet build/test logs + CI proof + owner closure verdict
```

**Final reminder:** این فایل برای تکمیل مباحث طراحی و نگهداری Context ساخته شده است، **نه مجوز Code یا برگ سبز Integration**. مسیر بعدی باید ابتدا تأیید Owner بر Decision Register §17.1 و وضعیت External §17.2 باشد.
