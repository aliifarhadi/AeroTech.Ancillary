# 02 — قرارداد دقیق API و DTOهای تخصصی (Pack v8)

**Authority:** Overview `01`, Dictionary `03`, Security `04`. **هیچ فیلدی که اینجا و در Dictionary تعریف نشده در API جدید اضافه نشود.** مثال‌های Id و Money fixture هستند؛ Rate و Currency واقعی باید از داده جاری استخراج شود.

## 1. Routes و Surface

| Operation | Route | Request | Response |
|---|---|---|---|
| Shop | `POST /Backoffice/v1/AncillaryOffers` | `{offerId:string}` | Backoffice Family lists |
| Shop | `POST /OtaPanel/v1/AncillaryOffers` | `{offerId:string}` | OtaPanel Family lists |
| Shop | `POST /Api/v1/AncillaryOffers` | `{offerId:string}` | API Family lists |
| Read Typed fields | `GET /{Surface}/v1/AncillaryOffers/{Family}/{reference}/Options` | Path only | Variant-specific form |
| Submit Typed | `POST /{Surface}/v1/AncillaryOffers/{Family}/{reference}/Options` | variant-specific selection | `{selectionId,expiresAtUtc}` |
| Resolve | `POST /Service/v1/Ancillaries/Selections/ResolveGrouped` | `{ids:string[]}` | Family-specific resolved arrays |

`{Family}`∈ `Baggages,Meals,Pets,AssistedTravel,AirportServices,PriorityServices,Connectivity,Upgrades`. `ExtraSeats` و `TravelInsurance` فعلاً هیچ Get/Post Options عملیاتی ندارند. نه `POST Quantity Options`، نه `Get Quantity Details`، نه `OrderCommercialContextReader`، نه Endpoint عمومی خرید جدید افزوده شود.

Security: سه Surface همان JWT/Policy موجود خود را اعمال کنند. Service Resolve فقط Service Auth موجود، نه public access. Field Bindingهای Offer issuer و Sales channel در شناسه؛ مسیر Options باید current requester را با binding اصلی بررسی کند.

## 2. Shopping request

```json
{"offerId":"OFFER-123"}
```

| Field | CLR / JSON | Req | Policy |
|---|---|---|---|
| `offerId` | `string` | Y | nonblank, source Offer lookup موجود؛ هیچ payload دیگر اجباری/پذیرفته نشود |

### 2.1 Shopping response مشترک از نظر business semantics

| Field | JSON / Req | Source |
|---|---|---|
| `offerId` | string / Y | validated Shopping source |
| `currencyId` | integer / Y | actual internal Catalog ID; EUR sample 47 per Owner |
| `baggages` | BaggageOfferItem[] / Y | Profile=Baggage |
| `meals` | MealOfferItem[] / Y | Profile=Meal |
| `pets` | PetOfferItem[] / Y | Profile=Pet |
| `assistedTravel` | AssistedTravelOfferItem[] / Y | Profile=AssistedTravel |
| `airportServices` | AirportServiceOfferItem[] / Y | Profile=AirportService |
| `priorityServices` | PriorityOfferItem[] / Y | Profile=Priority |
| `connectivity` | ConnectivityOfferItem[] / Y | Profile=Connectivity |
| `upgrades` | UpgradeOfferItem[] / Y | Profile=Upgrade |
| `extraSeats` | ExtraSeatOfferItem[] / Y | `[]` until A09 gated evidence |
| `travelInsurance` | InsuranceOfferItem[] / Y | `[]` until Domain authorization |

Surface semantics equal؛ `Api` DTO جدید Family lists است، نه legacy `included.other_services`. `Backoffice` نباید با append تمام diagnostics به آیتم مشتری Response را غیرقابل‌استفاده کند؛ گزارش‌های مدیریتی Legacy خارج Scope محفوظ.

### 2.2 هر ردیف در Family Array (Common base فقط این‌ها)

| JSON field | Type | Required | Source / condition |
|---|---|---|---|
| `variantCode` | string | Y | actual `AncillaryVariant` code |
| `title` | string | Y | CommercialName |
| `description` | string | N | candidate Description; omit absent |
| `selectionScope` | enum string | Y | `PerOrder`, `PerTraveller`, `PerTravellerPerPortion`, `PerTravellerPerFlight` **only supported mappings** |
| `unit` | UnitDto | Y | exact `UnitKey` from `CoveragePlanner` |
| `coverage` | CoverageDto | Y | trusted Shopping candidate, including Portion/Flight refs |
| `unitPrice` | MoneyDto | Y | actual `PricingRate` effective unit total in offer currency |
| `quantityPolicy` | QuantityPolicyDto | Y | rules from Provision, not UI guess |
| `selectionId` | string | conditional | Direct only, prefix `d_`; null/omitted for Qty/Typed |
| `quantityOptions` | QuantityChoiceDto[] | conditional | Qty-only; one for each integer allowed count, each signed d_ |
| `reference` | string | conditional | Typed only; prefix `r_` |
| `optionsEndpoint` | string | conditional | Typed only; exact route returned to caller |
| `details` | FamilySpecificDto | Y | §3 / `03` |

**Disjointness:** Direct ⇒ `selectionId` and no reference/quantityOptions. Qty-only ⇒ `quantityOptions` and no `selectionId/reference`. Typed ⇒ `reference+optionsEndpoint` and no `selectionId/quantityOptions`. JSON serializer **omits** inapplicable fields (no forests of nulls).

### 2.3 UnitDto (discriminated, NOT nullable soup)

| `selectionScope` | Exact `unit` keys | Constraint |
|---|---|---|
| `PerOrder` | `{ "scope": "Order" }` | once per rate + order source |
| `PerTraveller` | `{ "travellerRef":string }` | single canonical ref |
| `PerTravellerPerPortion` | `{ "travellerRef":string,"portionRef":string }` | one traveller and one Bound/Portion |
| `PerTravellerPerFlight` | `{ "travellerRef":string,"flightRef":string }` | `flightRef` canonical e.g `B1:F1` |

`PerFlight`, `PerPortion`, `PerJourney` independent are reserved concepts, not enabled by current `CoveragePlanner` (no fake output).

### 2.4 CoverageDto

| Field | Type | Meaning |
|---|---|---|
| `travellerRefs` | string[] required | true beneficiary references from Source |
| `portionRefs` | string[] required | true BoundRefs from Source |
| `flightRefs` | string[] required | canonical string flight identifiers Source provided |

All arrays unique/canonical. **No invented flight IDs for Portion at Resolve**. Coverage in Shopping may be rich; Resolve only returns the coverage actually recoverable from token/Snapshot, so Ordering must expand it from its own Offer.

### 2.5 Money/Quantity

| Field | Type | Meaning |
|---|---|---|
| `unitPrice.currencyId` | int | internal Currency ID; sample EUR=47 per Owner |
| `unitPrice.amount` | decimal | complete unit price from selected Rate; 0 valid |
| `quantityPolicy.selectable` | bool | whether user can choose quantity |
| `quantityPolicy.min` | int | `Provision.Quantity.MinQuantity` if selectable; 1 for fixed Direct |
| `quantityPolicy.max` | int | `Provision.Quantity.MaxQuantity` if selectable; 1 for fixed Direct |
| `quantityPolicy.unit` | enum | `Each`, `Piece`, `Kilogram` supported Domain; enumerated Qty only integral discrete products |
| `quantityOptions[].quantity` | int | exact count; no zero ID |
| `quantityOptions[].selectionId` | string | HMAC direct `d_` bound to count |
| `quantityOptions[].totalAmount` | decimal | quantity×price when billing per discrete unit; no incomplete fees/FX |

For taxes and fee lines, Shopping simple response uses final complete unit total; Resolve returns breakdown as needed for Ordering. Pricing per Kilogram with continuous metric without determined price is **not enumerable/issuable** under D1.

## 3. تخصصی بودن `details` در Shopping

**عدم ساخت یک `AncillaryServiceDto` با تمام فیلدها.** Family mappers DTO خود را دارند. JSON fields and provenance:

| Family DTO | Field:Type | Source/when |
|---|---|---|
| `BaggageDetails` | `chargeKind:string`, `allowanceConcept?:string`, `packageWeightKg?:decimal`, `maxKgPerPiece?:decimal`, `weightFromExclusiveKg?:decimal`, `weightToInclusiveKg?:decimal`, `maxSizeCm?:DimensionsCm`, `maxLinearSumCm?:decimal`, `equipmentKind?:string`, `chargeCombination?:string` | BaggageSpecification; only relevant Variant fields |
| `MealDetails` | `mealKind:string`, `mealCode?:string`, `menuItemRef?:string`, `dietaryCode?:string`, `cateringLeadTimeMinutes:int`, `exclusiveMealFamilyCode?:string` | MealSpecification |
| `PetDetails` | `transportMode:string`, `allowedAnimalTypes:[{animalType,otherCode?}]`, `maxCombinedWeightKg:decimal`, `carrierDimensionsMaxCm:DimensionsCm`, `minAnimalAgeWeeks?:int`, `requiredDocumentCodes:string[]`, `allowedHoldAnimalSizeBrackets?:[{code,weightFromExclusiveKg,weightToInclusiveKg}]` | PetSpecification |
| `AssistedTravelDetails` | `assistanceKind:string`, variant-specific `wheelchair?`, `disabilityAssistance?`, `medicalEquipment?`, `bassinet?`, `unaccompaniedMinor?` | only matching subtype object, not all nullable siblings |
| `AirportServiceDetails` | `kind:string`, `airportId:int`, `terminalRef?:string`, `facilityId?:long`, `direction:string`, `serviceWindowStart?:string`, `serviceWindowEnd?:string`, `ianaTimeZone?:string`, `maxGuestsPerPrimary?:int`, `requiresSpecificAppointment:bool`, `includedComponentCodes?:string[]` | AirportServiceSpecification |
| `PriorityDetails` | `kind:string`, `priorityZoneCode?:string`, `priorityGroupCode?:string`, `fareBenefitRef?:string` | PrioritySpecification |
| `ConnectivityDetails` | `planKind:string`, `durationMinutes?:int`, `includedDataMb?:int`, `maxDevices?:int`, `deliveryStage:string` | ConnectivitySpecification |
| `UpgradeDetails` | `fromCabinId:int`, `toCabinId:int`, `allowedUpgradeKind:string`, `requiresTicketExchange:bool` | UpgradeSpecification; fixed-price/no Quote only |

`DimensionsCm` = `{lengthCm:decimal,widthCm:decimal,heightCm:decimal}` positive; values in existing Domain, not guessed from UI. `A07/A08` not mapped; `A09/Insurance` arrays empty.

## 4. Typed Options only

`GET /{Surface}/v1/AncillaryOffers/{Family}/{reference}/Options` requires valid signed `r_`. Response has **only** `{variantCode:string, fields:{...}}`; no arbitrary pricing/rate metadata in form. Each field descriptor: `control:select|text|number|checkbox|date-time`, `required:bool`, `choices:[{value,label}]` **only select**, `unit?:string`, `min?:decimal`, `max?:decimal`, `maxLength?:int` only when real Domain/source exists. Distinguish `unknown choices` ⇒ no issuance (not empty choices invented).

Examples:

**A15 GET**:
```json
{"variantCode":"A15","fields":{"assistanceSsrCode":{"control":"select","required":true,"choices":[{"value":"WCHR","label":"Ramp"},{"value":"WCHS","label":"Steps"}]}}}
```
The SSR codes actually come from `Wheelchair.AllowedSsrCodes`; example values not guaranteed for any airline.

**A15 POST**:
```json
{"assistanceSsrCode":"WCHS"}
```
**A13 POST**:
```json
{"animalType":"Dog","combinedWeightKg":7,"carrierDimensions":{"lengthCm":45,"widthCm":30,"heightCm":25}}
```

**A17 POST** (only after approved at-rest protection and nonapproval product):
```json
{"equipmentCode":"AOXY","oxygenUnits":2,"oxygenFlowRateLitersPerMinute":2.0,"frequentTravellerMedicalCardNumber":"FREMEC-EXAMPLE"}
```

**POST Response**:
```json
{"selectionId":"s_<opaque-key>","expiresAtUtc":"2026-10-23T19:00:00Z"}
```
No full echo of sensitive inputs; no Quantity-only POST. Typed with selectable Quantity adds `{quantity:int}` to its **own** type DTO, snapshot includes count.

**POST validation:** HMAC/TTL; current actor's SalesBinding; active Definition/Provision/PricingRate; exact Variant; required/conditional fields; allowed select values; Qty limits; any Eligibility dependent on source must be proven by signed Shopping evidence or existing trusted evaluation; missing evidence => refuse snapshot (never guess). `GET` may expose only approved option choices. The `reference` path is the only binding argument; no OfferId/OrderId/POS/Country/rateId in POST body.

## 5. ResolveGrouped

Request:
```json
{"ids":["d_signed_1","s_snapshot_2"]}
```
`ids:string[]` required; 1..50, distinct, nonempty, max size per token from `04`, no other body keys. Service auth required. Direct: HMAC and expiry. Typed: DB key and expiry. Do not call Offer/Ordering/AirPrice/FlightFlow. DB read of PricingRate whose parent Pricing.Status=Active, plus Provision.Status=Active and Definition.Status=Active is allowed. Retired/Suspended parents must be rejected even for signed unexpired tokens. No business re-evaluation; checking current Active status is structural validity and mandatory.

Response has **exact family arrays** matching §2.1, no extra metadata envelopes. Common **Resolved record** with `details` specialized by family:

| Field | Type | Required | Ownership |
|---|---|---|---|
| `selectionId` | string | Y | echo ID |
| `variantCode` | string | Y | real Definition |
| `selectionScope` | enum | Y | signed/persisted Unit |
| `unit` | UnitDto | Y | signed/persisted |
| `coverage` | CoverageDto | Y | persisted exact; for signed ID use Unit refs; `flightRefs:[]` if Portion flight set unknown |
| `sourceBinding` | `{offerHash:string,salesHash:string}` | Y | issuer binding, compare at Ordering; **not** raw POS/Country |
| `definition` | DefinitionDto | Y | §5.1 |
| `provision` | ProvisionDto | Y | §5.2 |
| `pricing` | PricingDto | Y | §5.3 |
| `details` | FamilyDto | Y | §3 + `03` |
| `selected` | type-specific object | N | only Typed, from Snapshot; Direct/Qty omit |

### 5.1 DefinitionDto

| Field:Type | Source |
|---|---|
| `definitionVersionId:long` | `AncillaryServiceDefinition.Id` |
| `serviceDefinitionRef:string` | `AncillaryServiceDefinition.ServiceDefinitionRef` |
| `variantCode:string` | real Definition |
| `serviceTypeCode:string` | Definition |
| `serviceSubCode:string` | Definition |
| `rfic?:string,rfisc?:string` | Definition document metadata when present |
| `ssrCode?:string` | Definition booking metadata when present |

### 5.2 ProvisionDto

| Field:Type | Source |
|---|---|
| `provisionId:long` | Provision.Id |
| `coverageScope:Sector|Portion|Journey|Order` | Provision.CoverageScope |
| `purchaseStage:PreOrder|Both|...` | Provision.PurchaseStage |
| `bookingMethod:string` | Definition.Booking.Method |
| `bookingRequired:bool` | Definition.Booking |
| `confirmationRequirement:string` | Definition.Booking |
| `fulfillmentProviderKey?:string` | Provenance from existing Provision.Fulfillment (not derived from Product title) |
| `documentRouting:NoAncillaryDocument|Emd|TicketOrExchange` | Definition.DocumentRouting |
| `documentType:None|EmdAssociated|EmdStandalone` | Definition.Document.Type |
| `documentRequired:bool` | Definition.Document; existing policy |
| `requiresCheckAtReserve:bool` | inventory/fulfillment policy; **informational** |

### 5.3 PricingDto

| Field:Type | Source |
|---|---|
| `pricingId:long` | Pricing Aggregate Id, NOT Rate Id |
| `pricingRateId:long` | actual PricingRate.Id >0 even Free |
| `pricingVersion:int` | AncillaryPricing.Version |
| `currencyId:int` | Rate.CurrencyId; sample EUR 47 (Owner) |
| `quantity:int` | signed direct qty / snapshot qty |
| `unitAmount:decimal` | actual complete per-unit amount |
| `totalAmount:decimal` | exact qty multiplication when allowed D1 |
| `pricingUnit:string` | Domain enum: PerPassenger/PerItem/PerPiece/PerKilogram etc |
| `baseAmount:decimal` | actual rate BaseAmount |
| `taxLines:PriceLineDto[]` | category Tax from rate components, may be empty |
| `feeLines:PriceLineDto[]` | category Fee from rate components, may be empty; no unapplied fee lines published |

`PriceLineDto = {code?:string,amount:decimal,currencyId:int,countryId?:int,stationAirportId?:int,taxTreatment?:string,feeApplicationUnit?:string}`; include only actual domain fields; countryId in tax evidence IS NOT Country input to Resolve. No unverified markup, conversion or supplier quote.

## 6. Error contract

| HTTP | Error code | Applies | Meaning |
|---|---|---|---|
| 400 | `INVALID_REQUEST` | all | malformed JSON/IDs, unknown body field, incorrect unit shape |
| 401/403 | existing auth policy | all | caller not authenticated / no permission |
| 403 | `SELECTION_BINDING_MISMATCH` | Options | caller sales identity differs from Shopping reference |
| 404 | `SELECTION_NOT_FOUND` | Options/Resolve | key/rate/reference not found |
| 409 | `SELECTION_NOT_SELLABLE` | Shop/Options | parent Pricing / Provision / Definition not Active, Quote/Approval, incomplete price, no eligible product |
| 410 | `SELECTION_EXPIRED` | Options/Resolve | past signed / snapshot expiry |
| 422 | `INVALID_SELECTION_OPTIONS` | POST Options | type-specific required/mismatch/outside authorized choices/quantity |
| 422 | `INVALID_SELECTION_SIGNATURE` | Options/Resolve | invalid MAC, tampered payload, bad purpose (avoid disclosing details) |

Existing centralized error envelope/exception patterns take precedence; **do not introduce a new global error framework**. All batches in Resolve fail as a whole on invalid ID (no partial success unless separate Owner decision).

## 7. Contract rules preventing over-engineering

- No new Aggregate, mediator of mediators, generic selection form DSL, second shopping engine, Order reader, IOrderCommercialContextReader.
- No source DTO with 30 nullable fields in public API.
- Fixed options come from Definition; do not call Options needlessly.
- Quantity-only has IDs directly in Shopping; never insert Quantity Snapshot.
- Reuse existing three Surface controllers/DI/Engine conventions. Breaking changes for this planned API are intentional, not new version negotiation.
- Do not modify frozen SeatMap endpoints or DTOs. Do not invent Real Insurance product or ExtraSeat inventory integration.
