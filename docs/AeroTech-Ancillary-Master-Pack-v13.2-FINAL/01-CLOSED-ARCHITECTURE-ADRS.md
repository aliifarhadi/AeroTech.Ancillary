# 01 — ADRهای قطعی و مرز Contextها

## ADR-00 — قانون Owner در CLAUDE.md مقدم بر Pack
- One write path per aggregate؛ Typed Backoffice facade فقط مسیر دیگری به همان Define/Change Service است، نه Use Case موازی.
- ممنوع: endpoint مستقل برای `CancellationTerms`، `PricingRate` یا هر child rule. قواعد کنسلی بخشی از draft موجود `AncillaryProvision` و زیر DTO مشترک `Settlement.CancellationRules` هستند.
- Seat Backoffice و ساختار خروجی `SeatMapsDto` Freeze هستند؛ Extension کاملاً اختیاری `Settlement` باعث شکستن قرارداد Seat نمی‌شود. مقدار `null`/missing در Change، قواعد قبلی را حفظ می‌کند؛ `[]` صریح آنها را پاک می‌کند.
- Data Historical Production وجود ندارد، اما قرارداد فعال UI باید محفوظ بماند.
- اختلافات آینده نسبت به CLAUDE.md نباید توسط Agent با پیشنهاد استثنای خودسرانه حل شود.


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
