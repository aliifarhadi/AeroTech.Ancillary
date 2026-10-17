# AeroTech Ancillary — Master Pack v13.2 FINAL

**تاریخ:** 2026-10-16  
**وضعیت:** تصمیم‌های طراحی و قراردادها FINAL؛ وضعیت پیاده‌سازی را صرفاً بر اساس شواهد کد می‌توان «بسته» دانست.  
**مرجع یگانه:** این مجموعه فایل‌ها با رعایت الزامی `CLAUDE.md` فعلی مخزن (قواعد مالک بالاتر از Pack) و نسخه یکپارچه آن؛ تمامی Packها و Promptهای v12.x و v13.0 و گزارش‌های پیشین صرفاً پیشینه تاریخی‌اند و هیچ الزام متناقضی از آنها منتقل نمی‌شود.

## اصلاح قطعی تناقض v13.1
قانون دوم `CLAUDE.md` بر Pack مقدم است: **هیچ GET/PUT مستقل برای CancellationTerms یا child/rule group ایجاد نمی‌شود**. Rules زیر همان `Settlement.CancellationRules` در درخواست کامل Define/Change یک Provision مدیریت می‌شوند. API/فرم Seat هیچ فیلد اجباری جدیدی ندارد؛ عدم ارسال rules در Seat PUT آنها را پاک نمی‌کند. `[]` صریح تنها راه حذف rules از Draft است. درباره Enumها، Nullableها، Time Windows و Publish به فصل‌های 13 و 22 مراجعه کن.

## چرا این نسخه؟
- توسعه Ancillary **تنها در سه مرحله صریح**: **S1 Authoring**، **S2 Shopping**، **S3 Reservation & Servicing**؛ هر مرحله یک هدف، یک API قراردادشده و معیار پذیرش دامنه دارد.
- تمرکز Pack بر **Business Domain، رفتار، فیلدها، Interface/Endpoint، مالکیت سرویس‌ها، سناریو و Compatibility** است؛ روش نوشتن تست، نوع دیتابیس تست، CI و ساختار داخلی برنامه به ایجنت واگذار شده و در Pack تجویز نمی‌شود.
- نه تغییر DTO/نام‌گذاری/ساختار پاسخ `SeatMaps` و نه بازنویسی فرم و API موجود Seat Backoffice مجاز نیست.
- هیچ Aggregate جداگانه برای هر Variant، هیچ Shopping Engine دوم، هیچ Endpoint عمومی `Meal/Details` و مانند آن، و هیچ اختراع اصطلاحاتی به‌جای مفاهیم ATPCO/PSS.

## وضعیت مرجع سورس (قابل بازبینی مجدد در شروع هر Stage)
| سرویس | Repository / Branch | آخرین SHA هنگام تدوین | نقش |
|---|---|---|---|
| Ancillary | `aliifarhadi/AeroTech.Ancillary` / `k8s-stg` | `9aca1cd3985d00d1e79356cb645d0cfd02eab082` | مرجع کد فعلی برای S1/S2/P3 اولیه |
| AirAvail / AirOffer | `aliifarhadi/AeroTech.AirAvail` / `k8s-stg` | `ac840401d38cb407af339d6fa07f98e052b1bd04` | `Service/v1/FlightOffers/Details` برای Offer priced |
| FlightFlow | `aliifarhadi/Aerotech.FlightFlow` / `k8s-stg` | `962f41880df0f10a1d105559c233af63ef9db968` | مقایسه الگوی Hold و OccupiedSeats؛ **نسخه انتشار/استقرار خارجی gate Ancillary نیست** |
| Ordering.Final | `aliifarhadi/AeroTech.Ordering.Final` | دسترسی خواندن کد فعلی برقرار نشد | مسیرهای پیشنهادی آن در این Pack **قرارداد هدف، نه واقعیت فعلی** هستند |

`[CURRENT]` = مشاهده‌شده در سورس؛ `[TARGET]` = قرارداد قطعی برای اجرای جدید؛ `[EXTERNAL]` = نیاز همکاری سرویس دیگر/استقرار خارج از این Repo. **در صورت تعارض با سورس فعلی، TARGET هدف رفتار را تعیین می‌کند، اما مسیر موجود و قرارداد کلاینت را بی‌دلیل نمی‌شکند.**

## تعریف دقیق تحویل سه Stage
| Stage | یک هدف | خروجی دقیق | نباید داخل آن انجام شود |
|---|---|---|---|
| **S1 — Authoring Closure** | هر ۹ Profile را واقعاً در Backoffice تعریف و مدیریت کنیم | ۹ Definition تخصصی حفظ؛ Seat Provision ثابت؛ ۸ Provision تخصصی اضافه؛ Pricing و Lifecycle مشترک؛ قواعد اختیاری کنسلی در **بدنه همان Provision Define/Change** (داخل `Settlement`)؛ **بدون Endpoint جدید**؛ ۲۴ Variant با فیلدهای دقیق | Shopping rewrite، Hold/EMD/Refund، تغییر Seat UI |
| **S2 — Shopping Closure** | مشتری برای هر Scope بتواند فهرست، گزینه واقعی، قیمت نهایی و شناسه انتخاب معتبر بگیرد | `AncillaryOffers` + `Details` + `Selections/Evaluate` سه Surface؛ `SeatMaps` همان DTO فعلی؛ `pricedSelectionRef` قابل Resolve؛ کنترل واقعی Source/Price/Availability | Hold/Confirm/Cancel، تغییر ساختار SeatMaps |
| **S3 — Reservation & Servicing Closure** | Ordering بتواند مجموعه انتخاب‌های S2 را Hold، Confirm، Release، Cancel و Quote کند | Aggregate رزرو فعلی تکمیل؛ Hold گروهی + Unit refs؛ local/supplier/manual؛ cancel جزئی، expiry، idempotency، cancellation quote و event/contract Ordering | پرداخت وجه، صدور/ریفاند EMD یا E-ticket داخل Ancillary؛ دستکاری FlightFlow |

در S3 قرارداد کامل Refund end-to-end تعریف می‌شود ولی **اجرای مالی و اسنادی** متعلق به Ordering/Payment/Ticketing است، نه Ancillary. بنابراین «S3 Ancillary Closed» به معنی انتشار خودکار کد دیگر سرویس‌ها نیست.

## قطعیات غیرقابل مذاکره
1. **۹ Profile، ۲۴ Variant** A01..A24؛ `Definition -> Provision -> Pricing`، موجودی فقط در صورت واقعا موردنیاز.
2. `Seat` تجاری: A07/A08 از SeatMap؛ A09 ExtraSeat در مسیر فعلی سایر Ancillaryها باقی می‌ماند تا قرارداد مستقل EXST مصوب شود؛ هیچ `AncillaryInventoryPolicy` فیزیکی برای Seat.
3. `SeatMapsDto` و نام همه فیلدهای nested، Statusهای `AVAILABLE|BLOCKED|OCCUPIED`، `travelerPricing[].id`، Route و Envelope ثابت؛ اصلاح رفتارِ قیمت جعلی/منبع ناقص **در همین شکل** انجام شود.
4. کد فعلی Backoffice Seat (`Definition`, `ProvisionForm`, `Provisions`, `Form`, `PUT`) با فرانت موجود سازگار می‌ماند؛ سایر Profileها از الگوی تایپ‌شده Seat استفاده می‌کنند.
5. **یک Engine مشترک**، Surfaceهای `Backoffice`, `Api`, `OtaPanel`، یک Details مشترک برای غیر Seat؛ Optionهای متمایز و Quantity نتیجه انتخاب را تعیین می‌کنند.
6. Shopper بدون `OrderId` حق Shopping دارد؛ Hold تنها پس از ایجاد Order/OrderService توسط Ordering و با `pricedSelectionRef` معتبر.
7. `OfferItemRef` شناسه Candidate است، `pricedSelectionRef` شناسه انتخاب قیمت‌گذاری‌شده؛ هیچ‌یک سند رزرو یا ضمانت ظرفیت نیست.
8. قیمت Free فقط با شاهد تجاری/حق fare معتبر؛ قیمت unknown ≠ صفر؛ «پاسخ غایب» ≠ «ظرفیت آزاد»؛ external Pending ≠ Confirmed.
9. Cancel سرویس ≠ Refund وجه ≠ Void/Refund سند؛ جریمه باید Quote با مستند معتبر باشد و مجدداً اعتبارسنجی شود.
10. **داده production تاریخی وجود ندارد**: ملاحظات حفظ Legacy Database/dual-write/مهاجرت‌های پیچیده حذف؛ اما قرارداد API فعال و صحت مدل فعلی حفظ شود.
11. اتصال FlightFlow که به Push/Deployment دیگری وابسته است، مانع authoring/Shopping closure داخلی نیست؛ تنها با status `[EXTERNAL]` و interface مشخص گزارش شود.
12. ایجنت صرفاً داخل `AeroTech.Ancillary` کد می‌نویسد؛ ایجاد/تغییر قرارداد سرویس دیگر در این Pack فقط به‌صورت پیشنهاد/نیاز integration ثبت می‌شود.

## ترتیب خواندن
`01` ADRها؛ `02` Source audit؛ `03` Domain و Dictionary؛ `04` Authoring routes؛ `05` Provision/Pricing/Inventory؛ `06` 24 Variant؛ `07` Shopping surfaces/options؛ `08` Selection identity؛ `09` SeatMap Freeze؛ `10` Reservation Aggregate؛ `11` S2S Operations؛ `12` Provider/Local؛ `13` Cancellation/Refund؛ `14` Interservice؛ `15` API examples؛ `16` E2E scenario matrix؛ `17` اجرای سه مرحله؛ `18` Benchmark؛ `19` Agent Prompt؛ `20` ChatGPT Continuation؛ `21` Traceability & Decisions؛ `22` تصمیم‌های بسته‌شده Stage 1؛ `23` پرامپت فوری برای Agent.

## قرارداد وضعیت پایان کار
- `S1_CODE_CLOSED` و `S2_CODE_CLOSED` و `S3_CODE_CLOSED` صرفاً در گزارش Agent پس از اثبات تمام رفتارهای پذیرش همان Stage و تأیید مالک اعلام می‌شوند.
- وضعیت integration خارجی `EXTERNAL_PENDING` جداست؛ موفقیت داخلی ساختگی نباید به معنی اتصال واقعی FlightFlow/Ordering/Payment تعبیر شود.
- Pack یک طرح کامل است، نه ادعای تکمیل کد.
