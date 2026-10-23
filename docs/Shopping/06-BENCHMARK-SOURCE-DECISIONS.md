# 06 — تصمیم‌ها، شواهد بنچ‌مارک و تطبیق سورس (Pack v8)

**Authority:** فقط Pack v8، سند تجاری `01` و قراردادهای `02` تا `05`. این فایل سند شواهد و محدوده است، **نه مرجع موازی پیاده‌سازی**.  
**Baseline observed:** `aliifarhadi/AeroTech.Ancillary`, `k8s-stg@49e9209a40e046fe71c7913cc1fcf8a8fca6799d`; Gate0 SHA فعلی را مستقل تأیید می‌کند.

## A. بنچ‌مارک؛ آنچه واقعاً پشتیبانی می‌کند

| شاهد | یافته قابل اتکا | تصمیم AeroTech، نه ادعای استاندارد |
|---|---|---|
| Duffel available services and order creation | خدمات به مسافر و Segment وابسته‌اند؛ ID خدمت و Quantity در فرآیند خرید نقش جدا دارند | `quantityOptions[]` با `d_` مخصوص هر تعداد و `ResolveGrouped(ids[])` قرارداد داخلی ما هستند |
| Travelport Ancillary shopping | Application Limit می‌تواند Passenger/Segment/PassengerSegment/Itinerary باشد؛ SeatMap مسیر جداگانه دارد | `PerTravellerPerPortion` را از `ServiceCoverageScope.Portion` خودمان می‌گیریم؛ PassengerOD را خودکار Portion ترجمه نمی‌کنیم |
| تصاویر Singapore Airlines ارسالی Owner | UI Add-ons و صفحه تعیین چمدان به تفکیک مسافر و مسیر | مقدار ۲۳kg/سقف ۲/قیمت نمونه را به عنوان قانون جهانی hardcode نمی‌کنیم؛ Quantity-only در Backend به POST نیاز ندارد |
| OWASP MAC/Key Management/Authorization | امضا از تغییر Payload جلوگیری می‌کند؛ Auth/Authorization و مدیریت کلید مستقل لازم‌اند | HMAC-SHA256 برای `r_`/`d_`، بررسی Eligibility هنگام Issuance/POST، تطبیق Offer در Ordering |

لینک‌های مرجع: [Duffel extra bags](https://duffel.com/docs/guides/adding-extra-bags)، [Duffel API order creation](https://duffel.com/docs/api/orders/create-order)، [Travelport Ancillary Shop](https://support.travelport.com/webhelp/JSONAPIs/Airv11/Content/Air11/Ancillaries/APIRef_AncillaryShop.htm)، [OWASP Key Management](https://cheatsheetseries.owasp.org/cheatsheets/Key_Management_Cheat_Sheet.html)، [OWASP Authorization](https://cheatsheetseries.owasp.org/cheatsheets/Authorization_Cheat_Sheet.html).

## B. تصمیم‌های قطعی یا پیشنهاد صریح

| موضوع | وضعیت | حکم Pack v8 |
|---|---|---|
| SeatMap | Owner approved | A07/A08 Controllers/DTOs/Reserve دست‌نخورده |
| Shopping | Owner approved | OfferId تنها پارامتر، سه Surface، Family arrays و Unitهای مشتق‌شده از Domain |
| Source Scope | Owner approved concept + Source evidence | Sector+Traveller / Portion+Traveller / Journey+Traveller / Order؛ nonTraveller PerFlight/PerPortion در کد فعلی ندارد و نباید جعل شود |
| Quantity | Owner corrective request | `quantityOptions[]` در Shopping با HMAC و Qty داخلی d_؛ **NO POST/SNAPSHOT** |
| Typed | Owner approved | امضای `r_`، GET/POST Variant-specific، یک جدول کوچک Snapshot، `s_` کوتاه |
| Resolve | Owner approved | فقط `{ids:[]}`، خواندن DB داخلی، خروجی Family DTOهای تخصصی؛ بدون Order reader |
| Forgery | Owner corrective request | Signed issuer evidence و TTL؛ Rate/POS/PTC/Unit دستکاری‌شده مردود |
| Pricing retired | Owner corrective request | Pricing parent **Active** در Issuer، POST Typed و Resolve؛ Retired/Suspended **هیچ read for registration** ندارد |
| Free | Owner approved | محصول رایگان نیز Paid/Filed با **PricingRateId واقعی + Amount=0** طبق اصلاح SeatMap؛ بدون fake Rate یا تغییر Free branch به workaround |
| Currency | Owner-supplied | `EUR currencyId=47` (در Gate0 از Catalog تأیید)، ISO numeric 978 شناسه داخلی نیست |
| D1 | Owner approved | Quote/Approval/FX تبدیلی/Unapplied Order Fee/نامشخص بودن مبلغ منتشر نشود |
| D2 | Owner approved fields | Oxygen Flow و FREMEC در Domain مجاز؛ A17 Snapshot پزشکی بدون at-rest approval **GATED** |
| D3 | Owner conditional | A09 فقط پس از FlightFlow occupancy/availability evidence؛ فعلاً GATED |
| Insurance | Source gap | هیچ Family/Variant Insurance در Registry فعلی نیست؛ array empty، نه Domain اختراعی |
| TTL | Proposal requiring Owner review | `min(sourceValidUntil,now+MaxAge)`، مقدار ۱۵ دقیقه فقط config پیشنهادی؛ هیچ hardcode Domain |
| Retry | Lean contract | POST Typed تکراری می‌تواند s_ متفاوت بسازد، Resolve با همان ID تا expiry قابل Retry؛ no dedup infra |

## C. سورس دقیق و شکاف‌های معتبر

- `src/AeroTech.Ancillary.Domain/AncillaryServiceDefinitionAggregate/AncillaryVariant.cs`: A01..A24 و ۹ Profile؛ **Insurance ندارد**.
- `src/AeroTech.Ancillary.Domain/AncillaryServiceDefinitionAggregate/CustomerSelectionContract.cs` و `src/AeroTech.Ancillary.Shopping/Selection/AncillarySelection.cs`: انتخاب‌های Typed؛ A19 متن در Contract در برابر Bool در Selection **GATED**.
- `src/AeroTech.Ancillary.Shopping/Engine/CoveragePlanner.cs`: واحدهای Traveller+Sector/Portion/Journey یا بدون Traveller در Order؛ Portion الزماً یک Flight نیست.
- `src/AeroTech.Ancillary.Shopping/Engine/PriceAssessor.cs`: مسیر قدیمی Free بدون RateId؛ با Authoring Filed صفر اصلاح شود نه جعل Rate.
- `src/AeroTech.Ancillary.Domain/AncillaryPricingAggregate/AncillaryPricing.cs`: `Status` والد؛ `src/AeroTech.Ancillary.Domain/AncillaryPricingAggregate/Entities/AncillaryPricingRate.cs`: Rate فرزند بدون Status؛ **رد Retired بر اساس parent**.
- `src/AeroTech.Ancillary.Query/Shopping/Common/FlightOffers/FlightOfferShoppingContextMapper.cs`: `FlightRef` و PortionRef فعلی؛ `src/AeroTech.Ancillary.Query/Shopping/SeatMaps/SeatMapSelectionId.cs`: Seed رفتار SeatMap، **بدون تغییر**.
- `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/GetAncillaryOfferDetail/OfferOptionCatalog.cs`: Allowed SSR و form options از Domain؛ انتخاب ناشناخته مجوز ایجاد Fake Options نیست.
- `src/AeroTech.Ancillary.Query/Shopping/Common/SalesScope/AncillarySalesScope.cs`: OwnerAirlineId, Channel, POS lists, optional Agency/Office/Customer/Country — Binding تنها از context trusted.
- `src/AeroTech.Ancillary.Shopping/Reading/IActiveAncillaryPricingReader.cs`: lookup روی Pricing aggregate، نه child Rate؛ برای RateId lookup حداقل Query لازم است.
- A18 allowed infant/guardian نیاز به actual trusted source دارد، از hash قابل بازسازی نیست؛ GATED اگر شواهد موجود کافی نیست.

## D. ممنوعیت معماری زائد

- NO generic Ancillary DTO with 30 nullable fields؛ no new Shopping Engine / Aggregate / Session / Saga / QtySnapshot / Order Reader / token DB for HMAC.
- NO ExtraSeat/Insurance/Medical/Guardian variant fictitious unapproved code to make tests green.
- NO skip of Rate/eligibility proof; no accepted Retired quote merely because signed ID exists.
- When source contradicts a required v8 field: **record exact gap in Gate0** and gate the affected capability; don't import old-pack spec or invent it.
