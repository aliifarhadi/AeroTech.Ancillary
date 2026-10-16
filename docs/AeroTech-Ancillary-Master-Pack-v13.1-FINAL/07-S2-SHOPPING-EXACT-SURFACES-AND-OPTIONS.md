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
