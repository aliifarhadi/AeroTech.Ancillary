# AeroTech Ancillary Master Pack v13.1 — COMPLETE SINGLE AUTHORITY

> این نسخه جایگزین کامل v13.0 و همه Packهای v12.x است. اجرا فقط در سه Stage مجاز است؛ نمونه‌های `[TARGET]` در سورس فعلی موجود فرض نشوند.

**Compiled:** 2026-10-16 · **Chapters:** 22 · **Language:** Persian with exact code/API terms.


---

<!-- BEGIN: 00-START-HERE-AND-SINGLE-AUTHORITY.md -->

# AeroTech Ancillary — Master Pack v13.1 FINAL

**تاریخ:** 2026-10-16  
**وضعیت:** تصمیم‌های طراحی و قراردادها FINAL؛ وضعیت پیاده‌سازی را صرفاً بر اساس شواهد کد می‌توان «بسته» دانست.  
**مرجع یگانه:** این مجموعه فایل‌ها و نسخه یکپارچه آن؛ تمامی Packها و Promptهای v12.x و v13.0 و گزارش‌های پیشین صرفاً پیشینه تاریخی‌اند و هیچ الزام متناقضی از آنها منتقل نمی‌شود.

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
| **S1 — Authoring Closure** | هر ۹ Profile را واقعاً در Backoffice تعریف و مدیریت کنیم | ۹ Definition تخصصی حفظ؛ Seat Provision ثابت؛ ۸ Provision تخصصی اضافه؛ Pricing و Lifecycle مشترک؛ دو Route مشترک `CancellationTerms` با قواعد اختیاری؛ ۲۴ Variant با فیلدهای دقیق | Shopping rewrite، Hold/EMD/Refund، تغییر Seat UI |
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
`01` ADRها؛ `02` Source audit؛ `03` Domain و Dictionary؛ `04` Authoring routes؛ `05` Provision/Pricing/Inventory؛ `06` 24 Variant؛ `07` Shopping surfaces/options؛ `08` Selection identity؛ `09` SeatMap Freeze؛ `10` Reservation Aggregate؛ `11` S2S Operations؛ `12` Provider/Local؛ `13` Cancellation/Refund؛ `14` Interservice؛ `15` API examples؛ `16` E2E scenario matrix؛ `17` اجرای سه مرحله؛ `18` Benchmark؛ `19` Agent Prompt؛ `20` ChatGPT Continuation؛ `21` Traceability & Decisions.

## قرارداد وضعیت پایان کار
- `S1_CODE_CLOSED` و `S2_CODE_CLOSED` و `S3_CODE_CLOSED` صرفاً در گزارش Agent پس از اثبات تمام رفتارهای پذیرش همان Stage و تأیید مالک اعلام می‌شوند.
- وضعیت integration خارجی `EXTERNAL_PENDING` جداست؛ موفقیت داخلی ساختگی نباید به معنی اتصال واقعی FlightFlow/Ordering/Payment تعبیر شود.
- Pack یک طرح کامل است، نه ادعای تکمیل کد.


<!-- END: 00-START-HERE-AND-SINGLE-AUTHORITY.md -->


---

<!-- BEGIN: 01-CLOSED-ARCHITECTURE-ADRS.md -->

# 01 — ADRهای قطعی و مرز Contextها

## ADR-01 — سه مفهوم تجاری مستقل
`AncillaryServiceDefinition`: «چه می‌فروشیم؟» (Profile/Variant، S5/SubCode، EMD/SSR/Specification).  
`AncillaryProvision`: «چه زمانی و به چه کسی، در چه مسیر/پرواز/تاریخی و با چه محدودیت فروشی؟» (S7-like Rules/Eligibility).  
`AncillaryPricing`: «به چه ارز، واحد و ترکیب نرخ/مالیات/هزینه؟» (نسخه فعال و نرخ).

این سه همان Aggregateهای موجودند. برای Meal/Baggage/Pet/Seat Aggregate مجزا یا «ProductConfiguration» عمومی جدید نسازید. Definition قیمت ندارد؛ Provision سطر نرخ ندارد؛ Pricing ظرفیت فیزیکی ندارد.

## ADR-02 — Typed Facade / Shared Core
۹ API/DTO تعریف محصول تخصصی هستند، ولی به یک منطق Domain مشترک می‌رسند. Provision در هر Profile فقط Rules قابل اعمال همان Profile را آشکار می‌کند؛ Pricing، List، Status Changes و Publish مشترک‌اند. حفظ کامل API فعال Seat شرط است.

## ADR-03 — Ancillary و FlightFlow مرز موجودی
Ancillary مالک **قانون تجاری Seat** است؛ FlightFlow/مرجع معتبر پرواز مالک Layout/Occupied/Hold صندلی است؛ SeatMap فعلی یک *Read Composition Facade* در Ancillary می‌ماند و از یک Engine تجاری واحد قیمت می‌گیرد. Presence یک Adapter Read به معنی مالکیت Seat Inventory نیست. ساختار SeatMap تغییر نمی‌کند و استخراج آن به BFF پروژه مستقلی خواهد بود. A09 EXST جایگزین «یک صندلی معمولی» نیست.

## ADR-04 — Shopping یک Engine، سه Surface
`AncillaryShoppingEngine` Canonical Output تولید می‌کند؛ Auth، Commercial SalesScope و DTO Mapping مخصوص `Backoffice`، `Api` و `OtaPanel` است. برای هر Variant Shopping Controller نسازید. NonSeat = `AncillaryOffers`، Seat = `SeatMaps`.

## ADR-05 — انتخاب واقعی دارای هویت مستقل
`offerItemRef` فقط Candidate را پیدا می‌کند. `optionRef` تنها برای گزینه‌های واقعی فهرست‌شده و Scopeدار است. `pricedSelectionRef` فقط برای انتخاب Accepted، FullyPriced و قابل Resume/Resolve صادر می‌شود؛ شامل Qty/Option/Scope/Rev/Currency/Price/Validity. انتخاب متفاوت، رکورد انتخاب متفاوت؛ بدون Hold اتوماتیک.

## ADR-06 — Scope و Beneficiary با PricingUnit متفاوت‌اند
`CoverageScope = Sector|Portion|Journey|Order`; `PricingUnit = PerPassenger|PerRoom|PerItem|PerVehicle|PerSeat|PerPiece|PerKilogram`. Order-level یک Scope است نه PricingUnit جدید. `TravellerId=null` فقط وقتی آن محصول مجاز به `Order` scope است. چند Beneficiary در یک سرویس مشترک باید حقیقتا قرارداد Supplier داشته باشد؛ قیمت یک سرویس مشترک را به‌ازای هر مسافر تکرار نکنید.

## ADR-07 — Source of Truth در پرداخت و سند
Ordering مالک Order/OrderItem/OrderService/OrderTraveller، buyer/seller، history و تعهد مالی است. Payment مالک authorization/capture/refund/wallet/cash/settlement؛ Ticketing/Ordering مالک ETKT/EMD-A/EMD-S/coupons/void/refund. Ancillary فقط Offer و fulfillment/cancellation facts می‌دهد؛ هیچ EMD یا پولی در Ancillary «ریفاند» نمی‌شود.

## ADR-08 — عملیات P3 همان Aggregate موجود
`AncillaryReservation`/`AncillaryReservationUnit` از کد فعلی توسعه می‌یابند؛ Group HoldRef/OrderId/IdempotencyKey و UnitId/OrderServiceId/ProviderRef دارند. Batch نباید همه یا هیچ فرض شود وقتی چند Supplier متفاوت طرف معامله‌اند. Retry و Partial Outcomes حقیقت را حفظ می‌کنند؛ هیچ Distributed Transaction بین سرویس‌ها.

## ADR-09 — Cancel و Refund مستقل + Quote پیش از اقدام
Cancellation Preview/Quote باید penalty/refundability/waiver و source را برای هر Unit بدهد؛ Ordering با EMD/Payment نتیجه مالی نهایی را ترکیب می‌کند. Quote نامعتبر یا نامعلوم => عملیات خودکار مالی ممنوع؛ manual review. Stale Quote => درخواست تأیید مجدد. لغو Supplier بدون امکان جبران => نباید خودکار به موفقیت مالی تعبیر شود.

## ADR-10 — Supplier capability به‌جای یک workflow اجباری
`LocalFinite`، `UnlimitedExplicit`، `ExternalHoldConfirm`، `ExternalDirectConfirm`، `ManualFulfillment` و `CommercialOnlySeat` حالت‌های واقعی اجرای واحد هستند؛ نه Aggregateهای جدا. Strategy/adapter مناسب از trusted Supplier، Capability و Provision انتخاب شود. Supplier خارجی بدون ثبت رسمی ProviderKey نباید Confirmed تلقی شود.

## ADR-11 — اثر قیمت، Fare inclusion و شرایط فروش
Fare benefits از AirAvail/Order Context وارد Eligibility می‌شوند؛ خدمات شامل در بلیت دوباره فروخته نمی‌شوند. قیمت `Free` فقط منبع معتبر `PriceOrigin.Free` یا حق مشخص Fare دارد؛ نبود نرخ/FX/وزن قابل محاسبه => not fully priced. Tax (code/country/station)، markup/commission/fee line از هم تفکیک می‌شوند؛ Pricing Unit و Application Unit متفاوت می‌مانند.

## ADR-12 — شناسه و امنیت
`AirOfferId`, `offerItemRef`, `optionRef`, `pricedSelectionRef`, `OrderId/OrderServiceId`, `HoldId/UnitId`, `ProviderUnitRef`, `EMD` هر یک مالک جدا دارند. Authz فروش و POS از Caller و Reference معتبر، نه Body دلخواه آژانس. رفرنس‌های Supplier/دفتر داخلی به بیرون leak نمی‌شوند. Cross-Surface/Cross-POS Replay مجاز نیست مگر اصالت دوباره بررسی شود.

## ADR-13 — دو حالت ورودی Shopping
قبل Order: تنها `priced AirOfferId` و commercial context معتبر AirAvail. بعد Order: Snapshot تجاری/مسافر/پرواز معتبر Ordering؛ صرفاً کپی Offer قدیمی یا حدس از PNR نیست. یک Engine هر دو را می‌پذیرد. توسعه ورودی PostOrder به شکل additive و با Auth مجاز است؛ Response فعلی SeatMap تغییر نمی‌کند.

## ADR-14 — Change با Delta، نه تغییر تاریخچه
Add/Remove/Replace محصول یا انتخاب پس از صدور بلیت، `OrderChange`/Operation جدید در Ordering می‌خواهد؛ Ancillary فقط Units متاثر را hold/confirm/cancel می‌کند. Original sale و pricing/doc history immutable می‌مانند. اگر تفاوت قیمت است، Ordering با Payment تسویه می‌کند.

## ADR-15 — تصمیم درباره پیچیدگی
همه قواعد در **همین ۲۴ Variant** بیان می‌شوند. Rule/Enum/API/Database Entity تازه تنها وقتی مجاز است که سناریوی واقعی قابل بیان با مدل فعلی نباشد و در جدول ADR توجیه شود. Optionهای ترکیبی `SelectionKind` تایپ‌شده‌اند، نه انبوه endpoint. Split group صرفاً وقتی نیاز واقعی Ordering اثبات شد؛ اجباری نیست.

## ADR-16 — وضعیت خارجی‌ها مانع مرحله داخلی نیست
قرارداد `OccupiedSeats` در FlightFlow فعلی منتشر شده، ولی وضعیت استقرار و سایر تغییرات FlightFlow توسط Owner خارج از gate Ancillary تعیین می‌شود. از روی API Source قول عملکرد Runtime یا تأیید end-to-end نمی‌دهیم. Ordering repo برای بررسی فعلی قابل دسترسی نبود؛ مسیرهای Target آن عمداً علامت `[EXTERNAL]` دارند.


<!-- END: 01-CLOSED-ARCHITECTURE-ADRS.md -->


---

<!-- BEGIN: 02-SOURCE-VERIFIED-BASELINE-AND-DELTAS.md -->

# 02 — Ledger سورس: چه چیزی هست و چه چیزی نیست؟

**تمام CURRENTهای این فصل بر مبنای GitHub SHAهای فصل 00 هستند؛ FINAL بودن Pack به معنی Deploy بودن Targets نیست.**

## Ancillary — Current
| بخش | سورس بررسی‌شده | وضعیت |
|---|---|---|
| ۹ Definition تخصصی POST/PUT/GET | `src/AeroTech.Ancillary.RestApi/V1/AncillaryServiceDefinitionAggregate/Controllers/BackofficeController.cs` | `[CURRENT]` |
| Seat typed Provision create/edit | `src/AeroTech.Ancillary.RestApi/V1/AncillaryProvisionAggregate/Controllers/SeatBackofficeController.cs` | `[CURRENT]`، سازگار با فرم عملیاتی Seat |
| Non-Seat Provision | `.../AncillaryProvisionAggregate/Controllers/BackofficeController.cs` | `[CURRENT]` Generic؛ ۸ typed facade `[TARGET]` |
| Pricing + Publish/Switch | `.../AncillaryPricingAggregate/Controllers/BackofficeController.cs` و `.../AncillaryProvisionAggregate/Controllers/BackofficeController.cs` | `[CURRENT]` مشترک |
| InventoryPolicy | `.../AncillaryInventoryPolicyAggregate/Controllers/BackofficeController.cs` | `[CURRENT]` برای non-Seat |
| ۲۴ Variant | `src/AeroTech.Ancillary.Domain/AncillaryServiceDefinitionAggregate/AncillaryVariant.cs` | `[CURRENT]` A01..A24، ۹ profile |
| Canonical Shopping | `src/AeroTech.Ancillary.Shopping/Engine/AncillaryShoppingEngine.cs` | `[CURRENT]` |
| ۳ سطح Ancillary Offers / Details / Evaluate | `src/AeroTech.Ancillary.RestApi/V1/AncillaryOffers/Controllers/{Backoffice,Ota,OtaPanel}Controller.cs` | `[CURRENT]` |
| ۳ سطح SeatMap مجزا | `src/AeroTech.Ancillary.RestApi/V1/SeatMaps/Controllers/{Backoffice,Ota,OtaPanel}Controller.cs` | `[CURRENT]` |
| قرارداد SeatMap | `src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Dto/SeatMapsDto.cs` | `[CURRENT]`، Frozen |
| شناسه Candidate/Seat | `src/AeroTech.Ancillary.Query/Shopping/Common/OfferItemRef.cs` | `[CURRENT]` SHA-256 locator؛ Quantity/options در آن نیست |
| `Details` | `.../Query/Shopping/AncillaryOffers/Queries/GetAncillaryOfferDetail/GetAncillaryOfferDetailService.cs` | `[CURRENT]` Spec/Rules/Rates؛ لیست واقعی Optionهای dynamic/price incomplete |
| `Selections/Evaluate` | `.../Query/Shopping/AncillaryOffers/Queries/EvaluateSelection/EvaluateSelectionService.cs` | `[CURRENT]` evaluation؛ `pricedSelectionRef` مستقل ندارد |
| Hold/GET/Confirm | `.../RestApi/V1/AncillaryReservationAggregate/Controllers/ServiceController.cs` | `[CURRENT]` skeleton؛ بدون selection ref/options، Release/Cancel/Quote ندارد |
| Hold Domain | `.../Application/AncillaryReservationAggregate/Commands/HoldAncillaryServices/HoldAncillaryServicesService.cs`; `.../Domain/AncillaryReservationAggregate/AncillaryReservation.cs` | `[CURRENT]` به شکل ساده گروهی؛ External Supplier را رد می‌کند |

## نقص‌های مستند (TARGET)
1. Provisionهای غیر Seat ورودی بزرگ و عمومی دارند؛ فرم‌های تایپ‌شده مانند Seat ندارند.
2. `offerItemRef` مخصوص Candidate است. تغییر A01 از Quantity 1 به 2 ممکن است همان ref را حفظ کند؛ رفرنس انتخاب نهایی مستقل و تضمین‌شده نداریم.
3. `Details` بازتاب کاتالوگ منتشرشده است، نه لزوماً تمام optionهای واقعی بر اساس Source؛ نباید از `MenuItemRef` تنها، وجود گزینه قابل خرید نتیجه گرفت.
4. در SeatMap Mapper، `PriceAssessment=null` به صفر تبدیل می‌شود؛ نبود Candidate نیز می‌تواند `AVAILABLE` بسازد. باید بدون تغییر DTO، فقط وقتی فروش مجاز و Free اثبات شده، رایگان اعلام شود.
5. پاسخ ناقص OccupiedSeats برای یکی از FlightIdها ممکن است به empty set تعبیر شود؛ Fail closed.
6. AirAvail `ServiceFlightOfferDetailDto` دارای `PricedAt`, `LastTicketingDate`, FlightCapacityId و جزئیات پرواز/مسافر است، **فیلد `ValidUntilUtc` ندارد**؛ local TTL باید جدا از ضمانت واقعی producer باشد.
7. Holder فعلی انتخاب قیمت/ProviderRefs واقعی را رزرو نمی‌کند؛ `AncillaryHoldChecks.Accept` External Supplier را رد می‌کند؛ Release/Extend/Cancel/RefundQuote هنوز طراحی نشده در کد.
8. `AncillaryReservation.Hold` فعلی برخورد چند Unit با Definition+Scope مشابه را رد می‌کند؛ انتخاب متفاوت برای یک محصول واقعی باید قابل قبول باشد، ولی **double-book یک selection واحد** ممنوع بماند.

## AirAvail — Contract حقیقی
`POST /Service/v1/FlightOffers/Details` با `{offerId}` و DTO `ServiceFlightOfferDetailDto` از مسیر
`AeroTech.AirAvail/AirOffer/src/AeroTech.AirOffer.RestApi/V1/AirOffers/Controllers/Service2Service/ServiceController.cs`.  
`Services/Services/Service2Service/ServiceFlightOfferDetailService.cs` Offer priced را validate و دوباره price می‌کند. DTO دارای `OfferId, PricedAt, LastTicketingDate, CurrencyId, CurrencyCode, AirTransports[BoundId,BoundOfferId,Flights[FlightId,FlightCapacityId,FlightVersion,AircraftId,CabinClassId,RbdId,OperatingAirlineId]], Tickets[TravellerRef,Coupons[...]], PricingUnits[FareBasis,FareFamily], RatesOfExchange` است. `FlightId != FlightCapacityId`. لازم نیست برای این اطلاعات endpoint جدید در AirAvail بسازیم.

## FlightFlow — Contract قابل تطبیق، بدون gate استقرار
در SHA `962f4188`، `Service/v1/Flights` دارای `POST OccupiedSeats` است؛ همچنین Seat-Holds/Confirmations/Release/Extend/Cancellation و Hold group ref با per-seat `SeatHoldReference` را ارائه می‌کند. این یک **الگوی طراحی عملیات گروهی Ancillary** است. فرایندهایی که Owner هنوز Push/Deploy نکرده جزو Closure تست Ancillary نیستند.

## Ordering — وضعیت شاهد
Repository `AeroTech.Ordering.Final` در اتصال GitHub این نوبت `404` برگرداند. پس هیچ مسیر REST از Ordering با برچسب CURRENT در Pack وجود ندارد. Interfaceهای عمومی Ordering و S2S **Proposed integration contract** هستند. تا دسترسی به سورس و تطبیق contract، ادعای End-to-End deployed مردود است.

## نحوه استفاده در Agent
وقتی Agent شروع می‌کند، SHA جدید را ثبت می‌کند؛ فقط Delataهای همین فصل را هدف می‌گیرد؛ کد درست فعلی را از نو نمی‌سازد و اجازه ندارد به استناد سطرهای `[CURRENT]`، مرحله ناتمام را بسته اعلام کند.


<!-- END: 02-SOURCE-VERIFIED-BASELINE-AND-DELTAS.md -->


---

<!-- BEGIN: 03-DOMAIN-TYPES-AND-FIELD-DICTIONARY.md -->

# 03 — مدل دامنه، Aggregate و Dictionary فیلدهای مهم

## 3.1 Aggregateها و مسئولیت
| Domain object | وظیفه | مجاز نیست |
|---|---|---|
| `AncillaryServiceDefinition` | Owner airline, stable reference, revision, Profile/Variant, specification, booking/document instructions, statuses | Price/Inventory/FlightSeatHold |
| `AncillaryProvision` | eligibility, passenger/flight/fare/geo/date/time/POS, quantity, coverage, fulfillment/settlement/outcome | قیمت نسخه‌بندی‌شده، ETKT/EMD صادرشده |
| `AncillaryPricing` | Active rate version, currency, fare/passenger rate, tax/fee/FX evidence | انتخاب شماره صندلی یا تغییر Stock |
| `AncillaryInventoryPolicy` | authority/count/weight/slot rules برای non-Seat | هیچ Seat Policy برای A07..A09 |
| `FlightCountInventory` / `FlightWeightInventory` / `AirportSlotInventory` | موجودی واقعی Local به‌تناسب الگوی موجود | رقم فرضی در غیاب Source |
| `Supplier` | local/external identity, fulfillmentProviderKey, verified capability | mark confirmation بدون evidence |
| **`PricedSelectionSnapshot` [TARGET]** | هویت انتخاب کوتاه‌عمر و نتیجه قیمت‌گذاری قابل Resolve | Reserve/Order/Payment aggregate جدید؛ Price بدون TTL |
| `AncillaryReservation` / `AncillaryReservationUnit` | گروه Hold و عملیات واحدها + ProviderRefs/Status، قابلیت عملیات جزئی | خود Order/Payment/EMD/Seat Inventory |

**ساده‌سازی:** `PricedSelectionSnapshot` یک snapshot کوتاه‌عمر قابل Lookup است، نه Domain Aggregate با Lifecycle تجاری مستقل؛ از duplication منطق قیمت جلوگیری می‌کند.

## 3.2 Definition fields [CURRENT]
| Field | Type / nullable | معنا |
|---|---|---|
| `Id` | `long` | نسخه دیتابیسی Definition؛ مشتق از مرجع پایدار |
| `OwnerAirlineId` | `int` required | ایرلاین مالک/tenant business (نه یک TenantId ساختگی) |
| `SupplierId` | `long` required | تأمین‌کننده صاحب Fulfillment |
| `ServiceDefinitionRef` | `string` required, stable | هویت تجاری پایدار across revisions |
| `ServiceSubCode` | `string` required | Optional Service Sub Code صنعت/ایرلاین |
| `SubCodeSource` | enum | IndustryDefined/CarrierDefined مطابق enum فعلی |
| `ServiceTypeCode`, `GroupCode`, `SubGroupCode`, `Description1Code`, `Description2Code` | `string?` | ساختار ATPCO-like؛ domain rule اعتبار را تعیین می‌کند |
| `PricingUnit` | enum required | واحد پایه محاسبه، نه Scope |
| `ServiceDateBasis` | enum required | مبنای تاریخ/پرواز یا ServiceStart |
| `VariantCode` | `string` required | A01..A24، تایید registry |
| `DocumentRouting` | enum | EMD/TicketOrExchange/NoDocument طبق واقعیات Variant |
| `Specification` | `TProfile` required | subobject فقط مخصوص Profile/Variant |
| `CommercialName`, `Description` | `string`, `string?` | نمایش محصول |
| `Document` | `ServiceDefinitionDocumentInput` | نوع سند و `RFIC/RFISC` اگر لازم |
| `Booking` | `ServiceDefinitionBookingInput` | `BookingMethod`, `SSR/SSIM`, confirmationRequirement |
| `SalesEffectiveFrom`, `SalesDiscontinueOn` | `DateOnly?` | زمان اعتبار فروش Definition |
| `Status`, `Version`, effective dates | Types فعلی | وضعیت و نسخه‌بندی |

از `GET /.../{Profile}/{id}` ورودی `PUT` قابل round-trip باشد؛ `OwnerAirlineId` و `ServiceDefinitionRef` فقط در Create دیده می‌شوند؛ در Change نباید تغییر هویت پذیرفته شود.

## 3.3 Provision fields [CURRENT], Typed Facades [TARGET]
| Field | Type | قاعده |
|---|---|---|
| `ServiceDefinitionId` | `long` route-derived در Typed API | مالکیت Profile/Definition باید ثابت شود |
| `Sequence` | `int` | اولویت/ترتیب Rule |
| `CoverageScope` | `ServiceCoverageScope` | Sector/Portion/Journey/Order |
| `PurchaseStage` | `PurchaseStage` | PreOrder/PostTicketed/Both/OnBoard؛ نه همه برای هر Provider |
| `PriceOrigin` | `PriceOrigin` | Filed/ExternalQuote/Free/NotAvailable |
| `QuoteProviderKey` | `string?` | فقط وقتی مستند به provider quote است |
| `Quantity` | `ProvisionQuantityInput` | min/max/unit، قواعد Quantity و استثناهای variant |
| `Outcome`, `Settlement` | Typed existing | service disposition، refund/commissionable/form of refund طبق قرارداد جاری |
| `Availability`, `Fulfillment` | Typed existing | نیاز/روش اعتبارسنجی، بدون فرض capacity |
| `PassengerEligibility` | `...?` | age/PTC/eligibility |
| `SalesRestrictions` | `...?` | POS/کانال/time؛ POS null/empty = unrestricted بر اساس rule فعلی |
| `Geography` | `...?` | origin/destination/airport/country |
| `FlightApplication`, `FareApplication` | `...?` | operating carrier, flight, fare family, cabin/RBD |
| `TravelDate`, `DayTimeApplication`, `AdvancePurchase` | `...?` | Seasonality، blackout، local time/lead time |
| `BaggageApplication` | only Baggage | piece/weight/allowance logic |
| `SeatApplication` | only Seat | seat numbers/characteristics/aircraft filtering |
| `PetRule` | only Pet | quota/size/eligibility |
| `AssistedTravelRule` | only AssistedTravel | medical/guardian/SSR rules |
| `AirportServiceRule` | only AirportService | facility/window/capacity restriction |

هیچ فیلد مخصوص Profile دیگر را از مسیر Typed قبول نکنید. برای Upgrade/Meal/Priority/Connectivity درصورت کافی‌بودن فیلدهای Common، Rule اختصاصی تهی اضافه نشود.

## 3.4 Pricing fields [CURRENT]
`AncillaryPricing(AncillaryProvisionId:long, Rates:list, Version/Status)`؛ `Rate` شامل `PassengerTypeCode`, `AgeFromInclusive`, `AgeToExclusive`, `CurrencyId`, `BaseAmount`, components` است. `PriceAssessment` فعلی شامل:
- `Origin:PriceOrigin`, `Status:PriceAssessmentStatus`؛ `CurrencyId:int?`, `FiledCurrencyId:int?`, `RateOfExchange?`;
- `PricingRevisionId:long?`, `PricingRateId:long?`, `PricingUnit:enum?`;
- `BaseAmount:decimal?`, `CompleteUnitTotal:decimal?`, `RequestedQuantityTotal:decimal?`;
- `AddedTaxLines`, `IncludedTaxLines`, `AppliedUnitFeeLines`, `UnappliedFeeLines` با `Category`, `Code`, `CountryId`, `StationAirportId`, `Amount`, `CurrencyId`, `TaxTreatment`, `FeeApplicationUnit`;
- `IsOrderLevelTotalComplete:bool`, `QuoteProviderKey?`, `ExternalQuoteRef?`, `QuoteExpiresAtUtc?`, `ReasonCodes[]`.

**الزام:** مرز قیمت واحد، مبلغ Qty، surcharge/fee Order و مالیات را حفظ کن؛ برای PerKilogram وزن معلوم نباشد، Total=null و نه صفر؛ ارز ISO/DecimalPlaces و RateOfExchange با دقت منابع معتبر، no early rounding. یک بار Order-level charge را به ازای هر مسافر/Unit تکرار نکنید.

## 3.5 Shopping context [CURRENT + TARGET]
`AirOfferId` priced + `SourceKind`, `SourceVersion`, `PricedAt`, local `ValidUntilUtc`, AirTransports (BoundOfferId, FlightId/FlightCapacityId, Cabin, RBD, Aircraft, OperatingAirline), Tickets(TravellerRef/PTC, Coupons, baggage benefit), FareFamily/FareBasis, POS/Channel/Country/Currency, PurchaseStage, date/time, references. `OrderId` فقط برای PostOrder Context. 

**مطابقت هواپیما/مسافر:** `FlightId`, `FlightCapacityId`, `BoundId`, `FlightVersion` معادل یکدیگر نیستند. `TravellerRef` AirAvail باید به `OrderTravellerId` توسط Ordering map شود. SeatNum تنها در SeatMap/Selection/physical evidence است، نه Definition.

## 3.6 Identifiers [CURRENT + TARGET]
| Id | Type | Owner | معنی |
|---|---|---|---|
| `ServiceDefinitionRef` | string | Ancillary | هویت ثابت محصول |
| `DefinitionVersionId`, `ProvisionId`, `PricingRevisionId` | long | Ancillary | شواهد rule/price |
| `AirOfferId` | string | AirAvail | Context معتبر priced قبل Order |
| `offerItemRef` | opaque string | Ancillary | Candidate در Shopping، Qty در آن نیست |
| `optionRef` | opaque string? | Ancillary/Trusted Provider | گزینه حقیقی در context |
| `pricedSelectionRef` | opaque string | Ancillary | Accepted selection + pricing + TTL + Scope |
| `SeatMaps.travelerPricing[].id` | opaque string? | Ancillary facade | SeatMap locator، بدون تغییر قرارداد |
| `OrderId`, `OrderServiceId`, `OrderTravellerId` | long | Ordering | هویت فروش و beneficiary |
| `HoldId`, `UnitId` | long | Ancillary | گروه و هر خرید عملیاتی |
| `ProviderUnitRef` | string? | Supplier | شناسه عملیاتی provider |
| `FlightFlow.HoldId`, `SeatHoldReference` | string | FlightFlow | رزرو واقعی صندلی، مستقل از Ancillary |
| `EMDNumber`, `CouponId`, `PaymentRef`, `RefundRef` | owner-defined | Ordering/Ticketing/Payment | اسناد و تسویه؛ خارج از Ancillary |

شناسه‌های vendor نباید جایگزین شناسه‌های OrderService شوند. هر بار صرفا یک نوع identifier را به «id» بدون توضیح نام ندهید.


<!-- END: 03-DOMAIN-TYPES-AND-FIELD-DICTIONARY.md -->


---

<!-- BEGIN: 04-S1-BACKOFFICE-EXACT-API.md -->

# 04 — Stage 1: API دقیق Backoffice و Typed Authoring

**هدف:** هر Variant بدون فیلد نامربوط در فرم خودش تعریف، ویرایش، منتشر و قیمت‌گذاری شود؛ APIهای موجود Seat و Definition بدون Breaking Change بمانند.

## 4.1 Definition — همه مسیرها [CURRENT]
Prefix: `Backoffice/v1/AncillaryServiceDefinitions`

| عملیات | Method / Path | دامنه رفتار |
|---|---|---|
| لیست همه | `GET /Paginated?profile={Profile?}&...` | به تفکیک Owner/Status، Pagination مشترک |
| Metadata واقعی Variantها | `GET /Variants` | A01..A24، Profile، فیلدهای انتخاب و allowable Pricing Units |
| تعریف اختصاصی | `POST /{Profile}` | فقط `TProfileSpecification` |
| خواندن فرم اختصاصی | `GET /{Profile}/{serviceDefinitionId}` | `Form` با همان فیلدهای PUT |
| ویرایش اختصاصی | `PUT /{Profile}/{serviceDefinitionId}` | فقط Draft؛ هویت Owner/StableRef غیرقابل تغییر |
| خواندن عمومی | `GET /{serviceDefinitionId}` | اطلاعات نمایشی/مدیریتی |
| چرخه عمر | `POST /{id}/Activate|Suspend|Reactivate|Retire|Revise` | چرخه مشترک موجود |
| تغییر مجاز Unit/Date Basis | `POST /{id}/AssignPricingUnit`, `POST /{id}/AssignServiceDateBasis` | فقط قواعد مجاز Variant، طبق مدل جاری |

`{Profile}`: **`Baggage`, `Seat`, `Upgrade`, `Meal`, `Pet`, `AssistedTravel`, `AirportService`, `Priority`, `Connectivity`**. اسم‌ها مطابق کد هستند؛ `/Ancillaries/Seat` یا ۲۴ Controller مستقل نسازید. `Backoffice/v1/AncillaryServiceDefinitions/Seat` و DTOهای آن **فریز هستند**.

## 4.2 Provision — الگوی یکسان Typed Facade، نه منطق جدا
Prefix: `Backoffice/v1/AncillaryServiceDefinitions/{Profile}/{serviceDefinitionId}`

| Method / Path | موجود/جدید | خروجی |
|---|---|---|
| `GET /ProvisionForm` | `[CURRENT] Seat`; `[TARGET]` هشت دیگر | metadata form مجاز/constraints/variant و field visibility |
| `POST /Provisions` | `[CURRENT] Seat`; `[TARGET]` هشت دیگر | Provision draft، Id/Status/DefinitionId |
| `GET /Provisions/{provisionId}/Form` | `[CURRENT] Seat`; `[TARGET]` هشت دیگر | مقدار قابل ویرایش با همان بدنه PUT |
| `PUT /Provisions/{provisionId}` | `[CURRENT] Seat`; `[TARGET]` هشت دیگر | Update Draft با guard مالکیت |

**فهرست و Lifecycle مشترک فعلی** زیر `Backoffice/v1/AncillaryProvisions` می‌مانند:
`GET /Paginated?serviceDefinitionId=...`, `GET /{id}`, `GET /{id}/Form`, `GET /{id}/Pricings`, `POST /{id}/Activate`, `POST /{id}/Publish`, `POST /{id}/SwitchActivePricing`, `POST /{id}/Suspend`, `POST /{id}/Reactivate`, `POST /{id}/Retire`.

Definition ownership: Wrong Profile Id = **409/16219**؛ Provision متعلق به Definition دیگر = **404/16301** (همان کد موجود). فیلد غیرمجاز برای Profile به‌صورت typed rejected؛ هیچ کد جدیدی بدون ضرورت اختراع نشود.

## 4.3 Pricing — صددرصد مشترک [CURRENT]
Prefix: `Backoffice/v1/AncillaryPricings`

`POST /`, `PUT /{pricingId}`, `GET /{pricingId}`, `GET /Paginated`, `POST /{pricingId}/Activate`, `POST /{pricingId}/Suspend`, `POST /{pricingId}/Reactivate`, `POST /{pricingId}/Retire`, `POST /{pricingId}/Revise`.

**اصل:** یک قیمت فعال/نسخه revision مطابق rule موجود؛ نرخ/fee/tax/FX مستقل از مشخصات Profile. در پایان، `Provision.Publish` به Pricing معتبر لینک می‌شود؛ SwitchActivePricing تغییر قیمت آتی است و quote قبلی باید زمان Hold دوباره اعتبارسنجی شود.

## 4.4 CancellationTerms مشترک و اختیاری [TARGET]
`GET /Backoffice/v1/AncillaryProvisions/{provisionId}/CancellationTerms` و `PUT /Backoffice/v1/AncillaryProvisions/{provisionId}/CancellationTerms` برای ثبت Ruleهای filed penalty/waiver در **Draft Provision**، بدون تغییر DTO فرم Seat؛ `Settlement.ReissueRefund` به‌تنهایی مبلغ جریمه نیست. Ruleها زیر ValueObject Provision می‌مانند، نه Aggregate جدید. در P3 فقط مصرف/Quote می‌شوند؛ جزئیات فصل 13 و 21.

## 4.5 Inventory — فقط در موارد دارای Inventory واقعی [CURRENT]
Prefix: `Backoffice/v1/AncillaryInventoryPolicies`; Define/Change/Activate/Suspend/Retire/ByServiceIdentity/Paginated. خانواده `FlightCountInventory`, `FlightWeightInventory`, `AirportSlotInventory` خودشان endpoint مدیریتی فعلی دارند. **هیچ Seat InventoryPolicy Tab یا الزام Policy برای Seat ایجاد نکنید.**

## 4.6 تفاوت Requestهای تخصصی — فقط بخش مناسب
| Profile | فیلدهای اختصاصی Provision که فرم می‌بیند | فیلدهای نامربوط که نباید بپذیرد |
|---|---|---|
| Baggage | `BaggageApplication` + Shared eligibility | SeatApplication/PetRule |
| Seat | `SeatApplication` + Shared؛ **بدون تغییر نسبت به فرم موجود** | BaggageApplication، InventoryPolicy |
| Upgrade | Shared Fare/Flight + Definition.Upgrade | Seat/Pet/Assisted Rules |
| Meal | Shared AdvancePurchase/Passenger/Fare + Definition.Meal | Pet/Seat/Baggage Rule |
| Pet | `PetRule` + Shared | Seat/Assisted/Airport Rule |
| AssistedTravel | `AssistedTravelRule` + Shared | Pet/Seat/Baggage Rule |
| AirportService | `AirportServiceRule` + Shared | Seat/Pet/Baggage Rule |
| Priority | Shared Flight/Fare/Passenger | تخصصی تهی و بی‌دلیل |
| Connectivity | Shared Flight/Fare/Date | تخصصی تهی و بی‌دلیل |

Profile-specific specialization در `Specification` و در صورت وجود rule-specific section است؛ از اضافه‌کردن Rule دسته‌ای فقط برای زیبایی فرم خودداری کنید. API عمومی Generic Provision تا زمانی که کد داخلی مصرف دارد باقی می‌ماند ولی UI Backoffice از typed استفاده می‌کند؛ حذف آن تغییر جداگانه می‌خواهد.

## 4.7 نمونه سلسله‌مراتب یک محصول
1. `POST .../AncillaryServiceDefinitions/Meal`: ایجاد Paid Meal A12، تعریف `MenuItemRef`, lead time، Supplier.
2. `GET .../Meal/{id}/ProvisionForm`: فقط Field/Ruleهای قابل اعمال A12.
3. `POST .../Meal/{id}/Provisions`: Fare/Passenger/POS/Flight/date/Qty rule.
4. `POST /Backoffice/v1/AncillaryPricings`: Rate/Tax/Fee.
5. `POST /Backoffice/v1/AncillaryProvisions/{provisionId}/Publish`: فعال‌شدن فروش طبق Lifecycle.
6. `GET /.../Paginated`, `/.../Pricings`, `/.../Form`: نمایش دقیق اطلاعات بدون فرم General All-In-One.

## خروجی Stage 1
Definition همه ۹ Profile و ۲۴ Variant، Provision تایپ‌شده برای هر Profile، Pricing مشترک، `CancellationTerms` اختیاری و publish/lifecycle کامل؛ قرارداد Backoffice Seat فعلی بدون کوچک‌ترین تغییر و بدون الزام اتصال FlightFlow. **S1 در این Pack طراحی‌شده، نه لزوماً در کد بسته‌شده**.


<!-- END: 04-S1-BACKOFFICE-EXACT-API.md -->


---

<!-- BEGIN: 05-S1-PROVISIONS-PRICING-STOCK-SUPPLIERS.md -->

# 05 — قواعد تجاری Stage 1 و مرز Stock / Supplier

## Rule publication and eligibility
- Definition باید Active باشد؛ Provision باید فعال/منتشرشده و برای Order/Fare/Passenger/Route/POS/Date قابل اعمال باشد؛ Pricing نسخه فعال/قابل محاسبه داشته باشد مگر `PriceOrigin.Free` با evidence. ترکیب ناسازگار نباید Shopping Offer قابل خرید بدهد.
- Null/empty `SalesRestrictions.PointsOfSale` یعنی unrestricted طبق تصمیم commit فعلی؛ مقادیر غیرخالی دقیقاً با trusted eligible POS match شوند. این به معنی دادن POS دلخواه به Agency نیست.
- `TravelDate` با permitted/blackout date periods، `DayTime` با local time/Timezone، `AdvancePurchase` با lead time، `FlightApplication` با operating marketing facts، `FareApplication` با FareBasis/FareFamily/RBD/cabin/benefits، `PassengerEligibility` با PTC/age/other approved facts.
- برای موارد مشمول محدودیت seat exit-row یا PET/UMNR documents، Eligibility = Blocked/NeedsVerification تا fact واقعی فراهم شود؛ راه میانبرِ تیک «قبول قوانین» جایگزین تایید واقعی eligibility نیست.

## نرخ و Quantity
- `PricingUnit` تعریف می‌کند واحد پول چیست؛ `CoverageScope` تعیین می‌کند آن واحد به کدام پرواز/مسافر/سفارش مربوط است؛ `AncillaryQuantityUnit` تعریف Quantity را مشخص می‌کند. PerPiece ≠ PerKilogram ≠ PerPassenger؛ مقدار معادل بسته وزنی PerItem ممکن است متفاوت باشد.
- `CompleteUnitTotal` شامل نرخ پایه و tax/fee applicable (بدون Feeهای خارج از واحد)؛ `RequestedQuantityTotal` فقط پس از Quantity واقعی و verified، با Tax/Fee Apply درست. Order level fee فقط یک بار در Order جمع می‌شود، نه روی هر Pax/Leg.
- Currency code/decimal places/FX reference باید مشخص باشد. کد Tax، کشور و ایستگاه را در صورت موجود بودن نگه دارید. همواره همان published price revision و applied rate/FX را در `PricedSelectionSnapshot` ضبط کنید.
- `PriceOrigin=ExternalQuote`: quote معتبر ProviderKey/QuoteRef و TTL لازم؛ `Free`: فقط declaration و eligibility معتبر؛ `NotAvailable`: هرگز خودکار 0؛ `Filed`: rate منتشرشده، currency دقیق.
- Commissionable flag، form-of-refund و settlement rule در Ancillary metadata است. مبلغ واقعی Agency commission، Agent fee، Markup/Discount و Payment fee با ترکیب سياست فروش و تعهد مالی در Ordering/Payment تسویه می‌شود. قیمت نمایش‌داده‌شده باید fee application را دقیق و از accounting جدا نگه دارد.

## Stock authority matrix
| Source/Policy | Shopping status | Hold Stage 3 | اصل |
|---|---|---|---|
| Seat (A07/08/09) | `NotApplicable` به StockPolicy داخلی | Ancillary فقط commercial؛ Seat physical در FlightFlow | هیچ ظرفیت/Policy Seat داخلی |
| Local finite Count | Count bounded و verified | unit quantity decrement/hold با منبع معتبر | یک واحد ظرفیت دوبار Hold نشود |
| Local Weight | وزن واقعی/صحت واحد کیلوگرم | وزن Hold/Release با conservation | PerKilogram price با وزن واقعی مرتبط |
| Local AirportSlot | slot/window facility verified | تخصیص interval/count واقعی | تداخل زمانی و Zone مشخص |
| Explicit Unlimited | Unlimited فقط وقتی Policy **فعّال و صریح** | no local consumption، commercial hold | غیاب Policy Unlimited نیست |
| Supplier delegated | availability unknown/needs verification مگر source authoritative | Supplier API/manual capability | response نامعلوم sold-out یا free نیست |
| Missing local configuration | `NotConfigured`/`NeedsVerification` | Hold reject/pending بر اساس rule | ساختگی Count=∞ ممنوع |

`InventoryAuthority` جاری `Unlimited=1`, `Local=2`, `Supplier=3`. نه `FlightFlow=4` و نه renumber. `InventoryCapacityReadState.NotApplicable=8` برای Seat مجاز است. `FlightCountInventory` و `FlightWeightInventory` بی‌دلیل حذف نشوند.

## Filed cancellation rule در مرحله S1
`Settlement.ReissueRefund` فقط Refundability را می‌گوید. برای تعیین جریمه داخل Carrier **در Stage 1** فهرست اختیاری `CancellationRules` در ValueObject Provision و دو Endpoint مشترک `GET/PUT /Backoffice/v1/AncillaryProvisions/{id}/CancellationTerms` ایجاد شود؛ Seat Form موجود بدون تغییر بماند. `FixedAmount`, `Percentage`, `ProviderQuoted`, `ManualReview` و RuleRef/Source در فصل 13 و enumهای فصل 21 هستند. این توسعه به‌معنی Refund مالی داخل Ancillary نیست. برای Supplier با quote، مبلغ فایل نشود. بدون Rule/Provider معتبر در Stage 3 باید ManualReview شود.

## Supplier capability (TARGET metadata on verified provider setup)
| Capability | شرح | Rule |
|---|---|---|
| `HoldConfirm` | provider قابلیت reserve/confirm جدا دارد | پیش از Hold موفق، provider hold ref را ذخیره کن |
| `DirectConfirm` | provider فقط خرید/تأیید مستقیم دارد | در مرحله Hold نتیجه «PendingCommit»/pending به caller؛ پس از verified payment orchestration commit؛ هرگز fake Held |
| `ManualConfirmation` | تماس/manual/supplier panel | task + Pending + evidence؛ Confirm تنها پس از evidence |
| `NoBookingProcessRequired` | entitlement/خدمت بدون عملیات supplier | Commercial-only record و وضعیت شفاف |

از `SupplierFulfillmentKind.Local=1 / External=2` موجود استفاده کنید؛ Capability زیرمجموعه مشخصات Provider است، نه «SupplierKind» موازی. Capability صرفاً وقتی adapter واقعی یا قرارداد business تایید شود فعال گردد. Unknown Provider ⇒ blocked/error, not success.

## Child/family, agency and supplier constraints
- OwnerAirlineId، SupplierId و FulfillmentProviderKey trusted و تغییرناپذیر پس از Hold؛ cross-airline provisioning رد شود.
- Included baggage allowance در AirAvail coupon داده می‌شود؛ Extra checked bags و overweight باید با همان allowance و خریدهای موجود reconcile شوند؛ فروش مجدد حق پایه ممنوع.
- Pet cabin/hold، wheelchair و medical equipment تابع operating airline، محدودیت route/animal/certification/age/SSR واقعی‌اند. نیاز به تایید دستی را به «موجودی نامحدود» تبدیل نکنید.
- Involuntary flight disruption، no-show، partially flown coupon، interline، ancillary provided-by-third-party همگی Reason/Provider context دارند، نه hardcoded blanket refund.

## Output
قواعد Definition/Provision/Pricing قابل انتشار و ارزیابی‌اند؛ nonSeat InventoryPolicy پابرجا؛ Seat Pricing مستقل از StockPolicy؛ هیچ external authority «آزاد/تایید» فرض نشود.


<!-- END: 05-S1-PROVISIONS-PRICING-STOCK-SUPPLIERS.md -->


---

<!-- BEGIN: 06-A01-A24-BUSINESS-AND-SELECTION-MATRIX.md -->

# 06 — ماتریس قطعی ۲۴ Variant و حداقل Facts لازم

**منبع Code/Profile/PricingUnit:** Registry فعلی `AncillaryVariant.cs`. هر سطر بیان می‌کند Selection چه چیزی می‌خواهد، Domain چه چیزی چک می‌کند و Stage 3 چه تعهدی خواهد داشت. Optional Fields تنها وقتی Variant اجازه دهد فعال می‌شوند.

| Code | Profile / محصول | Definition/Options و Selection کلیدی | Billing / Coverage | Stage 3 و محدودیت |
|---|---|---|---|---|
| **A01** | Baggage / Extra checked bag | PTC، allowance پایه، extra `Quantity` | `PerPiece`, Pax/Portion | hold bag quota/eligible supplier؛ چند Pax جدا |
| **A02** | Baggage / Weight package | `PackageWeightKg`, `PackageProductRef`, `Quantity` | `PerItem`, Pax/Portion | وزن بسته معتبر، no double charge |
| **A03** | Baggage / Overweight | `BagRef`, `MeasuredWeightKg`, band | `PerPiece` یا `PerKilogram`, Pax/Portion | بدون وزن verified => quote/total ناقص |
| **A04** | Baggage / Oversize | `Dimensions`, maxLinearSumCm | `PerPiece`, Pax/Portion | overlap با A03 بر اساس ترکیب rule |
| **A05** | Baggage / Cabin bag | `Quantity`, cabin allowance | `PerPiece`, Pax/Portion | verified policy/space supplier |
| **A06** | Baggage / Sports/special equipment | `EquipmentKind`, `WeightKg`, `Dimensions` | `PerItem`/`PerPiece`, Pax/Portion | provider specialized approval |
| **A07** | Seat / Standard | SeatNumber, Cabin, characteristic, traveller/flight | `PerSeat`, Pax/Sector | SeatMap؛ physical owner FlightFlow |
| **A08** | Seat / Preferred | preferred characteristics, exitRow eligibility, SeatNumber | `PerSeat`, Pax/Sector | child/exit row restrictions؛ SeatMap |
| **A09** | Seat / ExtraSeat (EXST) | `RequiresAdjacentSeat`, purpose, extra occupied count | `PerSeat`, Pax/Sector | **generic AncillaryOffers**؛ coordinated physical+ticket action |
| **A10** | Upgrade / Cabin | from/to cabin, fare family, `TargetCabinId`, `QuoteRef` if dynamic | `PerPassenger`/`PerSeat`, Pax/Flight | verify booking class and need ticket exchange |
| **A11** | Meal / Free special meal | `MealCode`, diet, leadtime | `PerPassenger`/`PerItem`, Pax/Flight | SSR & explicit Free; confirmation if required |
| **A12** | Meal / Paid preorder | `MenuItemRef`, `Quantity`, meal family | `PerItem`/`PerPassenger`, Pax/Flight | verified menu stock, prep/cutoff |
| **A13** | Pet / cabin | `AnimalType`, weight/carrier, age/docs | `PerItem`, Pax/Portion | cabin pet quota/operating rules |
| **A14** | Pet / hold | A13 + `SizeBracket` | `PerItem`, Pax/Portion | hold capability & restrictions |
| **A15** | AssistedTravel / Wheelchair | `AssistanceSsrCode` WCHR/WCHS/WCHC | `PerPassenger`, Pax/Flight | SSR & airport/provider acknowledgement |
| **A16** | AssistedTravel / Disability | assistance type, contact/disclosure | `PerPassenger`, Pax/Flight | manual approval if relevant |
| **A17** | AssistedTravel / MedicalEquipment | `EquipmentCode`, docs, oxygenUnits | `PerPassenger`/`PerItem`, Pax/Flight | source approval, oxygen inventory if real |
| **A18** | AssistedTravel / Bassinet | `InfantRef`, `GuardianRef`, max age/weight | `PerPassenger`/`PerItem`, Pax/Flight | verified bassinet/seat compatibility |
| **A19** | AssistedTravel / UMNR | `ChildRef`, guardian contacts, connection policy | `PerPassenger`, Pax/Bound | guardian/age/route evidence |
| **A20** | AirportService / Lounge | AirportId/FacilityRef/TimeWithOffset/GuestCount | `PerPassenger`, pax/airport | time slot/supplier guest quota |
| **A21** | AirportService / FastTrack | AirportId, terminal, window | `PerPassenger`, pax/airport | supplier/local slot |
| **A22** | AirportService / CIP | included components/GuestCount/appointment | `PerPassenger`/`PerItem`, pax/order if supported | combined supplier package, no duplicate fees |
| **A23** | Priority / BoardingCheckin | OptIn, zone/priorityGroup, fare benefit | `PerPassenger`, pax/flight | fare inclusion, no duplicate entitlement |
| **A24** | Connectivity / Wi-Fi | PlanCode, DeviceCount, duration/data | `PerItem`/`PerPassenger`, pax/order if supported | verified activation/provider ref |

## قواعد تفاوت دسته‌ها
1. A07/A08 فقط SeatMap؛ A09 با اینکه Profile=`Seat` است در خروجی generic `AncillaryOffers` فعلی باقی می‌ماند. این یک **استثنای صریح** است، نه دو نوع Pricing Engine.
2. A11 free special meal می‌تواند SSR رایگان باشد؛ Free بودن A11 به معنی فرض رایگان‌بودن همه meal نیست.
3. A10 قیمت داینامیک بدون QuoteRef/Expiry معتبر، `NeedsQuote` است؛ Order/Ticket exchange را Ancillary انجام نمی‌دهد.
4. A13/A14 حیوان را هرگز با baggage «piece» یکسان نکنید؛ Carrier rule و Supplier approval جداست.
5. A18 infant+guardian، A19 minor+contacts و A15 SSR code مستلزم beneficiary دقیق‌اند؛ order-scoped بدون Pax برای اینها معتبر نیست.
6. A20-A22 Time/Facility ممکن است cross timezone باشد؛ appointment/slot صرفاً با source verified قابل Hold است.
7. A24 PlanCode انتخاب شده باید واقعی و قابل ارائه باشد؛ PlanCode دلخواه Caller پذیرفته نشود.

## PerOrder به معنی چه؟
Profileهای واقعاً Order-scoped فقط وقتی Rule/Provider اجازه دهد (مثلاً بسته مشترک CIP یا اتصال اینترنت گروهی) `CoverageScope.Order` و `TravellerId=null` خواهند داشت. قیمت یک‌بار برای همان Service ثبت می‌شود. برای هر Profile که ذاتاً Traveller-specific است، Scope=Order **رد** شود؛ مدل به زور از ۲۴ سناریو حق تازه اختراع نمی‌کند.

## سناریوهایی که باید از یک Engine عبور کنند
- پرواز با ۲ Bound و ۲ Pax (ADT/CHD)، Partial fare inclusion برای بار فقط یک Segment؛ اضافه بار مجزا برای هر واحد و هر نفر.
- یک meal free برای Pax1 و paid menu برای Pax2؛ selectionId مستقل و قیمت مستند.
- infant bassinet به همراه Guardian و محدودیت Exit-row برای child.
- چند خدمات با Supplier خارجی/Local/Manual، بازه زمانی متفاوت و امکان خریدار آژانس.
- Order بدون Traveller برای **محصولی که واقعاً Order-scoped تعریف شده**.


<!-- END: 06-A01-A24-BUSINESS-AND-SELECTION-MATRIX.md -->


---

<!-- BEGIN: 07-S2-SHOPPING-EXACT-SURFACES-AND-OPTIONS.md -->

# 07 — Stage 2: Endpointهای دقیق Shopping در ۳ Surface

## 7.1 Route inventory — چهار عملیات اصلی در هر Surface
در جدول زیر `{Surface} ∈ {Backoffice, Api, OtaPanel}`. سه Surface **DTO و Policy/Auth جدا**، اما **Engine و Domain مشترک** دارند.

| Operation | Canonical path | Current/Target |
|---|---|---|
| فهرست Ancillary غیر صندلی | `POST /{Surface}/v1/AncillaryOffers` | `[CURRENT]`؛ تکمیل مبلغ/Scope/Option info |
| جزئیات یک Candidate | `POST /{Surface}/v1/AncillaryOffers/Details` | `[CURRENT]`؛ `[TARGET]` Optionهای واقعی و typed |
| انتخاب و قیمت دقیق | `POST /{Surface}/v1/AncillaryOffers/Selections/Evaluate` | `[CURRENT]`؛ `[TARGET]` `pricedSelectionRef` |
| نقشه و قیمت Seat | `POST /{Surface}/v1/SeatMaps` | `[CURRENT]`؛ **Response/Route FROZEN** |

**هیچ مسیر جدید `/{Profile}/Details` یا `/SeatOffers` ایجاد نکنید.** عملیات `Selections/Resolve` صرفاً S2S برای Ordering است، نه سطح عمومی Shopper.

## 7.2 Request context / Security
**PreOrder request:** `{ "offerId": "<priced-air-offer>", "options": { "preferredCurrencyId": 70, "pointOfSaleId": 1 } }` برای Backoffice به‌صورت نمونه؛ وجود/امکان هر Option مطابق DTO فعلی و Caller مجاز؛ Agency نباید با `options.pointOfSaleId/ownerAirlineId/customerId` Scope را تغییر دهد.  
**PostOrder [TARGET additive]:** `{ "orderId": 81235 }` تحت Rule `offerId XOR orderId`، فقط پس از وجود verified `IOrderCommercialContextReader` متعلق به Ordering؛ SalesScope، available segments و ticket/coupon state از Order Source، نه درخواست Caller. پاسخ SeatMap همان شکل می‌ماند.  
`Api/OtaPanel`: Agency/Auth/eligibility و POS از JWT context و مرجع trusted استخراج می‌شود؛ هیچ body spoofing. `Backoffice`: Airline user/office/home-airline و POS معتبر.

## 7.3 List پاسخ باید چه اطلاعاتی بدهد؟
هر Candidate: `offerItemRef`, `serviceDefinitionRef`, `profile`, `variantCode`, `commercialName`, `travellerRefs[]` (یا Order scope), `flightRefs[]`, `portionRefs[]`, `coverageScope`, `pricingUnit`, `quantityUnit/min/max`, `priceOrigin`, `priceStatus`, `currency`, `base/unitTotal/quantityTotal?`, `tax/fees summary`, `offerReadiness`, `selectionKind`, `availability/sourceState`, `reasonCodes[]`, `pricingRevisionId` فقط در سطح داخلی. `PointOfSaleId` تنها در Surface مجاز. `NeedsSelection` و `NeedsQuote` با `Selectable` یا `Guaranteed` یکی نیستند.

## 7.4 Details مشترک اما تایپ‌شده — contract additive `[TARGET]`
ورودی `{offerId|orderId, offerItemRef}`؛ Lookup فقط در SalesScope همان Caller/Source/Rev. خروجی Existing Detail + موارد زیر، بدون شکستن DTO موجود:

| Field | Type | Rule |
|---|---|---|
| `selectionContract.kind` | `SelectionKind` existing | QuantityChoice/TypedForm/SeatMapSelection/ExternalQuote/... مطابق Registry |
| `selectionContract.fields[]` | list of `{name,type,required}` | فقط فیلدهای واقعی Variant، نه Form Backoffice |
| `optionGroups[]` | list of `OptionGroup` | فقط برای Dimensionهایی که **فهرست واقعی قابل انتخاب** دارند |
| `optionGroups[].fieldName` | string | نام موجود `MenuItemRef`، `PackageProductRef`، `PlanCode`، `FacilityRef` و غیره |
| `optionGroups[].choices[]` | list | `optionRef`, `value`, `label`, `applicability`, `pricePreview?`, `availability`, `sourceVersion?` |
| `quantity` | `{unit,min,max,step?,selectionRequired}` | Qty معتبر طبق Unit/SalesRules، در صورت لزوم Quoted at Evaluate |
| `commercialTerms` | existing + additive | `PurchaseStage`, settlement/refundability/confirmationRequirement, mandatory approvals |
| `optionSourceState` | `Known/Unknown/NotApplicable` presentation value | Empty choices با Unknown به معنی «هیچ گزینه‌ای موجود نیست» نیست |
| `priceCompleteness` | boolean/reason | اگر Qty، اندازه، quote یا currency ناقص است مبلغ نهایی نده |

**هشدار:** `optionRef` همیشه اجباری نیست: Product با ورود اندازه/وزن/SSR/پرکردن فرم Typed، گزینه از قبل enumerate شده ندارد؛ `SelectionContract.fields` و constraints کافی است. اگر Option از Supplier dynamic می‌آید، تنها پس از تأیید Source/Version/TTL در `choices` لیست می‌شود؛ هیچ Master Table تصنعی ساخته نشود.

## 7.5 Selection/Evaluate (یک API مشترک)
ورودی `{offerId|orderId, offerItemRef, selection:{...}}`. `AncillarySelection` جاری فیلدهای زیر را دارد:
`Quantity`, `PackageProductRef`, `BagRef`, `MeasuredWeightKg`, `Dimensions`, `EquipmentKind`, `WeightKg`, `SeatNumber`, `ExitRowTermsAccepted`, `Purpose`, `TargetCabinId`, `QuoteRef`, `MealCode`, `MenuItemRef`, `AnimalType`, `OtherAnimalCode`, `AnimalAgeWeeks`, `CombinedWeightKg`, `CarrierDimensions`, `DocumentAcknowledgements`, `SizeBracket`, `AssistanceSsrCode`, `EquipmentCode`, `EvidenceDocumentRefs`, `OxygenUnits`, `InfantRef`, `GuardianRef`, `ChildRef`, `GuardianHandoffContactProvided`, `GuardianPickupContactProvided`, `AirportId`, `FacilityRef`, `TimeWithOffset`, `GuestCount`, `OptIn`, `PlanCode`, `DeviceCount`.

پاسخ موجود `status, offerItemRef, currentOfferItemRef, provisionChangedBySelection, price, fieldIssues, service details` حفظ و در حالت `Accepted + FullyPriced + Authorized` **`pricedSelectionRef`** به‌صورت additive اضافه می‌شود. برای غیر Accepted هیچ ref رزروپذیر صادر نمی‌شود. Required `optionRef` را در صورت عرضه Option Group می‌توان به Request افزود، ولی مقدار option در Selection باید به همان optionRef اشاره کند؛ تضاد آنها 422.

## 7.6 رفتار قیمت و Scope
- `A01` Qty1 و Qty2 همان `offerItemRef` ابتدایی را می‌توانند داشته باشند؛ ولی `pricedSelectionRef` متفاوت است و PriceTotal دقیق تغییر می‌کند.
- `A02` PackageProductRef، تعداد بسته و covered flights مشخص؛ package=5kg/10kg واقعاً گزینه‌های مستقل با قیمت قابل ارزیابی.
- `A03` PerKilogram بدون `MeasuredWeightKg` نمی‌تواند Money Total معتبر برگرداند؛ کد خطا/field issue، نه صفر.
- `A12` MenuItemRef و Qty/Passenger؛ نبود منوی Supplier با `Unknown` مشخص می‌شود، نباید menu hardcode شود.
- Order-scoped CIP یا WiFi مجاز فقط وقتی واقعی Domain/Supplier Rule اجازه دهد، TravellerRefs تهی و Total یک بار؛ باقی کالاها و Seat به Pax/Segment مقیدند.
- Free SSR و Included Fare حق معتبر دارد و ممکن است سند EMD نخواهد؛ zero فقط با explicit proof.
- چند Currency/POS/Child/Adult/FareFamily می‌تواند Quoteهای متفاوت بدهد. Source error، stale revision، benefit conflict یا اختیار agency ناکافی ref معتبر صادر نمی‌کند.

## 7.7 Pre-order و Post-order
- **قبل Order:** `AirAvail` priced `OfferId` راه‌انداز context است؛ نیازی به `OrderId` نیست. `LastTicketingDate` را با expiration Ancillary Quote یکی نگیرید.
- **بعد Order:** دقیقاً همان `AncillaryOffers` و `Details` برای سفارش موجود، context از Ordering (ticket/used/refund/coupon) و قیمت جدید؛ Cross Order/Carrier/POS Guard؛ SeatMaps همان output و فقط Request source additive.

## خروجی Stage 2
برای هر Shopper پاسخ قابل نمایش، انتخاب واقعی، مبلغ درست با tax/fee/quantity/scope و `pricedSelectionRef` قابل Resolve فراهم می‌شود؛ SeatMap با DTO قبلی کار می‌کند؛ **هیچ Reservation در Shopping ایجاد نمی‌شود**.


<!-- END: 07-S2-SHOPPING-EXACT-SURFACES-AND-OPTIONS.md -->


---

<!-- BEGIN: 08-S2-PRICED-SELECTION-IDENTITY-AND-VALIDITY.md -->

# 08 — مدل قطعی هویت گزینه، انتخاب نهایی و Revalidation

## 8.1 Referenceهای سه‌گانه
| Reference | منشأ | محتوا/قانون | می‌تواند Hold شود؟ |
|---|---|---|---|
| `offerItemRef` `[CURRENT]` | Shop | owner/channel/POS/airOffer/DefinitionVersion/Provision/PricingRevision/Traveller/CoverageRef | **نه، بدون انتخاب کامل** |
| `optionRef` `[TARGET]` | Details OptionChoice | ref unique همان گزینه/Source/Version/Scope؛ فقط اگر گزینه واقعی فهرست‌شدنی باشد | نه |
| `pricedSelectionRef` `[TARGET]` | Accepted Evaluate | انتخاب canonical نهایی+Quote/TTL+Ccy/Rev+Beneficiary+Scope | **بله؛ فقط پس از Resolve و revalidate** |
| `SeatMaps.travelerPricing[].id` `[CURRENT]` | SeatMap | Offer+POS+Pax+Flight+Seat+Commercial Candidate، 43-character opaque | فقط پس از Resolve معتبر و physical Hold جداگانه |

## 8.2 `PricedSelectionSnapshot` [TARGET] — short-lived lookup (not aggregate)
**پیاده‌سازی منتخب برای شفافیت:** یک Record کوتاه‌عمر در Storage موجود Ancillary؛ opaque reference generated at Accepted Evaluate و قابل lookup Service-to-Service. این Record «رزرو» نیست و چند عملیات مالی روی آن انجام نمی‌شود؛ TTL/config و cleanup با کد جاری سازگار است. از Signed Token بلند یا Session store دوم بدون دلیل خودداری کنید.

**Dictionary الزامی** (nullable به‌تناسب variant):
| Field | Type | منبع/ناوردا |
|---|---|---|
| `PricedSelectionRef` | `string` required, opaque, unique | server generated |
| `OfferItemRef` | `string` required | candidate source |
| `SourceKind` | `ShoppingSourceKind` required | PreOrderAirOffer یا PostOrderOrder (map به enum موجود) |
| `AirOfferId` / `OrderId` | `string?` / `long?` XOR | Source verified |
| `SourceRevision` | `string?` | offer/Order revision if available؛ null not proof stable |
| `OwnerAirlineId`, `Channel`, `PointOfSaleId` | `int`, enum, `long` | trusted sale scope |
| `DefinitionVersionId`, `ProvisionId`, `PricingRevisionId?`, `PricingRateId?` | `long`/`long?` | rule/price evidence |
| `Profile`, `VariantCode`, `PricingUnit` | enum/string/enum | cross-check actual Definition |
| `CoverageScope` | enum | order/pax/segment/portion/journey rule |
| `TravellerRefs[]`, `FlightRefs[]`, `PortionRefs[]` | `string[]` | canonical sorted unique IDs; no fake pax |
| `SelectedOptions` | typed canonical value object / normalized serialized fields | EXACT source-approved Choice, Qty, measurement/seat/spec |
| `Quantity`, `QuantityUnit` | `int` (count/piece/packages) + enum | `PerKilogram` decimal, piece count integer؛ no implicit truncation |
| `PriceOrigin`, `CurrencyId`, `PriceBase`, `CompleteUnitTotal`, `RequestedQuantityTotal` | enum,int,decimal? | verified engine outcome; money not user-supplied |
| `TaxFeeComponents[]`, `FXEvidence?`, `ExternalQuoteRef?` | existing types | reconciliation, no missing fees |
| `CreatedAtUtc`, `ValidUntilUtc` | DateTimeOffset | quote TTL = min of known source/publisher/quote expiries and configured local TTL |
| `OptionSourceVersion?`, `CommercialDigest` | string/hash | detect changed options/pricing/rules |

**بستن انتخاب:** قیمت Qty=2 با Qty=1 یک PriceSelection نیست. دو گزینه Meal مختلف، دو صندلی/مسافر مختلف، دو FareFamily/دو POS مختلف، Scope Order در برابر Pax، و تغییر Pricing Revision => انتخاب متمایز؛ رفرنس قدیمی بعد تغییر باید Reject یا Requote و Confirm جدید کاربر بگیرد.

## 8.3 Resolve API S2S [TARGET]
`POST /Service/v1/Ancillaries/Selections/Resolve` با `{pricedSelectionRefs:[...], orderId?, expectedOrderServiceIds?, callerCorrelationId}`؛ SeatMap ID فقط با discriminated `{seatMapSelection:{id,offerId,travellerRef,flightRef}}` تحت Auth داخلی پذیرفته می‌شود. خروجی `ResolvedSelections[]` با Price/Scope/Validity/ProviderClassification/Reason و status `Valid|Stale|Expired|NotAuthorized|NeedsQuote|NeedsAvailability`. این Resolve **Hold یا Capture** نمی‌کند.

**رسیدن به Valid:** شناسه وجود دارد؛ Owner/Channel/POS matches؛ AirAvail/Ordering trusted context معتبر است؛ Declaration/Rate/Option/beneficiary/quantity نسبت به زمان تصمیم‌گیری دوباره معتبرند. PriceChanged ⇒ status stale + مبلغ جدید (در صورت امکان) برای پذیرش دوباره توسط Ordering/UI؛ هیچ Silent Reprice در Hold.

## 8.4 Offer expiration و Source version
- AirAvail DTO فعلی `PricedAt` و `LastTicketingDate` دارد اما `ValidUntilUtc` را به‌عنوان Contract منتشر نکرده؛ configured Ancillary short TTL صرفاً «محدودیت quote داخلی» است، نه ضمانت ایرلاین.
- `ValidUntilUtc` هرجا در source موجود است با Owner policy `min` می‌شود؛ در غیر این صورت local quote TTL و reprice قبل از Hold؛ pending supplier بعد TTL به موجب rule باید review/reprice شود.
- Source quote approval اگر لحظه‌ای باشد، provider quote expiry می‌تواند کمتر از local TTL باشد. فارغ از TTL، Stock در زمان Hold مستقل دوباره check می‌شود.
- Returned Prices should not expose unverified unit fee as included; اگر fee بیرون از PriceUnit اعمال می‌شود، `isTotalComplete=false` تا محاسبه مسئول Order.

## 8.5 Auth و PII
`pricedSelectionRef` bearer secret نیست؛ revalidating trusted Owner/POS/Order/Pax در سرویس الزامی است. قیمت را از Body کاربر یا از `orderServiceId` تنها trust نکنید. Store نباید نام/موبایل/مدرک مسافر داشته باشد، فقط refs. Foreign POS/agency/caller شناسه را نتواند Resolve کند. End-user نباید Supplier credentials یا internal office codes ببیند.

## 8.6 مثال تمایز
برای Pax1 در Flight1:
- A01 extra bag Qty1/IRR ⇒ `psel_A`; Qty2/IRR ⇒ `psel_B`; Qty2/EUR ⇒ `psel_C`.
- A12 Menu `VG1` Qty1 ⇒ `psel_D`; `CH1` Qty1 ⇒ `psel_E`.
- A07 Seat 12A Pax1 و 12A Pax2 ⇒ دو SeatMap ID؛ یکی نمی‌تواند برای هر دو روی همان صندلی Confirm شود.
- Order-scope CIP بدون Passenger-specific fee ⇒ `TravellerRefs=[]`, `CoverageScope=Order`, total یک‌بار.

## خروجی
هر انتخاب پذیرفته‌شده یک سند **تجاری قیمت‌گذاری‌شده و قابل رجوع** در Ancillary دارد؛ هیچ‌وقت به معنای رزرو/صدور EMD نیست و قابل سوءاستفاده Cross-Order نیست.


<!-- END: 08-S2-PRICED-SELECTION-IDENTITY-AND-VALIDITY.md -->


---

<!-- BEGIN: 09-S2-SEATMAP-FROZEN-AND-VERIFIED-BEHAVIOR.md -->

# 09 — قرارداد SeatMap ثابت (FROZEN)؛ تصحیح رفتاری بدون Breaking Change

## 9.1 مسیرها و نوع داده تغییر نکنند
`POST /Backoffice/v1/SeatMaps`  
`POST /Api/v1/SeatMaps`  
`POST /OtaPanel/v1/SeatMaps`

**سورس Contract:** `src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Dto/SeatMapsDto.cs` در SHA `9aca1cd...`.

**ساختار رسمی و دقیق پاسخ جاری:**
```
SeatMapsDto
  Meta { Count:int }
  Data[] {
    Type:string, FlightOfferId:string, SegmentId:string,
    Departure { IataCode?, Terminal?, At:DateTimeOffset },
    Arrival { IataCode?, Terminal?, At:DateTimeOffset },
    CarrierCode?, Number?, Operating { CarrierCode? },
    AircraftId:int, Class?, CabinClassId?,
    Decks[] {
      DeckType?, DeckConfiguration { Width,Length,StartSeatRow,EndSeatRow,
        StartWingsX,EndWingsX,StartWingsRow,EndWingsRow,ExitRowsX[] },
      Cabins[] { CabinClass?,CabinClassId?,CabinStartSeatRow,CabinEndSeatRow,
        Items[] { Number?,Code?,Column,Row,Title?,CharacteristicsCodes[],
          Conditions[],Coordinates {X,Y,Length}, TravelerPricing[] {
            Id?, TravelerId:string, SeatAvailabilityStatus:string,
            Price? {Currency:string,Base:string,Total:string,
              Taxes[]{Amount:string,Code?}, Fees[]{Amount:string,Type?}} }
        }}
    }, AvailableSeatsCounters[] { TravelerId,Value }
  }
  Dictionaries { Facilities:map, SeatCharacteristics:map }
```
**هیچ نام، نوع، nullability، collection shape، nesting، status string یا نام route تغییر نکند.** حتی اصلاح Price/Id فقط از طریق مقادیر مجاز همین DTO انجام شود. Consumer Backoffice SeatMap نمی‌شکند.

## 9.2 رفتار صحیح Status/Price/ID
| وضع واقعی | `SeatAvailabilityStatus` | `Price` | `Id` | توضیح |
|---|---|---|---|---|
| Occupied معتبر | `OCCUPIED` | null | null | Physical owner |
| blocked by Cabin/Exit/Eligibility | `BLOCKED` | null | null | حتی وقتی Physical Empty |
| Physical Empty + verified published Paid price + commercial eligible | `AVAILABLE` | valid money lines | opaque ID | ما تنها Offer می‌دهیم؛ Hold هنوز لازم است |
| Physical Empty + **verified** Free entitlement/published `Free` | `AVAILABLE` | صفر معتبر همان Currency | opaque ID | Zero با Evidence، نه از Null |
| Physical Empty + no commercial product/unknown quote/source | `BLOCKED` | null | null | UI نمی‌تواند آن را قطعی خرید تلقی کند |
| missing occupancy for requested flight / source outage | **typed unavailable response** با رفتار خطای فعلی | — | — | response موفقِ خالی/قیمت صفر ساختگی نده |

این اصلاحات **رفتار دامنه** هستند، نه migration ظاهری DTO. `AvailableSeatsCounters` فقط واقعا AVAILABLE را می‌شمارد. `Price.Total` تمام components قابل اعمال را بازتاب دهد؛ اگر charge/fee قابل تعیین نیست Seat نباید FullyPriced یا رزروپذیر نشان داده شود.

## 9.3 Ownership
Aircraft physical layout از مرجع Aircraft/Flight معتبر، occupied/hold/seat assignment از FlightFlow. Ancillary خود `SeatSpecification`/`SeatApplication`/`FareApplication`/pricing را دارد و برای هر Pax/Flight/Seat می‌سنجد. **نقشه در Ancillary موجود است به‌عنوان Read Composition Facade**؛ حذف Adapter یا تغییر Route صرفاً بر اساس ADR قدیمی ممنوع. `AncillaryInventoryPolicy` برای Seat تعریف نمی‌شود.

## 9.4 Source guards
- همه flightIdهای مورد درخواست دقیقاً در OccupiedSeats response presence داشته باشند؛ missing flight ≠ empty occupied list؛ duplicate response یا invalid seat Number => fail closed.
- layout از همان Aircraft/FlightVersion واقعی باشد؛ mismatch، seat coordinates stale، تغییر هواپیما یا کد ویژگی نامشخص => blocked/unavailable.
- POS باید یکتا و authorized برای همان price quote باشد؛ چند POS نامشخص ⇒ error existing؛ first random POS ممنوع.
- `travelerPricing[].id` 43-char opaque موجود حفظ؛ Hash بدون Candidate معتبر حق خرید نیست. Resolve/Order چک Scope/Commercial Price و Physical Hold جدا را انجام می‌دهند.
- `A09 ExtraSeat` فقط به‌عنوان Option واقعی with special adjacent seat/ticket requirements فروخته می‌شود؛ در این Stage مسیر Generic AncillaryOffers باقی می‌ماند، نه seat cell با `Price=0`.

## 9.5 FlightFlow gating
در Repo FlightFlow SHA `962f4188` Endpoint `POST Service/v1/Flights/OccupiedSeats` مشاهده شد. **این صرفاً حقیقت منبع، نه تأیید محیط استقرار است.** Owner می‌گوید تغییرات FlightFlow برای این پروژه Gate نباشد. در closure S2، External integration را جداگانه با عنوان `[EXTERNAL_PENDING]` ثبت کن؛ هیچ‌گاه عدم دسترسی به FlightFlow را به «هیچ صندلی اشغال نیست» تبدیل نکن.


<!-- END: 09-S2-SEATMAP-FROZEN-AND-VERIFIED-BEHAVIOR.md -->


---

<!-- BEGIN: 10-S3-RESERVATION-AGGREGATE-LIFECYCLE.md -->

# 10 — Stage 3: دامنه Reservation، فیلدهای گروه و Unit و چرخه عمر

## 10.1 فقط Aggregate فعلی توسعه می‌یابد
`AncillaryReservation` و `AncillaryReservationUnit` در Repo موجودند و فعلاً از OrderService/Definition/Provision/Quantity یک Hold تجاری می‌سازند. `Confirm` همه Units را صرفاً به status Confirmed منتقل می‌کند؛ External supplier و stock واقعی را انجام نمی‌دهد. قرارداد جدید **نسخه تکمیل‌شده همان مدل** است، نه دومین Aggregate یا ReservationPreparation.

## 10.2 Header `AncillaryReservation` — فیلدهای الزامی
| Field | Type | Rule |
|---|---|---|
| `Id` (`HoldId`) | `long` | Group reference برگشتی به Ordering |
| `OrderId` | `long` required | verified Owner Ordering |
| `IdempotencyKey` | `string` required, max فعلی 128 | Scoped by Order/operation؛ same key same canonical payload ⇒ replay |
| `Reference` | `string` required | client Order correlation، نه PNR/EMD |
| `RequestedExpiresAt`, `ExpiresAt` | `DateTimeOffset?` | حداکثر واقعی supplier/local TTL؛ null تنها اگر Business rule اجازه دهد |
| `Status` | `AncillaryReservationStatus` | derived from Units، هرگز Held/Confirmed ساختگی |
| `Outcome` | `Complete|Partial|Pending|Failed` response projection | برای batch mixed، نه جایگزین Unit truth |
| `CreatedAt`, `UpdatedAt` | `DateTimeOffset` | audit |
| `Units` | `List<AncillaryReservationUnit>` | حداقل یک Unit واقعی؛ OrderService duplicate guard |

## 10.3 `AncillaryReservationUnit` — هر خرید عملیاتی مستقل
| Field | Type | Rule |
|---|---|---|
| `Id` (`UnitId`) | `long` | مرجع جزء به جزء برای Confirm/Cancel/Retry |
| `OrderServiceId` | `long` | یک selected service از Ordering؛ هر واحد ناشی از یک selection دقیقا map شود |
| `PricedSelectionRef` | `string` | resolved & revalidated؛ never arbitrary price from Caller |
| `DefinitionVersionId`, `ServiceDefinitionId`, `ProvisionId`, `PricingRevisionId?` | `long` | snapshot of rule identity |
| `TravellerId` | `long?` | null only when valid Order scope |
| `BeneficiaryTravellerIds` | `long[]?` only if legitimately shared purchase | بدون Duplication قیمت؛ عدم نیاز برای تک‌مسافر/Order-simple |
| `CoverageScope` | enum | Sector/Portion/Journey/Order مطابق priced selection |
| `CoveredFlightIds` | `long[]` | Count/scope requirements؛ Order-only may empty |
| `Quantity` | `int` | هماهنگ با `Provision.Quantity` فعلی؛ وزن اندازه‌گیری‌شده جدا decimal |
| `SelectedOptions` | existing typed selection normalized | SeatNumber/MealCode/Package/Weight/Facility/Plan/... |
| `CurrencyId`, `PriceOrigin`, `UnitAmount?`, `TotalAmount?` | int/enum/decimal? | source-verified money; not client supplied |
| `TaxFeeComponents`, `FXEvidence?` | typed existing | برای reconciliation و Order totals |
| `FulfillmentProviderKey` | string | immutable پس از ایجاد Unit |
| `ProviderUnitRef?`, `StockPoolId?` | string?/long? | تنها پس از واقعی شدن commitment |
| `SupplierActionRef?`, `ProviderEvidenceAtUtc?` | string?/DateTimeOffset? | webhook/poll/manual trace |
| `Status` | unit status | حقیقی؛ با Pending pending operation |
| `PendingOperation?`, `PendingReasonCode?` | string? constrained | Hold/Confirm/Release/Cancel؛ Supplier/Manual/Reconcile |
| `FailureReasonCode?`, `IsRetryable` | string?/bool | بدون نمایش جزئیات حساس Supplier |
| `CreatedAt`, `UpdatedAt` | DateTimeOffset | trace |

برخی فیلدهای Unit فعلی (`StockPoolId?`, `ProviderUnitRef?`) وجود دارند؛ دو بار نسازید. `QuantityRule.Unit` Enum `Each/Piece/Kilogram` و `QuantityRule.Min/Max:int` است؛ وزن `MeasuredWeightKg` decimal Selection است، نه Quantity int را به decimal تغییر بدهیم مگر Rule واقعی لازم شود.

## 10.4 Statusها و semantics بدون دروغ
**Existing numeric values حفظ می‌شوند:** `Held=1`, `Confirmed=2`, `Released=3`, `Expired=4`, `Cancelled=5` برای گروه و Unit.

**افزودنی حداقلی P3 فقط برای ثبت نتیجه‌نامعلوم واقعی:**
- `AncillaryReservationUnitStatus.Requested=6`, `Pending=7`, `Failed=8`؛ `PendingOperation` و `PendingReasonCode` نوع انتظار را روشن می‌کنند؛ PendingManual/Unknown دو state entity مستقل نمی‌خواهند.
- `AncillaryReservationStatus.Pending=6`, `Mixed=7`, `Failed=8`؛ وضعیت گروه از Units به‌روز می‌شود: تمام Held ⇒ Held؛ تمام Confirmed ⇒ Confirmed؛ تمام Released ⇒ Released؛ تمام Cancelled ⇒ Cancelled؛ تمام Expired ⇒ Expired؛ any Pending/Requested ⇒ Pending؛ ترکیب کارهای کامل/ناموفق ⇒ Mixed؛ all Failed ⇒ Failed. `Outcome` مکمل برای Partial success.

این **پیشنهاد قطعی افزودنی به enumهای Ancillary در Stage 3** است؛ نه ادعای وجود در کد فعلی. Existing status numberها هرگز عوض نشوند. اگر EF Framework روش دیگری برای Derived Status دارد، equivalent با همین behavior قابل قبول است؛ دو Status منبع تناقض نسازید.

## 10.5 Operation semantics
| Command | Preconditions | Per-unit effect | Idempotency / failure |
|---|---|---|---|
| Hold | verified OrderService + pricedSelectionRef + valid offer/amount/stock/policy | Requested→Held یا Pending/Failed | same request replay; same key different data 409 |
| Confirm | unit Held و payment/authority gate از Ordering تأیید | Held→Confirmed (یا Pending when provider pending) | repeat confirmed returns same refs |
| Release | unit Held و not confirmed | Held→Released، recover resource/provider hold | unknown supplier release ⇒ Pending، not released locally falsely |
| Extend | unit Held با capability تمدید و Rule expiry | update effective expiry فقط پس از provider acceptance | no TTL beyond source rule |
| Expire | clock >= expiresAt، برای held units | Expired و آزادسازی/reconciliation | expired unit never confirmed |
| Retry | only Failed(retryable)/Pending after reconciliation | same unit/provider idempotency | no duplicate booking; no new commercial charge |
| Cancel | confirmed selected units, valid quote/authority | Confirmed→Cancelled only after supplier/local result | void/refund not inside Ancillary |
| Cancel Partial | selected `UnitIds[]` belong same Order/Hold | فقط Units مربوط تغییر می‌کنند | mixed group outcome authentic |

`Confirmed -> Released` و `Cancelled -> Confirmed` ممنوع. لغو بعد استفاده خدمت بدون waiver/Provider rule مسدود است؛ `NoShow` و involuntary رویدادهای مستقل دارند. **هیچ Confirm group کلّی مادامی که حتی یک Unit نامعلوم است گزارش نشود.**

## 10.6 Multi-unit semantics
- یک request می‌تواند A01 برای Pax1 Qty2 + A12 برای Pax2 + A22 Order-scoped را با هم داشته باشد؛ هر کدام Unit مستقل.
- چند انتخاب متفاوت یک Definition با Scope یکسان (مثلاً دو Menu یا دو Baggage Option) مجاز هستند اگر `OrderServiceId`, `PricedSelectionRef` و Domain uniqueness درست باشد؛ Code فعلی که `Definition+Coverage` را unique فرض می‌کند باید اصلاح شود.
- یک انتخاب واحد با همان OrderService نباید همزمان دوبار reserved شود؛ تنها راه retry، عملیات idempotent همان Unit است.
- Group reference `HoldId` متعلق به Ancillary است، `OrderId` متعلق به Ordering، `ProviderUnitRef` متعلق به Supplier، `SeatHoldReference` متعلق به FlightFlow. هیچ کدام را جای دیگری ننویسید.

## 10.7 Seat specific
`AncillaryReservationUnit` برای A07/A08 هزینه/حق تجاری را Hold/Confirm می‌کند ولی **انجام Hold فیزیکی را ادعا نمی‌کند**. Ordering برای same Traveller/Flight/Seat باید FlightFlow `Seat-Holds` با `SeatHoldReference` دریافت کرده باشد و فقط با شاهد آن، Confirm نهایی مجاز است. Ancillary FlightFlow Seat inventory را تخصیص نمی‌دهد. A09 EXST بسته به ticket/adjacent-seat، نیاز به co-confirm چند رفرنس physical و ticket action دارد؛ انجامش در Ordering است.

## خروجی
همان Aggregate موجود با truthful گروه/Unit، complete/partial/pending و امکان recovery و cancel جزئی؛ بدون invent کردن یک ReservationCoordinator داخل Ancillary.


<!-- END: 10-S3-RESERVATION-AGGREGATE-LIFECYCLE.md -->


---

<!-- BEGIN: 11-S3-INTERNAL-SERVICE-ENDPOINTS-AND-DTO.md -->

# 11 — قرارداد نهایی Service-to-Service Ancillary (Stage 3)

تمام مسیرهای این فصل صرفاً برای Caller سرویس معتبر (`Ordering`/approved internal provider) هستند؛ هیچ `Api`/`OtaPanel` اجازه فراخوانی مستقیم آنها را ندارد. Prefix فعلی: `Service/v1/Ancillaries`.

## 11.1 Endpoint inventory
| عمل | Endpoint | وضعیت | هدف |
|---|---|---|---|
| Resolve selections | `POST /Service/v1/Ancillaries/Selections/Resolve` | `[TARGET]` | snapshot price/options/scope before hold؛ no reservation |
| Hold Group | `POST /Service/v1/Ancillaries/Service-Holds` | `[CURRENT]` route، `[TARGET]` payload/behavior | batch چند selected services |
| Query Hold | `GET /Service/v1/Ancillaries/Service-Holds/{holdId}` | `[CURRENT]` route، `[TARGET]` enriched response | per unit status + refs + evidence |
| Confirm | `POST /Service/v1/Ancillaries/Service-Holds/{holdId}/Confirmations` | `[CURRENT]` route، `[TARGET]` truthful | commit only held/eligible units |
| Release All | `DELETE /Service/v1/Ancillaries/Service-Holds/{holdId}` | `[TARGET]` | release all held units only |
| Release Selected | `POST /Service/v1/Ancillaries/Service-Holds/{holdId}/Releases` | `[TARGET]` | body `unitIds[]`, all when omitted if explicit policy |
| Extend Hold | `PATCH /Service/v1/Ancillaries/Service-Holds/{holdId}` | `[TARGET]` | `{expiresAt,idempotencyKey}` under provider capability |
| Retry unresolved | `POST /Service/v1/Ancillaries/Service-Holds/{holdId}/Retries` | `[TARGET]` | `{unitIds[],idempotencyKey}`, same physical unit refs |
| Cancellation Quote | `POST /Service/v1/Ancillaries/Service-Confirmations/{holdId}/CancellationQuotes` | `[TARGET]` | read-only per unit penalty/supplier eligibility |
| Cancel Confirmed | `POST /Service/v1/Ancillaries/Service-Confirmations/{holdId}/Cancellations` | `[TARGET]` | quoteRef+unitIds+reason, cancel selected after check |
| Manual/Supplier outcome | `POST /Service/v1/Ancillaries/Service-Holds/{holdId}/Units/{unitId}/ProviderOutcomes` | `[TARGET]` strictly internal | authenticated supplier/manual receipt/evidence/reconciliation |

**قرارداد API**: ۱۱ عملیات؛ هیچ `/Baggage/Holds`/`Meal/Confirm` یا aggregate جدید. Expand endpoint only for a proven new provider use case.

## 11.2 Hold request **wire contract [TARGET]**
```json
{
  "idempotencyKey": "ORD-81235-ANC-HOLD-v1",
  "orderId": 81235,
  "reference": "ORDER-81235",
  "requestedExpiresAt": "2026-10-16T19:00:00Z",
  "services": [
    {"orderServiceId": 891, "pricedSelectionRef": "psel-bag-1", "travellerId": 154, "coverageScope": "Portion", "coveredFlightIds": [8001,8002]},
    {"orderServiceId": 892, "pricedSelectionRef": "psel-cip-order", "travellerId": null, "coverageScope": "Order", "coveredFlightIds": []}
  ]
}
```
در Code فعلی `AncillaryHoldServiceInput` دارای `OrderServiceId,ServiceDefinitionId,ProvisionId,TravellerId,CoverageScope,CoveredFlightIds,Quantity` است؛ Target جدید: **`PricedSelectionRef` الزامی و مرجع trusted قیمت/Qty/Options**. Body `ServiceDefinitionId/ProvisionId/Quantity` قدیمی اگر باقی ماند صرفاً cross-check می‌شود، حق override Resolve ندارد. نیاز به `OrderId` فعلی حفظ. Request بدون `OrderId` در فاز ۳ Hold نمی‌شود؛ Shopping preOrder بدون OrderId آزاد است.

## 11.3 Hold/Status response **wire contract [TARGET]**
```json
{
  "holdId": 11882,
  "orderId": 81235,
  "reference": "ORDER-81235",
  "idempotencyKey": "ORD-81235-ANC-HOLD-v1",
  "status": "Pending",
  "outcome": "Partial",
  "expiresAt": "2026-10-16T19:00:00Z",
  "units": [
    {"unitId": 11901,"orderServiceId": 891,"pricedSelectionRef":"psel-bag-1",
     "status":"Held","quantity":2,"currencyId":70,"totalAmount":"1800000",
     "fulfillmentProviderKey":"LOCAL_BAG","providerUnitRef":null,"retryable":false},
    {"unitId":11902,"orderServiceId":892,"pricedSelectionRef":"psel-cip-order",
     "status":"Pending","pendingOperation":"Hold","pendingReasonCode":"ManualConfirmation",
     "quantity":1,"providerUnitRef":null,"retryable":true}
  ]
}
```
مقادیر نمونه و نام Provider illustrative هستند؛ External ProviderRef و Internal status فقط S2S. `GET Hold` همین Snapshot را با evidence و history/reconciled status می‌دهد؛ Caller پس از timeout باید GET/reconcile کند، نه تکرار کورکورانه درخواست Supplier.

## 11.4 Confirm/Release/Cancel bodies
- `POST .../Confirmations`: Body `{idempotencyKey, unitIds?:long[], paymentOrGuaranteeReference?, externalSeatEvidenceRefs?:[]}`؛ null/empty `unitIds` = همه Unitهای Held، اما اگر mixed result پس باید response per Unit بازگردد؛ هر evidence از Caller trusted و قابل اعتبارسنجی باشد. در صورت حفظ route بدون Body، compatibility با command فعلی برقرار بماند، ولی no fake confirm.
- `DELETE .../Service-Holds/{id}`: release all (reason optional sourced from Order). `POST .../Releases`: `{idempotencyKey,unitIds:[...],reasonCode}`. `PATCH`: `{idempotencyKey,expiresAt}`، supplier TTL مطابقت.
- `POST .../Retries`: `{idempotencyKey,unitIds:[...],reasonCode?}` فقط failed/pending قابل پیگیری؛ no duplicate expense.
- `POST .../CancellationQuotes`: `{orderId,unitIds:[...],reasonKind:"Voluntary|Involuntary|SupplierFailure|NoShow",disruptionRef?,requestedAtUtc?}`؛ `requestedAtUtc` از server trusted است نه تاریخ جعلی Caller.
- `POST .../Cancellations`: `{idempotencyKey,orderId,unitIds:[...],cancellationQuoteRef,reasonKind,waiverRef?}`؛ Quote valid برای همین Group/Unit/Reason/Order، otherwise 409.
- `POST .../ProviderOutcomes`: `{providerOperationRef,result,providerUnitRef?,evidenceAtUtc,providerDecisionCode?,sourceKind}` authenticated internal provider/manual agent only.

## 11.5 CancellationQuote result [TARGET]
`CancellationQuoteRef:string`, `HoldId:long`, `OrderId:long`, `IssuedAtUtc`, `ValidUntilUtc`, `ReasonKind`, `OverallQuoteState`, `Units[]`:
`{UnitId, OrderServiceId, CanCancel, CanRefund, QuoteState, GrossServiceAmount?, CurrencyId?, FiledPenaltyAmount?, SupplierPenaltyAmount?, EligibleTaxComponents[], NonRefundableComponents[], WaiverApplied, WaiverEvidenceRef?, ProviderQuoteRef?, RuleSourceRef?, ReasonCodes[], RequiresManualReview}`.

**اصل:** `NetCustomerRefund` در این پاسخ **نیست**؛ Ordering باید Payment/EMD/Coupon/agency settlement را بررسی و quote نهایی Refund بسازد. `EligibleRefundAmount` اگر Ancillary ارائه می‌کند «service-component estimate» با qualifier است نه دستور پرداخت.

## 11.6 HTTP/Domain errors
- 400/422 invalid selection, quantity, coverage, field, provider capability.
- 401/403 unauthorized Service caller, wrong OwnerAirline/Order binding.
- 404 unknown Hold/Unit/selection/quote; ID متعلق به Caller دیگر و unauthorized must not leak existence.
- 409 expired/stale quote, conflict on IdempotencyKey, invalid unit transition, changed amount, stock contention.
- 422 manual/provider quote unknown (business result `RequiresManualReview`) بدون pretend success.
- 502/503 supplier outage: if action side effect may have occurred, persist Pending/Reconciliation and provide status; response HTTP به تنهایی حقیقت Outcome Supplier نیست.

از کدهای error موجود Ancillary استفاده کنید؛ برای کد جدید exact numeric اختراع نکنید مگر در قرارداد API مصوب مرحله ۳ با تعریف enum/error catalog واحد. در هر خطا `correlationId`, `reasonCode`, `unitId?`, `retryable` در سطوح مجاز قابل تشخیص باشد.


<!-- END: 11-S3-INTERNAL-SERVICE-ENDPOINTS-AND-DTO.md -->


---

<!-- BEGIN: 12-S3-LOCAL-SUPPLIER-MANUAL-COMPENSATION.md -->

# 12 — Stage 3: Provider Strategies و رفتار واقعی ظرفیت، عملیات دستی و جبران

## 12.1 ProviderCapability per ServiceDefinition/Provision
Strategy یک‌بار بر اساس trusted `FulfillmentProviderKey`، Supplier و Provision resolved می‌شود. `SupplierFulfillmentKind` فعلی `Local=1`, `External=2` کافی برای مالکیت است ولی Capability را به‌تنهایی بیان نمی‌کند؛ Capability جدول زیر از Provider معتبر خوانده/پیکربندی می‌شود، **نه از Body Agency**.

| Capability | Hold | Confirm | Release/Cancel | Reference/Evidence |
|---|---|---|---|---|
| `LocalFiniteCount` | decrement/reserve quantity واقعی | commit allocation | recover counted units / restock if permitted | `StockPoolId`, UnitId ledger |
| `LocalFiniteWeight` | reserve verified kg | commit measured weight | return kg if allowed | immutable measured weight |
| `LocalAirportSlot` | book time interval/quota | confirm slot | cancel/release slot according to policy | slot/time/location ref |
| `ExplicitUnlimited` | تجاری، بدون stock subtraction | confirm entitlement | cancel entitlement | reason, no fake stock count |
| `ExternalHoldConfirm` | provider hold receipt | provider confirm receipt | provider release/cancel if available | ProviderUnitRef/ProviderOperationRef |
| `ExternalDirectConfirm` | no external hold: PendingCommit | after verified guaranteed payment/approval provider commit | cancel only if provider supports | truthful tentative state, operational risk |
| `ManualConfirmation` | PendingManual + operator work item | confirmed **only with verified manual result** | manual cancel with evidence | agent/time/supplier proof |
| `NoBookingProcessRequired` | commercial record; eligible | confirm after allowed Order/payment gate | status reversal only | no fictitious supplier hold |
| `Seat` | commercial price/selection only | requires coordinator's physical FlightFlow evidence | commercial cancel; physical in FlightFlow | no Ancillary physical seat stock |

## 12.2 قوانین شکست/جبران
- یک Unit محلی به‌صورت Atomic و idempotent ظرفیت مصرف می‌کند؛ در ۲ درخواست همزمان بیش از ظرفیت معتبر صادر نمی‌شود. Release/Cancel دوباره نباید دو بار ظرفیت برگرداند.
- در Supplier API call پیش از جواب ممکن است Provider عملیات را انجام داده باشد. Timeout ⇒ `Pending` و `PendingReasonCode=Reconciliation`؛ retry با **همان** Provider operation key و GET/status پیش از ایجاد booking دوم. وضعیت «موفق» یا «شکست قطعی» جعل نشود.
- اگر Group شامل ۳ Supplier است و یکی failure قطعی دارد، موفقیت بقیه با Unit refs حفظ می‌شود؛ Ordering تصمیم به compensation/retry می‌گیرد. درخواست Group به هیچ وجه یک transaction مشترک بانکی/هواپیمایی نیست.
- اگر Confirm از لحاظ Provider ممکن نیست اما Payment guarantee معتبر است، تا دریافت ProviderResult `Pending` است؛ Payment captured الزاماً به معنی Supplier Confirmed نیست و در صورت ناتوانی در fulfillment، Ordering باید refund/rebook/notification را مدیریت کند.
- اگر Provider اصلاً Hold را پشتیبانی نمی‌کند، Stage 3 با `ExternalDirectConfirm` و policy verified معامله می‌کند. برای paid item، Ordering قبل از commit باید payment authorization/guarantee مناسب و راه جبران داشته باشد؛ ترتیب برای Providerهای متفاوت قابل تنظیم است، نه hardcoded یک نسخه.
- Manual Supplier فقط توسط اپراتور مجاز می‌تواند evidence رویداد `ProviderOutcomes` بدهد؛ `PendingManual` در **PendingReason** است نه وضعیت مستقل اجباری. نبود evidence = Pending.
- اگر Confirm/Cancel دیرتر از expiry یا cancellationQuoteTTL برسد، authoritative recheck و تشخیص اثر انجام‌شده لازم است؛ side-effect بدون idempotency دوباره اجرا نشود.

## 12.3 Reservation references و partial operations
`HoldId` ۱ group؛ `UnitId` برای هر service؛ provider may have ۱ ProviderRef برای چند Unit، اما mapping Unit→ProviderRef explicit باشد و هیچ حدس زده نشود. `POST .../Releases` و `POST .../Cancellations` فقط روی UnitIdهای انتخاب‌شده اثر کنند. برای order-scoped CIP چند beneficiary مجاز اگر supplier policy اجازه دهد، یک Unit ممکن است چند covered persons داشته باشد؛ Qty/Price فقط یک‌بار ثبت شود.

## 12.4 Inventory source missing
- Missing Policy برای nonSeat با availability-required ⇒ `NeedsVerification` در Shopping و Hold noncommittal.
- Local/External provider unavailable ⇒ `Pending` یا `Failed` با reason، نه Unlimited.
- A07/A08/A09 Seat هیچ `AncillaryInventoryPolicy` ندارد؛ تصویر صندلی و رزرو physical در FlightFlow است و در این سرویس دوباره کپی نمی‌شود.

## 12.5 Post-sale changes
افزودن Meal/Baggage به Order صادرشده، درخواست جدید در Ordering است که Ancillary را با Order commercial context re-shop می‌کند؛ Unit جدید Hold و Confirm می‌شود. حذف/تغییر انتخاب، واحد قدیمی Cancel + انتخاب جدید Hold/Confirm با اختلاف قیمت/Refund جدا، بدون mutation تاریخچه اولیه. Flight disruption/irregular ops/aircraft swap ممکن است Availability و pricing قبلی را باطل کند؛ Owner orchestrator تکلیف Unit/EMD/physical assignment را هماهنگ می‌کند.

## 12.6 Event and eventual consistency contract
از الگوهای Event/Outbox فعلی سرویس پیروی شود؛ دامین Eventهای موردنیاز S3 با پیشنهاد نام:
`AncillaryHoldOutcomeChanged`, `AncillaryUnitConfirmed`, `AncillaryUnitReleased`, `AncillaryUnitCancelled`, `AncillaryUnitProviderReconciliationRequired`.

حداقل payload همه: `EventId`, `OccurredAtUtc`, `CorrelationId`, `OrderId`, `HoldId`, `UnitId`, `OrderServiceId`, `Status`, `ReasonCode?`, `ProviderUnitRef?` فقط Service scope مجاز، `Quantity`, `EffectiveAt` در صورت نیاز. `OrderPaymentStatus` یا EMD number از Ancillary صادر نشود مگر fact از مالک آن دریافت شده و لازم برای correlation باشد. **اینها Target event names هستند؛ وجودشان در فعلی ادعا نمی‌شود.**


<!-- END: 12-S3-LOCAL-SUPPLIER-MANUAL-COMPENSATION.md -->


---

<!-- BEGIN: 13-CANCELLATION-PENALTY-REFUND-AND-DOCUMENTS.md -->

# 13 — End-to-End Servicing: Cancel، Penalty، Refund، Voucher، Void و EMD

## 13.1 چهار عمل مستقل
1. **`Release`**: Hold هنوز Confirm نشده را آزاد می‌کند؛ محاسبه Refund الزامی نیست مگر Provider هزینه cancellation/late release معتبر داشته باشد و Ordering واقعاً وجه دریافت کرده باشد.
2. **`Cancel`**: entitlement/reservation تاییدشده را طبق شرایط Supplier/Carrier لغو می‌کند. لزوماً حق Refund ایجاد نمی‌کند.
3. **`Void/Refund Document`**: Ticketing/Ordering روی `ETKT`, `EMD-A`, `EMD-S` و coupon انجام می‌دهد؛ Ancillary EMD صادر یا Refund نمی‌کند.
4. **`Refund Payment/Wallet/Cash/Credit`**: فقط Payment/JetPay و Accounting/Ordering با مرجع پرداخت واقعی. این مرحله پس از Eligibility و تنظیم Document انجام می‌شود، یا طبق workflow تامین‌کننده و payment method با جبران معتبر؛ نتیجه مالی مستقل از لغو عملیاتی است.

## 13.2 چرا Settlement موجود کافی نیست؟
در کد `SettlementDefinition` فقط `ReissueRefund:ReissueRefundPolicy` (`Refundable=1`, `NonRefundable=2`, `NonRefundableReusable=3`)، `FormOfRefund` (`OriginalPayment=1`, `EVoucher=2`)، `Commissionable:bool`, `InterlineSettlement:bool` دارد. **این فیلدها مبلغ Penalty یا Tax refundable را تعریف نمی‌کنند.** بنابراین از `Refundable` نباید Penalty=0 یا 100% return حدس زد.

### کمینه توسعه `[TARGET]`
- از `ReissueRefundPolicy` و `FormOfRefund` فعلی برای eligibility/base disposition استفاده شود؛ بدون ساخت «RefundAggregate» داخل Ancillary.
- برای Carrier-filed cancellation fees، تنها یک Rule ValueObject **اختیاری** زیر Provision (مشترک با Settlement) کافی است (نه Aggregate جدید): `CancellationRules[]` که **مدیریت آن در Stage 1 از دو Endpoint مشترک `GET/PUT /Backoffice/v1/AncillaryProvisions/{provisionId}/CancellationTerms` بسته می‌شود و فاز ۳ فقط آن را می‌خواند**، از `{RuleRef, ReasonKind, AppliesFromUtc?/BeforeStartMinutes?, AppliesUntilUtc?, NoShowApplicability?, PenaltyKind, PenaltyFixedAmount?, PenaltyPercentage?, PenaltyCurrencyId?, PercentageBase?, WaiverPermitted?, SourceReference?}`. لازم نیست همه فیلدها برای هر Variant پر شوند؛ هر Rule باید غیرمتناقض و قابل محاسبه باشد.
- `PenaltyKind` target constrained values: `None=1`, `FixedAmount=2`, `Percentage=3`, `ProviderQuoted=4`, `ManualReview=5`. اگر Provider معتبر quote مستقیم می‌دهد، `ProviderQuoted` انتخاب شود؛ اگر فقط Refundable flag داریم، **QuoteState=RequiresManualReview** نه Penalty=0.
- `ReasonKind` target values: `Voluntary=1`, `Involuntary=2`, `SupplierFailure=3`, `NoShow=4`; `WaiverRef` فقط با شاهد صادره از منبع معتبر airline/disruption، نه string آزاد آژانس.
- Tax refundability per `TaxTreatment` و authoritative issuance/coupon context جدا مشخص شود؛ مالیات را به‌صورت global refunded فرض نکنید.

## 13.3 دو Quote مجزا؛ حفظ مسئولیت مالی
### Quote 1 — **Ancillary Service Cancellation Quote** (S2S)
Endpoint: `POST /Service/v1/Ancillaries/Service-Confirmations/{holdId}/CancellationQuotes`.

**حداقل Unit detail:** `unitId`, `canCancel`, `canRefund`, `quoteState=Quoted|NotPermitted|NeedsManualReview`, `originalServiceAmount?`, `currencyId`, `penaltyAmount?`, `penaltyCurrencyId?`, `penaltyKind`, `providerCancellationCharge?`, `refundableServiceComponents[]`, `nonRefundableComponents[]`, `taxComponentDisposition[]`, `ruleRef?`, `providerQuoteRef?`, `waiverApplied`, `waiverEvidenceRef?`, `noShow/used evidence`, `quoteValidUntilUtc`, `reasonCodes[]`.

### Quote 2 — **Ordering Refund Quote** (مشتری/آژانس)
Ordering این Quote را با `OrderServiceAcceptedTotal`, coupon/EMD state, payment transactions, agent/customer/commission, prior refunds, currency/FX, wallet/agency credit, voucher option و caps ترکیب می‌کند. خروجی `netAmountToRefund`, `refundDestination`, `documentAction`, `serviceCancellationAction`, `penaltyBreakdown`, `isFinal/approvalNeeded` است. Service quote مستقیماً دستور Bank/Wallet refund نیست.

**قانون ساده:** `NetRefund = max(0, refundablePaidComponents - legitimatePenalties - nonRefundablePaidFees - priorRefundedAmount)` **فقط روی اقلامی که واقعاً قبلاً پرداخت شده‌اند** و پس از policy سند/Tax/commission. اگر Penalty جداگانه بیشتر از مبلغ استرداد است، هیچ مبلغ منفی را به Payment Refund نفرستید؛ هر بدهی جدید در Ordering/Payment فرآیند جدا دارد. مثال عددی به‌تنهایی قانون مالی همگانی نیست.

## 13.4 Flow عمومی کنسلی و Refund (تحت Ordering)
1. Surface مجاز ⇒ `POST /{Surface}/v1/Orders/{orderId}/Ancillaries/CancellationQuotes` با `orderServiceIds[]`, `reasonKind`.
2. Ordering authorization/ownership+coupon/EMD/payment/usage validation؛ `UnitId[]` را از OrderService links به دست می‌آورد.
3. Ordering ⇒ Ancillary S2S `CancellationQuotes`؛ Ancillary Rule/Supplier quote را با current status/reason/expires بررسی می‌کند.
4. Ordering ⇒ Ticketing/Payment checks و یک **OrderCancellationQuoteRef** با مبلغ، جریمه، اسناد و destination ایجاد می‌کند؛ به Surface نشان می‌دهد. `ManualReview` به‌جای عدد نامعلوم.
5. User/Agency مجاز تأیید می‌کند: `POST /{Surface}/v1/Orders/{orderId}/Ancillaries/Cancellations` با `orderCancellationQuoteRef`.
6. Ordering Quote، price/coupon/supplier status را revalidate و cancellation workflow را شروع می‌کند؛ Ancillary `.../Cancellations` فقط Units انتخاب‌شده را با `cancellationQuoteRef` معتبر لغو می‌کند؛ physical seat cancellation از FlightFlow به‌صورت جدا.
7. Document owner EMD/ETKT `Void` یا `Refund` / coupon adjustment را مطابق قواعد انجام می‌دهد؛ Payment Refund/Wallet/agency settlement توسط مسئول پول؛ نتیجه‌ها ممکن است async باشند. **ترتیب دقیق commit هر Provider طبق قابلیت جبران، pending/unknown evidence و سند است**؛ نباید فرض شود همگی atomically تغییر می‌کنند.
8. Ordering `OrderChange`, `OrderServiceStatus`, penalty/refund/payment refs و immutable history را ثبت و وضعیت قابل پیگیری به Surface می‌دهد.

## 13.5 سناریوها و احکام خاص
| سناریو | Service operation | Document/Financial | Rule |
|---|---|---|---|
| مشتری قبل Confirm انصراف دهد | Release Held | اگر وجه capture شده، مالی جدا | معمولاً EMD نداریم؛ supplier late-release fee فقط با evidence |
| مشتری بعد Confirm ولی قبل Issue انصراف دهد | Cancel confirmed | document yet absent؛ refundable amount ممکن | enforce provider policy و penalty |
| paid bag با EMD-A صادرشده | Cancel bag اگر مجاز | EMD-A coupon refund/void توسط Ticketing؛ Payment توسط owner | flight coupon state مهم |
| Lounge EMD-S | Cancel Supplier + entitlement | EMD-S standalone refund/void و payment | no flight coupon dependency unless rule says |
| Free SSR | Cancel SSR اگر ممکن | غالباً مبلغ refund صفر چون پرداخت صفر | Source Free روشن باشد |
| NonRefundable ticketed ancillary | Cancel ممکن یا ناممکن | `NetRefund=0` یا voucher طبق policy | `Refundable` ≠ `CanCancel` |
| Reusable non-refundable | Cancel/credit | فقط credit reuse با شرایط اصل قرارداد | cash refund جعل نشود |
| بعد از استفاده از سرویس | معمولاً blocked | penalty/refund طبق UseStatus/provider | صرف Paid ⇒ Refundable نیست |
| No-show | reason `NoShow` | filed/supplier rule/waiver | تاریخ قطعی سفر/airport time zone |
| Flight disruption involuntary | cancel/reaccommodate | waiver/EMD/Payment طبق verified disruption | reason/waiverRef لازم |
| Partial cancel یک Order | فقط UnitIdهای انتخابی | فقط اسناد/قیمت همان Units | سایر Services untouched |
| Payment با wallet/agency credit/cash | same service cancel | refund destination تابع funding liability | Ancillary payment model ندارد |
| supplier cancel timeout | PendingReconciliation | **پول خودکار refund قطعی نشود** تا service/document effect معلوم شود | recover idempotently |
| اختلاف نرخ پس از Quote | quote stale | درخواست تأیید جدید | بدون silent penalty increase |
| خرید پس از ticket به صورت ancillary add-on | عملیات OrderChange + Hold Confirm | EMD/Payment افزایشی | AirAvail priced offer اولیه ضروری نیست |
| refund فقط taxes (service nonrefundable) | supplier/document quote | tax component refundable موردی | Gross zero فرض نشود |

## 13.6 تفاوت EMD-A/EMD-S و A10
`AncillaryDocumentType` فعلی `None=1`, `EmdAssociated=2`, `EmdStandalone=3`; `DocumentRouting` `NoAncillaryDocument=1`, `Emd=2`, `TicketOrExchange=3`. A10 Cabin Upgrade گاهی تعویض بلیت می‌خواهد و نمی‌شود آن را به EMD عمومی تبدیل کرد؛ تصمیم doc routing از Definition و Ordering ticket context می‌آید. EMD-A به coupon/e-ticket مرتبط است؛ EMD-S standalone است. تغییر Service نباید بی‌اجازه خودکار coupon پرواز را Cancel کند؛ تغییر itinerary و involuntary disruption هماهنگ‌کننده Ordering دارد.

## 13.7 Refund quote expiry و double refund
- هر QuoteId به specific OrderServiceId+UnitId+reason+amount+document snapshot+recipient destination+expiry و user approval متصل است؛ دوباره استفاده پس از commit ممنوع.
- Ancillary Unit `Cancelled` به معنی `Refunded` نیست. Ordering transaction/PaymentRef/DocumentCoupon status را مستقل نگه می‌دارد؛ repeat request همان نتیجه را با idempotency برمی‌گرداند.
- Chargeback/failed refund/partial processor settlement به Payment/Ordering error/retry stream می‌رود؛ Ancillary status کاذب تغییر نکند.

## خروجی
Ancillary قادر به اعلام Service cancellation eligibility، penalty و اجرای cancellation واقعی است؛ کل چرخه مالی و EMD در Ownerهای درست مدیریت می‌شود، با quote دو مرحله‌ای مشابه الگوهای صنعت.


<!-- END: 13-CANCELLATION-PENALTY-REFUND-AND-DOCUMENTS.md -->


---

<!-- BEGIN: 14-INTERSERVICE-CONTRACTS-AND-OWNERSHIP.md -->

# 14 — Integration قراردادهای PSS: AirAvail، FlightFlow، Ordering، JetPay، Ticketing

## 14.1 Ownership table
| Fact/Action | صاحب حقیقت | قرارداد Ancillary |
|---|---|---|
| AirFare/FareFamily/FareBasis/RBD/Allowance/Price Offer | AirPrice → AirAvail | S2S `FlightOffers/Details`، فقط projection shopping |
| FlightId/FlightCapacityId/FlightVersion/Seat occupancy/Physical Hold | FlightFlow | Read-only occupancy/layout و physical seat evidence توسط Ordering، no local seat stock |
| Ancillary Definition/Provision/Pricing/Option/Selection/Service cancellation terms | Ancillary | read Shop/Quote و S2S Hold/Cancel |
| Ownership user, Order/OrderService/Traveller, PNR, full Order history | Ordering | supplied/trusted Order context and OrderServiceId links |
| Payment guarantee/authorize/capture/refund/wallet/cash/agency credit | Payment/JetPay | Ordering orchestrates; Ancillary no money movement |
| E-ticket, EMD-A/EMD-S, coupon status, void/exchange/refund | Ordering/Ticketing | Ancillary DocumentRouting metadata only |
| Travel Agency eligibility, Office/PCC/commission and customer channel | authorized Sales/Auth/Ordering reference | Caller-derived trusted sales scope, no public office leaks |
| Supplier actual hold/cancel/confirmation | Fulfillment supplier/provider | provider port with scoped refs and evidence |

## 14.2 AirAvail [CURRENT]
`POST /Service/v1/FlightOffers/Details` `{offerId}` — priced `AirOffer`، validating offer context؛ پاسخ شامل Tickets/Travellers, AirTransports/Flights, FareBasis/FareFamily, allowances, Currency, FX, LastTicketingDate. `AirOfferId` الزام Order هنوز نیست؛ PreOrder Shopping از همین داده شروع می‌شود.

`FlightId` != `FlightCapacityId`؛ S2 `FlightRef` می‌تواند flight instance باشد، اما physical FlightFlow Hold با `FlightCapacityId` مربوط انجام می‌شود. `FareFamily` فعلی در `ServiceOfferFareComponentDto` `string?` است؛ آن را به ID عددی دلخواه تبدیل نکنید. Source `ValidUntilUtc` در DTO منتشرشده نیست.

## 14.3 FlightFlow [CURRENT endpoint observed; EXTERNAL environment separate]
- `POST /Service/v1/Flights/OccupiedSeats` read occupancy.
- `POST /Service/v1/Flights/Seat-Holds` {IdempotencyKey,Reference,ExpiresAt,Passengers[],Flights[FlightCapId,Seats[PaxReference,Revenue,Seat]]} → `{HoldId,Seats[FlightId,PaxReference,Seat,SeatHoldReference,SeatHoldStatus]}`.
- `POST /.../Seat-Holds/{id}/Confirmations`, `PATCH /.../Seat-Holds/{id}`, `DELETE /.../Seat-Holds/{id}`, `POST /.../Seat-Confirmations/{id}/Cancellation` (+ `SeatHoldReferences[]`).

این قرارداد الگوی Group+Unit reference و Partial Cancellation را فراهم می‌کند؛ **Ancillary نباید flight seat reservation را با API خودش دوباره اجرا کند.** در هماهنگی Seat Purchase، Ordering فیزیکی FlightFlow و تجاری Ancillary را با Context یکسان map می‌کند. FlightFlow اخیراً منتشرشده در GitHub به معنی deployed بودن route نیست؛ Owner این وابستگی را **non-blocking برای کد Ancillary** تعیین کرده است.

## 14.4 Ordering [TARGET ONLY — source inaccessible]
### Internal context reader required
`IOrderCommercialContextReader.GetAsync(orderId,callerScope)` باید Snapshot معتبر Order را برای Shopping پس از خرید ارائه کند:
`OrderId,CommercialVersion,OwnerAirlineId,Channel/POS,BuyerSellerReference(optional internal),Travellers[OrderTravellerId,AirOfferTravellerRef?,PTC],Flights[FlightId,FlightVersion,FlightCapacityId,Cabin/FareBasis/FareFamily,BoundRef,Airline],OrderServices[OrderServiceId,Status/Document],Tickets/Coupons[Status,Used/Flown/Refunded],Funding/SettlementSummary minimal,PurchaseStage`.

**این یک پورت دامنه پیشنهادی است؛ وجود REST endpoint در Ordering ادعا نمی‌شود.** سرویس Ordering باید بسته Contract/URL نهایی را با Owner خود تطبیق دهد؛ Ancillary نباید DTO واقعی Ordering را از روی نام حدس بزند.

### Public Selling/Servicing entry points (TARGET, not Verified)
Prefix `{Surface}` = `Backoffice`, `Api`, `OtaPanel`.

| Intent | Suggested Ordering route | نکته |
|---|---|---|
| ایجاد سفارش پرواز با انتخاب Ancillary | `POST /{Surface}/v1/Orders` | referenced priced AirOffer+selected ancillary refs؛ conforms current Ordering lifecycle after source alignment |
| مشاهده Order/Services | `GET /{Surface}/v1/Orders/{orderId}` | Order status, payment and document refs |
| افزودن خدمات به Order موجود | `POST /{Surface}/v1/Orders/{orderId}/Ancillaries` | input selections و payment intent؛ managed as OrderChange |
| بررسی امکان لغو/مبلغ/جریمه | `POST /{Surface}/v1/Orders/{orderId}/Ancillaries/CancellationQuotes` | returns **OrderCancellationQuoteRef** و full customer refund summary |
| اجرای لغو تاییدشده | `POST /{Surface}/v1/Orders/{orderId}/Ancillaries/Cancellations` | accept quoteRef/selected OrderServices؛ async if necessary |
| وضعیت کارهای تغییر و refund | `GET /{Surface}/v1/Orders/{orderId}/Ancillaries/Changes/{changeId}` | واحدهای Cancel/EMD/Payment statuses، immutable history |

**Confirm و Payment Refund نیازی به Endpoint عمومی موازی داخل Ancillary ندارند.** Checkout/issuance/Refund orchestration داخل Ordering انجام می‌شود و Surface فقط نتیجه/Status را می‌خواند. مسیرهای پیشنهادی در Repo Ordering قبل از اعمال **باید با مسیرهای واقعی آن تطبیق و در صورت تعارض بدون duplication اصلاح شوند**؛ تغییر آن repo خارج Scope Agent Ancillary است.

## 14.5 Payment/JetPay [INTERFACE CONTRACT]
Ancillary endpoint Payment ندارد. برای حفظ E2E، Ordering این intentها را از Payment owner می‌گیرد: `CreateIntent/Guarantee/Authorize/Capture/ReleaseAuthorization/Refund/PartialRefund/RefundStatus`, wallet ledger/agency-credit/cash settlement. Actual API routes بر اساس قرارداد JetPay معتبر شوند؛ **هیچ مسیر خیالی `/Service/v1/Payments/Refunds` را به‌عنوان Current اعلام نکنید.**

ترتیب `Hold -> Payment guarantee -> Confirm` برای Providerهای HoldConfirm استاندارد داخلی است. DirectConfirm supplierها می‌توانند بعد از payment authorization یا credit guarantee تایید شوند و در شکست Supplier، Compensation/Refund لازم است. فروش Airport Cash یا Agency billing با تعهد معتبر ممکن است Payment فوری نداشته باشد؛ Eligibility مالی را Ordering تعیین می‌کند.

## 14.6 Ticketing / EMD contract
Ordering از Ancillary per Unit این fields را می‌خواهد: `OrderServiceId,HoldId,UnitId,ProviderUnitRef,Traveller/Coverage,Price/currency/tax components,ServiceSubCode/Source,DocumentRouting,AncillaryDocumentType,RFIC/RFISC,BookingMethod,SSR/SSIM,ConfirmationRequirement,Service Status,Refundability/Eligible cancellation terms`.

Ticketing EMD-A با coupon مرتبط، EMD-S standalone، ETKT exchange برای upgrade؛ شماره سند و state `Issued|Exchanged|Refunded|Void` را فقط Owner سند ایجاد می‌کند. Ancillary event lifecycle نباید بدون OrderDocument proof EMD issued ادعا کند.

## 14.7 Webhook/events [TARGET minimal]
`AncillaryHoldOutcomeChanged`, `AncillaryUnitConfirmed`, `AncillaryUnitCancelled`, `AncillaryUnitReleaseCompleted`, `AncillaryProviderOutcomeUnknown`؛ payload `EventId,CorrelationId,OrderId,OrderServiceId,HoldId,UnitId,ProviderKey?(internal),Status,OccurredAtUtc,ReasonCode?,ChangedAt`. Contract واقعی Messaging را پیش از انتشار با Ordering مطابقت دهید؛ cross-office PCC/PII leak ممنوع. Event حداقل یک‌بار، Ordering idempotent consumption/reconciliation.

## 14.8 Auth cross-surfaces
- Backoffice: Caller Airline/Office/POS مجاز؛ internal manual fulfillment only approved role.
- Api: Agency/Partner authenticated؛ همان Order owner و SupplierProducts، no forged sales scope.
- OtaPanel: همان business rules ولی mapper/view جدا، نه Caller bypass.
- Service: trusted Ordering internal; dedicated capabilities برای Manual Supplier webhooks؛ External provider secret internal only.

## 14.9 No accidental external dependency blocking
Stage 1 and 2 ذاتاً با مرجع کد و صحت Internal Contracts قابل بسته شدن‌اند؛ اگر real FlightFlow/Ordering/Payment environment ready نیست، `EXTERNAL_PENDING` جدا در Report بیاید. P3 Ancillary closure صرفا صحت قرارداد و رفتار خود Ancillary را ثابت می‌کند؛ Cross-service E2E verification فقط وقتی همه سرویس‌ها مستقرند، جداگانه قابل اعلام است.


<!-- END: 14-INTERSERVICE-CONTRACTS-AND-OWNERSHIP.md -->


---

<!-- BEGIN: 15-END-TO-END-WIRE-EXAMPLES.md -->

# 15 — نمونه قراردادهای End-to-End (TARGET، نه ادعای وجود در کد)

تمام ID/Amountها صرفاً **مثال خوانایی** هستند. JSONهای زیر «شكل مطلوب Wire» را نشان می‌دهند؛ SeatMaps JSON عمداً بازنویسی نمی‌شود زیرا **خروجی واقعی آن در فصل 09 فریز است**.

## 15.1 Shopping فهرست محصول غیر Seat
```http
POST /Api/v1/AncillaryOffers
Content-Type: application/json
```
```json
{"offerId":"priced-air-offer-A"}
```
پاسخ شامل `offerItemRef` به ازای Candidate محصول و Scope مجاز، `selectionKind`, `priceStatus`, `currency/price`, `quantity min/max`, `offerReadiness` و `reasonCodes` است. Caller نیازی به `OrderId` ندارد؛ POP/POS را از Authorization دریافت می‌کند.

## 15.2 Details واقعی Paid Meal (A12)
```http
POST /Api/v1/AncillaryOffers/Details
```
```json
{"offerId":"priced-air-offer-A","offerItemRef":"offer-item-meal-ADT-F1"}
```
پاسخ **additive بخش مربوط به انتخاب**:
```json
{
  "offerItemRef":"offer-item-meal-ADT-F1",
  "profile":"Meal", "variantCode":"A12",
  "coverageScope":"Sector", "travellerRefs":["ADT-1"], "flightRefs":["F1"],
  "selectionContract":{"kind":"QuantityChoice","fields":[
    {"name":"MenuItemRef","type":"Code","required":true},
    {"name":"Quantity","type":"Integer","required":true}]},
  "optionGroups":[{"fieldName":"MenuItemRef","sourceState":"Known","choices":[
    {"optionRef":"opt-menu-VGML","value":"VGML","label":"Vegetarian meal","pricePreview":{"currencyId":70,"unitTotal":"250000","isComplete":true}},
    {"optionRef":"opt-menu-CHML","value":"CHML","label":"Children's meal","pricePreview":{"currencyId":70,"unitTotal":"180000","isComplete":true}}
  ]}],
  "quantity":{"unit":"Each","min":1,"max":4},
  "priceOrigin":"Filed"
}
```
قیمت/Choiceها فقط وقتی supplier/catalog معتبر واقعاً آن را بازگرداند. اگر Supplier option list ندارد، `optionGroups=[]`, `sourceState=Unknown/NotApplicable` و Typed fields/constraints کافی است؛ بستن فروش با صفر گزینه مجهول غلط است.

## 15.3 Evaluate انتخاب نهایی یک Ancillary
```http
POST /Api/v1/AncillaryOffers/Selections/Evaluate
```
```json
{"offerId":"priced-air-offer-A","offerItemRef":"offer-item-meal-ADT-F1","selection":{"menuItemRef":"VGML","quantity":2}}
```
پاسخ افزوده به DTO فعلی:
```json
{
  "status":"Accepted", "offerItemRef":"offer-item-meal-ADT-F1",
  "currentOfferItemRef":"offer-item-meal-ADT-F1",
  "pricedSelectionRef":"psel-891-2xVGML-r4",
  "price":{"currencyId":70,"completeUnitTotal":"250000","requestedQuantityTotal":"500000","isTotalComplete":true},
  "scope":{"travellerRefs":["ADT-1"],"flightRefs":["F1"],"coverageScope":"Sector"},
  "validUntilUtc":"2026-10-16T19:00:00Z", "fieldIssues":[]
}
```
اینجا `pricedSelectionRef` شکل **illustrative** دارد؛ در تولید opaque/random lookup key خواهد بود. `status=NeedsQuote/NeedsVerification/Rejected` ⇒ no purchasable pricedSelectionRef.

## 15.4 SeatMap بدون تغییر Output
```http
POST /OtaPanel/v1/SeatMaps
```
```json
{"offerId":"priced-air-offer-A"}
```
پاسخ دقیق DTO فعلی فصل 09 و Base Project است: `meta`, `data[].decks[].cabins[].items[].travelerPricing[].id/seatAvailabilityStatus/price`, `dictionaries`. خرید Seat در Ordering از **همان existing seat item `id`** با `offerId+Pax+FlightRef` Resolve می‌شود، بدون فیلد جدید در SeatMap.

## 15.5 ایجاد Order و Hold گروهی (از طریق Ordering)
UI ⇒ Ordering (route نهایی با واقعی Ordering تطبیق شود) `POST /{Surface}/v1/Orders` با AirOffer/انتخاب‌ها و traveler data. Ordering یک `OrderId` و برای هر انتخاب `OrderServiceId` می‌سازد، سپس S2S:
```http
POST /Service/v1/Ancillaries/Service-Holds
```
```json
{
 "orderId":81235,"reference":"ORDER-81235","idempotencyKey":"O81235-H1",
 "requestedExpiresAt":"2026-10-16T19:30:00Z",
 "services":[
   {"orderServiceId":891,"pricedSelectionRef":"psel-bag-P1-2pcs","travellerId":154,"coverageScope":"Portion","coveredFlightIds":[8001,8002]},
   {"orderServiceId":892,"pricedSelectionRef":"psel-lounge-order","travellerId":null,"coverageScope":"Order","coveredFlightIds":[]}
 ]
}
```
Response target:
```json
{"holdId":11882,"orderId":81235,"status":"Pending","outcome":"Partial",
 "units":[
   {"unitId":11901,"orderServiceId":891,"status":"Held","providerUnitRef":"BAG-H-27","quantity":2},
   {"unitId":11902,"orderServiceId":892,"status":"Pending","pendingOperation":"Hold","pendingReasonCode":"ManualConfirmation","providerUnitRef":null,"quantity":1}
 ]}
```
**Ordering نمی‌تواند کل Group را Confirmed اعلام کند تا Unit دوم evidence معتبر بگیرد؛ Unit اول از بین نرفته است.**

## 15.6 Confirmation/Provider callback/Recovery
```http
POST /Service/v1/Ancillaries/Service-Holds/11882/Units/11902/ProviderOutcomes
```
```json
{"providerOperationRef":"REQ-CIP-07","result":"Held","providerUnitRef":"CIP-H-552","evidenceAtUtc":"2026-10-16T19:10:00Z","sourceKind":"AuthorizedManual"}
```
Caller internal approved operator only. سپس Ordering بعد payment/guarantee:
```http
POST /Service/v1/Ancillaries/Service-Holds/11882/Confirmations
```
```json
{"idempotencyKey":"O81235-CONFIRM-1","unitIds":[11901,11902],"paymentOrGuaranteeReference":"PG-81235"}
```
اگر Provider دوم Confirm را نمی‌پذیرد، Unit2 `Pending(Confirm)` است؛ Group `Pending/Mixed`، نه Confirmed. بعد reconnect GET Hold و Retry همان Unit، نه ساخت Group جدید.

## 15.7 CancellationQuote جزئی و Refund مالی
```http
POST /Service/v1/Ancillaries/Service-Confirmations/11882/CancellationQuotes
```
```json
{"orderId":81235,"unitIds":[11901],"reasonKind":"Voluntary"}
```
Service quote target:
```json
{"cancellationQuoteRef":"anc-cq-1","holdId":11882,"orderId":81235,
 "validUntilUtc":"2026-10-16T20:00:00Z",
 "units":[{"unitId":11901,"orderServiceId":891,"canCancel":true,"canRefund":true,
   "quoteState":"Quoted","originalServiceAmount":"1800000","currencyId":70,
   "penaltyAmount":"350000","penaltyCurrencyId":70,"penaltyKind":"FixedAmount",
   "ruleRef":"BAG-CANCEL-RULE","waiverApplied":false,"requiresManualReview":false}]}
```
Ordering این مورد را با `EMD-Coupon` و پرداخت/Tax و Refund قبل بررسی می‌کند و **OrderCancellationQuoteRef** جدا به مشتری ارائه می‌دهد. پس از تأیید:
```http
POST /Service/v1/Ancillaries/Service-Confirmations/11882/Cancellations
```
```json
{"idempotencyKey":"O81235-CANCEL-11901","orderId":81235,"unitIds":[11901],
 "cancellationQuoteRef":"anc-cq-1","reasonKind":"Voluntary"}
```
مبلغ پرداخت نهایی در Ancillary هیچ API ندارد؛ همان‌جا `UnitStatus=Cancelled` فقط معنای لغو entitlement دارد. Ordering EMD و Payment Refund را پیگیری می‌کند.

## 15.8 Add-on پس از Ticket
`GET /{Surface}/v1/Orders/{orderId}` ⇒ Shop همان Engine با trusted Order Context؛ `Details` ⇒ `Evaluate` ⇒ `pricedSelectionRef` ⇒ `POST /{Surface}/v1/Orders/{orderId}/Ancillaries` (Ordering target) ⇒ new OrderService/Unit, payment/guarantee, provider confirm, EMD issue. **هیچ AirOffer قدیمی را به‌عنوان منبع post-order price مجدداً مصرف نکنید**.


<!-- END: 15-END-TO-END-WIRE-EXAMPLES.md -->


---

<!-- BEGIN: 16-END-TO-END-SCENARIO-COVERAGE-MATRIX.md -->

# 16 — سناریوهای E2E و ماتریس تصمیم دامنه

این فهرست **رفتارهای قابل مشاهده و قراردادی** است، نه دستور نوشتن تست یا انتخاب دیتابیس. هر مورد باید نتیجه روشن (Selectable/Blocked/Pending/Confirmed/Cancelled/Refund) و Owner مشخص داشته باشد. ۲۴ Variant در فصل 06 جدا پوشش داده شده‌اند؛ این جدول چالش‌های بین‌سرویسی و چرخه عمر را اضافه می‌کند.

## 16.1 Authoring/Commercial (S1)
| # | سناریو | رفتار الزامی |
|---|---|---|
| C01 | ایجاد Definition هر یک از ۹ Profile | فقط spec همان Profile؛ Owner/Supplier معتبر |
| C02 | ارسال BaggageRule به Meal form | schema reject؛ no hidden columns |
| C03 | ویرایش Draft Seat موجود | Backend همان DTO/Route؛ front working unchanged |
| C04 | انتشار Provision بدون active pricing (Filed) | reject/pending publish، no sellable offer |
| C05 | Free Service با PriceOrigin.Free معتبر | Zero with evidence، not zero fallback |
| C06 | Price revise after Publish | old snapshot price stale؛ new active revision |
| C07 | POS null/empty vs nonempty | null/empty unrestricted sale rule؛ nonempty trusted POS only |
| C08 | Date range/blackout/seasonality/daytime | blocked out-of-period؛ timezone-aware |
| C09 | Country/station/TaxCode fee rules | monetary components correct and isolated |
| C10 | nonSeat Local Count/Weight/Slot policy | only verified configured stock consumable |
| C11 | No Seat InventoryPolicy | Seat Definition/Provision/Pricing work independently |
| C12 | A09 ExtraSeat special DocumentRouting | no regular seatmap cell substitution |

## 16.2 Shopping / Options / Pricing (S2)
| # | سناریو | رفتار الزامی |
|---|---|---|
| S01 | ۲ Pax ADT/CHD × ۲ Bound | per Pax/Flight/Portion offers and prices correctly |
| S02 | Included bag only one coupon | upsell extra only where allowed; no duplicate allowance |
| S03 | A01 Qty1 vs Qty2 | same CandidateRef permissible; different pricedSelectionRef and exact totals |
| S04 | A02 5kg vs 10kg package | real optionRef/weight package; different price and selection refs |
| S05 | A03 measured weight absent | total unknown; no pricedSelectionRef |
| S06 | A04 oversize + A03 overweight | charge combination follows filed rule, not arbitrary double fee |
| S07 | A11 SSR Free vs A12 Paid Menu | separate Definition/PriceOrigin, no inferred free paid meal |
| S08 | A12 MenuChoice source outage | source unknown/not verified; no fake option |
| S09 | Pet cabin vs hold, weight, docs | typed constraints + possible provider pending |
| S10 | Wheelchair/UMNR/medical | required PTC/SSR/guardian or medical evidence, no fake approval |
| S11 | CIP Lounge facility/time guests | valid appointment/guestCount, source capacity if required |
| S12 | A24 plan/device cap | planRef real, no arbitrary plan code accepted |
| S13 | A10 dynamic cabin Upgrade | NeedsQuote until exact ProviderQuote expiry verified |
| S14 | Orders with Order-scoped service | TravellerId null valid only with true Order-scope and one total |
| S15 | Fare benefit makes seat free | explicit validated zero Price, not missing-Candidate fallback |
| S16 | no Seat Candidate with physically free seat | SeatMaps same DTO; blocked, price/id null |
| S17 | OccupiedSeats partial return for F1,F2 | fail closed; missing F2 != empty seats |
| S18 | aircraft swap, layout mismatch | stale map not shown as sellable |
| S19 | Exit row child, disabled eligibility | blocked per rules, no fake source |
| S20 | 2 POS with different Seat rates | no price cross contamination, no random first POS |
| S21 | Foreign Agency uses another `offerItemRef` | reject/not leak existence |
| S22 | Expired/pricing-revised pricedSelectionRef | stale/expired; new Evaluate required |
| S23 | Currency/ROE precision & charges | unit vs Order fee correct; tax code/country/station kept |
| S24 | Post-ticketed Shopping | trusted Order context, only eligible future services |
| S25 | Coupon used/flown/refunded | post-order applicability correct; no offer for consumed service |
| S26 | Two same Definition different selection instances | distinct purchase refs, no over-dedup |
| S27 | No Option enum in source | typed constraints, `optionSourceState=Unknown`; not empty-discount |

## 16.3 Hold/Confirm/Supplier (S3)
| # | سناریو | رفتار الزامی |
|---|---|---|
| R01 | Group A01 Pax1 + A12 Pax2 + A22 Order | one HoldId, three Units, exact coverage |
| R02 | Request replay same idempotency key | same refs and result; no double hold |
| R03 | Same key different payload | explicit conflict; no side effect |
| R04 | Two simultaneous last-slot/local count | at most one Unit validly Held |
| R05 | ExternalHoldConfirm success | ProviderUnitRef saved, confirm after evidence |
| R06 | Supplier timeout after physical hold | PendingReconciliation, retries same op id, no duplicate booking |
| R07 | Manual provider awaiting call | Unit Pending until approved outcome, not fake Confirmed |
| R08 | Supplier DirectConfirm (no hold) | pending/commit after payment gate; compensatable if fails |
| R09 | Partial provider success group | per-Unit truth and `Partial` outcome |
| R10 | Confirm only subset | successful units Confirmed, other Units unchanged/pending |
| R11 | Expired Hold / extend unsupported | no unauthorized confirm/extension |
| R12 | Release group and partial release | only Held units released; stock returned once |
| R13 | Supplier cancel fails asynchronously | Pending status; no unconditional Cancelled |
| R14 | Seat paid physical plus commercial | FlightFlow seat HoldId/SeatHoldReference alongside Ancillary Unit; not conflated |
| R15 | Seat EXST (2 occupied seats) | multiple physical refs & ticket action coordinated |
| R16 | Post-order add-on to ticketed Order | new OrderService/Unit and correct EMD/payment |
| R17 | Duplication same OrderServiceId | reject double reservation; recovery idempotent |
| R18 | Order-scoped Unit with null Pax | valid if actual Supplier/Rule allows, no dummy Traveller |

## 16.4 Cancel / Penalty / Document / Financial
| # | سناریو | رفتار الزامی |
|---|---|---|
| F01 | cancel before Confirm | Release, not CancelConfirmed; document/refund separate |
| F02 | voluntary after Confirm pre-EMD | ServiceCancellationQuote then Cancel; proper payment handling |
| F03 | paid bag EMD-A partially refunded | coupon-linked document action, once-only amount |
| F04 | Lounge EMD-S standalone | correct standalone document handling |
| F05 | A10 ticket exchange | not emitted as arbitrary EMD |
| F06 | nonrefundable yet cancellable | CanCancel independent of CanRefund; net refund may 0 |
| F07 | refundable policy with no penalty rule | NeedsManualReview/ProviderQuote, not auto 0 penalty |
| F08 | fixed penalty, tax partially refundable | amount with currency and tax components, correct net |
| F09 | no-show after cutoff | provider rule/clock timezone, not normal voluntary refund |
| F10 | airline disruption + waiver evidence | involuntary reason, waiver only if proof |
| F11 | used/consumed service | cannot auto refund without filed waiver |
| F12 | partial cancellation one of three Units | unrelated services and documents untouched |
| F13 | expired/changed cancellation quote | new preview/approval, no silent extra penalty |
| F14 | Payment wallet + card split | refund by original funding liability, Payment owner |
| F15 | agency credit/cash airport office | net settlement obligation, not forced online card |
| F16 | voucher instead of refund | follow FormOfRefund/issuer document policy |
| F17 | supplier cancel timed out | PendingReconciliation, do not double refund or claim success |
| F18 | duplicate refund request | same payment refs; no double payment |
| F19 | cancel Seat + refund Seat fee | FlightFlow physical cancel, Ancillary commercial cancel, Ticketing/Payment financial actions separately |
| F20 | flight cancellation with 3 suppliers | selected partial compensation, PNR/coupon/ancillary history preserved |
| F21 | interline/codeshare operating carrier restriction | quote/provider rule not invented; blocked/manual if unsupported |
| F22 | fail mid doc void or processor refund | OrderChange pending and recoverable; Ancillary may already Cancelled truthfully |

## 16.5 Coverage acceptance rule
برای هر سناریو در گزارش Agent تنها outcome `Supported/RequiresExternal/NotSupportedByCurrentSupplier`، **Owner of fact**، Route involved و دلیل لازم است. **Pack هیچ ابزار/سبک/تعداد تست اجباری نمی‌کند.** `NotSupportedByCurrentSupplier` می‌تواند نتیجه درست باشد، ولی UI نباید قابلیت unsupported را قابل فروش/تضمین‌شده نشان دهد.


<!-- END: 16-END-TO-END-SCENARIO-COVERAGE-MATRIX.md -->


---

<!-- BEGIN: 17-THREE-STAGE-EXECUTION-AND-BUSINESS-CLOSURE.md -->

# 17 — تنها ۳ Stage اجرایی، با هدف/Scope/Output دقیق

**اصل:** هر Stage یک خروجی تجاری قابل مشاهده دارد و به جای ۱۵ اصلاح پراکنده یا بازبینی دوباره Pack، فقط Gap همان Stage را می‌بندد. پیاده‌سازی و روش تست/دیتابیس تست/چارچوب تکنیکی انتخاب Coding Agent مطابق conventions Repository است؛ این Pack آن را دیکته نمی‌کند.

## Stage 1 — Authoring Closure
**هدف واحد:** همه ۹ Profile و ۲۴ Variant به صورت Typed Definition/Provision و Pricing مشترک، در Backoffice قابل تعریف و مدیریت باشند.

**شروع از:** ۹ Definition facade موجود و Seat Provision facade موجود.  
**تغییر:** فقط تکمیل Typed Provision برای هشت Profile دیگر، field visibility/variant validation/ownership و رفع ناسازگاری واقعی Publish/Pricing، همراه با دو Endpoint مشترک `GET/PUT /Backoffice/v1/AncillaryProvisions/{provisionId}/CancellationTerms` و Rule اختیاریِ ثبت‌شده برای Penalty. No refactor Seat.  
**خروجی قابل مشاهده:** وقتی اپراتور `Meal` را باز می‌کند فقط فیلدهای Meal+Shared را می‌بیند؛ `Pet` فقط Pet+Shared؛ `Seat` دقیقاً مثل قبل. Provisionهای مرتبط در List و قیمت‌های مرتبط در Pricing هستند. همه A01..A24 قابل publish طبق rules.  
**Acceptance کسب‌وکاری:** create/edit/form roundtrip، correct rule ownership، lifecycle price revision، no Seat inventory policy، wrong airline/profile rejection، negative rules واضح.  
**مرز:** Shopping/SeatMaps/P3 تغییر نمی‌کند مگر رفع compatibility روشن.  
**تحویل Agent:** `S1_IMPLEMENTED` با route inventory و تفاوت‌های کوتاه نسبت به این Contract؛ Owner پس از مشاهده نتیجه Stage را می‌بندد.

## Stage 2 — Shopping Closure
**هدف واحد:** در هر سه Surface لیست، گزینه‌های واقعی، قیمت/Qty/Scope صحیح و شناسه انتخاب معتبر و قابل Resolve داده شود؛ SeatMap بدون تغییر ظاهری.

**شروع از:** `AncillaryShoppingEngine`, `AncillaryOffers`, `Details`, `Selections/Evaluate`, `SeatMaps` موجود.  
**تغییر:** `Details` typed option groups واقعی؛ `pricedSelectionRef` Short-lived + S2S Resolve؛ price completeness/FX/SalesScope؛ SeatMap source/zero fix بدون تغییر DTO؛ preOrder first و postOrder source adapter وقتی contract authoritative موجود شد.  
**خروجی قابل مشاهده:** Shopper A01 Qty2 و A12 MenuVG Qty1 را با دو شناسه جدا و قیمت دقیق به Ordering می‌دهد؛ `SeatsMaps` همچنان کلاینت فعلی را بدون update اجرا می‌کند؛ blocked/unknown به غلط Available/Free نشان داده نمی‌شود.  
**Acceptance کسب‌وکاری:** همه A01..A24 appropriate outcome، IncludedFare، multiple Traveller/Flights/Currencies/POS، accepted select resolve/stale expiry، no double order fees، SeatMap shape identical، trusted input no spoofing.  
**مرز:** هیچ Hold/Confirm/Cancel یا PriceOwner duplicate به S2 اضافه نمی‌شود.  
**تحویل Agent:** `S2_IMPLEMENTED` با قرارداد API سه Surface، Contract Freeze SeatMaps و Request/Response Resolve؛ external reference blockers در لیست `[EXTERNAL_PENDING]` بدون ادعای deploy.

## Stage 3 — Reservation & Servicing Closure
**هدف واحد:** Ordering بتواند گروهی از انتخاب‌های معتبر را با هویت OrderService، از Reserve/Hold تا Confirm/Release/Cancel جزئی و CancellationQuote به نتیجه واقعی برسد.

**شروع از:** `AncillaryReservation`, `AncillaryReservationUnit` و سه API Hold/GET/Confirm موجود.  
**تغییر:** extension همین Aggregate برای `PricedSelectionRef`/selected options/price evidence، supplier capability/local stock, status Pending/Mixed/Failed, per-unit refs, idempotency/retry, release/extend, cancellation quote + selected cancel, provider manual outcome، minimal integration events.  
**خروجی قابل مشاهده:** یک request با A01 pax1 + A12 pax2 + A22 OrderRef یک `HoldId` و ۳ `UnitId` می‌دهد؛ زمان Pending/Partial را دقیق نشان می‌دهد؛ انتخاب Unit1 برای cancel Unit2 را دست‌نخورده می‌گذارد؛ refund amount را جعل نمی‌کند؛ Ordering می‌تواند quote و payable/refund workflow را مصرف کند.  
**Acceptance کسب‌وکاری:** valid selection only، duplicate-safe Hold، partial/pending/recovery، local/supplier/manual/directConfirm، physical seat independent، cancellation quote with penalty evidence، release/extend/cancel، provider/cash/wallet/document orchestration contract.  
**مرز:** Issuing ETKT/EMD و Payment refund داخل Ancillary پیاده نشود؛ Ordering/Payment/FlightFlow repo را تغییر ندهید.  
**تحویل Agent:** `S3_IMPLEMENTED` با API table، Unit states، event contract، end-to-end interaction sample و external pending items.

## گیت‌های عملی و تعریف «بسته»
برای هر Stage فقط سه مدرک لازم است: **(۱) رفتار قابل مشاهده با نمونه Request/Response واقعی، (۲) عدم شکستن قراردادهای قبلی مرتبط، (۳) لیست دقیق باقی‌مانده‌های خارجی یا unsupported provider.** روش تولید این مدارک با Agent است؛ نه تعداد تست، نه بانک تست، نه نوع Mock و نه CI در این Pack تحمیل نمی‌شود.

> «S1 Closed / S2 Closed / S3 Closed» فقط وقتی Owner نتایج را پذیرفته باشد. در غیر این صورت «Implemented — Owner Audit Pending» و پاسخ دقیق Gap. فاز بعدی را به خاطر نوشتن مجدد تست‌های غیرضروری گیر نیندازید.

## توقف منطقی، نه بستن دست ایجنت
ایجنت اجازه دارد در **Ancillary repo** کلاس‌ها/enumهای هدف موردنیاز برای این سه Stage را با سادگی لازم اضافه/حذف کند؛ داده تاریخی وجود ندارد و مهاجرت Legacy لازم نیست. فقط **شکستن SeatMaps یا Seat Authoring جاری، ساخت Aggregate موازی، دستکاری مخازن دیگر، جعل قیمت/موجودی/رفاند و ادعای اجرای Provider بدون Source** مجاز نیست. اگر Ordering API واقعی فرق داشت، در خود Pack آن را با UI/Owner نهایی align کند و delta منطقی بدهد؛ تغییر خارج Repo بدون مجوز خیر.


<!-- END: 17-THREE-STAGE-EXECUTION-AND-BUSINESS-CLOSURE.md -->


---

<!-- BEGIN: 18-PUBLIC-BENCHMARK-AND-DESIGN-EVIDENCE.md -->

# 18 — شواهد Benchmark عمومی و دلایل انتخاب قراردادها

**اصل روش‌شناسی:** منابع عمومی API/Carrier فقط رفتار و قابلیت مشاهده‌شده را اثبات می‌کنند؛ از روی ظاهر سایت نمی‌توان topology داخلی میکروسرویس‌ها را حدس زد. نام و کدهای ATPCO/IATA پایه Semantic هستند؛ v13.1 یک clone محصول شرکت دیگری نیست.

## 18.1 Standards / airline distribution
| مرجع | رفتار/شاهد قابل اتکا | نتیجه معماری AeroTech |
|---|---|---|
| **ATPCO Optional Services** | Service SubCode, Group/SubGroup/Descriptions, BookingMethod (SSR/Auxiliary/...), EMD type, RFIC و filed conditions؛ `ReissueRefund` جدا از price | Definition/Provision/Pricing مجزا؛ ServiceSubCode/booking/document را حفظ کن؛ Penalty از صرف Refundable استنباط نشود |
| **IATA NDC** | Separation در `ServiceList`, `SeatAvailability`, `OrderCreate/Change/Reshop/Cancel`, `OrderRetrieve` مفهومی است | Shopping و SeatMap جدا، Order owner جدا؛ P3 S2S under Ordering |
| **Amadeus Self-Service** | Flight Offers Price با `additionalServices` برای baggage؛ `Seatmap Display` با `AVAILABLE/BLOCKED/OCCUPIED`؛ airline/flight limitations | SeatMap خارجی UI ثابت؛ Fare/Pax/Segment Source؛ no universal ready assumption |
| **Sabre NDC** | `Get Ancillaries`, `Get Seats` و Order Management/Reshop جدا هستند؛ GetAncillaries Public NDC برخی موارد free-only است | Seat separate read، ancillary universal engine، Order-change lifecycle؛ هر benchmark capability/provider محدود است |
| **Travelport JSON Air** | `AncillaryShop`, `AncillaryPrice`, `Seat Map`, `Ancillary Book/Cancel` و EMD در جریان مجزا؛ ability با Carrier/GDS/NDC فرق دارد | Generic Details/Options و Evaluate، P3 provider strategy، مالی/doc جدا |
| **Duffel** | Offer `available_services` ID و Quantity، Seat Map ID جدا، Order Services و Order Cancellation Quote→Confirm | `offerItemRef`/`pricedSelectionRef`, Seat IDs، cancellation quote + confirmation و Order Ownership |

### منابع اصلی (قابل بازبینی؛ تاریخ رجوع 2026-10-16)
- ATPCO Optional Services authoring: https://faremanager.atpco.net/atpapps/fmhelp/mergedProjects/Optional%20Services/Create_and_Update_Services.htm
- ATPCO industry service subcodes/RFIC PDF: https://www.atpco.net/sites/atpco-public/files/all_pdfs/Opt_Scvs_Industry_Sub_Codes_Online_C.pdf
- IATA NDC message names documented in implementation forum (پیام‌ها، نه topology داخلی): https://airtechzone.iata.org/docs/ImpForum/CN20/TravelNDC.pdf
- Amadeus Self-Service Flight/Seat API FAQ: https://admin.developers.amadeus.com/self-service/apis-docs/guides/developer-guides/faq/
- Sabre NDC hub: https://developer.sabre.com/product-collection/new-distribution-capability-ndc/v1/index.html
- Sabre Stateless Seats API: https://developer.sabre.com/rest-api/stateless-seats-api/1.4/_attachments/spec.yml
- Travelport Ancillary Guide & EMD (limits GDS/NDC): https://support.travelport.com/webhelp/JSONAPIs/Airv11/Content/Air11/Ancillaries/AncillaryGuide.htm
- Travelport Ancillary Shop/Book/Cancel APIs: https://support.travelport.com/webhelp/JSONAPIs/Airv11/Content/Air11/Ancillaries/APIRef_AncillaryShop.htm ; https://support.travelport.com/webhelp/jsonapis/airv11/content/air11/Ancillaries/APIRef_AncillaryBook.htm ; https://support.travelport.com/webhelp/JSONAPIs/Airv11/Content/Air11/Seats/APIRef_AncillaryCancel.htm
- Travelport Seat Guide: https://developer.travelport.com/docs/flights/guides/seats-and-ancillaries/seats-guide
- Duffel baggage selection/available services: https://duffel.com/docs/guides/adding-extra-bags
- Duffel Seat Maps API: https://duffel.com/docs/api/v2/seat-maps/get-seat-maps
- Duffel Order additions/services: https://duffel.com/docs/api/v2/orders
- Duffel Cancellation quote/confirm: https://duffel.com/docs/api/order-cancellations

## 18.2 Airline UX/Policy — تأیید الگو، نه API پشت‌پرده
| Airline | شاهد عمومی | استفاده صحیح در Product design |
|---|---|---|
| Qatar Airways | انتخاب صندلی و دسته‌های Seating options در سایت رسمی | Seat category/Fare/Pax differences؛ شرط های Carrier vary |
| Singapore Airlines | مدیریت بلیت و cancellation/refunds بر اساس شرایط و زمان | Rule/Refund بعد Issuance باید Airline-specific باشد |
| Lufthansa | Seat selection با seat category؛ هزینه extra baggage با cabin/fare/route؛ exceptions involuntary | FareFamily/Route/Date/NoShow/waiver Source، نه یک penalty جهانی |
| flydubai | disruptions/cancellations و مدیریت خدمات از کانال Carrier | involuntary disruption/service status separate |

منابع نماینده:
- https://www.qatarairways.com/en-qa/complement-your-journey/preferred-seat.html
- https://www.singaporeair.com/en_UK/id/faq/cancellations-and-refunds/
- https://www.lufthansa.com/rs/en/advance-seat-reservations-terms-and-conditions
- https://www.lufthansa.com/tr/en/common/agb-extra-services-at-online-check-in
- https://www.flydubai.com/en/book-and-manage/disruptions-and-cancellations/

**محدودیت مهم:** صفحه فروش Carrier داده داخلی API/Provider را نشان نمی‌دهد. از آن نمی‌توان نتیجه گرفت Carrier همه‌جا Hold/Refund الکترونیکی دارد یا همه خدمات/خریداران Eligibility یکسان دارند.

## 18.3 برداشت‌های نادرست که Pack ممنوع می‌کند
1. **Duffel ≠ all 24 variants out-of-box:** Docs `available_services` فعلی عمدتاً baggage است و محدودیت booking/post-order Seat دارد؛ ما مدل Contract را بنچ‌مارک می‌کنیم، نه ادعای پوشش همه محصولات آن.
2. **Travelport GDS/NDC≠one workflow:** بعضی NDCها پیش از book به AncillaryPrice نیاز دارند، برخی refund/cancel Service unsupported، بعضی eMD handling متفاوت؛ بنابراین capability per provider و Pending truthful ضروری است.
3. **Amadeus Self-Service ≠ Altea internal Reservation:** از APIهای publicly exposed نمی‌توان internal microservice shape حدس زد.
4. **ATPCO RFIC ≠ یک RFIC ثابت برای هر Profile:** Source/subcode mapping authoritative است، نه hardcoded enum `Baggage=...` برای همه انواع.
5. **Carrier terms ≠ universal penalty:** airline/route/fare/purchase stage/EMD/date تعیین‌کننده‌اند؛ cancellation quote پیش از اجرای نهایی ضروری است.

## 18.4 Trace تصمیم به Benchmark
| Design decision | Benchmark fact | داخل Pack |
|---|---|---|
| Definition/SubCode/Provision/Pricing | ATPCO Optional Services | 03–06 |
| single generic nonSeat Shopping | Travelport Ancillary Shop + Sabre Get Ancillaries | 07 |
| SeatMap جدا با Statusهای موجود | Amadeus/Sabre/Duffel/Travelport Seats | 09 |
| indexed option/selection & reprice | Duffel service ID / Travelport ancillary price | 07–08 |
| Booking group refs + per unit | FlightFlow AeroTech Source + order workflows industry | 10–12 |
| quote → customer approval → cancel → refund | Duffel OrderCancellation/Travelport | 11/13–14 |
| EMD ownership separate | ATPCO EMD + Travelport EMD Guide | 13–14 |


<!-- END: 18-PUBLIC-BENCHMARK-AND-DESIGN-EVIDENCE.md -->


---

<!-- BEGIN: 19-BACKEND-AGENT-MASTER-EXECUTION-PROMPT.md -->

# Coding Agent — AeroTech Ancillary v13.1 FINAL — فقط سه مرحله

## نقش و مرجع
تو Coding Agent هستی؛ تنها مسئول **پیاده‌سازی در مخزن `aliifarhadi/AeroTech.Ancillary`**، نه تصمیم‌گیری آزاد Domain و نه تغییر دیگر repos. مالک پروژه تصمیم‌های تجاری را مصوب کرده است. `AeroTech-Ancillary-Master-Pack-v13.1-FINAL` **تنها Authority** است. v12.x و v13.0 و promptهای Seat/Inventory/Shopping قبلی جایگزین شده‌اند. از پوشه همین Pack Chapters 00–21 و نسخه COMPLETE استفاده کن؛ اگر تناقض دیدی Chapter 00، سپس ADR/Contract دقیق فصل مربوط ملاک است. گزارش‌های قدیمی شواهد تاریخی‌اند، نه دستور جدید.

## چرا این معماری انتخاب شد؟
- ATPCO Definition(SubCode) / Provision(eligibility) / Pricing(revision) را جدا نگه می‌دارد؛ Domainهای موازی per Variant هزینه تغییر را بالا می‌برند.
- Amadeus/Sabre/Travelport/Duffel SeatMap را از سایر Ancillary Shopping جدا نشان می‌دهند؛ ما Route و `SeatMapsDto` فعال را حفظ می‌کنیم و یک Engine قیمت مشترک باقی می‌ماند.
- فرم Backoffice Seat نوشته‌شده و کار می‌کند؛ تعمیم الگوی Typed Facade آن به ۸ Profile دیگر، ساده‌تر و امن‌تر از بازنویسی یا ۹ Domain است.
- Shopping Item identity، انتخاب Qty/Option و Stock Hold سه چیز متمایزند؛ برای فاز ۳ باید `pricedSelectionRef` معتبر و قابل revalidate داشته باشیم.
- FlightFlow یک HoldRef گروهی و unit-level references دارد؛ Ancillary همان semantics را در Aggregate فعلی خود تکمیل می‌کند، اما **Seat physical stock را اداره نمی‌کند**.
- Cancel/Refund/EMD ownership جداست؛ Penalty Quote از Rule/Provider معتبر است، Refund واقعی با Ordering/Payment/Ticketing.
- Owner می‌خواهد کار **در ۳ مرحله** به نتیجه برسد، بدون over-engineering و بدون بحث روش نوشتن تست/DB تست.

## مبنای سورس؛ در شروع بررسی کن
- Ancillary `k8s-stg` baseline audited: `9aca1cd3985d00d1e79356cb645d0cfd02eab082`.
- AirAvail `k8s-stg`: `ac840401d38cb407af339d6fa07f98e052b1bd04`؛ S2S `POST /Service/v1/FlightOffers/Details`.
- FlightFlow `k8s-stg`: `962f41880df0f10a1d105559c233af63ef9db968`، صرفاً reference؛ owner گفته push/deployment آن gate توسعه Ancillary نیست.
- Ordering.Final این نوبت قابل خواندن نبود؛ Endpointهای Ordering در فصل 14 Proposed هستند؛ **هیچ Contract/route جاری آن را از روی حدس claim نکن**.

## مرحله ۱ — S1 Authoring (فقط همین گام در اولین اجرا)
**هدف:** Backoffice تعریف/Provision تخصصی و Pricing برای تمام ۹ Profile؛ ۲۴ Variant واقعی.

بخوان: Chapters `03,04,05,06,17`.

**KEEP:** Definition POST/PUT/GET هر ۹ Profile، Seat `ProvisionForm/Provisions/Form/PUT`، generic Pricing/Lifecycle/Publish/Query، InventoryPolicy nonSeat. **CHANGE:** ۸ Profile غیر Seat معادل Typed Provision Facade Seat داشته باشند، فقط Common+Relevant specialty fields، correct route owner guard، roundtrip Form، Pricing shared؛ `CancellationTerms` ValueObject/Shared GET+PUT مطابق Ch13/21 برای carrier-filed penalty را همین‌جا کامل کن، **بدون تغییر Seat Provision Form**؛ any real domain rule gaps. No fake rule section، no Seat UI modification، no new AncillaryInventoryPolicy for Seat. درست بودن اجازه no-POS restriction را حفظ کن.

**خروجی ملموس:** برای هر Profile (`Baggage, Seat, Upgrade, Meal, Pet, AssistedTravel, AirportService, Priority, Connectivity`) route inventory Definition+Provision و یک نمونه صحیح فرم/انتشار و price association؛ تمام ۲۴ Variant behavior. `S1_IMPLEMENTED` + Owner audit؛ اگر نسبت به Chapter 04 تفاوت وجود دارد دلیل دقیق بده. **قبل از شروع Stage 2، S1 را به مالک تحویل بده.**

## مرحله ۲ — S2 Shopping (پس از تأیید S1)
**هدف:** هر Surface گزینه‌های واقعی، Money/Scope صحیح و PricedSelectionRef قابل Resolve بدهد؛ SeatMap compatibility فریز.

بخوان: Chapters `07,08,09,15,16,17`.

**KEEP:** `AncillaryShoppingEngine`, `AncillaryOffers`, `Details`, `Selections/Evaluate`, `SeatMaps` و DTO/route فعلی، A09 generic route. **CHANGE:** `Details` real typed option choices when source verified; `Evaluate` complete normalized choice Price + new `pricedSelectionRef`, short-lived lookup `PricedSelectionSnapshot`, internal `Selections/Resolve`; all price revisions/expiry/traveller-scope/POS checked; no free/available from unknown state; physical source partial response fail closed. Use same Engine and three surface mappers. **DO NOT change a single public/nested SeatMaps DTO field/route/status string**. PostOrder shopping request/context additive only with Ordering source evidence؛ اگر ordering external هنوز قابل اتصال نیست، Contract/Adapter interface را accurate design کن و external pending اعلام کن؛ PreOrder را بلاک نکن.

**خروجی ملموس:** API list/details/evaluate/seatmaps در Backoffice/Api/OtaPanel، نمونه Paid/Free/NeedsQuote/NeedsVerification/Unknown، qty difference priced refs، resolve/stale/requote، snapshot/money mapping؛ `S2_IMPLEMENTED` + Owner audit. **قبل از Stage 3 تحویل بده.**

## مرحله ۳ — S3 Reservation/Servicing (پس از تأیید S2)
**هدف:** یک درخواست چند selectionRefs با/بدون Pax را تا HoldGroup/perUnit/Confirm/Release/Extend/PartialCancel/CancellationQuote واقعی مدیریت کند.

بخوان: Chapters `10–16,17`.

**KEEP:** `AncillaryReservation`, `AncillaryReservationUnit`, `Service/v1/Ancillaries/Service-Holds`, `/GET`, `/Confirmations`. **CHANGE:** Extend same model with trusted selection+pricing/quantity/option refs, valid Unit/Provider evidence/Status+Pending/Mixed, idempotency and retry, local finite and explicit unlimited, supplier hold/direct/manual truthful outcomes, partial operations, cancellation quotes including penalties only where justified, owner scope and relevant events. Exact new Service routes and payload Chapter 11. A09/Seat require trusted Ordering physical evidence; no local seat stock. `Settlement.ReissueRefund` alone cannot price penalties: Chapter 13 minimal filed rule or authoritative ProviderQuote/ManualReview. No EMD issuance/Payment refund in Ancillary.

**خروجی ملموس:** one Group `HoldId`, per Unit `UnitId/ProviderUnitRef/Status`, real pending/partial, Confirm/Release/Cancel selection, Fee/Penalty quote/Provider contract, Order mapping and retry/recovery؛ `S3_IMPLEMENTED` + Owner audit. **آزادسازی/Refund مالی تنها در contract Ordering تعریف شود و خارج repository بماند.**

## محدودیت‌های سراسری و مجوز تغییر
- فقط `AeroTech.Ancillary` تغییر کند؛ سایر repos تنها خواندن/تطبیق. از طراحی Domain قبلی v1/v2 Donor به‌عنوان Authority استفاده نکن.
- Owner **هیچ داده تاریخی Production** ندارد؛ اجبار migration compatibility، dual-write و حفاظت Legacy Seat Policy نامربوط است. تو می‌توانی Schema/Test fixtures مربوط به مدل نهایی را پاکسازی کنی؛ با این حال API فعال Seat و سایر بخش‌های قابل استفاده شکسته نشود.
- از اختراع مفهوم، duplication aggregate/engine، middleware جدید به خاطر آینده فرضی، Endpoint `Meal/Details`، Enum جدید برای `PerOrder`, Pricing از Body، Provider Success کاذب، صفر بودن قیمت فاقد Evidence یا Seller/PCC leakage جلوگیری کن.
- Validation و ادله لازم است، ولی Master Pack **نحوه تست/تکنولوژی دیتابیس تست/فریم‌ورک معماری** تعیین نمی‌کند. استانداردهای repo را رعایت کن. Document فقط Business Outcomes را ارائه می‌کند؛ اجازه نداری سناریوی خطرناک را به خاطر «تست سخت است» حذف کنی.
- Stage1 و Stage2 درونی را به خاطر Push نشده/غیر مستقر بودن FlightFlow نگه ندار؛ با `[EXTERNAL_PENDING]` تفکیک کن. اما هیچ‌وقت دروغ نگوی که integration خارجی واقعاً اجرا شده است.
- قراردادی که در Ordering واقعاً متفاوت است باید به شکل Δ قرارداد پیشنهادی/مرجع واقعی همراه دلیل به Owner گزارش شود؛ بدون مجوز خارجی تغییر نده.

## گزارش حداقلی هر Stage
`reports/V13.1-S1-OWNER-REVIEW.md`, `...-S2-...`, `...-S3-...` (نام فایل گزارش پیشنهاد است): Current SHA، بخش‌های واقعی اصلاح‌شده/حفظ‌شده، جدول Endpoint واقعاً موجود، نمونه درخواست/پاسخ، رفتار هر acceptance outcome، Regression با Seat/NonSeat، `EXTERNAL_PENDING` و blockers واقعی. **روش خاص اجرا یا تعداد تست خاص شرط Pack نیست.** در پایان همان Stage متوقف شو و Approval بخواه؛ اگر صاحب پروژه اجازه داده Stageها پشت‌سرهم اجرا شوند، در هر گام یک گزارش مستقل بگذار.

**شروع کار:** آخرین SHA Ancillary را fetch و با baseline بالا مقایسه کن، سپس **فقط Stage 1** را اجرا کن. سه مرحله را به ده‌ها پروژه فرعی تبدیل نکن.


<!-- END: 19-BACKEND-AGENT-MASTER-EXECUTION-PROMPT.md -->


---

<!-- BEGIN: 20-CHATGPT-CONTINUATION-PROMPT-FULL-CONTEXT.md -->

# پرامپت جامع ادامه این گفتگو در ChatGPT — AeroTech.Ancillary v13.1 FINAL

> **این فایل برای ChatGPT در یک چت جدید است، نه برای Coding Agent.** کل آن را در چت جدید قرار بده یا همین Pack را ضمیمه و بگو این Chapter 20 مرجع Context/Handoff است. چنین انتقالی حافظه پنهان حساب را تضمین نمی‌کند، اما این سند تمام تصمیم‌های قابل انتقال پروژه و مرجع کامل آنها را فراهم می‌کند. تمام Pack Chapters 00–21 به‌عنوان تنها Project Authority استفاده شود.

## نقش تو
تو معمار مستقل Domain (DDD)، متخصص PSS/ATPCO/IATA، reviewer قراردادهای میکروسرویس و PM پروژه **AeroTech.Ancillary** هستی. کاربر Owner/Approver است؛ Coding Agent کد می‌نویسد. تو باید repo را بخوانی، طراحی/بنچ‌مارک/نقد و قراردادهای دقیق و پرامپت کامل `.md` برای Agent بدهی. **تمام گفتگو فارسی**؛ Type/Routes/Enums/Tech names به انگلیسی استاندارد بمانند. نسبت به ایده Owner مستقل و نقاد، اما بدون over-engineering باش.

## Single authority و نسخه
**`AeroTech-Ancillary-Master-Pack-v13.1-FINAL` (2026-10-16)** شامل 00–21 + نسخه COMPLETE، جایگزین **تمام v12.x/v13.0 و همه old promptها** است. نه v12.3 پیشنهاد حذف SeatMaps و نه v13.0 نکات مبهم Refund بر این Pack مقدم نیستند. تصمیم اخیر Owner: در ۲–۳ Stage دقیق اجرا شود؛ فقط Domain/behavior/fields/endpoint/interface و E2E scenarios در Pack، نه تست implementation details. از حدس و اختراع data/contracts اجتناب کن.

## مخازن و Source وضعیت خوانده‌شده
- `https://github.com/aliifarhadi/AeroTech.Ancillary` `k8s-stg@9aca1cd3985d00d1e79356cb645d0cfd02eab082` (2026-10-14، `Fix Ancillary Offer Options`). آخرین SHA قبل هر audit دوباره بخوان.
- `https://github.com/aliifarhadi/AeroTech.AirAvail` `k8s-stg@ac840401d38cb407af339d6fa07f98e052b1bd04`. `POST Service/v1/FlightOffers/Details` priced offer context: Flights, FlightId/FlightCapacityId, Cabin/RBD, FareFamily/FareBasis, Ticket/Coupon/allowance, currency/ROE, `PricedAt` و `LastTicketingDate`, **نه ValidUntilUtc**.
- `https://github.com/aliifarhadi/Aerotech.FlightFlow` `k8s-stg@962f41880df0f10a1d105559c233af63ef9db968`، `OccupiedSeats Endpoint Added`, FlightFlow Seat-Holds/Confirmations/Release/Extend/PartialCancellation؛ **User گفته FlightFlow push/deploy نهایی هنوز برای پروژه اهمیتی ندارد و نباید gate Stageهای Ancillary شود**؛ کد موجود را می‌توان benchmark قرارداد گرفت، نه ضمانت محیط.
- `https://github.com/aliifarhadi/AeroTech.Ordering.Final`: دسترسی GitHub هنگام نگارش Pack `404`; بنابراین Endpointهای Ordering در Chapter 14 `[TARGET]` و قرارداد پیشنهادی‌اند، نه current routes. هر زمان دسترسی فراهم شد first source audit و mapping؛ بدون source مسیرهای Ordering موجود را تأیید نکن.
- Repos مرتبط از تاریخ: `AeroTech.AirAvail`, `Aerotech.AirPrice`, `Aerotech.FlightFlow`, `Aerotech.Backoffice`, `AeroTech.Ordering.Final`; **بدون مجوز Owner repo دیگر تغییر نکند**.

## تصویر بزرگ‌تر PSS
Ordering lifecycle از Offer→Reservation→Issue→Cancel/Refund→Payment (Stageهای ۱–۵ طبق اظهار Owner بسته) و Ancillary/EMD هماهنگی Stage بعدی است. Ordering مالک Order, OrderService, Traveller, payment integration, E-ticket, EMD-A/EMD-S, ticket/coupon history, buyer/seller/actor, agency/corporate/B2C/Partner, cancellation/refund orchestration است. Payment/JetPay مالک Authorize/Capture/Refund/Wallet/AgencyCredit/Cash/Settlement. FlightFlow مالک actual physical seats/seat holds. AirAvail/Price مالک flight fare/offers/benefits. Ancillary مالک Definition/Provision/Pricing/InventoryPolicy(nonSeat)/Shopping/operational ancillary Hold/Confirm/Cancel.

## تصمیم‌ها و ساختار دقیق
1. **۹ Profile:** Baggage, Seat, Upgrade, Meal, Pet, AssistedTravel, AirportService, Priority, Connectivity؛ **۲۴ Variant** A01..A24 با ماتریس Chapter 06. No invented extra profile or type. `PricingUnit.PerSeat=5` و سایر enum values طبق Chapters 03/21 حفظ.
2. S1: Definitionها برای ۹ Profile **اکنون specialized موجودند**؛ Seat Provision Typed API+Front Backoffice **اکنون کار می‌کند و Freeze**. فقط Provisionهای ۸ Profile دیگر به‌شکل Typed Facade هم‌الگو می‌شوند؛ Pricing/Lifecycle و Persistence مشترک؛ Filed Penalty authoring با دو Route مشترک `CancellationTerms` در Stage 1، بدون تغییر Seat Form؛ `no historical production data`, expensive migration/dual-write unnecessary. No Seat InventoryPolicy؛ Actual inventory nonSeat retained.
3. S2: **Shopping موجود است**: `POST /{Backoffice|Api|OtaPanel}/v1/AncillaryOffers`, `/Details`, `/Selections/Evaluate`; Seat `POST /.../SeatMaps` (جدا). یک `AncillaryShoppingEngine`؛ `SeatMapsDto` و nested response کاملا Freeze حتی `travelerPricing[].id`، فیلدها/نام Status/Envelope. A07/A08 SeatMap؛ A09 ExtraSeat همچنان Generic AncillaryOffers. State Unknown نباید Free/Available فرض شود، incomplete occupancy fail closed. Dto SeatMap تعمیر شود؟ فقط behavior internal، DTO نه.
4. S2 هدف: `Details` مشترک nonSeat Typed options per variant (بدون ۹ details controller)، Qty/option/beneficiary/coverage/pricing per person/item/piece/kg/order. OfferItemRef Candidate است؛ `pricedSelectionRef` Short-lived snapshot انتخاب **Accepted + FullyPriced + Scope** است و S2S Resolve می‌شود. Shopper preOrder به OrderId نیاز ندارد. For postOrder trusted Order commercial context لازم و route same/additive.
5. S3 هدف: توسعه `AncillaryReservation` و `AncillaryReservationUnit` موجود، نه Aggregate جدید. `POST/GET /Service/v1/Ancillaries/Service-Holds`, `POST .../Confirmations` existing؛ new Release/Extend/Retry/perUnit cancel/CancellationQuote/ProviderOutcome + Service `Selections/Resolve`. Group HoldId و per UnitId/ProviderUnitRef، nullable TravellerId فقط Order-scoped واقعی. External HoldConfirm/DirectConfirm/Manual، local Count/Weight/Slot، explicit Unlimited، commercial seat with FlightFlow physical ref outside Ancillary. Idempotent, partial/pending/reconciliation truthful.
6. Service cancellation با refund مالی یک چیز نیست: `ReissueRefundPolicy` current only refundable/nonrefundable/reusable؛ **مبلغ penalty را تعیین نمی‌کند**. Cancellation Quote Service-side filed rule/ProviderQuote/ManualReview و QuoteRef؛ Ordering با Ticketing/Payment/EMD/Coupon و prior-refunds Quote نهایی Customer/Agency را می‌سازد؛ customer approval→Cancel service→Document void/refund→Payment refund under recoverable choreography. EMD issuance/refund داخل Ancillary ممنوع.
7. Service surfaces Backoffice/API/OtaPanel برای Shopping سه mapping جدا ولی Domain مشترک؛ Service-to-Service Reservation فقط internal. Public Order/Refund endpoints proposed under Ordering هستند نه Ancillary؛ existing real Ordering routes باید هنگام دسترسی reconcile شوند.
8. Benchmarks: ATPCO Optional Services SubCodes/S5/S7, IATA/NDC ServiceList/SeatAvailability/OrderChange/Reshop/Cancel, Amadeus seatmap and baggage, Sabre Get Seats/Get Ancillaries, Travelport Ancillary Shop/Price/Book/Cancel+EMD with GDS/NDC differences, Duffel available_services/serviceIds and SeatMaps and cancellation quote, airlines Qatar/Lufthansa/Singapore/flydubai for sales/refund policy context. API surfaces لزوماً structure داخلی را ثابت نمی‌کنند.

## دقیقاً سه مرحله اجرا — نه مرحله‌های بی‌پایان
- Stage 1 — S1 Authoring: typed Definitions (existing), ۸ typed nonSeat Provision Facades، Seat existing unchanged، `CancellationTerms` optional filed rule مشترک، ۲۴ variants و shared Pricing/Lifecycle؛ report+Owner signoff.
- Stage 2 — S2 Shopping: `Details` real options، exact price + Qty/scope، `pricedSelectionRef`/Resolve، nonbreaking SeatMap repairs، three surfaces؛ report+Owner signoff. External deployment gate جدا.
- Stage 3 — S3 Reservation & Servicing: group Hold, per Unit, Confirm/Release/Extend/Retry/PartialCancel, provider/manual/finite-stock, CancellationQuote, Ordering/Payment/Ticketing contract؛ report+Owner signoff. No EMD code inside Ancillary.

## قواعد سخت پاسخ‌دهی
- هر سوال/پیشنهاد درباره کد: **Authority v13.1 → GitHub HEAD واقعی → Diff → API/Domain fields → Ownership → impact/cross-service → verdict**؛ برای benchmark claims منابع معتبر citation.
- سه وضعیت را مخلوط نکن: `[CURRENT]` source proven، `[TARGET]` new design، `[EXTERNAL]` owner/deployment dependent. Test reports از Agent ≠ اجرای مستقل.
- اگر مطمئنی، کامل جواب بده؛ از کاربر سوال تکراری نپرس. فقط Unknown/External را دقیق و کوتاه بیان کن. از «حفظ داده قدیمی نداریم» برای بستن دست Agent یا مهاجرت طولانی استفاده نکن؛ بر Compatibility UI فعال تمرکز کن.
- Simplify: no aggregate/engine/controller duplication، no new enum `PerOrder`, no invented stable quote guarantee, no zero from null، no Supplier success fake.
- **در پایان هر تحلیلی که تغییر کد لازم دارد، پرامپت کامل قابل دانلود `.md` برای Coding Agent بده**؛ برای ادامه چت ChatGPT پرامپت جدا است. Skill/doc artifacts only if requested; plain md کافی.
- از تعریف مراحل پشت‌سرهم از روی ویژگی‌های نیمه‌شده خودداری؛ Owner wants three outcomes. درخواست Push/Commit فقط اگر Owner مجوز دهد؛ Coding Agent implementation details را خودش انتخاب کند.

## اگر کاربر گفت «کد پوش شد»
۱. HEAD سه repository مرتبط را به‌روزرسانی و فقط Diff Ancillary نسبت به baseline آخرین مرحله را بخوان. ۲. به API/Domain/State/Contract v13.1 compare کن. ۳. روشن بگو `S1/S2/S3 Implemented/Pending/Closed` با شواهد، ExternalPending جدا. ۴. فقط در صورت defect واقعی Prompt اصلاحی واحد بده. ۵. دوباره Pack v12 یا SeatMap redesign پیشنهاد نکن.

## شروع چت جدید
این سند را پذیرفتم و قراردادهای v13.1 را مرجع قرار می‌دهم. منتظر درخواست بعدی کاربر یا ممیزی آخرین Push هستم. **این متن تنها زمینه است؛ بدون درخواست جدید کد تغییر نمی‌دهم.**


<!-- END: 20-CHATGPT-CONTINUATION-PROMPT-FULL-CONTEXT.md -->


---

<!-- BEGIN: 21-ENUMS-TRACEABILITY-OPEN-INTERFACES.md -->

# 21 — Enum catalogue، Traceability و مرزهای بازِ صادقانه

## 21.1 Enum values `[CURRENT]` — از کد Ancillary
| Enum | مقادیر Numeric صحیح |
|---|---|
| `AncillaryProfile` | `Baggage=1`, `Seat=2`, `Upgrade=3`, `Meal=4`, `Pet=5`, `AssistedTravel=6`, `AirportService=7`, `Priority=8`, `Connectivity=9` |
| `ServiceCoverageScope` | `Sector=1`, `Portion=2`, `Journey=3`, `Order=4` |
| `PricingUnit` | `PerPassenger=1`, `PerRoom=2`, `PerItem=3`, `PerVehicle=4`, `PerSeat=5`, `PerPiece=6`, `PerKilogram=7` |
| `AncillaryQuantityUnit` | `Each=1`, `Piece=2`, `Kilogram=3` |
| `PriceOrigin` | `Filed=1`, `ExternalQuote=2`, `Free=3`, `NotAvailable=4`, `LegacyUnspecified=5` |
| `InventoryAuthority` | `Unlimited=1`, `Local=2`, `Supplier=3` |
| `InventoryCapacityReadState` | `NotConfigured=1`, `Unlimited=2`, `ConfiguredNotGuaranteed=3`, `ClosedForSale=4`, `DelegatedCheckRequired=5`, `Unknown=6`, `UnsupportedPattern=7`, `NotApplicable=8` |
| `OfferReadiness` | `NotEligible=1`, `NeedsSelection=2`, `NeedsQuote=3`, `NeedsVerification=4`, `Selectable=5`, `Unavailable=6` |
| `PurchaseStage` | `PreOrder=1`, `PostTicketed=2`, `Both=3`, `LegacyUnspecified=4`, `OnBoard=5` |
| `ProvisionApplicationType` | `Standard=1`, `Baggage=2`, `Seat=3` |
| `SupplierFulfillmentKind` | `Local=1`, `External=2` |
| `AncillaryReservationStatus` | `Held=1`, `Confirmed=2`, `Released=3`, `Expired=4`, `Cancelled=5` |
| `AncillaryReservationUnitStatus` | `Held=1`, `Confirmed=2`, `Released=3`, `Expired=4`, `Cancelled=5` |
| `ReissueRefundPolicy` | `Refundable=1`, `NonRefundable=2`, `NonRefundableReusable=3` |
| `FormOfRefund` | `OriginalPayment=1`, `EVoucher=2` |
| `DocumentRouting` | `NoAncillaryDocument=1`, `Emd=2`, `TicketOrExchange=3` |
| `AncillaryDocumentType` | `None=1`, `EmdAssociated=2`, `EmdStandalone=3` |
| `BookingMethod` | `Ssr=1`, `AuxiliarySegment=2`, `DisplayPriceContactCarrierForBooking=3`, `NoBookingProcessRequired=4`, `PerServiceRecord=5` |
| `SelectionKind` | `SimpleOptIn=1`, `QuantityChoice=2`, `TypedForm=3`, `SeatMapSelection=4`, `ExternalQuote=5` |
| `SelectionFieldType` | `Integer=1`, `Decimal=2`, `Text=3`, `Boolean=4`, `Code=5`, `CodeSet=6`, `Reference=7`, `DimensionsCm=8`, `DateTimeWithOffset=9` |
| `SeatPurpose` | `Standard=1`, `Preferred=2`, `ExtraSeat=3` |

`InventoryAuthority.FlightFlow=4` **در HEAD فعلی Ancillary وجود ندارد** و نباید دوباره اضافه شود. Enumهای قدیمی renumber نمی‌شوند.

## 21.2 پیشنهاد محدود enum جدید `[TARGET]`
| Domain | فقط مقادیر واقعا لازم | چرا |
|---|---|---|
| `AncillaryReservationUnitStatus` append | `Requested=6`, `Pending=7`, `Failed=8` | truthful asynchronous/provider unresolved |
| `AncillaryReservationStatus` append | `Pending=6`, `Mixed=7`, `Failed=8` | batch complete vs partial/fail در یک field |
| `PenaltyKind` if filed-policy needed | `None=1`, `FixedAmount=2`, `Percentage=3`, `ProviderQuoted=4`, `ManualReview=5` | penalty source/treatment، فیلد به جای string arbitrary |
| `CancellationReasonKind` if existing enum absent | `Voluntary=1`, `Involuntary=2`, `SupplierFailure=3`, `NoShow=4` | مصوب و auditability؛ به enum همان پروژه مپ شود |

`PendingOperation` و `PendingReasonCode` می‌توانند Code constrained String باقی بمانند و نیاز به Enumهای جدید ندارند. `Outcome=Complete|Partial|Pending|Failed` فقط projection است، نه Domain Aggregate. قبل از اضافه کردن Enum دوم به کد، نام معادل موجود در Repo را چک کن؛ Duplicate نساخته و Numeric را شفاف گزارش کن.

## 21.3 جای دقیق Rule جریمه و ارتباط Stageها
`SettlementDefinition.ReissueRefund` **تنها eligibility** است؛ برای Carrier-filed penalty باید یک `CancellationRules` ValueObject اختیاری زیر Provision/Settlement ثبت شود. برای اینکه S3 دوباره S1 را باز نکند، **Authoring shared آن در S1 بسته می‌شود** و S3 فقط محاسبه/Quote/استفاده می‌کند.

قرارداد مدیریتی مشترک جدید `[TARGET]`، بدون تغییر request/response فرم موجود Seat:
```
GET /Backoffice/v1/AncillaryProvisions/{provisionId}/CancellationTerms
PUT /Backoffice/v1/AncillaryProvisions/{provisionId}/CancellationTerms
```
`PUT` فقط Draft Provision را تغییر می‌دهد؛ بعد Publish/Revision immutable rules فعال‌اند. بدنه `CancellationRules[]` و eligibility evidence با ValueObject فصل 13؛ فقط Rules قابل استناد Carrier/Supplier، نه جریمه فرضی همه Variantها. اگر Policy معتبر نزد Supplier است، `PenaltyKind=ProviderQuoted` و جزئیات مبلغ در S1 فایل نمی‌شود؛ برای Free/no document نیز Rule `None` **صریح** لازم است اگر محاسبه خودکار quote مجاز باشد. هیچ Create/Manage Aggregate/DbSet جدید برای Penalty لازم نیست.

## 21.4 Known external pending — بدون تظاهر به تطبیق
| Dependency | وضع موجود | شرط تصدیق real end-to-end |
|---|---|---|
| AirAvail priced offer | Contract واقعی و سورس قابل خواندن | Running connection، correctness per-channel |
| FlightFlow OccupiedSeats/physical holds | سورس Contract قابل خواندن؛ Owner گفته deployment work غیر gate | runtime response، flightIds complete، physical refs صحه‌گذاری |
| AirInfo Aircraft layout | Ancillary Adapter current | matching AircraftVersion + accurate seat map |
| Ordering source and APIs | Repo inaccessible current session | owner-consumed contract `IOrderCommercialContextReader`, OrderService identity/reconciliation |
| JetPay/Payment | boundary identified، routes not verified | guarantee/capture/refund status+idempotency |
| Ticketing EMD-A/EMD-S | boundary identified، routes not verified | approved issuance/void/refund per coupon and status |
| Supplier external/manual | adapter capability per provider not generally deployed | actual hold/confirm/cancel evidence; unsupported capabilities reported |

این لیست مانع نوشتن دامنه Ancillary نیست و جایگزین ارائه Contract شفاف نیز نیست.

## 21.5 Traceability contracts→scenario→owner
| Requirement | Domain/API chapter | سناریوها در Ch16 | Owner |
|---|---|---|---|
| Typed forms ۹ Profile، بدون تغییر Seat | 03–06 | C01..C04, C11..C12 | Ancillary |
| Rules/penalty authoring بدون تغییر Seat Form | 05,13,21 | C04,C06,F06,F07,F08 | Ancillary |
| Price/Quantity/Tax/Benefit/Options | 05–08 | S01..S15,S23 | Ancillary + AirAvail facts |
| SeatMap DTO Freeze و physical availability | 09,14 | S16..S20,R14,R15 | Ancillary read facade + FlightFlow |
| `pricedSelectionRef` و context secure | 07–08 | S21,S22,S26,S27 | Ancillary |
| Group Hold/Unit/Idempotency | 10–12 | R01..R18 | Ancillary / Ordering |
| Cancellation quote/penalty | 11,13 | F01..F13,F19..F21 | Ancillary service portion |
| EMD/Payment/Refund/History | 13–14 | F03..F22 | Ordering + Ticketing + Payment |

## 21.6 Superseded conflicts resolved
- v12.3 `SeatOffers` separate **رد شد**؛ current SeatMaps retained.
- v12.2 «Shopping هنوز فاز بعد است» رد شد؛ S2 کد موجود دارد و تکمیل می‌شود.
- v13.0 «refund rule only ReissueRefund flag» برای محاسبه Penalty ناکافی بود؛ v13.1 filed rule optional و ProviderQuote/ManualReview را مشخص کرده است.
- v13.0 «P3 status pending بدون enum values» در v13.1 numeric append دارد.
- v13.0 راه‌اندازی Stage3 از `serviceDefinitionId/provisionId` غیرقابل اعتماد بود؛ v13.1 `pricedSelectionRef` و Resolve را mandatory کرده است.
- Stage1 closed بدون CancellationTerms باعث باز شدن دوباره Authoring در Stage3 می‌شد؛ v13.1 دو Route shared CancellationTerms را در S1 می‌بندد، بدون دست‌زدن به Seat Form.
- old «FlightFlow missing OccupiedSeats» صرفاً گزارش SHA قدیمی است؛ در SHA `962f4188` route مشاهده شده؛ با این وجود Owner deployment external را gate نکرده است.


<!-- END: 21-ENUMS-TRACEABILITY-OPEN-INTERFACES.md -->
