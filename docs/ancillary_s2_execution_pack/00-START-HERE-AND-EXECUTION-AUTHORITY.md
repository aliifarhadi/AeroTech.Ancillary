# AeroTech.Ancillary — S2 one-pass execution pack — START HERE

**تاریخ:** 2026-10-20  
**نقطه شروع مشاهده‌شده:** `aliifarhadi/AeroTech.Ancillary`, branch `k8s-stg`, SHA `7efee95cbd01b5ff30bd4c8c36326debc5020574` (`S2-02`). پیش از کار HEAD واقعی را دوباره بررسی کن.  
**وضعیت:** بستهٔ دستور پیاده‌سازی S2؛ نه گزارش انجام کد و نه مجوز Commit/Push. **ارائه این بسته به Coding Agent با دستور اجرا، مجوز ویرایش فایل‌های همین مخزن است؛ نه مجوز تغییر سایر مخازن یا Commit/Push/Merge.**  

## 1. فایل‌ها و ترتیب مطالعه

1. `00-START-HERE-AND-EXECUTION-AUTHORITY.md` — تقدم و اختلافات.
2. `01-S2-OWNER-DESIGN-MASTER-v1.0.md` — **متن کاملِ ۱۷۰۴خطی** طراحی، فیلدها، ۲۴ Variant، Wireهای Seat، ۱۰۰+ سناریو و معیارهای قبول؛ مرجع تجاری یکتا.
3. `02-CODING-AGENT-ONE-PASS-IMPLEMENTATION-PROMPT.md` — دستور اجرای گام‌به‌گام برای Agent.
4. `03-API-AND-INTEGRATION-CONTRACT-FREEZE.md` — قرارداد مسیرها و DTOها/Identity/Security.
5. `04-ORDERING-CONSUMER-AND-EXTERNAL-DEPENDENCIES.md` — نیازهای واقعی Ordering و مرز S2/S3.
6. `05-TEST-MATRIX-AND-CLOSURE-GATES.md` — شرط‌های قابل آزمون و سناریوهای برگرفته از خود Master.
7. `06-CONTINUATION-PROMPT-FOR-NEW-CHAT.md` — حمل کامل Context.

## 2. Authority و حل تعارض برای **این دستور اجرایی**

1. دستورات صریح Owner و `CLAUDE.md` مخزن مقدم بر همه‌اند. Rule 1 در `CLAUDE.md`: الگوی sibling واقعی را بخوان؛ اگر با Contract در تضاد غیرقابل‌حل است، با شاهد گزارش بده، نه اختراع.
2. برای **رفتار S2**، سند `01-S2-OWNER-DESIGN-MASTER-v1.0.md` که Owner درخواست اجرای «طبق این سند» کرده، Target موردنظر است؛ Appendixها/Prompt اجرایی فقط تفسیر قابل آزمون هستند و حق تغییر معنی Master را ندارند.
3. `docs/AeroTech-Ancillary-Master-Pack-v13.2-FINAL` برای Domain/Stage1/S3 و قراردادهای تأییدشدهٔ قبلی معتبر است؛ فقط این تعارض‌های خاص S2 با یک ADR جدید داخل Ancillary Supersede می‌شوند:
   - §08 ذخیرهٔ `PricedSelectionSnapshot` در DB → **S2 جدید: Protected self-contained, short-lived Selection Token؛ بدون جدول، Aggregate یا Migration مربوط به Quote**.
   - Freeze SeatMap → **فقط JSON/Route/Status value و schema موجود فریز است**؛ رفع خطاهای داخلی قیمت/Availability/ID eligibility و Regeneration اجباری است.
   - Shopping از `OfferId` تنها → دو Source Offer و Order، از لحظه ساخت Order منبع اصلی Order.
   - Details لازم برای گزینه‌ها → List برای Inline قابل انتخاب، Details روی تقاضا؛ اشتراک Option-building.
   - Evaluate تک‌انتخاب → Batch افزایشی با حفظ Legacy و mixed Seat/Ancillary.
4. فایل‌های R1/V13.2 قدیمی که DB Snapshot را الزام کرده‌اند **برای اجرای S2 منسوخ هستند**. آنها را به Agent به‌عنوان Task فعال نده.
5. اگر Master هنوز عبارت `OWNER-REVIEW` دارد، این فایل اجرای همان طراحی را مشخص می‌کند؛ **تأیید پایان Stage 2 تنها با Owner است**. این فایل مجوز حل سایر Open Pointهای صریح Master به روش دلخواه نیست.
6. قبل از اعمال تغییر، ADR واحد `docs/S2-Execution-Authority-2026-10-20.md` ایجاد و تعارضات بالا را با Source/Decision/Consequence ثبت کن؛ در Repo این ADR باید تنها Authority اجرایی جدید باشد. Stage1 Authoring و Stage3 بیرون Scope باقی می‌مانند، جز رفع اشکال سازگاری مشخص و مستند لازم برای S2.

## 3. محدوده و بی‌طرفی نسبت به Rollback

**فقط Ancillary را تغییر بده.** Ordering/FlightFlow/AirAvail/AirPrice/Backoffice فقط Read-Only. ترجیح با **Delta کمینه نسبت به HEAD** است. Rollback کامل مجاز پیش‌فرض نیست؛ اگر لازم دانستی، Diff و فهرست از دست‌رفته‌ها و Test implications را ارائه کن و بدون دستور صریح Owner عمل نکن. S2-01/S2-02 بخش‌های صحیحی دارند.

## 4. وضعیت تحویل

دو برچسب جدا: `S2_INTERNAL_CONTRACT_READY` یعنی Tests و قرارداد Producer داخل Ancillary واقعاً موفق و تحویل شده؛ `S2_EXTERNAL_INTEGRATION_PENDING` یعنی Ordering S2S واقعی/FlightFlow Deployment یا Supplier Source هنوز به اثبات نرسیده است. `S2_CLOSED` فقط با تأیید Owner. `Full E2E` بدون اجرای واقعی Consumer و سایر سرویس‌ها ممنوع.

## 5. توافق بر سر روش اجرا

قبل از تغییر Source: کوتاه و مستند بررسی sibling و شکاف‌های Source؛ سپس **تمام S2 را در یک پیاده‌سازی منسجم** بساز و تست کن، نه سؤال ریز درباره Fieldهای از قبل مشخص‌شده. اگر blocker واقعی، تضاد Authority یا شواهد ناسازگار پیدا شد، دقیق بگو؛ حل حدسی ممنوع. **Commit/Push/Merge صرفاً پس از دستور مستقیم Owner**. No code comments، reports در `reports/` و شواهد اجرای هر Test شامل command/hash/count مطابق `CLAUDE.md`.
