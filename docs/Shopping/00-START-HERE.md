# AeroTech Ancillary — Pack v8 / مرجع واحد قابل اجرا

**تاریخ:** 2026-10-23  
**وضعیت:** قرارداد فنی/تجاری و پرامپت‌های مرحله‌ای برای اجرای **فقط پس از تأیید Owner**.  
**مبنای سورس بررسی‌شده:** `aliifarhadi/AeroTech.Ancillary`, `k8s-stg@49e9209a40e046fe71c7913cc1fcf8a8fca6799d`. Gate0 باید SHA واقعی روز اجرا را مقایسه کند.  
**نقطه شروع اجباری برای ایجنت:** همین فایل `00-START-HERE.md` در پوشه‌ای که از ZIP استخراج شده است.

## مرجع یگانه

**تمام قواعد این فاز فقط در همین Pack v8 تعریف شده‌اند.** هیچ فایل مربوط به نسخه قبلی (از جمله ماتریس پذیرش، Prompt یا Master Contract قدیمی) نباید به‌عنوان مرجع موازی در اختیار ایجنت باشد. ترتیب اعمال مقررات در داخل همین Pack:

1. تصمیم‌های صریح Owner، سپس `01-OVERVIEW-AND-BUSINESS-CONTRACT-FA.md` برای قواعد و دامنه تجاری؛
2. `02-EXACT-API-DTO-AND-FIELD-CONTRACT.md` برای هر Request/Response و نام دقیق فیلد؛
3. `03-A01-A24-VARIANT-DICTIONARY.md` برای Required/Optional/Conditional هر Variant و DTO تخصصی؛
4. `04-SIGNED-IDS-TYPED-SNAPSHOT-AND-SECURITY.md` برای الگوریتم ID، HMAC، TTL و DB؛
5. `05-ORDERING-HANDOFF-AND-TESTS.md` برای ۷۸ سناریوی پذیرش و handoff؛
6. `06-BENCHMARK-SOURCE-DECISIONS.md` برای منبع/گیت/شواهد؛ `07-RELEASE-READINESS-AND-UNRESOLVED.md` برای محدودیت‌های باقی‌مانده؛
7. `10` تا `15` صرفاً دستور اجرای مرحله‌ای **تحت مرجع همین 01..07** هستند؛ Prompt حق تغییر یا تکمیل خودسرانه قرارداد را ندارد.

در صورت تعارض دو فایل همین Pack، **ایجنت نباید یکی را حدس بزند**؛ اختلاف را با مسیر/بند گزارش می‌کند و فقط قسمت بدون اختلاف را اجرا می‌کند. هیچ نسخه قدیمی برای رفع تعارض Authority نیست.

## هدف تجاری و معماری در یک خط

`Shopping({offerId})` ⇒ Direct: `d_` امضاشده / Quantity قابل شمارش: `quantityOptions[]` امضاشده **بدون Snapshot و POST** / Typed: `r_` امضاشده ⇒ `GET/POST Options` و تنها یک Snapshot برای انتخاب Typed با `s_` ⇒ `ResolveGrouped({ids:[...]})` ⇒ DTOهای تخصصی قابل تحویل به Ordering.

- **SeatMap A07/A08 و همه Controller/DTO/Reserve آن فریز و مستقل است.**
- **Selection Unit تعیین‌کننده تعداد ردیف‌ها است** (`Order`, `Traveller+Journey`, `Traveller+Portion`, `Traveller+Flight`)؛ Quantity هرگز مبنای تعداد ردیف نیست.
- **برای Quantity-only هر مقدار مجاز دقیقاً یک `d_` امضاشده مجزا دارد**؛ نه Snapshot، نه Options POST/GET.
- **برای Direct نیز Snapshot نداریم**؛ Only Typed ذخیره می‌شود.
- **Resolve فقط `{ids:string[]}` می‌گیرد**؛ هیچ Offer/Order/POS/Country یا SelectedOptions در Body ندارد.
- هر محصول حتی رایگان **PricingRateId واقعی و BaseAmount=0** دارد؛ نه RateID مصنوعی و نه fallback به Provision.
- صدور `r_`/`d_` فقط پس از Shopping معتبر و تمام Eligibility/Price/Rate/POS/PTC؛ POST Typed امضا/Scope/Options/Rate را بررسی می‌کند؛ Resolve MAC/Expiry یا Snapshot و **Active Pricing/Provision/Definition** را می‌خواند.
- **Pricing Retired/Suspended در هیچ مرحله‌ای برای خرید جدید یا Resolve انتخابِ هنوز ثبت‌نشده پذیرفته نمی‌شود**؛ `AncillaryPricingRate` مستقل Status ندارد، Status والد Pricing معیار است.
- A09 (FlightFlow), A17 (حفاظت داده), A19 (تماس سرپرست), Insurance (Domain ندارد) و A18 در صورت فقدان evidence معتبر صریحاً GATED هستند.
- **هیچ Aggregate/Engine/OrderReader/TokenStore/QtySnapshot/Payment/Reservation جدید ساخته نشود.**

## موجودی الزامی ۱۵ سند

| شماره | فایل واقعی | نقش |
|---|---|---|
| 00 | `00-START-HERE.md` | این مرجع و فهرست کامل |
| 01 | `01-OVERVIEW-AND-BUSINESS-CONTRACT-FA.md` | Overview کامل روان با نمونه‌ها |
| 02 | `02-EXACT-API-DTO-AND-FIELD-CONTRACT.md` | Endpoint/DTO دقیق |
| 03 | `03-A01-A24-VARIANT-DICTIONARY.md` | فیلدهای هر Variant |
| 04 | `04-SIGNED-IDS-TYPED-SNAPSHOT-AND-SECURITY.md` | HMAC/Snapshot/TTL |
| 05 | `05-ORDERING-HANDOFF-AND-TESTS.md` | **ماتریس v8 — ۷۸ سناریو**، Qty بدون Snapshot، Retired رد |
| 06 | `06-BENCHMARK-SOURCE-DECISIONS.md` | **مبنای v8**، نه مرجع نسخه قدیمی |
| 07 | `07-RELEASE-READINESS-AND-UNRESOLVED.md` | محدودیت‌های Gate |
| 08 | `08-PACK-INTEGRITY-AND-QA.md` | نتیجه تطبیق فایل‌ها و قرارداد |
| 10 | `10-AGENT-GATE0-READONLY.md` | Gate0 read-only |
| 11 | `11-AGENT-STAGE1-SECURITY-AND-PRICING.md` | امضا/صفر/Rate |
| 12 | `12-AGENT-STAGE2-SHOPPING.md` | Shopping + quantityOptions |
| 13 | `13-AGENT-STAGE3-TYPED-OPTIONS.md` | Typed Snapshot |
| 14 | `14-AGENT-STAGE4-RESOLVE-AND-HANDOFF.md` | Resolve IDs-only |
| 15 | `15-AGENT-STAGE5-ACCEPTANCE-CLOSURE.md` | تست همه ۷۸ مورد و Closure |

**تحویل فیزیکی:** ZIP باید شامل **همه ۱۵ سند بالا** به‌علاوه `SHA256SUMS.txt` و `VERIFY-PACK.py` باشد. بعد از Extract اجرای `python3 VERIFY-PACK.py` در پوشه Pack باید `PASS` بدهد. در غیر این صورت اجرای Agent ممنوع است.

## نحوه انجام کار

- Gate0 از Branch مبنای واقعی و SHA گزارش می‌دهد؛ بدون تغییر کد. ایجاد Branch جدید فقط بعد از تأیید Owner و بر اساس `k8s-stg` است. Branch قبلی Grouped مبنای Donor یا اجرای کد نیست.
- Stage1→Stage5 هرکدام با گزارش و شواهد تست و **تأیید Owner** فعال می‌شوند؛ Agent بدون اجازه Commit/Push/Merge ندارد.
- برای `R10–R12` بدون دسترسی به Ordering صرفاً Fixture قراردادی قابل ادعاست، نه Integration عملیاتی. `GATED` معادل `PASS` نیست.
