# AeroTech Ancillary — Reservation Pack v10 — START HERE

**وضعیت:** قرارداد طراحی آماده اجرا؛ کدنویسی فقط توسط Coding Agent در Gateهای تعریف‌شده.  
**تاریخ:** 2026-10-26  
**مرجع کد Ancillary:** `aliifarhadi/AeroTech.Ancillary`, `feat/ancillary-offers-v9@2025efd4fbdb4ddb9c72a1ac1eb5023c0b29a3c0`.  
**بنچ‌مارک هم‌خانواده:** `aliifarhadi/Aerotech.FlightFlow`, `k8s-stg@962f41880df0f10a1d105559c233af63ef9db968`.

## تصمیم Gate 0 مالک (2026-10-26) — ادغام‌شده
`AeroTech-Ancillary-v10-Gate0-Owner-Decisions-FA.md` پاسخ مالک به شش سؤال Gate 0 است و در فایل‌های `01` تا `14` همین Pack ادغام شده است. Gate 0 بسته است (`reports/V10-GATE0-SOURCE-AUDIT.md`). خلاصه الزام‌آور:
1. **ظرفیت:** Ledger تخصیص (`AncillaryReservationUnitAllocation`) + وضعیت Unit + انقضای منطقی؛ Counter قابل‌تغییر ممنوع.
2. **قفل منبع:** `sp_getapplock` با `@LockOwner='Transaction'` روی همان Connection و Transaction؛ RedLock چندکلیدی ممنوع.
3. **عملیات جزئی:** فقط روی مسیرهای گروهی موجود با `unitIds` اختیاری (`DELETE …?unitIds=` و بدنه `Cancellations`)؛ مسیر `Units/{unitId}` ساخته نمی‌شود.
4. **Local:** پیاده‌سازی کامل با اثبات SQL واقعی؛ HTTP زنده Local تا اتصال Reference واقعی **GATED**.
5. **`AcceptedWeightKg`:** فعلاً Unsupported صریح؛ فیلد/ستون وزن پذیرفته‌شده ساخته نمی‌شود؛ فقط `FixedKgPerAcceptedUnit × Quantity`.
6. **رویداد:** هیچ Domain Event، Outbox یا Projection تازه.

## قانون تنها مرجع
1. تصمیم جدید مالک در همین Pack، شامل تصمیم Gate 0 بالا و به‌ویژه **عدم مداخله Ancillary در موجودی فیزیکی SeatMap**، اولویت قطعی دارد.
2. `01-MASTER-RESERVATION-CONTRACT-FA.md` مرجع واحد تصمیم‌های تجاری، مرز سرویس‌ها و Lifecycle است.
3. فایل‌های `02` تا `07` **تفصیل اجرایی همین Master** هستند؛ سند مستقل و رقیب نیستند. در تعارض احتمالی، Master مقدم است و ایجنت باید گزارش کند، نه حدس بزند.
4. سورس Pin‌شده مرجع نام کلاس‌ها/ساختار موجود است؛ برای موارد فاقد پیاده‌سازی، قرارداد این Pack مرجع است.
5. اسناد تاریخی v12/v13، P3.3، و v8/v9 در این فاز authority نیستند و فقط شاهد تاریخی‌اند. **از آن‌ها مدل Pending، Supplier orchestration، FlightFlow Seat delegation یا Aggregate جدید کپی نکنید.**

## فهرست مستقل و کامل
| فایل | نقش |
|---|---|
| `00-START-HERE.md` | authority، ترتیب، قواعد |
| `01-MASTER-RESERVATION-CONTRACT-FA.md` | Overview کامل و جریان نهایی، تصمیم‌ها و مثال‌ها |
| `02-EXACT-ENDPOINTS-AND-DTOS.md` | endpointها و ورودی/خروجی فیلدبه‌فیلد |
| `03-DOMAIN-AND-LIFECYCLE.md` | Aggregate/Entity/Allocation/Status/Idempotency |
| `04-INVENTORY-ALLOCATION-ALGORITHM.md` | Count/Weight/Slot، تخصیص اتمیک، هم‌زمانی |
| `05-SEATMAP-AND-ORDERING-HANDOFF.md` | SeatMap، مسئولیت Ordering/FlightFlow، mapping |
| `06-FAILURE-RETRY-EXPIRY-AND-MIGRATION.md` | rollback، cancel، expiry، migration و safety |
| `07-ACCEPTANCE-MATRIX.md` | تست‌های شماره‌دار دامنه، دیتابیس، API و سناریوهای منفی |
| `08-BENCHMARK-AND-SOURCE-EVIDENCE.md` | استنادها، واقعیت کد، مرز benchmark و طراحی داخلی |
| `09-SOURCE-TRACEABILITY.md` | نقشه دقیق سورس/گپ/تغییر موردنیاز |
| `10-AGENT-GATE0-AUDIT.md` | ممیزی فقط خواندنی + ارزیابی تعارض |
| `11-AGENT-GATE1-DOMAIN-CONTRACTS.md` | Domain/DTO/Schema/partial unit |
| `12-AGENT-GATE2-LOCAL-INVENTORY.md` | تخصیص واقعی Local و آزمون concurrency |
| `13-AGENT-GATE3-SEATMAP-AND-LIFECYCLE.md` | Seat commercial و lifecycle مشترک |
| `14-AGENT-GATE4-E2E-CLOSURE.md` | تست/HTTP/گزارش closure |
| `AeroTech-Ancillary-v10-Gate0-Owner-Decisions-FA.md` | پاسخ مالک به Gate 0؛ در `01` تا `14` ادغام شده است |
| `SHA256SUMS.txt` و `VERIFY-PACK.py` | صحت مستقل بسته |

## اصل تحویل
- Gate 0 **بدون کدنویسی** انجام و با تصمیم مالک بسته شد. گیت‌های اجرایی به ترتیب انجام شوند؛ هیچ Gate بعدی پیش از قبول مورد قبلی آغاز نشود.
- تغییر مجاز: **فقط سرویس Ancillary**. Ordering/FlightFlow/SeatMap shopping/Price/Resolve را خودسرانه تغییر ندهید.
- تست Domain و SQL واقعی شرط عبور است؛ Green بودن تست‌های Mock به‌تنهایی اثبات عدم oversell نیست. HTTP زنده برای Unlimited و Seat لازم است؛ Local تا اتصال Reference واقعی با SQL واقعی اثبات و در HTTP زنده GATED گزارش می‌شود.
- `InventoryAuthority.Supplier` بدون Adapter واقعی و قرارداد اثبات‌شده، **خطای روشن** می‌دهد؛ ادعای Held/Confirmed کاذب ممنوع.
- Stage پایان‌یافته باید شامل Diff، migration، گزارش تست، مقایسه سورس، و موارد gated واقعی باشد.
- در فایل اصلی **هیچ جدول یا Engine عمومی تازه‌ای برای هر 24 Variant** تجویز نشده است. تنها یک جدول تخصیصِ لازم برای Inventory Local وجود دارد.
