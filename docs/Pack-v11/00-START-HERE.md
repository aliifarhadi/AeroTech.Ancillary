# AeroTech.Ancillary — Reservation v11 — قرارداد یکپارچه Domain-First

**مرجع واحد اجرای اصلاح v10:** همین بسته، مبنای کد `aliifarhadi/AeroTech.Ancillary`، `feat/ancillary-offers-v9@7a74055c774b2466fe239b8307ec93b15264e50c` (2026-10-26). **وضعیت:** قرارداد هدف؛ پیاده‌سازی نشده؛ هیچ ادعای موفقیت E2E وجود ندارد.

## ترتیب مرجع
1. `01-MASTER-AND-OWNER-DECISIONS-FA.md` تصمیم‌های مصوب و جریان عملیاتی.
2. `02-EXACT-RESERVATION-API-AND-DTOS.md` **تنها مرجع ورودی/خروجی بیرونی**؛ `03-VARIANT-BY-VARIANT.md` انتخاب‌های A01–A24؛ `04-AGGREGATE-AND-RELATIONAL-STORAGE.md` Domain؛ `05-INVENTORY-AND-LIFECYCLE.md` عملیات.
3. `06-V10-REFORM-AND-MIGRATION.md` حفظ/حذف دقیق؛ `07-ACCEPTANCE-MATRIX.md` تست‌ها؛ `08-SOURCE-AND-BENCHMARK-FA.md` تفاوت بین شواهد و تصمیم.
4. `10` تا `15`: شش پرامپت ایجنت واقعی، در همین ZIP. ایجنت پس از هر Gate گزارش می‌دهد؛ **بدون اجازه Owner Commit/Push/Merge نکند**.

## قواعد قطعی
- Ancillary **هرگز Ordering را Call نمی‌کند و عضویت Order/Traveller/Service در Ordering را تطبیق نمی‌دهد**. Ordering فقط Caller سرویس‌به‌سرویس است، Ancillary قواعد **خود** را بررسی می‌کند.
- **مبدأ طراحی Hold، خروجی `AncillaryOffers` و `Resolve` است؛** همان `baggages`, `meals`, `pets`, `assistedTravel`, `airportServices`, `priorityServices`, `connectivity`, `upgrades` و شکل `variantCode → products → units → unit/coverage/selected`. محتوای `unit` و `coverage` در Hold، نگاشت شناسه‌های عملیاتی Order است؛ دلیل مستند در 02.
- **فیلدهای ممنوع ورودی**: `services[]`, `coverageScope`, `serviceDefinitionId`, `inventoryFacts`, `inventorySourceId`, `stockPoolId`, `pricingId`, `currencyId`, `offerId`, `selectionId`, `unitPrice`, `taxes`؛ هیچ JSON payload عمومی/بدون نوع.
- تنها Product identifier ضروری: `provisionId` موجود در `Resolve.provision.provisionId`؛ Variant/Definition/Policy/Scope را خود Ancillary پیدا می‌کند. `productKey` نمایشی AncillaryOffers نباید به Hold ارسال شود.
- `selected` اگر ارسال شود همان مدل **`Resolve.selected`** است: `quantity` و `options` با کلید خانواده (`baggage`,`pet`,`assistedTravel`,`airportService`,`priority`,`connectivity`)؛ فیلدهای انتخابی درون Options دقیقاً همان Price/Resolve هستند. مقادیر ثابت از Definition/Provision با Snapshot زمان Hold محفوظ می‌شوند؛ نه از داده زنده در GET.
- یک Reservation Aggregate و Entityهای Unit/Allocation موجود حفظ شوند. Typed Selectionها کم‌شمار، رابطه‌ای و immutable؛ **نه ۲۴ Aggregate، نه طراحی generic JSON، نه موتور جدید Stock**.
- SeatMap Endpoint اختصاصی فعلی؛ Seat commercial فقط. رزرو اشغال فیزیکی و آزادسازی SeatNumber متعلق به Ordering/FlightFlow است؛ Ancillary به آن سرویس‌ها Call نمی‌کند.
- **بدون تطبیق با Price سابق:** Ancillary آنچه در Hold می‌گیرد را با قواعد خودش validate و نگهداری می‌کند؛ نمی‌تواند ادعا کند دقیقاً همان مشتری و همان Price قبلی بوده است. این محدودیت آگاهانه و مصوب است.
- Gate 0 بر اساس Source واقعی دقیق DTOهای `AncillaryOffers`/`Resolve`/`SeatHoldFlightInput` را کنترل کند. هیچ عدد Enum یا فیلد غیرموجود از نمونه‌های نمایشی نتیجه‌گیری نشود. موارد فاقد پشتیبانی **GATED**، نه دورزده.

## محتویات الزامی
`00-START-HERE.md`, `01-MASTER-AND-OWNER-DECISIONS-FA.md`, `02-EXACT-RESERVATION-API-AND-DTOS.md`, `03-VARIANT-BY-VARIANT.md`, `04-AGGREGATE-AND-RELATIONAL-STORAGE.md`, `05-INVENTORY-AND-LIFECYCLE.md`, `06-V10-REFORM-AND-MIGRATION.md`, `07-ACCEPTANCE-MATRIX.md`, `08-SOURCE-AND-BENCHMARK-FA.md`, `10-AGENT-GATE0-SOURCE-AUDIT.md`, `11-AGENT-GATE1-WIRE-CONTRACT.md`, `12-AGENT-GATE2-TYPED-DOMAIN.md`, `13-AGENT-GATE3-INVENTORY-HOLD.md`, `14-AGENT-GATE4-SEAT-AND-LIFECYCLE.md`, `15-AGENT-GATE5-E2E-CLOSURE.md`, `SHA256SUMS.txt`, `VERIFY-PACK.py`.

اجرای کنترل: `python3 VERIFY-PACK.py`. هر فایل نام‌برده واقعاً باید در محیط ایجنت حاضر باشد. هیچ Doc یا Pack تاریخی برای تکمیل جزئیات این نسخه مرجع نیست.
