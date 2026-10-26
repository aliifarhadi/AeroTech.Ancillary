# ماتریس پذیرش v9 — ONLY UI Grouping & Batch Typed Options

این تست‌ها مخصوص v9 هستند؛ تست‌های v8 برای این تغییر کافی نیستند و فایل دیگری به‌عنوان authority وجود ندارد. PASS فقط با اجرای واقعی/شواهد کد معتبر است، نه با ادعای گزارش قبلی.

| ID | سناریو / ورودی | نتیجه لازم |
|---|---|---|
| G01 | خانواده با A01 و A05 | دو Variant Card و بدون تلفیق |
| G02 | A01 با دو مسافر و دو Flight | یک Card A01 و Units مستقل چهارگانه در محصول مربوط؛ IDهای متفاوت |
| G03 | A01 با دو Definition متمایز | یک Card A01 و دو Product مجزا |
| G04 | یک Definition، دو Provision | دو ProductGroup و `productKey` متفاوت؛ تجمیع ممنوع |
| G05 | دو Row با همان Unit ولی pricing/POS متفاوت | دو UnitOffer مستقل؛ هیچ حذف/averaging |
| G06 | Portion با دو FlightRef | فقط یک Unit برای همان Traveller+Portion و Coverage شامل هر دو Flight |
| G07 | PerOrder | یک Unit Order با همان Contract موجود، نه تکثیر به مسافران |
| G08 | Direct `d_` | همان SelectionId و Money قبل از Grouping |
| G09 | Quantity `quantityOptions` | تعداد/قیمت/IDهای همه گزینه‌ها عین v8؛ هیچ `s_` جدید |
| G10 | Typed `r_` | همان Reference قبلی در Unit؛ هیچ auto snapshot |
| G11 | خانواده خالی یا Gated | `[]`، Card تصنعی ساخته نشود |
| G12 | Title | کارت از Variant registry؛ Product از `CommercialName` |
| G13 | `details` تخصصی | مربوط به Definition+Provision همان Product و نه اولین Variant اشتباه |
| G14 | Backoffice `selectionScope`, `quantityPolicy.unit` | همان شکل `{value,name,title}` قبلی |
| G15 | OtaPanel / Api | همان enum string و DTOهای specialist، بدون Backoffice wrapper |
| G16 | Row count preservation | flatten مجموع UnitOfferها = تعداد `snapshot.Rows` عرضه‌شده، بدون حذف/تکرار |
| G17 | Grouping output order | ترتیب variant/product/unit منطبق با first appearance |
| F01 | Batch Forms N=1 | همان fields و variantCode GET فعلی + reference |
| F02 | Batch Forms N=2، دو Traveller | دو Form مطابق همان Reference و ترتیب ورودی |
| F03 | N=50 | موفق؛ Offer Load دقیقاً یک بار |
| F04 | N=0 یا N=51 | خطای validation؛ بدون partial forms |
| F05 | repeated/blank reference | 422 موجود، بدون partial forms |
| F06 | `r_` جعلی/دستکاری امضا | رد مطابق GET تک‌موردی |
| F07 | Expired Reference | رد مطابق TTL فعلی |
| F08 | Reference محصول یا Family دیگر | رد؛ عدم leak گزینه |
| F09 | Reference از Offer دیگر | رد با Offer Binding mismatch |
| F10 | Reference از SalesScope غیرمجاز | رد با همان Guard فعلی |
| F11 | نرخ Retired/suspended | فرم ارائه نشود |
| F12 | فرم A15 با choices متفاوت در دو context | هر Form choices همان Unit، نه copy کردن اولی |
| F13 | Query Forms | هیچ INSERT/SaveChanges یا Snapshot |
| F14 | Single GET فعلی | HTTP status و response بدون regression |
| F15 | سه Surface | Auth درست و یک Offer Load در هر Batch |
| P01 | Batch Submit ۲ A15 با options متفاوت | ۲ Snapshot و دو `s_` مستقل، پاسخ مرتب |
| P02 | یک مورد موفق و مورد دوم invalid | ۴۲۲/۴۰۹ موجود؛ **صفر Snapshot جدید** |
| P03 | ۲ invalid و ۱ valid | هیچ partial response یا insert |
| P04 | Batch 1..50 | تعداد INSERT برابر تعداد valid selections؛ یک SaveChanges |
| P05 | N=0/51 یا reference تکراری | رد قبل از Write |
| P06 | invalid MAC/expired/wrong Offer/wrong family | رد؛ صفر Snapshot جدید |
| P07 | Wrong allowed SSR / bad Pet weight | Engine/validator فعلی رد کند، بدون bypass |
| P08 | unknown JSON property در typed options | 400 مطابق رفتار موجود |
| P09 | typed Baggages | همان BaggageSelectionOptions و همان Proof تک‌موردی |
| P10 | typed Pets | همان PetSelectionOptions و همان Proof تک‌موردی |
| P11 | typed AssistedTravel | همان AssistedTravelSelectionOptions و همان Proof |
| P12 | typed AirportServices | همان Appointment/GuestCount rule |
| P13 | typed PriorityServices | `optIn=true` الزام قبلی حفظ |
| P14 | typed Connectivity | محدودیت DeviceCount همان قبلی |
| P15 | GET/POST Options برای Meals/Upgrades | به صورت تصنعی Batch خرید جدید صادر نشود |
| P16 | یک Batch N=2 یا 50 | یک `_source.LoadAsync`, یک SaveChanges, بدون 2/50 بار Load |
| P17 | شکست SQL روی SaveChanges | Transaction rollback کامل، بدون پاسخ موفق |
| P18 | دو Batch یکسان موفق | هر دو مجاز با s_ جدید؛ بدون idempotency layer ساختگی |
| P19 | single POST قدیمی | همان رفتار/Route حفظ |
| P20 | یک `s_` حاصل از Batch | `ResolveGrouped {ids:[...]}` جواب typed موجود را می‌دهد |
| E01 | Direct+Quantity+Typed Mix | ResolveGrouped با d_ و s_ بدون تغییر جواب می‌دهد |
| E02 | Shopping دو Traveller → Forms → Submit → Resolve | flow کامل end-to-end بدون گرفتن Offer چندباره در Batch |
| E03 | Card چند Product مختلف | UI قادر به انتخاب واحد مربوط به هر Product باشد |
| E04 | unselected Unit | هیچ Snapshot ایجاد نشود و ID به cart نرود |
| E05 | ۳ Surface HTTP routes | ۳ Forms parametric + ۱۸ typed Batch POST، حفظ single routes |
| N01 | SeatMap files | `git diff` تهی نسبت به branch baseline |
| N02 | Domain/Pricing/Snapshot schema/Migrations | diff تهی؛ migration جدید وجود نداشته باشد |
| N03 | ResolveGrouped/Reservation | diff تهی |
| N04 | v8 tests | suites اصلی بدون regressions PASS |
| N05 | performance | مقایسه 10 single GET+10 single POST با 1 Forms+1 Batch از نظر HTTP call و تعداد Offer Loads |
| N06 | evidence | نمونه‌های JSON با serializer واقعی؛ build/test/commit/hash گزارش شود |

**نتیجه نهایی قابل پذیرش:** تمام تست‌های امکان‌پذیر فوق PASS. برای هر سناریوی فاقد Dev Data، fixture مبتنی بر Domain واقعی + وضعیت GATED و دلیل مستند؛ ساخت داده/قیمت خیالی در Production ممنوع. هیچ واحدی صرفاً به علت گروه‌بندی حذف یا دوباره خریدنی نشود.
