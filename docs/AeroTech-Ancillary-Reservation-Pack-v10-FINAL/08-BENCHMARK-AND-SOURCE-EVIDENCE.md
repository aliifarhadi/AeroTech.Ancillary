# 08 — منطق تصمیم‌ها، بنچ‌مارک و مرز ادعاها

## 1. Benchmark واقعی چه چیزی را تأیید می‌کند؟
**Travelport official — Ancillary & EMD Guide:**
https://developer.travelport.com/docs/flights/guides/seats-and-ancillaries/ancillaries-and-emd-guide
- Shopping، pricing، افزودن Ancillary به رزرو، commit و EMD/Payment در همه Providerها یک رفتار یکسان ندارند؛ NDC/GDS و Airline-specific restrictions وجود دارد.
- امکان Cancel Ancillary در مراحل مجاز مطرح است؛ EMD و پرداخت از رزرو خدمت قابل‌تفکیک‌اند.
- **نتیجه برای ما:** Ancillary Reservation را از Issue/EMD در Ordering جدا نگه می‌داریم، و عملیات Unit با Status مستقل داریم.

**Travelport official — Seats Guide:**
https://developer.travelport.com/docs/flights/guides/seats-and-ancillaries/seats-guide
- SeatMap، Seat Book، Seat Cancel می‌توانند مراحل مجزا باشند و تغییر/لغو Seat محدودیت provider-specific دارد.
- **نتیجه برای ما:** Seat Selection commercial Unit را به‌صورت جداگانه ردیابی می‌کنیم. اما انتخاب فعلی ما که Seat physical فقط با Ordering/FlightFlow است یک **تصمیم داخلی AeroTech** است، نه الزام Travelport.

**Travelport official — Flights Booking Guide:**
https://developer.travelport.com/docs/flights/guides/booking-and-reservations/flights-booking-guide
- Booking می‌تواند Held و هنوز Ticketed نشده باشد؛ expiry و پرداخت/صدور سند lifecycle جداگانه دارند.
- **نتیجه برای ما:** Hold → Confirm را با Issue/EMD یکی نمی‌کنیم؛ TTL واقعی لازم است.

## 2. Benchmark واقعی خود پروژه — FlightFlow
Repo `aliifarhadi/Aerotech.FlightFlow@k8s-stg:962f4188`:
- `ServiceController`: `POST Seat-Holds`, `POST Seat-Holds/{id}/Confirmations`, `DELETE Seat-Holds/{id}`, `POST Seat-Confirmations/{id}/Cancellation`، `PATCH expiry`، `Splits`.
- `FlightHeldSeatsDto`: `HoldId`, `IdempotencyKey`, `Reference`, `ExpiresAt`, `Seats[]` با per-seat `SeatHoldReference`.
- `Flight.cs` و `FlightSeatHold`: هر Seat status مستقل دارد؛ Confirm/Release/Expire/Cancel بر پایه Idempotency و state guards.
- **نتیجه:** یک Group Reference + per-unit references و lifecycle صریح از قبل در AeroTech یک رفتار عملی است. پیاده‌سازی Ancillary باید همان اصول را با Domain خودش به کار ببرد، نه کپی یک‌به‌یک Schema FlightFlow.

## 3. کدام تصمیم‌ها عمداً طراحی AeroTech هستند؟
| Decision | Origin | چرا؟ |
|---|---|---|
| `AncillaryReservation` Root + Unit generic | Domain موجود در Ancillary + تصمیم مالک | بدون 24 Aggregates جداگانه، partial Unit release آسان |
| Allocation immutable زیر Unit (Ledger، نه Counter) | طراحی داخلی + تصمیم مالک Gate 0 (Q1) | Counterهای `FlightCapacity` در FlightFlow کپی نمی‌شوند؛ Counter دوم منبع حقیقت و هماهنگی پیچیده با Expiry می‌سازد؛ TotalCapacity موجود را تبدیل به Remaining نمی‌کنیم |
| Lock منبع در SQL (`sp_getapplock` Transaction) + محاسبه ledger | SQL Server + تصمیم مالک Gate 0 (Q2) | Hold دو Order روی یک stock نباید با order-lock تنها oversell شود؛ RedLock چندکلیدی FlightFlow کپی نمی‌شود |
| `SeatMaps/Holds` append با holdId | تصمیم پروژه برای mixed Hold | همان Reservation برای Seat commercial و Baggage/Meal؛ هیچ physical seat inventory Ancillary |
| Supplier fail closed | نبود Provider adapter واقعی در سورس | موفقیت رزرو خیالی خطرناک‌تر از گیت صریح است |
| All-or-nothing در هر local batch | baseline Hold فعلی و اولویت «Flow ساده» مالک | اضافه‌کردن Pending/Partial/Async Saga بدون Supplier واقعی غیرضروری است |
| Release/Cancel انتخابی با `unitIds` روی مسیر گروهی | تصمیم مالک Gate 0 (Q3) | Unit بخشی از Aggregate است و چند Unit در یک درخواست تغییر می‌کنند؛ حذف یکی از چند خدمت بدون حذف Hold؛ مسیر جدا برای Entity فرزند ساخته نمی‌شود |
| `AcceptedWeightKg` Unsupported | تصمیم مالک Gate 0 (Q5) | Price محصول PerKilogram را نمی‌فروشد، پس وزن پذیرفته‌شده قابل اثبات نیست |
| بدون Domain Event/Outbox/Projection | تصمیم مالک Gate 0 (Q6) | GET از CommandDb می‌خواند و مصرف‌کننده‌ای وجود ندارد |

## 4. تفاوت‌های واقعی نسبت به v13.2 تاریخی
اسناد قدیمی v13.2 برای Supplier/Recovery و Multi-provider صفحاتی درباره `Pending`, `Mixed`, `ProviderOperationAttempt`, `Outcome` نوشته‌اند. در این Pack آن‌ها **برای پیاده‌سازی فعلی اختیار ندارند**؛ زیرا Supplier API/response فعلی در دست نیست و مالک صریحاً پیچیدگی غیرضروری را رد کرده است. این حذف به معنی ادعای حل Supplier نیست؛ هر Supplier-managed Hold باید با خطای روشن **گیت** شود.

## 5. مواردی که Benchmark اثبات نمی‌کند
- Travelport هیچ لزومی برای شکل دقیق Endpoint یا پارامتر `unitIds` ایجاد نمی‌کند؛ این‌ها قرارداد داخلی‌اند.
- FlightFlow Side نمی‌تواند به Ancillary مجوز دست‌کاری Inventory Seat بدهد؛ Owner فعلی صریحاً آن را ممنوع کرده است.
- بنچ‌مارک EMD نمی‌گوید `Confirm` حتماً به معنی Issue است؛ برخلاف آن، این دو ممکن است جدا باشند.
- Scheme دقیق SQL `sp_getapplock Transaction` یک روش اجرایی برای ظرفیت خودمان است؛ رفتار PSS تجاری محسوب نمی‌شود.

## 6. مرز تحقیق و کد
در مخزن Ancillary، `HoldAncillaryServicesService.NewHoldAsync` فعلی تنها Definition/Provision/Supplier و duplicate OrderService را بررسی و یک Reservation در یک SaveChanges ذخیره می‌کند. `FlightCountInventory`, `FlightWeightInventory`, `AirportSlotInventory` capacity totals دارند اما Allocation/Reserve در Domain ندارند. `SeatSelectionVerifier` محصول/قیمت را بررسی می‌کند، اما Physical OccupiedSeat را از FlightFlow نمی‌گیرد. این واقعیت‌ها منشأ اصلی طراحی v10 هستند. برای جزئیات فایل/Commit، `09-SOURCE-TRACEABILITY.md` را بخوانید.
