# Agent Gate 0 — READ ONLY / قبل از هر تغییر

**Pack Authority (must be locally present):** `00-START-HERE.md`, `01-OVERVIEW-AND-BUSINESS-CONTRACT-FA.md`, `02-EXACT-API-DTO-AND-FIELD-CONTRACT.md`, `03-A01-A24-VARIANT-DICTIONARY.md`, `04-SIGNED-IDS-TYPED-SNAPSHOT-AND-SECURITY.md`, `05-ORDERING-HANDOFF-AND-TESTS.md`, `06-BENCHMARK-SOURCE-DECISIONS.md`, `07-RELEASE-READINESS-AND-UNRESOLVED.md`. **If any file is absent or `python3 VERIFY-PACK.py` fails, STOP before modifying code. Do not consult earlier Pack versions.**


**Source of authority:** `00`→`01`→`02/03/04/05`; کاربر Owner/Approver است؛ Agent تنها کد می‌نویسد **بعد از تأیید این Gate**. Never use deleted `feat/ancillary-grouped-api-gate-a` as implementation source.

1. Repo `aliifarhadi/AeroTech.Ancillary` را بررسی کن؛ SHA واقعی `k8s-stg` را گزارش بده و آن را با مستند v8 baseline مقایسه کن؛ بدون فرض ثبات SHA؛ اگر متفاوت است Diff کدهای relevant را استخراج کن.
2. لیست تمام SeatMap routes, DTOs, tests و hashes را ثبت کن. A07/A08 هیچ تغییری مجاز نیست.
3. بخش فعلی `CoveragePlanner`, `AncillaryVariant`, `CustomerSelectionContract`, `AncillarySelection`, `PriceAssessor`, `AncillaryPricingRate`, سه Shopping surface, `OfferItemRef`, ActivePricing readers, form mappers را با contract جدول‌به‌جدول تطبیق بده.
4. از Catalog واقعی بررسی کن EUR internal CurrencyId=47، و Authoring صفر-Rate موجود یا نیازمند اصلاح است؛ SeatMap را دست نزن.
5. نرخ‌های CHD/ADT و POS current eligibility در Shop را trace کن، source validity و TTL upstream را مشخص کن. اگر `ValidUntilUtc` trusted قابل دریافت نیست، ریسک را گزارش و صدور شناسه را تا حل معتبر متوقف کن.
6. موجودبودن encryption-at-rest تأییدشده را بررسی کن: اگر نیست A17 gated. A09 FlightFlow proof absent→gated. A19 type mismatch→gated. A18 trusted guardian/infant selection refs not reconstructable from Offer hash alone→gated unless sourced. Insurance no Domain→empty array.
7. تعریف دقیقی از ۴ Unit supported و enum mapping واقعی بنویس. `PerFlight` بدون Traveller را ادعا یا implement نکن.
8. baseline build/test و SeatMap regression command را ثبت کن. فقط پیشنهاد نام Branch جدید بده؛ قبل از Owner هیچ Commit/Push.
9. `AncillarySelectionMaxAge=15min` default پیشنهادی را در Approval Questions یک خط بیاور؛ **خودسرانه TTL دیگری اختراع نکن**.

**Deliverable:** `reports/V8-GATE0-SOURCE-EVIDENCE.md` with SHA, source paths/line ranges, exact field mapping gaps, risk flags, build evidence, SeatMap checksums, proposed smallest diff, GO/NO-GO. **No code or migration in Gate0.**
