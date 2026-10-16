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
