# AeroTech.Ancillary — Reservation Pack v12 — Source-Aligned Review & Closure

**تاریخ:** 2026-10-27  
**مرجع سورس:** `aliifarhadi/AeroTech.Ancillary`, branch `feat/ancillary-offers-v9`, HEAD `8c2fb087d43c04a06cf9eb18dc1bdd5734fff51d` (commit `v11`).  
**ماهیت:** قرارداد خودبسنده + ممیزی تطبیقی + دستورالعمل اصلاح محدود v11. **این Pack ادعای اجرای تغییرات یا تست‌های جدید ندارد.**

## مرجع واحد
برای تصمیم و اجرا فقط اسناد **داخل همین Pack** ملاک‌اند: 01 مرز دامنه و تصمیم‌ها؛ 02 قرارداد HTTP؛ 03 ماتریس A01–A24؛ 04 مدل و Persistence؛ 05 موجودی/چرخه وضعیت؛ 06 نگاشت مصرف‌کننده؛ 07 تست‌ها؛ 08 بنچ‌مارک؛ 09 نقشه سورس/Refactor؛ 10–14 پرامپت‌های اجرایی. اسناد `docs/Pack-v11` و `docs/AeroTech-Ancillary-Reservation-Pack-v10-FINAL` صرفاً **سوابق و شاهد تطبیق** هستند، نه مرجع اجرای بخش گمشده. قواعد بالادستی `CLAUDE.md` همچنان معتبر است.

## وضعیت
- v10 قبلاً پیاده شده بود؛ v11 در دو Commit بعدی با Typed Hold، Stored Selection، Migration و گزارش Closure در مخزن **وجود دارد**. این بسته نباید دوباره v11 را Greenfield بنویسد.
- `reports/V11-FINAL-CLOSURE.md`: گزارش ایجنت از Build 0 خطا/61 هشدار، Domain 293/293، Application 300/300، Shopping 125 pass/1 skipped؛ **مستقل اجرا/تأیید نشده**. Ordering E2E همچنان `NOT VERIFIED` و Local Stock live `GATED`.
- **P0 شناسایی شده از سورس HEAD:** در `ServiceController.Release` مقدار `unitIds` خالی به `null` تبدیل می‌شود، در حالی‌که `AncillaryReservation.Release(null,...)` تمام Units Held را Release می‌کند. اقدام: fail-closed برای `unitIds` حاضر اما خالی، بدون تغییر رفتار حذف کاملِ عمدی.
- موارد Source-aligned: Typed Family input / GET، Source Retire guard (گزارش v11)، per-unit status، موجودی SQL و SeatMap commercial path. آن‌ها را بی‌دلیل بازنویسی نکنید.

## مرزهای Freeze
- **هیچ outbound call / DB lookup / ACL عضویت سفارش به Ordering**، حتی برای Validation. OrderId/OrderServiceId فقط Correlation.
- **هیچ Hold/Confirm/Release فیزیکی Seat یا FlightFlow outbound** در Ancillary؛ Ordering/FlightFlow مسئول Seat فیزیکی.
- نه 24 Aggregate، نه generic `SelectedJson`، نه Saga/Outbox/Provider جعلی، نه Stock Counter دوم، نه Price/Resolve refresh از Hold، نه ورود به Identity.
- SeatMaps/Resolve، Shopping/Price/Resolve، legacy SeatMap inputs و EMD/Payment freeze، مگر دستور جداگانه مالک.
- GATED/Unsupported هرگز با Fake Hold باز نشود.
- هیچ commit/push/migration روی دیتابیس مالک بدون دستور صریح؛ ابتدا Gate0 audit، سپس تأیید مالک.

## اصول Wire
`POST /Service/v1/Ancillaries/Service-Holds`: آرایه‌های `baggages, meals, assistedTravel, airportServices, priorityServices, connectivity, upgrades`. `pets` فعلاً **عضو درخواست نیست**. `seats` از مسیر اختصاصی `SeatMaps/Holds` وارد می‌شوند. در هر family: `variantCode -> products[{provisionId, units[{orderServiceId,unit?,coverage?,selected?}]}]`. `provisionId` به‌تنهایی شناسه Product محلی است؛ Definition/Scope/Inventory در Ancillary derived می‌شوند. `coverageScope, inventoryFacts, serviceDefinitionId, offerId, selectionId, pricingId` در ورودی ممنوع.

## اجرای ایجنت
1. `10-AGENT-GATE0-READONLY-AUDIT.md` و گزارش اختلافات HEAD؛ هیچ کد.
2. با اجازه مالک، `11-AGENT-GATE1-SAFETY-AND-CONTRACT.md`.
3. فقط اگر Gap واقعی ثابت شد، `12-AGENT-GATE2-DOMAIN-AND-MIGRATION.md`.
4. `13-AGENT-GATE3-INVENTORY-SEAT-LIFECYCLE.md`.
5. `14-AGENT-GATE4-CONFORMANCE-CLOSURE.md` و Evidence واقعی.

## بسته و اصالت
تمام فایل‌های لازم در `SHA256SUMS.txt` فهرست شده‌اند. `python3 VERIFY-PACK.py` باید پس از Extract در پوشه‌ای جدید PASS بدهد. نتیجه اجرای Verify نشان‌دهنده **سلامت محتویات Pack** است، نه PASS شدن تست‌های نرم‌افزار.
