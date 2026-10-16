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
