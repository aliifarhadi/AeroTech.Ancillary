# 17 — تنها ۳ Stage اجرایی، با هدف/Scope/Output دقیق

**اصل:** هر Stage یک خروجی تجاری قابل مشاهده دارد و به جای ۱۵ اصلاح پراکنده یا بازبینی دوباره Pack، فقط Gap همان Stage را می‌بندد. پیاده‌سازی و روش تست/دیتابیس تست/چارچوب تکنیکی انتخاب Coding Agent مطابق conventions Repository است؛ این Pack آن را دیکته نمی‌کند.

## Stage 1 — Authoring Closure
**هدف واحد:** همه ۹ Profile و ۲۴ Variant به صورت Typed Definition/Provision و Pricing مشترک، در Backoffice قابل تعریف و مدیریت باشند.

**شروع از:** ۹ Definition facade موجود و Seat Provision facade موجود.  
**تغییر:** فقط تکمیل Typed Provision برای هشت Profile دیگر، field visibility/variant validation/ownership و رفع ناسازگاری واقعی Publish/Pricing، همراه با `Settlement.CancellationRules?` اختیاری در **همان Define/Change Provision** و بدون هیچ Endpoint child، با رفتار `null=preserve` و `[]=clear` در Change. No refactor Seat.  
**خروجی قابل مشاهده:** وقتی اپراتور `Meal` را باز می‌کند فقط فیلدهای Meal+Shared را می‌بیند؛ `Pet` فقط Pet+Shared؛ `Seat` دقیقاً مثل قبل. Provisionهای مرتبط در List و قیمت‌های مرتبط در Pricing هستند. همه A01..A24 قابل publish طبق rules.  
**Acceptance کسب‌وکاری:** create/edit/form roundtrip، correct rule ownership، lifecycle price revision، no Seat inventory policy، wrong airline/profile rejection، negative rules واضح.  
**مرز:** Shopping/SeatMaps/P3 تغییر نمی‌کند مگر رفع compatibility روشن. Stage-1 صرفاً ۵ Request-shape برای ۸ Facade جدید دارد؛ چهار Profile استاندارد Shape مشترک دارند. Cancellation Rules تنها تحت Provision و نه مسیر نوشتن موازی پیاده می‌شوند.  
**تحویل Agent:** `S1_IMPLEMENTED` با route inventory و تفاوت‌های کوتاه نسبت به این Contract؛ Owner پس از مشاهده نتیجه Stage را می‌بندد.

## Stage 2 — Shopping Closure
**هدف واحد:** در هر سه Surface لیست، گزینه‌های واقعی، قیمت/Qty/Scope صحیح و شناسه انتخاب معتبر و قابل Resolve داده شود؛ SeatMap بدون تغییر ظاهری.

**شروع از:** `AncillaryShoppingEngine`, `AncillaryOffers`, `Details`, `Selections/Evaluate`, `SeatMaps` موجود.  
**تغییر:** `Details` typed option groups واقعی؛ `pricedSelectionRef` Short-lived + S2S Resolve؛ price completeness/FX/SalesScope؛ SeatMap source/zero fix بدون تغییر DTO؛ preOrder first و postOrder source adapter وقتی contract authoritative موجود شد.  
**خروجی قابل مشاهده:** Shopper A01 Qty2 و A12 MenuVG Qty1 را با دو شناسه جدا و قیمت دقیق به Ordering می‌دهد؛ `SeatsMaps` همچنان کلاینت فعلی را بدون update اجرا می‌کند؛ blocked/unknown به غلط Available/Free نشان داده نمی‌شود.  
**Acceptance کسب‌وکاری:** همه A01..A24 appropriate outcome، IncludedFare، multiple Traveller/Flights/Currencies/POS، accepted select resolve/stale expiry، no double order fees، SeatMap shape identical، trusted input no spoofing.  
**مرز:** هیچ Hold/Confirm/Cancel یا PriceOwner duplicate به S2 اضافه نمی‌شود.  
**تحویل Agent:** `S2_IMPLEMENTED` با قرارداد API سه Surface، Contract Freeze SeatMaps و Request/Response Resolve؛ external reference blockers در لیست `[EXTERNAL_PENDING]` بدون ادعای deploy.

## Stage 3 — Reservation & Servicing Closure
**هدف واحد:** Ordering بتواند گروهی از انتخاب‌های معتبر را با هویت OrderService، از Reserve/Hold تا Confirm/Release/Cancel جزئی و CancellationQuote به نتیجه واقعی برسد.

**شروع از:** `AncillaryReservation`, `AncillaryReservationUnit` و سه API Hold/GET/Confirm موجود.  
**تغییر:** extension همین Aggregate برای `PricedSelectionRef`/selected options/price evidence، supplier capability/local stock, status Pending/Mixed/Failed, per-unit refs, idempotency/retry, release/extend, cancellation quote + selected cancel, provider manual outcome، minimal integration events.  
**خروجی قابل مشاهده:** یک request با A01 pax1 + A12 pax2 + A22 OrderRef یک `HoldId` و ۳ `UnitId` می‌دهد؛ زمان Pending/Partial را دقیق نشان می‌دهد؛ انتخاب Unit1 برای cancel Unit2 را دست‌نخورده می‌گذارد؛ refund amount را جعل نمی‌کند؛ Ordering می‌تواند quote و payable/refund workflow را مصرف کند.  
**Acceptance کسب‌وکاری:** valid selection only، duplicate-safe Hold، partial/pending/recovery، local/supplier/manual/directConfirm، physical seat independent، cancellation quote with penalty evidence، release/extend/cancel، provider/cash/wallet/document orchestration contract.  
**مرز:** Issuing ETKT/EMD و Payment refund داخل Ancillary پیاده نشود؛ Ordering/Payment/FlightFlow repo را تغییر ندهید.  
**تحویل Agent:** `S3_IMPLEMENTED` با API table، Unit states، event contract، end-to-end interaction sample و external pending items.

## گیت‌های عملی و تعریف «بسته»
برای هر Stage فقط سه مدرک لازم است: **(۱) رفتار قابل مشاهده با نمونه Request/Response واقعی، (۲) عدم شکستن قراردادهای قبلی مرتبط، (۳) لیست دقیق باقی‌مانده‌های خارجی یا unsupported provider.** روش تولید این مدارک با Agent است؛ نه تعداد تست، نه بانک تست، نه نوع Mock و نه CI در این Pack تحمیل نمی‌شود.

> «S1 Closed / S2 Closed / S3 Closed» فقط وقتی Owner نتایج را پذیرفته باشد. در غیر این صورت «Implemented — Owner Audit Pending» و پاسخ دقیق Gap. فاز بعدی را به خاطر نوشتن مجدد تست‌های غیرضروری گیر نیندازید.

## توقف منطقی، نه بستن دست ایجنت
ایجنت اجازه دارد در **Ancillary repo** کلاس‌ها/enumهای هدف موردنیاز برای این سه Stage را با سادگی لازم اضافه/حذف کند؛ داده تاریخی وجود ندارد و مهاجرت Legacy لازم نیست. فقط **شکستن SeatMaps یا Seat Authoring جاری، ساخت Aggregate موازی، دستکاری مخازن دیگر، جعل قیمت/موجودی/رفاند و ادعای اجرای Provider بدون Source** مجاز نیست. اگر Ordering API واقعی فرق داشت، در خود Pack آن را با UI/Owner نهایی align کند و delta منطقی بدهد؛ تغییر خارج Repo بدون مجوز خیر.
