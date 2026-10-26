# 02 — تنها قرارداد دقیق Reservation HTTP v11 — فیلدبه‌فیلد

**این سند Contract هدف است، نه پیاده‌سازی فعلی.** `Service` surface موجود، فقط ۶ Route (هیچ Route موازی). پاسخ‌ها و نمونه‌ها از نظر Property name **camelCase** هستند. نمونه‌های `status` با Enum متنی صرفاً خوانایی دارند؛ Representation واقعی Enums در Source/serializer فعلی Gate0 باید تثبیت شود، بدون تغییر بی‌دلیل. شناسه‌ها نمونه‌اند.

## 1. Create mixed Hold
`POST /Service/v1/Ancillaries/Service-Holds` — `Content-Type: application/json` — موفق `201 Created`.

```json
{
  "idempotencyKey":"O-12001-H1",
  "orderId":12001,
  "reference":"ORD-12001",
  "requestedExpiresAt":"2026-10-27T10:30:00Z",
  "baggages":[{"variantCode":"A01","products":[{"provisionId":701,"units":[
    {"orderServiceId":501,"unit":{"travellerId":1001},"coverage":{"flightIds":[801,802]},"selected":{"quantity":2}}
  ]}]}],
  "meals":[{"variantCode":"A11","products":[{"provisionId":702,"units":[
    {"orderServiceId":502,"unit":{"travellerId":1002},"coverage":{"flightIds":[801]}}
  ]}]}],
  "assistedTravel":[{"variantCode":"A15","products":[{"provisionId":703,"units":[
    {"orderServiceId":503,"unit":{"travellerId":1001},"coverage":{"flightIds":[801]},"selected":{"options":{"assistedTravel":{"assistanceSsrCode":"WCHR"}}}}
  ]}]}],
  "airportServices":[{"variantCode":"A20","products":[{"provisionId":704,"units":[
    {"orderServiceId":504,"unit":{"travellerId":1001},"selected":{"options":{"airportService":{"timeWithOffset":"2026-10-27T08:00:00+03:30","guestCount":1}}}}
  ]}]}]
}
```

**این مثال تنها شکل ورودی را نمایش می‌دهد.** `provisionId`های نمونه باید در دیتابیس Active و متناسب با Variant/Policy باشند؛ AirportSlot تنها وقتی کار می‌کند که Source و Facility واقعی وجود داشته باشد. `meals.A11` فقط وقتی MealCode محصول ثابت و قابل ذخیره است مجاز است. اگر `CoverageScope` واقعی Provision برای یک row با `flightIds` متفاوت باشد، همان Unit با Validation Error رد شود. 

### 1.1 Header
| Wire field | Type | Required | معنی، قاعده، Source |
|---|---|---|---|
| `idempotencyKey` | string 1..128 | بله | کلید یک Create، replay معادل same hold؛ payload متفاوت 409؛ در Reservation ثبت می‌شود |
| `orderId` | long positive | بله | correlation از سرویس Caller؛ **بدون استعلام/تطبیق Ordering** |
| `reference` | string 1..128 | بله | مرجع نمایشی/ارتباطی Ordering، **نه شناسه Policy یا PNR ساختگی** |
| `requestedExpiresAt` | DateTimeOffset آینده | بله | انقضای Hold؛ مقدار Arbitrary داخلی تولید نشود؛ در append Seat برابر Root |
| `baggages` | `HoldVariant<Baggage>`[] | شرطی | همان نام در AncillaryOffers؛ اگر خالی حذف/[] مجاز، دست‌کم یک Unit در مجموع |
| `meals` | `HoldVariant<Meal>`[] | شرطی | همان نام Offers |
| `pets` | `HoldVariant<Pet>`[] | شرطی | همان نام Offers |
| `assistedTravel` | `HoldVariant<AssistedTravel>`[] | شرطی | همان نام Offers |
| `airportServices` | `HoldVariant<AirportService>`[] | شرطی | همان نام Offers |
| `priorityServices` | `HoldVariant<Priority>`[] | شرطی | همان نام Offers |
| `connectivity` | `HoldVariant<Connectivity>`[] | شرطی | همان نام Offers |
| `upgrades` | `HoldVariant<Upgrade>`[] | شرطی | همان نام Offers؛ A10 unsupported ExternalQuote باید صریح GATED |

`extraSeats`/`travelInsurance` در خروجی AncillaryOffers فعلی وجود دارند اما **آرایه خرید قابل‌انجام در این generic Hold نیستند**؛ A09 SeatMap مجزا و TravelInsurance بدون Variant فعال است. **اگر Caller آن‌ها را در این Endpoint بفرستد 400**؛ نه سکوت و نه پاسخ موفق جعلی. هیچ `services[]` generic وجود ندارد.

### 1.2 لایه Variant → Product → Units (نام/شکل مطابق AncillaryOffers)
| Path | Type | Required | توضیح دقیق |
|---|---|---|---|
| `{family}[].variantCode` | string `A01`..`A24` | بله | **همان نام/مقدار** Offers; در Hold فقط cross-check با `Definition.VariantCode`؛ حقیقت از Provision/Definition، نه Client |
| `{family}[].products` | array | بله، nonempty | همان Nesting موجود؛ بین چند Definition/Provision متفاوت Variant تمایز می‌گذارد |
| `products[].provisionId` | long | بله | **همان نام** `Resolve.provision.provisionId`؛ از `AncillaryOffers.products[].productKey` نمایشی استخراج نشود؛ `provisionId` جایگزین کم‌هزینه و دقیق برای شناسایی محصول در Hold است |
| `products[].units` | array | بله، nonempty | همان نام/لایه Offers؛ هر Unit یک OrderService جدا، هیچ Row حذف/ادغام نشود |
| `units[].orderServiceId` | long | بله | شناسه سرویس Caller؛ در کل درخواست یکتا؛ فقط correlation، نه بررسی تعلق به Order |
| `units[].unit` | `HoldUnitDto` | شرطی | همان Property `unit` Offers؛ در Hold فقط `travellerId:long` وقتی Provision مسافرمحور است. `travellerRef` Offer Scoped به‌دلیل عدم کفایت برای Order ledger منتقل نمی‌شود |
| `units[].coverage` | `HoldCoverageDto` | شرطی | همان Property `coverage` Offers؛ **تنها** `flightIds:long[]` وقتی خدمت واقعاً Flight-related/Local stock نیاز دارد؛ `flightRefs` در Offer string است و مستقیماً `FlightCountInventory` کلید numeric ندارد |
| `units[].selected` | `HoldSelectedUnitDto` | شرطی | همان Property و شکل `Resolve.selected`؛ `quantity` یا `options` فقط در موارد موردنیاز؛ وقتی Qty ثابت=1 و بدون Options می‌تواند حذف شود |

**این دو تغییر، نه اختراع مفاهیم جدید بلکه تغییر نوع شناسه بین Surfaceها هستند:** `AncillaryOffers.unit.travellerRef:string` / `coverage.flightRefs:string[]` برای نمای Shopping؛ در Reservation `unit.travellerId:long` / `coverage.flightIds:long[]` برای نگهداری شناسه واقعی اجرایی. Caller، نه Ancillary، این نگاشت را انجام می‌دهد. **هیچ `coverageScope`، `coveredFlightIds` و `inventoryFacts` در Wire نیست؛** `CoverageScope` از Provision است و `CoveredFlightIds` داخلی از `coverage.flightIds` مشتق می‌شود.

### 1.3 `HoldUnitDto`, `HoldCoverageDto`, `HoldSelectedUnitDto`
| Field | Type | When required | Checks / storage |
|---|---|---|---|
| `unit.travellerId` | long >0 | Provision.Pax-scoped | فقط ساختار مثبت، بدون lookup Ordering؛ PerOrder باید `unit` حذف یا `{}` باشد |
| `coverage.flightIds` | long[] positive/distinct | وقتی Flight Stock یا Coverage پروازی است | `Sector` دقیقاً ۱؛ `Portion/Journey` تعداد واقعی از Caller؛ اگر منفعت محصول Order-wide باشد `coverage` ممکن است حذف شود؛ SQL resource mapping از Source موجود |
| `selected.quantity` | int >0 | اگر QuantityChoice یا مقدار به‌جز ۱ انتخاب شده | با `Provision.Quantity.MinQuantity/MaxQuantity` کنترل؛ fixed=1 در صورت نبود انتخاب، derive؛ Qty=0 یعنی هیچ Unitی نباید به Hold فرستاده شود |
| `selected.options` | `SelectionOptionsDto` typed | فقط Typed Selection | **همان نام و شکل `Resolve.selected.options`**: دقیقاً یک کلید Family، هیچ Client-owned CustomJson |

**نکته دقیق:** `selected.options` برای Typed باید همان DTO خانوادگی پذیرفته‌شده در Price باشد. اگر در Resolve `options` null است اما محصول Variant دارای انتخاب متغیر در `details` است، Ancillary فقط وقتی می‌تواند مقدار را از Definition/Provision جاری **به‌صورت یکتا** تعیین کند Hold می‌سازد؛ در غیراین‌صورت GATED تا منبع انتخاب صریح تعریف شود. هیچ حدس یا تبدیل `Price` به Lookup اجباری جدید.

### 1.4 Typed Options (نام‌ها **دقیقاً** از Price/Resolve فعلی)
| Family option key در `selected.options` | Field | Type | Variant/Rule |
|---|---|---|---|
| `baggage` | `bagRef` | string | A03، required |
| `baggage` | `measuredWeightKg` | decimal | A03، >0، محدودیت AcceptedWeightKg در Policy فعلی پابرجا |
| `baggage` | `dimensions` | `{lengthCm,widthCm,heightCm}` | A04/A06 اگر لازم، مطابق `SelectedDimensionsInput` |
| `baggage` | `weightKg` | decimal | A06 اگر Rule انتخاب می‌خواهد |
| `pet` | `animalType` | existing PetAnimalType enum | A13/A14؛ مقدار واقعی enum از Source، عدد حدسی ممنوع |
| `pet` | `otherAnimalCode` | string? | وقتی animalType other و کد لازم است |
| `pet` | `animalAgeWeeks` | int? | اگر Source مشتری/قانون لازم دارد |
| `pet` | `combinedWeightKg` | decimal | A13/A14، وزن نهایی حیوان+باکس |
| `pet` | `carrierDimensions` | SelectedDimensionsInput | A13/A14؛ **نه `carrierDimensionsCm` خودساخته** |
| `pet` | `documentAcknowledgements` | string[]? | اگر کدهای مدرک در Definition لازم است، بدون ذخیره سند واقعی |
| `pet` | `sizeBracket` | string? | A14 وقتی انتخاب لازم است |
| `assistedTravel` | `assistanceSsrCode` | string | A15/A16؛ مثل WCHR/WCHS |
| `assistedTravel` | `infantRef`, `guardianRef` | string | A18، فقط با Source/Rules معتبر و بدون Ordering lookup |
| `airportService` | `timeWithOffset` | DateTimeOffset? | A20/A21/A22 وقتی Appointment/Slot لازم دارد؛ از **همین فیلد** Inventory fact استخراج شود |
| `airportService` | `guestCount` | int? | وقتی محصول مجاز، ≥0؛ تعداد افراد از Policy |
| `priority` | `optIn` | bool | A23؛ true یا بدون Unit؛ false نباید Hold شود |
| `connectivity` | `deviceCount` | int? | A24 وقتی Definition اجازه می‌دهد |

A02 `packageProductRef`, A06 `equipmentKind`, A11 `mealCode`, A12 `menuItemRef`, A24 `planCode` در `CustomerSelectionContract`/Details وجود دارند اما بعضی **در Selected.Options فعلی Price وجود ندارند**. در v11 آن‌ها را در Hold DTO خودسرانه اضافه نکنید: اگر واقعاً از Definition/Provision به‌شکل **یکتا** ثابت‌اند، در زمان Hold snapshot کنید؛ در غیراین‌صورت همان Variant GATED تا قرارداد Price/Resolve با Owner در Stage جدا تأیید شود. **این خلأ نباید با nullable fields جعلی پنهان شود.**

### 1.5 قواعد خطا و تطبیق Family/Variant
- JSON Strict (`JsonUnmappedMemberHandling.Disallow`) در همه DTOهای v11؛ unknown field, unexpected `services[]`, `coverageScope`, `inventoryFacts`, duplicate keys → 400/422 طبق پروژه؛ هیچ silent ignore.
- `variantCode` با `provision.ServiceDefinitionId -> definition.VariantCode` و Family با `definition.Profile` تطبیق محلی؛ mismatch → 422. این **اعتبارسنجی خود Ancillary** است، نه Ordering.
- `provisionId` متعلق به Definition فعال و Policy فعال باشد؛ Supplier/Approval/unsupported: fail-closed. اختیاری‌بودن `unit/coverage/selected` برحسب Scope/Variant/Policy است، نه آزادبودن بی‌قاعده.
- یک `orderServiceId` در کل درخواست تکرار نشود. Retired/inactive Definition و Provision Hold جدید را رد می‌کند.
- Canonical idempotency request شامل Variant, ProvisionId, OrderServiceId, travellerId, flights canonical, Qty و Selected typed options است؛ ترتیب Family/Product/Unit در replay بی‌اهمیت باشد. هیچ جدول Fingerprint جدا ساخته نشود.

## 2. Receipt کوچک برای پاسخ عملیات Write
`201` Create و `200` Confirm/Release/Cancel؛ Seat-only/append مطابق status code REST موجود.

```json
{
  "holdId":9001,
  "reference":"ORD-12001",
  "status":1,
  "expiresAt":"2026-10-27T10:30:00Z",
  "units":[
    {"unitId":9101,"orderServiceId":501,"status":1},
    {"unitId":9102,"orderServiceId":502,"status":1},
    {"unitId":9103,"orderServiceId":503,"status":1},
    {"unitId":9104,"orderServiceId":504,"status":1}
  ]
}
```

| Field | CLR/JSON type | Meaning |
|---|---|---|
| `holdId` | long | شناسه Aggregate و مرجع تمام عملیات بعدی؛ generated Ancillary |
| `reference` | string | همان Correlation request؛ **نه Unit reference ساختگی** |
| `status` | enum موجود | کل وضعیت عملیاتی Reservation |
| `expiresAt` | DateTimeOffset? | موعد Hold؛ بعد Confirm هم برای تاریخچه می‌ماند؛ nullable فقط برای Legacy |
| `units[].unitId` | long | شناسه مستقل داخل Hold، برای Cancel/Release جزئی |
| `units[].orderServiceId` | long | correlation به Caller |
| `units[].status` | enum موجود | وضعیت همان Unit؛ Released و Cancelled نیز گزارش می‌شوند |

**چرا Receipt کوتاه است؟** Caller خودش انتخاب و Order را دارد؛ تکرار `coverageScope`, Definition, Provision, pricing, SSR, token, flightIds در پاسخ Write لازم نیست. جزئیات تاریخچه را GET می‌دهد. هیچ ProviderUnitRef ساختگی تولید نشود.

## 3. GET Reservation با انتخاب‌های تخصصی تاریخی
`GET /Service/v1/Ancillaries/Service-Holds/{holdId:long}` → `200 OK`.

```json
{
  "holdId":9001,
  "orderId":12001,
  "reference":"ORD-12001",
  "status":1,
  "expiresAt":"2026-10-27T10:30:00Z",
  "baggages":[{"variantCode":"A01","products":[{"provisionId":701,"units":[
    {"unitId":9101,"orderServiceId":501,"status":1,"unit":{"travellerId":1001},"coverage":{"flightIds":[801,802]},"selected":{"quantity":2}}
  ]}]}],
  "meals":[{"variantCode":"A11","products":[{"provisionId":702,"units":[
    {"unitId":9102,"orderServiceId":502,"status":1,"unit":{"travellerId":1002},"coverage":{"flightIds":[801]},"selected":{"quantity":1}}
  ]}]}],
  "assistedTravel":[{"variantCode":"A15","products":[{"provisionId":703,"units":[
    {"unitId":9103,"orderServiceId":503,"status":1,"unit":{"travellerId":1001},"coverage":{"flightIds":[801]},"selected":{"quantity":1,"options":{"assistedTravel":{"assistanceSsrCode":"WCHR"}}}}
  ]}]}],
  "airportServices":[{"variantCode":"A20","products":[{"provisionId":704,"units":[
    {"unitId":9104,"orderServiceId":504,"status":1,"unit":{"travellerId":1001},"selected":{"quantity":1,"options":{"airportService":{"timeWithOffset":"2026-10-27T08:00:00+03:30","guestCount":1}}}}
  ]}]}]
}
```

**GET projection:** تنها از `AncillaryReservation` و childهای persisted. حتی اگر Definition/Provision Retired یا Price Snapshot expired باشد، برگرداندن اطلاعات تخصصی مجاز است. **هیچ Query زنده به Price/Offer/Ordering/Definition برای ساخت اطلاعات پذیرفته‌شده** مجاز نیست. اگر بعضی داده ثابتِ مؤثر (مثلاً MealCode/Plan) در زمان Hold snapshot شده، آن‌ها در Detail تخصصی GET به‌صورت نام‌های همان `Details` در `AncillaryOffers` برگردند؛ نه فیلد اختراعی Generic `attributes`.

| Field اضافی GET | Type | Meaning |
|---|---|---|
| `orderId` | long | persisted correlation، فقط GET |
| `units[].unitId` | long | شناسه Entity |
| `units[].status` | enum موجود | state واحد |
| `units[].cancellationReasonCode` | string? | فقط Cancelled؛ در حالت دیگر حذف |
| `units[].selected.quantity` | int | همیشه مقدار واقعی snapshot شده، حتی اگر در Create omitted بود |
| `units[].selected.options` | family-specific options? | فقط انتخاب تخصصی واقعی؛ در SQL immutable و قابل round-trip |
| `products[].details` | same relevant `*DetailsDto` subset? | فقط ثابت مؤثر بر Fulfillment که در Hold ثبت شده؛ از Definition زنده تولید نشود؛ اگر ثابت کاربردی وجود ندارد حذف |

در GET Familyهای بدون Unit می‌توانند حذف شوند؛ **این تصمیم هدف قرارداد v11 است** تا به‌جای ۱۰ آرایه خالی پاسخ کم‌حجم بماند. در Gateway فقط یک Serializer policy ثابت اعمال شود؛ واردکردن شکل متفاوت به ازای status ممنوع.

## 4. سایر endpointها ــ دقیق
### 4.1 Confirm all
```http
POST /Service/v1/Ancillaries/Service-Holds/9001/Confirmations
```
Body ندارد؛ `200` Receipt. فقط Held فعال قبل از deadline Confirm؛ Unitهای Released دست‌نخورده؛ تأیید مجدد Idempotent. **این عملیات صدور EMD نیست.**

### 4.2 Release all یا برخی Held
```http
DELETE /Service/v1/Ancillaries/Service-Holds/9001?unitIds=9102&unitIds=9103
```
بدون Body؛ `unitIds:long[]?` positive/distinct؛ absent = کل Unitهای Held؛ وجود یکی invalid/Confirmed/foreign ⇒ **همه عملیات rollback**. Replay همان Released no-op و response current Receipt؛ ظرفیت از predicate ledger با تغییر وضعیت آزاد می‌شود. **بدون Route `Units/{unitId}`**.

### 4.3 Cancel all یا برخی Confirmed
```http
POST /Service/v1/Ancillaries/Service-Holds/9001/Cancellations
Content-Type: application/json

{"reasonCode":"CUSTOMER_REQUEST","unitIds":[9101]}
```
| Field | Type | Rule |
|---|---|---|
| `reasonCode` | string 1..50 | بله؛ در Unit persist؛ CancellationRule تجاری/Refund متعلق Ordering، نه fake در Ancillary |
| `unitIds` | long[]? | اختیاری؛ absent = تمام Confirmed؛ nonempty positive distinct؛ all-or-nothing |

`200` Receipt. Replay همان reason no-op؛ همان Unit با reason متفاوت 409؛ Held قبل Confirm باید Release شود. **Seat physical cancel انجام نمی‌دهد.**

### 4.4 SeatMap commercial Hold / Append (Facade کاملاً اختصاصی)
```http
POST /Service/v1/Ancillaries/SeatMaps/Holds
```
این یک API قبلاً موجود است؛ **در v11 ورودی‌های SeatMap را مجدداً طراحی نکنید**. فقط Endpoint به همان Root/Receipt کوتاه نگاشت می‌شود. JSON نمونه با داده نمایشی:

```json
{
  "idempotencyKey":"O12001-SEAT-1",
  "orderId":12001,
  "orderCommercialVersion":1,
  "reference":"ORD-12001",
  "requestedExpiresAt":"2026-10-27T10:30:00Z",
  "pointOfSaleId":1,
  "currencyId":47,
  "ticketedAtUtc":null,
  "holdId":9001,
  "seats":[{
    "orderServiceId":505,
    "travellerId":1001,
    "seatMapSelectionId":"<real-seatmap-selection-id>",
    "passengerTypeCode":1,
    "dateOfBirth":null,
    "flight":{"flightNumber":"W5-100","marketingAirlineId":1,"operatingAirlineId":1,"originAirportId":11,"destinationAirportId":21,"departureAt":"2026-11-01T06:00:00Z","arrivalAt":"2026-11-01T08:00:00Z","aircraftId":111,"cabinClassId":1,"rbdId":4},
    "fare":{"airFareId":778,"airFareType":1,"fareBasis":"YTEST","bookingClass":"Y"},
    "couponOpenForUse":null
  }]
}
```

**SeatMap جدول دقیق Fieldها بر اساس C# موجود**:
| Field | CLR type | Source / Rule |
|---|---|---|
| `holdId` | long? | null/absent ایجاد Seat-only؛ positive Append به Hold موجود؛ Header باید معادل باشد |
| `idempotencyKey` | string | Create root key / Append operation key؛ exact retry no new Units |
| `orderId` | long | Caller correlation؛ بدون Ordering lookup |
| `orderCommercialVersion` | int | >0، context Verifier فعلی؛ **نه تطبیق Ordering** |
| `reference` | string | Root reference؛ در Append همان Parent |
| `requestedExpiresAt` | DateTimeOffset | future/create و برابر Parent/append |
| `pointOfSaleId` | long | تجاری SeatMap Verifier؛ عدد نمونه ثابت نشود |
| `currencyId` | int | Currency catalog actual؛ EUR=47 فقط مثال محیط قبلی |
| `ticketedAtUtc` | DateTimeOffset? | Post-ticketing conditions، فقط اگر هست |
| `seats` | `SeatHoldSelectionInput[]` | حداقل ۱ انتخاب؛ duplicate SeatMap ID رد |
| `seats[].orderServiceId` | long | correlation |
| `seats[].travellerId` | long | correlation |
| `seats[].seatMapSelectionId` | string | شناسه انتخاب Commercial از SeatMap، نه `selectionId` basket |
| `seats[].passengerTypeCode` | `PassengerTypeCode` | عضو enum موجود (`ADT=1`,`CHD=27`)؛ actual serializer را Gate0 بررسی کند |
| `seats[].dateOfBirth` | DateOnly? | اگر rules نیاز دارند؛ بدون Ordering check |
| `seats[].flight` | `SeatHoldFlightInput` | facts پرواز دریافت‌شده از Caller، shape موجود زیر |
| `seats[].fare` | `SeatHoldFareInput?` | فقط طبق verifier/fare rules |
| `seats[].couponOpenForUse` | bool? | PostTicketed conditional |

**`SeatHoldFlightInput` — همان DTO موجود، فیلدبه‌فیلد:**

| Field | CLR type طبق قرارداد v10 | Validation / Usage |
|---|---|---|
| `flightNumber` | string | Required، nonblank، max16 |
| `marketingAirlineId` | int | Required، مثبت |
| `operatingAirlineId` | int | Required، مثبت |
| `originAirportId` | int | Required، مثبت |
| `destinationAirportId` | int | Required، مثبت |
| `departureAt` | DateTimeOffset | Required، زمان واقعی پرواز |
| `arrivalAt` | DateTimeOffset | Required، زمان ورود، rules همان Verifier |
| `aircraftId` | int | Required، مثبت |
| `cabinClassId` | int | Required، مثبت |
| `rbdId` | long? | Optional، در صورت حضور مثبت |

**`SeatHoldFareInput` — همان DTO موجود، فیلدبه‌فیلد:**

| Field | CLR type طبق قرارداد v10 | Validation / Usage |
|---|---|---|
| `airFareId` | long | Required، مثبت وقتی fare حضور دارد |
| `airFareType` | AirFareType existing enum | Required، عضو واقعی Enum |
| `fareBasis` | string? | Optional، max64 |
| `bookingClass` | string? | Optional، max2 |

این DTOها در Source فعلی بدون تغییر باقی می‌مانند؛ Gate0 باید نوع CLR و Nullable واقعی را تأیید کند. اگر مثال با Source فرق داشت **مثال را تصحیح کند**، نه اینکه Endpoint SeatMap را بازطراحی کند. Ancillary هرگز FlightFlow/OccupiedSeats را در Hold/Confirm/Cancel صدا نمی‌زند.

## 5. Status/Errors/retry/TTL
| وضعیت | Operation | Outcome |
|---|---|---|
| Held زنده | Confirm | Confirmed (Unitهای Released unaffected) |
| Held زنده | partial/all Release | Units Released، Root براساس باقی‌مانده |
| Confirmed | Cancel | Units Cancelled، Root براساس باقی‌مانده |
| Held منقضی | Confirm | 409، Inventory logically free even before poller |
| Confirmed | Release | 409 |
| Seat-only/Seat append | physical FlightFlow | **not invoked** |
| Idempotency key identical/body identical | Create/Append retry | Same ReservationId/UnitIds |
| Idempotency same key/different body | Retry | 409 |

**خطای عددی موجود حفظ** (`16402` invalid, `16403` idempotency conflict, `16407` expired, `16417` shortage, `16418` Unit foreign, `16419` not reservable). برای Variantهای GATED خطای سازگار موجود؛ هیچ Error taxonomy تازه ساخته نشود. Poller قدیمی Expire را persisted می‌کند؛ countdown از `ExpiresAt` خود Reservation، نه Selection TTL.
