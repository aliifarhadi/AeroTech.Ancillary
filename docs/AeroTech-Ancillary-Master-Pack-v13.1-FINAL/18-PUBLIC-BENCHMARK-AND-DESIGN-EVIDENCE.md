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
