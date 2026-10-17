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

## حاکمیت مخزن (verified 2026-10-16)
`CLAUDE.md` در `k8s-stg@9aca1cd` صریحاً یک write path برای کل Aggregate، ممنوعیت child endpoint، relational child table و validation در Define/Change را الزام می‌کند. این قانون بالاتر از هر متن متعارض در Pack است. پرونده v13.1 با پیشنهاد GET/PUT برای CancellationTerms خلاف این قانون بود و در v13.2 حذف شده است.

## دقت دامنه درباره Restriction metadata (additional source verification)
فایل `src/AeroTech.Ancillary.Domain/AncillaryProvisionAggregate/AncillaryProvision.Publication.cs` شامل `EnsureOutcomeOf` است: A11/A15 فقط `Paid` را رد می‌کند؛ `Free`/`NotAvailable` امکان دارند. برای `Upgrade` Paid FixedAncillary ↔ Filed، برای Connectivity OnBoard iff DeliveryStage OnBoard و A07/A08 MaxQuantity=1 صریح‌اند. محل فعلی Guard در publication است؛ برای تطبیق CLAUDE Rule5 همان invariant باید هنگام Define/Change enforce شود.
