# AeroTech.Ancillary — Reservation Pack v12.1 — Gate 0 Resolved / Minimal Gate 1

**تاریخ:** 2026-10-27  
**مرجع سورس:** `aliifarhadi/AeroTech.Ancillary`, branch `feat/ancillary-offers-v9`, HEAD `e6a2012ee101546f5728db6a68844adcb6051204` (`Hold Endpoint Renamed + v12 Stage 0`). Report: `reports/V12-GATE0-SOURCE-AND-CONFLICTS.md`. Gate 0 انجام شده است.  
**ماهیت:** قرارداد خودبسنده + ممیزی تطبیقی + دستورالعمل اصلاح محدود v11. **این Pack ادعای اجرای تغییرات یا تست‌های جدید ندارد.**

## مرجع واحد
برای تصمیم و اجرا فقط اسناد **داخل همین Pack** ملاک‌اند: 01 مرز دامنه و تصمیم‌ها؛ 02 قرارداد HTTP؛ 03 ماتریس A01–A24؛ 04 مدل و Persistence؛ 05 موجودی/چرخه وضعیت؛ 06 نگاشت مصرف‌کننده؛ 07 تست‌ها؛ 08 بنچ‌مارک؛ 09 نقشه سورس/Refactor؛ 10–14 پرامپت‌های اجرایی. اسناد `docs/Pack-v11` و `docs/AeroTech-Ancillary-Reservation-Pack-v10-FINAL` صرفاً **سوابق و شاهد تطبیق** هستند، نه مرجع اجرای بخش گمشده. قواعد بالادستی `CLAUDE.md` همچنان معتبر است.

## وضعیت
- v10 قبلاً پیاده شده بود؛ v11 در دو Commit بعدی با Typed Hold، Stored Selection، Migration و گزارش Closure در مخزن **وجود دارد**. این بسته نباید دوباره v11 را Greenfield بنویسد.
- `reports/V11-FINAL-CLOSURE.md`: گزارش ایجنت از Build 0 خطا/61 هشدار، Domain 293/293، Application 300/300، Shopping 125 pass/1 skipped؛ **مستقل اجرا/تأیید نشده**. Ordering E2E همچنان `NOT VERIFIED` و Local Stock live `GATED`.
- **Gate 0 / F02:** `?unitIds=` در آزمون HTTP با 400 قبل از Controller رد شده؛ P0 قبلی اثبات نشده و نباید برای آن Refactor اضافی ساخت. **F03 تنها ریسک مهم باقیمانده:** Query Key اشتباه مانند `?unitId=5` ممکن است Binder را دور بزند و به Release-All منجر شود؛ Gate 1 فقط guard محدودِ ناشناخته‌های Query و Regression Test آن را انجام دهد. F04–F06 اصلاح سند؛ F07 خارج از Scope؛ I05 یک SQL test در Gate 3.
- موارد Source-aligned: Typed Family input / GET، Source Retire guard (گزارش v11)، per-unit status، موجودی SQL و SeatMap commercial path. آن‌ها را بی‌دلیل بازنویسی نکنید.
- **Route مصوب Owner:** `POST/GET/DELETE /Service/v1/Ancillaries/Hold` و مسیرهای `Hold/{id}/Confirmations`, `Hold/{id}/Cancellations`. این rename در HEAD انجام شده و نیاز به اجرای مجدد ندارد.
- **Open but out of scope:** Offers/Resolve shared six-key `Options` union (R1-Q5) تا دستور مستقل Owner دست نخورد. قیمت‌های صفر تستی F09 مربوط به Catalog هستند، نه اصلاح Reservation.
- این v12.1 همان v12 با Errata محدود Gate 0 است؛ هیچ مدل دامنه یا EndPoint جدیدی معرفی نمی‌کند.

## مرزهای Freeze
- **هیچ outbound call / DB lookup / ACL عضویت سفارش به Ordering**، حتی برای Validation. OrderId/OrderServiceId فقط Correlation.
- **هیچ Hold/Confirm/Release فیزیکی Seat یا FlightFlow outbound** در Ancillary؛ Ordering/FlightFlow مسئول Seat فیزیکی.
- نه 24 Aggregate، نه generic `SelectedJson`، نه Saga/Outbox/Provider جعلی، نه Stock Counter دوم، نه Price/Resolve refresh از Hold، نه ورود به Identity.
- SeatMaps/Resolve، Shopping/Price/Resolve، legacy SeatMap inputs و EMD/Payment freeze، مگر دستور جداگانه مالک.
- GATED/Unsupported هرگز با Fake Hold باز نشود.
- هیچ commit/push/migration روی دیتابیس مالک بدون دستور صریح؛ **Gate 0 بسته شد**؛ شروع Gate 1 تنها با تأیید Owner برای اجرا (درخواست فعلی برای تحویل Prompt مجوز DB mutation نیست).

## اصول Wire
`POST /Service/v1/Ancillaries/Hold`: آرایه‌های `baggages, meals, assistedTravel, airportServices, priorityServices, connectivity, upgrades`. `pets` فعلاً **عضو درخواست نیست**. `seats` از مسیر اختصاصی `SeatMaps/Holds` وارد می‌شوند. در هر family: `variantCode -> products[{provisionId, units[{orderServiceId,unit?,coverage?,selected?}]}]`. `provisionId` به‌تنهایی شناسه Product محلی است؛ Definition/Scope/Inventory در Ancillary derived می‌شوند. `coverageScope, inventoryFacts, serviceDefinitionId, offerId, selectionId, pricingId` در ورودی ممنوع.

## اجرای ایجنت
1. Gate 0: **DONE**؛ به جای اجرای دوباره، از `reports/V12-GATE0-SOURCE-AND-CONFLICTS.md` استفاده کن.
2. **Next:** با اجازه مالک `11-AGENT-GATE1-SAFETY-AND-CONTRACT.md` (تنها Guard Query Key، یک Regression Test، اصلاح اسناد).
3. فقط اگر Gap واقعی ثابت شد، `12-AGENT-GATE2-DOMAIN-AND-MIGRATION.md`.
4. `13-AGENT-GATE3-INVENTORY-SEAT-LIFECYCLE.md`.
5. `14-AGENT-GATE4-CONFORMANCE-CLOSURE.md` و Evidence واقعی.

## بسته و اصالت
تمام فایل‌های لازم در `SHA256SUMS.txt` فهرست شده‌اند. `python3 VERIFY-PACK.py` باید پس از Extract در پوشه‌ای جدید PASS بدهد. نتیجه اجرای Verify نشان‌دهنده **سلامت محتویات Pack** است، نه PASS شدن تست‌های نرم‌افزار.
