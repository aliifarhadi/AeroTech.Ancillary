# 02 — تنها قرارداد دقیق Reservation HTTP v11 — فیلدبه‌فیلد

**This is the target contract, not the current implementation.** The existing `Service` surface keeps its six routes (no parallel route). The wire representation (camelCase names, enums by name, longs as strings) is fixed in §0. Ids in the samples are illustrative.

## 0. Wire representation (fixed by Gate 0 from the source)
- Property names are camelCase.
- Enums are written by name (`"status":"Held"`) and numbers are accepted on input (`JsonStringEnumConverter`, `Framework/AeroTech.Framework.Presentation/Extensions/PresentationExtensions.cs:68`).
- Every `long`, including inside arrays (ids, `flightIds`, `unitIds`), is written as a JSON string and accepted as a string or a number (`LongToStringJsonConverter`, same file lines 69–70). `int` and `decimal` stay numbers.
- Request samples below use numbers (accepted); response samples show the real output.

## 1. Create mixed Hold
`POST /Service/v1/Ancillaries/Hold` — `Content-Type: application/json` — موفق `201 Created`.

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
| `assistedTravel` | `HoldVariant<AssistedTravel>`[] | شرطی | همان نام Offers |
| `airportServices` | `HoldVariant<AirportService>`[] | شرطی | همان نام Offers |
| `priorityServices` | `HoldVariant<Priority>`[] | شرطی | همان نام Offers |
| `connectivity` | `HoldVariant<Connectivity>`[] | شرطی | همان نام Offers |
| `upgrades` | `HoldVariant<Upgrade>`[] | شرطی | همان نام Offers؛ A10 unsupported ExternalQuote باید صریح GATED |

`extraSeats`/`travelInsurance` در خروجی AncillaryOffers فعلی وجود دارند اما **آرایه خرید قابل‌انجام در این generic Hold نیستند**؛ A09 SeatMap مجزا و TravelInsurance بدون Variant فعال است. **اگر Caller آن‌ها را در این Endpoint بفرستد 400**؛ نه سکوت و نه پاسخ موفق جعلی. هیچ `services[]` generic وجود ندارد.

**`pets` is not a member of the Hold request (Owner R1 Q3).** Every pet variant (A13/A14) is GATED, so the array is left out until a stage opens pets. A request that sends it gets 400, like `extraSeats`/`travelInsurance`.

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
| `units[].selected` | `HoldSelected<Family>` | شرطی | typed by its family array (Owner R1 Q1, §1.3); same property and shape as `Resolve.selected`; omitted when the quantity is fixed at 1 and the family has no option |

**این دو تغییر، نه اختراع مفاهیم جدید بلکه تغییر نوع شناسه بین Surfaceها هستند:** `AncillaryOffers.unit.travellerRef:string` / `coverage.flightRefs:string[]` برای نمای Shopping؛ در Reservation `unit.travellerId:long` / `coverage.flightIds:long[]` برای نگهداری شناسه واقعی اجرایی. Caller، نه Ancillary، این نگاشت را انجام می‌دهد. **هیچ `coverageScope`، `coveredFlightIds` و `inventoryFacts` در Wire نیست؛** `CoverageScope` از Provision است و `CoveredFlightIds` داخلی از `coverage.flightIds` مشتق می‌شود.

**Resolve is flat per family** (`ServiceResolvedSelectionsDto`): each item carries `variantCode`, `provision.provisionId`, `unit`, `coverage` and `selected`. The caller groups Resolve items by `variantCode` and `provision.provisionId` to build `products[].units[]`; Offers groups its products the same way (product = provision, `AncillaryOffersMapper.ProductsOf`).

### 1.3 `HoldUnitDto`, `HoldCoverageDto`, `HoldSelected<Family>`
| Field | Type | When required | Checks / storage |
|---|---|---|---|
| `unit.travellerId` | long >0 | required whenever `unit` is sent (Owner R1 Q4) | فقط ساختار مثبت، بدون lookup Ordering؛ PerOrder `unit` را حذف می‌کند؛ `unit:{}` رد می‌شود (400, shape rule) |
| `coverage.flightIds` | long[] positive/distinct | وقتی Flight Stock یا Coverage پروازی است | `Sector` دقیقاً ۱؛ `Portion/Journey` تعداد واقعی از Caller؛ اگر منفعت محصول Order-wide باشد `coverage` ممکن است حذف شود؛ SQL resource mapping از Source موجود |
| `selected.quantity` | int >0 | اگر QuantityChoice یا مقدار به‌جز ۱ انتخاب شده | با `Provision.Quantity.MinQuantity/MaxQuantity` کنترل؛ fixed=1 در صورت نبود انتخاب، derive؛ Qty=0 یعنی هیچ Unitی نباید به Hold فرستاده شود |
| `selected.options` | the family's own options record | فقط Typed Selection | only the key of its own family array (Owner R1 Q1): `baggages` → `baggage`, `assistedTravel` → `assistedTravel`, `airportServices` → `airportService`, `priorityServices` → `priority`, `connectivity` → `connectivity`; `meals` and `upgrades` have no `options`. Same key and fields as `Resolve.selected.options`; no client-owned CustomJson |

**Exact rule (Owner R1 Q1, the Offers generic pattern):** each family array is typed by its own `selected` record, the way Offers types each family by its details (`AncillaryVariantGroupDto<BaggageDetailsDto>`): `AncillaryVariantHoldInput<TSelected>` → `AncillaryProductHoldInput<TSelected>` → `AncillaryUnitHoldInput<TSelected>`. An option family takes `SelectedUnitInput<TOptions>` = `{quantity?, options?}`, whose `options` record has exactly one member, the family key; `meals` and `upgrades` take `SelectedUnitInput` = `{quantity?}`. A key of another family is not a member, so the strict binder refuses it (400) before validation. The content of each key is Price's record (`BaggageSelectionOptions`, `AirportServiceSelectionOptions`, `PrioritySelectionOptions`, `ConnectivitySelectionOptions`) except `assistedTravel`, which carries `assistanceSsrCode` only (Owner R1 Q3). Price's flat `SelectionOptionsInput` union is not used by the Hold. A choice that is not in `options` is either fixed uniquely by the Definition/Provision (the fixed choices of §1.4, confirmed by Gate 0 for every supported variant) or the variant is GATED. No guess and no new mandatory lookup in Price.

### 1.4 Typed Options (نام‌ها **دقیقاً** از Price/Resolve فعلی)
| Family option key در `selected.options` | Field | Type | Variant/Rule |
|---|---|---|---|
| `baggage` | `bagRef` | string | A03، required |
| `baggage` | `measuredWeightKg` | decimal | A03، >0، محدودیت AcceptedWeightKg در Policy فعلی پابرجا |
| `baggage` | `dimensions` | `{lengthCm,widthCm,heightCm}` | A04/A06 اگر لازم، مطابق `SelectedDimensionsInput` |
| `baggage` | `weightKg` | decimal | A06 اگر Rule انتخاب می‌خواهد |
| `pet` | — | — | not a Hold member (Owner R1 Q3): A13/A14 are **GATED** (Owner Q1) because the Definition forces `SubjectToConfirmation`; the `pets` array comes back with the stage that opens pets |
| `assistedTravel` | `assistanceSsrCode` | string | A15/A16؛ مثل WCHR/WCHS |
| `assistedTravel` | `infantRef`, `guardianRef` | — | not Hold members (Owner R1 Q3): only A18 asks for them and A18 is **GATED** (Owner Q1); both are offer-scoped traveller refs, not Order ids |
| `airportService` | `timeWithOffset` | DateTimeOffset? | A20/A21/A22 وقتی Appointment/Slot لازم دارد؛ از **همین فیلد** Inventory fact استخراج شود |
| `airportService` | `guestCount` | int? | وقتی محصول مجاز، ≥0؛ تعداد افراد از Policy |
| `priority` | `optIn` | bool | A23؛ true یا بدون Unit؛ false نباید Hold شود |
| `connectivity` | `deviceCount` | int? | A24 وقتی Definition اجازه می‌دهد |

**Fixed choices (Owner Q1, Q4).** A02 package, A06 `equipmentKind`, A10 `toCabinId` (`FixedAncillary` only), A11 `mealCode`, A12 `menuItemRef`, A20–A22 `airportId`/`facilityId` and the A24 plan are not request fields. Each is fixed uniquely by the Definition (and the Provision for the facility) with the rule Price already uses (`TypedSelectionForm.WithFixedChoices`); Offers hides a product whose fixed choice is missing (`CustomerInputs.FixedChoicesAreDefined`). The Hold captures them at Hold time and the GET returns them from storage (§3). The A02 package and the A24 plan are the Definition's `serviceDefinitionRef`.

### 1.5 قواعد خطا و تطبیق Family/Variant
- JSON Strict (`JsonUnmappedMemberHandling.Disallow`) در همه DTOهای v11؛ unknown field, unexpected `services[]`, `coverageScope`, `inventoryFacts`, duplicate keys → 400/422 طبق پروژه؛ هیچ silent ignore. A `selected.options` key of another family, `pets`, `infantRef` and `guardianRef` are not members of the Hold request and fail binding with 400 (Owner R1).
- `variantCode` با `provision.ServiceDefinitionId -> definition.VariantCode` و Family با `definition.Profile` تطبیق محلی؛ mismatch → 422. این **اعتبارسنجی خود Ancillary** است، نه Ordering.
- `provisionId` متعلق به Definition فعال و Policy فعال باشد؛ Supplier/Approval/unsupported: fail-closed. اختیاری‌بودن `unit/coverage/selected` برحسب Scope/Variant/Policy است، نه آزادبودن بی‌قاعده.
- یک `orderServiceId` در کل درخواست تکرار نشود. Retired/inactive Definition و Provision Hold جدید را رد می‌کند.
- Canonical idempotency request شامل Variant, ProvisionId, OrderServiceId, travellerId, flights canonical, Qty و Selected typed options است؛ ترتیب Family/Product/Unit در replay بی‌اهمیت باشد. هیچ جدول Fingerprint جدا ساخته نشود.
- Option validation depth (Owner Q2): only the static rules of the Definition and Provision — the variant's required fields, allowed values (SSR codes, animal types, size brackets, document codes) and numeric ranges (weight, dimensions, guests, devices). It runs through one Application port implemented in Query with the same rules Price uses (`TypedSelectionForm`); commercial eligibility (traveller, route, point of sale) is not re-evaluated.
- `SeatMaps/Holds` keeps its current request binding (not strict); it is not redesigned in v11.

## 2. Small Receipt for every write
`201` for Create and for `SeatMaps/Holds` (create or append); `200` for Confirm, Release and Cancel. All of them answer the same Receipt.

```json
{
  "holdId":"9001",
  "reference":"ORD-12001",
  "status":"Held",
  "expiresAt":"2026-10-27T10:30:00+00:00",
  "units":[
    {"unitId":"9101","orderServiceId":"501","status":"Held"},
    {"unitId":"9102","orderServiceId":"502","status":"Held"},
    {"unitId":"9103","orderServiceId":"503","status":"Held"},
    {"unitId":"9104","orderServiceId":"504","status":"Held"}
  ]
}
```

| Field | CLR / JSON | Meaning |
|---|---|---|
| `holdId` | long / string | Aggregate id, generated by Ancillary; the reference for every later operation |
| `reference` | string | the request correlation; not a fabricated unit reference |
| `status` | `AncillaryReservationStatus` / name | overall state of the reservation |
| `expiresAt` | DateTimeOffset? | hold deadline; kept after Confirm for history |
| `units[].unitId` | long / string | unit id inside the hold, used by partial Release and Cancel |
| `units[].orderServiceId` | long / string | correlation to the caller |
| `units[].status` | `AncillaryReservationUnitStatus` / name | state of the unit; Released and Cancelled units are listed too |

The Receipt is short on purpose: the caller already has the selection and the Order. It echoes no Definition, Provision, scope, pricing, flights, SSR or SeatMap token. Details come from the GET. No fabricated ProviderUnitRef.

## 3. GET Reservation with the historical typed selections
`GET /Service/v1/Ancillaries/Hold/{holdId:long}` → `200 OK`.

```json
{
  "holdId":"9001",
  "orderId":"12001",
  "reference":"ORD-12001",
  "status":"Held",
  "expiresAt":"2026-10-27T10:30:00+00:00",
  "baggages":[{"variantCode":"A01","products":[{"provisionId":"701","serviceDefinitionRef":"BAG_EXTRA_23","units":[
    {"unitId":"9101","orderServiceId":"501","status":"Held","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801","802"]},"selected":{"quantity":2}}
  ]}]}],
  "meals":[{"variantCode":"A11","products":[{"provisionId":"702","serviceDefinitionRef":"MEAL_VGML","details":{"mealCode":"VGML"},"units":[
    {"unitId":"9102","orderServiceId":"502","status":"Held","unit":{"travellerId":"1002"},"coverage":{"flightIds":["801"]},"selected":{"quantity":1}}
  ]}]}],
  "assistedTravel":[{"variantCode":"A15","products":[{"provisionId":"703","serviceDefinitionRef":"ASSIST_WCHR","units":[
    {"unitId":"9103","orderServiceId":"503","status":"Held","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"quantity":1,"options":{"assistedTravel":{"assistanceSsrCode":"WCHR"}}}}
  ]}]}],
  "airportServices":[{"variantCode":"A20","products":[{"provisionId":"704","serviceDefinitionRef":"LOUNGE_IKA","details":{"airportId":2,"facilityId":"9001"},"units":[
    {"unitId":"9104","orderServiceId":"504","status":"Held","unit":{"travellerId":"1001"},"selected":{"quantity":1,"options":{"airportService":{"timeWithOffset":"2026-10-27T08:00:00+03:30","guestCount":1}}}}
  ]}]}],
  "seats":[{"variantCode":"A07","products":[{"provisionId":"705","serviceDefinitionRef":"SEAT_STANDARD","units":[
    {"unitId":"9105","orderServiceId":"505","status":"Held","unit":{"travellerId":"1001"},"coverage":{"flightIds":["801"]},"selected":{"quantity":1},"seatNumber":"12A","seatMapSelectionId":"<seat-map-selection-id>"}
  ]}]}]
}
```

**GET projection:** only from `AncillaryReservation` and its persisted children. Even when the Definition or Provision is retired or the Price snapshot has expired, the stored details are returned. **No live query** to Price, Offers, Ordering or the Definition to build accepted data.

| Field (GET only) | CLR / JSON | Meaning |
|---|---|---|
| `orderId` | long / string | stored correlation |
| family arrays | `baggages, meals, assistedTravel, airportServices, priorityServices, connectivity, upgrades, seats` | a family with no unit is omitted; one serializer policy for every status |
| `products[].provisionId` | long / string | the provision of the unit |
| `products[].serviceDefinitionRef` | string | snapshot of the Definition at Hold time (same name as the Offers product); it is also the A02 package and the A24 plan |
| `products[].details` | family record, omitted when the family has no fixed fact | only fixed facts snapshotted at Hold, with the Offers `*DetailsDto` names: baggages `{equipmentKind}` (A06); meals `{mealCode, menuItemRef}` (A11/A12); upgrades `{toCabinId}` (A10 fixed); airportServices `{airportId, facilityId}` (A20–A22) |
| `units[].unitId` | long / string | unit id |
| `units[].status` | enum name | state of the unit |
| `units[].cancellationReasonCode` | string? | only for Cancelled units; omitted otherwise |
| `units[].selected.quantity` | int | always the stored value, even when it was omitted at Create |
| `units[].selected.options` | the family's own options record | typed per family like Create (Owner R1 Q2): same key and fields, null members omitted; `meals`, `upgrades` and `seats` have no `options`; only the real specialist choice; immutable in SQL and round-trip exact |
| `seats[]…units[].seatNumber` | string | seat units only (Owner Q3); a seat unit has its own record, so no other family carries this field (Owner R1 Q2) |
| `seats[]…units[].seatMapSelectionId` | string | seat units only, on the seat unit record (Owner R1 Q2); `pricingId` is never exposed |

## 4. سایر endpointها ــ دقیق
### 4.1 Confirm all
```http
POST /Service/v1/Ancillaries/Hold/9001/Confirmations
```
Body ندارد؛ `200` Receipt. فقط Held فعال قبل از deadline Confirm؛ Unitهای Released دست‌نخورده؛ تأیید مجدد Idempotent. **این عملیات صدور EMD نیست.**

### 4.2 Release all یا برخی Held
```http
DELETE /Service/v1/Ancillaries/Hold/9001?unitIds=9102&unitIds=9103
```
بدون Body؛ `unitIds:long[]?` positive/distinct؛ absent = کل Unitهای Held؛ وجود یکی invalid/Confirmed/foreign ⇒ **همه عملیات rollback**. Replay همان Released no-op و response current Receipt؛ ظرفیت از predicate ledger با تغییر وضعیت آزاد می‌شود. **بدون Route `Units/{unitId}`**.

### 4.3 Cancel all یا برخی Confirmed
```http
POST /Service/v1/Ancillaries/Hold/9001/Cancellations
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
| `seats[].passengerTypeCode` | `PassengerTypeCode` | existing enum (`ADT=1`, `CHD=27`, `Contracts/AeroTech.Messages/AirPrice/Enums/PassengerTypeCode.cs`); written by name, numbers accepted (§0) |
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

These DTOs stay unchanged. Gate 0 confirmed the CLR types and nullability above (`SeatHoldSelectionInput.cs:5-13`, `SeatHoldFlightInput.cs:3-13`, `SeatHoldFareInput.cs:5`) and that the validators apply the same rules. The SeatMap endpoint is not redesigned. Ancillary never calls FlightFlow/OccupiedSeats in Hold, Confirm or Cancel.

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

**خطای عددی موجود حفظ** (`16402` invalid, `16403` idempotency conflict, `16407` expired, `16417` shortage, `16413` cancel replay with another reason, `16418` Unit foreign, `16419` not reservable). برای Variantهای GATED خطای سازگار موجود؛ هیچ Error taxonomy تازه ساخته نشود. Poller قدیمی Expire را persisted می‌کند؛ countdown از `ExpiresAt` خود Reservation، نه Selection TTL.
