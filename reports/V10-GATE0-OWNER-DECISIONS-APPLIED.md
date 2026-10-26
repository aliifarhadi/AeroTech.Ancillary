# Reservation v10 — ادغام تصمیم Gate 0 مالک در Pack

**تاریخ:** 2026-10-26 · **مبنا:** `feat/ancillary-offers-v9@debd065`
**منبع تصمیم:** `docs/AeroTech-Ancillary-Reservation-Pack-v10-FINAL/AeroTech-Ancillary-v10-Gate0-Owner-Decisions-FA.md`

این ادغام Gate 0 را دوباره باز نمی‌کند. فقط تصمیم مالک را در فایل‌های اجرایی Pack می‌نشاند تا یک مرجع واحد بماند.

## 1. تغییر هر فایل (جدول §9 تصمیم)

| فایل | تغییر |
|---|---|
| `00-START-HERE.md` | بخش «تصمیم Gate 0 مالک» با شش حکم؛ ردیف فایل تصمیم در فهرست؛ Gate 0 بسته؛ HTTP زنده برای Unlimited و Seat، و Local با SQL واقعی و HTTP زنده GATED |
| `01-MASTER-RESERVATION-CONTRACT-FA.md` | §1 و §3: عملیات جزئی با `unitIds` روی مسیر گروهی، قفل `sp_getapplock` Transaction و Ledger. §6: `AcceptedWeightKg` Unsupported. §7: `DELETE Service-Holds/9001?unitIds=9102`. §8: دو مسیر `Units/{unitId}` حذف شد. §10: Scope، Unsupportedها و استثنای محدود Price. §11: معیار Closure تفکیک SQL و HTTP |
| `02-EXACT-ENDPOINTS-AND-DTOS.md` | جدول مسیرها: شش مسیر و `unitIds` در Query `DELETE` و بدنه `Cancellations`. فیلد وزن پذیرفته‌شده از `InventoryHoldFacts` حذف شد (فقط Slot). §5 بازنویسی: عملیات انتخابی، قواعد فهرست، همه‌یا‌هیچ، replay و 409. §6: عملیات کلی، Unitهای terminal کنار گذاشته می‌شوند. «8گانه» به «6گانه» تغییر کرد |
| `03-DOMAIN-AND-LIFECYCLE.md` | فیلد وزن پذیرفته‌شده حذف شد و دو Fact Slot ماند. جدول Lifecycle: Release و Cancel کلی و انتخابی، و متدهای Domain با فهرست `UnitId`. §8: بررسی Legacy فقط‌خواندنی پیش از استقرار، بدون Reconciliation Runtime |
| `04-INVENTORY-ALLOCATION-ALGORITHM.md` | Weight فقط FixedKg و `AcceptedWeightKg` Unsupported. pseudocode: رد صریح، کد بازگشتی قفل و خواندن پس از قفل. بند DB: همان Connection/Transaction، `READ COMMITTED`، `@LockTimeout` محدود و تمایز با قفل Session Slot. Close، Suspend و Retire زیر همان قفل؛ Release، Cancel و Expire بدون قفل منبع. Legacy بدون Reconciliation Runtime |
| `05-SEATMAP-AND-ORDERING-HANDOFF.md` | ردیف نگاشت وزن پذیرفته‌شده حذف شد. نمونه لغو و آزادسازی Seat با `unitIds` |
| `06-FAILURE-RETRY-EXPIRY-AND-MIGRATION.md` | عملیات انتخابی. جدول Idempotency با `unitIds`. مقایسه canonical: `timeWithOffset`، `guestCount`، `quantity`. Migration: دو Fact و `AppendIdempotencyKey`. Legacy: بررسی پیش از استقرار |
| `07-ACCEPTANCE-MATRIX.md` | I12 و I13 (رد صریح و فیلد ناشناخته)، L08، L09، L11 تا L15 (مسیر گروهی با `unitIds`)، L20 (Fact و quantity)، E08، R07 (HTTP برای Unlimited/Seat، SQL برای Local، و Local زنده GATED). هیچ شناسه‌ای حذف نشد؛ همچنان 108 شناسه |
| `08-BENCHMARK-AND-SOURCE-EVIDENCE.md` | Ledger به‌جای کپی Counter، قفل SQL، `unitIds`، `AcceptedWeightKg` Unsupported و نبود Event، هر کدام با ارجاع به تصمیم مالک |
| `09-SOURCE-TRACEABILITY.md` | Unit با دو Fact. Controller: `unitIds` بدون Route تازه. استثنای مستند Guard در Price |
| `10-AGENT-GATE0-AUDIT.md` | وضعیت: بسته، با ارجاع به گزارش‌ها و تصمیم. پاسخ سؤال 4 |
| `11-AGENT-GATE1-DOMAIN-CONTRACTS.md` | دو Fact بدون فیلد وزن؛ Release و Cancel با فهرست `UnitId` بدون مسیر فرزند؛ ممنوعیت Event، Outbox، Projection و Route فرزند؛ تست‌های Idempotency و عملیات جزئی |
| `12-AGENT-GATE2-LOCAL-INVENTORY.md` | الگوها فقط FixedKg؛ قفل `sp_getapplock` Transaction با کلید و قواعد کامل؛ Close، Suspend و Retire زیر قفل؛ Local زنده GATED و بدون Reference ساختگی در DI تولید |
| `13-AGENT-GATE3-SEATMAP-AND-LIFECYCLE.md` | مورد 5: عملیات جزئی روی مسیرهای گروهی. سناریوی مختلط با `unitIds` |
| `14-AGENT-GATE4-E2E-CLOSURE.md` | HTTP زنده برای Unlimited + Seat؛ Local زنده GATED؛ مورد 6a برای Guard در Price؛ Ordering با وضعیت NOT VERIFIED E2E |

## 2. کنترل حذف تضاد

دستور (روی `00` تا `14`):

```text
grep -n -i -E "Units/\{unitId\}|Units/[0-9]|acceptedWeightKgTotal|AcceptedWeightKgTotal|accepted-weight|FlightFlow physical seat allocation|Local HTTP PASS|ReleaseUnit|CancelUnit|unit-level|Release one|Cancel one|Unit release|three inventory facts|3 InventoryFacts|8گانه|سه Fact"
```

نتیجه: هیچ دستور اجرایی منسوخی باقی نماند. جمله‌هایی که هنوز با این الگو پیدا می‌شوند همه منفی یا توصیفی‌اند و دستور اجرایی نیستند:
- `00:12`: «مسیر `Units/{unitId}` ساخته نمی‌شود».
- `06:60`، `07:40`، `11:8` و `11:27`: «فیلد یا ستون وزن پذیرفته‌شده نداریم» یا «رد می‌شود».
- `03:94`، `07:73`، `08:30` و `11:21`: درباره وضعیت Unit، نه مسیر.

## 3. یکپارچگی Pack

| کنترل | نتیجه |
|---|---|
| `SHA256SUMS.txt` | از نو نوشته شد: 16 ردیف، همان فهرست `EXPECTED` در `VERIFY-PACK.py` |
| `python VERIFY-PACK.py` | `PASS: 16 required files, 108 unique numbered acceptance tests, SHA-256, JSON, references, fences` |
| پایان خط فایل‌های Pack | LF، همانند نسخه تحویلی؛ تبدیل نشد |
| `git diff --stat` | 16 فایل: 138 درج و 122 حذف |

- فایل تصمیم در `SHA256SUMS.txt` نیست، چون فهرست ثابت `VERIFY-PACK.py` آن را ندارد؛ `VERIFY-PACK.py` تغییر نکرد. zip تحویلی دست نخورد و نسخه پیش از ادغام است.
- **محدودیت:** hashها روی بایت‌های LF حساب شده‌اند. با `core.autocrlf=true`، یک checkout تازه روی Windows فایل‌ها را CRLF می‌نویسد و `VERIFY-PACK.py` در آن نسخه mismatch می‌دهد. نسخه تحویلی مالک هم همین ویژگی را داشت.

## 4. داده dev

به دستور مالک، همه Holdهای قدیمی از `DotAirAncillary` حذف شدند:
- رکوردها: 8 Reservation و 9 Unit، در یک تراکنش.
- اسکریپت: scratch `v10/delete_holds.sql`.
- بعد از حذف: `Ancillary.AncillaryReservations = 0` و `Ancillary.AncillaryReservationUnits = 0`.
- تنها FK به این جدول‌ها از خود Units است.
