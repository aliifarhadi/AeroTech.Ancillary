# 04 — FINAL AirAvail Retailing & ACL v9.1

**Status:** implementation authority.

## 1. Boundary

AirAvail is the **only ancillary shopping/evaluation/pricing-composition engine**.

```text
trusted commercial context
+ traveller
+ flight/fare facts
+ published Ancillary Supplier/ServiceDefinition/Provision read model
+ authoritative availability source when required
+ AirAvail FX/monetary normalization
= ancillary offers for the requested Surface
```

Ancillary does not expose OTA/IBE customer-shopping endpoints and does not implement a duplicate evaluator.

---

# 2. Two sales paths — FINAL

## 2.1 Pre-order

```text
Surface
-> AirAvail AncillaryOffers(OfferId)
-> AirAvail reconstructs trusted Offer context
-> single evaluator
-> AirAvail response for that Surface
-> AirAvail Details
```

## 2.2 Post-order

```text
Surface
-> Ordering(OrderId)
-> Ordering builds trusted Order ancillary-shopping context
-> Ordering calls AirAvail Service/v1/AncillaryOffers with the full context
-> AirAvail single evaluator
-> AirAvail returns result to Ordering
-> Ordering returns the Surface response
```

Details follows the same one-way direction:

```text
Surface -> Ordering -> AirAvail Details -> Ordering -> Surface
```

**Prohibited:**

```text
AirAvail -> Ordering GET context
```

There is no runtime call cycle between AirAvail and Ordering.

---

# 3. Pre-order AirAvail Surface routes

Use the current surface/controller/auth conventions.

Required equivalent operations:

```text
Api/v1/AncillaryOffers
Api/v1/AncillaryOffers/Details

OtaPanel/v1/AncillaryOffers
OtaPanel/v1/AncillaryOffers/Details

Ibe/v1/AncillaryOffers
Ibe/v1/AncillaryOffers/Details

Backoffice/v1/AncillaryOffers
Backoffice/v1/AncillaryOffers/Details
```

Normal pre-order request:

```json
{
  "offerId": "opaque-priced-flight-offer-id"
}
```

Do not accept client-supplied PTC/fare/flight/POS/customer facts that AirAvail can reconstruct from its own trusted Offer state.

---

# 4. Pre-order trusted Offer context

AirAvail J2/equivalent immutable state must preserve all facts required by v9.1 evaluation:

```text
Travellers[]
  TravellerRef
  PassengerTypeCode

Flights[]
  FlightRef
  FlightId
  FlightCapacityId
  FlightNumber
  OriginAirportId
  DestinationAirportId
  DepartureDateTime
  ArrivalDateTime
  MarketingAirlineId
  OperatingAirlineId
  AircraftId
  CabinClassId?
  RbdId?

FareComponents[]
  FlightIds[]
  AirFareId
  AirFareType
  FareFamilyId?
  FareBasis?
  CabinClassId?
  RbdId?

Commercial
  Channel
  CustomerId?
  CustomerType?
  CurrencyId
  SalesDate
  AllowedPointOfSaleIds[]
```

The current state/codec that loses `InfantsWithSeat` or `AllowedPointOfSaleIds` must be corrected where those source facts are required to construct the correct PTC/POS context.

`FareFamilyId` is mandatory whenever FareFamily-qualified ancillary rules are evaluated.

---

# 5. Post-order Ordering -> AirAvail Service contract

Existing AirAvail Service route remains the internal shopping endpoint:

```http
POST Service/v1/AncillaryOffers
```

For post-order shopping, Ordering sends the complete authoritative context; AirAvail does not fetch it back.

Required request semantics:

```text
Surface
OrderId
CommercialVersion
AsOf
SellingCurrencyId
ResolvedPointOfSaleIds[]
CustomerId?
CustomerType?

Travellers[]
  TravellerId
  TravellerRef
  PassengerTypeCode

Flights[]
  FlightRef
  SegmentId
  FlightId
  FlightCapacityId?
  FlightNumber
  OriginAirportId
  DestinationAirportId
  DepartureDateTime
  ArrivalDateTime
  MarketingAirlineId
  OperatingAirlineId
  AircraftId?
  CabinClassId?
  RbdId?

FareComponents[]
  FlightIds[]
  AirFareId?
  AirFareType?
  FareFamilyId?
  FareBasis?
  CabinClassId?
  RbdId?

ExistingAncillaries[]
  OrderServiceId
  ServiceDefinitionId
  SupplierId
  TravellerId?
  CoveredFlightIds[]
  Quantity
```

The produced ancillary Offer is bound to:

```text
OrderId + CommercialVersion
```

Ordering rejects stale version before commit and sends the current version to AirAvail for Details validation.

---

# 6. Backoffice simulation uses the same evaluator

The following route is also the **only live simulation entry point**:

```http
POST Backoffice/v1/AncillaryOffers
```

For a normal Backoffice sales flow, request may contain `OfferId`.

For an authorized analyst simulation, Backoffice may send an explicit trusted context matching the fields in Sections 4/5.

AirAvail runs the same `AncillaryOfferEvaluator` in both cases.

Backoffice diagnostic output may include:

```text
EvaluatedProvisionIds[]
Rejected[] { ProvisionId, Reasons[] }
WinnerProvisionId?
WinnerSequence?
```

No equivalent rule evaluator exists in Ancillary.

---

# 7. Published catalog consumption

Preserve the current integration direction:

```text
IAncillaryCatalogRepository
-> Ancillary ReadModel snapshot
-> AncillaryCatalogCache
-> AncillaryOfferEvaluator
-> offer composer/mappers
```

Target published dataset includes:

```text
Suppliers
AncillaryServiceDefinitions
AncillaryProvisions
ProvisionPriceLines
BaggageApplications
SeatApplications
Ancillary-owned stock projection where required
```

Do not introduce a second remote rules protocol merely because the domain model changed.

### Supplier rule

For new shopping:

```text
Supplier Active
AND ServiceDefinition Active/effective
AND matching Provision
```

Several Supplier-specific definitions may independently match and become distinct OfferItems.

Supplier retirement removes new offers; it does not affect already accepted Order history.

---

# 8. Single evaluator semantics

`AncillaryOfferEvaluator` evaluates the v9.1 typed fields only:

```text
PTC
POS
Customer / CustomerType
route / via
travel date/day/time
marketing/operating carrier
FlightNumber / exact FlightId
AircraftId
AirFareId / AirFareType
FareFamilyId
FareBasis
CabinClassId / RbdId
AdvancePurchase where configured
quantity
BaggageApplication
SeatApplication
Sequence precedence
Paid / Free / NotAvailable
```

It does **not** implement v9.1 Gap-Register-only qualifiers such as age, passenger occurrence, FF status, ticket designator or account code.

For one ServiceDefinition:

```text
matching Provisions ordered by Sequence ascending
first match wins
NotAvailable winner => no purchasable OfferItem
```

There is no shared rule DSL and no parity requirement with a second engine because the second engine does not exist.

---

# 9. Canonical generic ancillary offer result

Internal canonical result:

```text
AncillaryOffer
  OfferId
  ContextKind                 Offer | Order | BackofficeSimulation
  ContextRef
  ContextVersion?
  OwnerAirlineId
  OfferExpirationDateTime
  CurrencyId                  selling currency
  OfferItems[]
  ServiceDefinitionList[]
```

### OfferItem

```text
OfferItemId                   opaque external/surface ID
ServiceDefinitionDisplayRef
Eligibility
  PaxRefIds[]
  JourneyRefId?
  SegmentRefIds[]
QuantityRule
  MinimumQty
  MaximumQty
  Unit
Availability
  Status
  RemainingQuantity?
UnitPrice
  BaseAmount
  TaxSummary
  TotalAmount
RequiresSeatMap
```

```text
AvailabilityStatus
  Available
  Unavailable
  OnRequest
```

Rules:

- public/surface OfferItem IDs are opaque;
- internal DB IDs do not need to be exposed to customer surfaces;
- the internal offer state behind the opaque IDs stores the canonical long IDs required by Ordering Details;
- multiple suppliers may produce multiple offer items for the same broad service/sub-code.

---

# 10. ServiceDefinition display data

Surface display definition may include:

```text
ServiceDefinitionDisplayRef
ServiceDefinitionRef
Name
Description?
SupplierName?
ServiceType
Classification
  ServiceTypeCode
  ServiceSubCode
  GroupCode
  SubGroupCode?
  Description1Code?
  Description2Code?
DocumentType?             // only where the Surface requires it
Unit
BaggageDisplay?
TermsSummary
```

Do not expose:

```text
ProvisionId
SupplierId DB key
FulfillmentProviderKey
StockPoolId
Sequence
provider credentials
```

to ordinary customer surfaces.

---

# 11. Details — customer/surface view

```http
POST {surface}/v1/AncillaryOffers/Details
```

Pre-order direct AirAvail request:

```json
{
  "offerId": "opaque-ancillary-offer-id",
  "selections": [
    { "offerItemId": "opaque-item-id", "quantity": 1 }
  ]
}
```

Purpose:

- verify offer expiry;
- verify Offer-context integrity;
- calculate selected quantity;
- return public terms/current selected selling price detail.

Post-order customer/surface does not call this AirAvail route directly; Ordering proxies/orchestrates it as defined in `05`.

---

# 12. Details — Service-to-Service accepted snapshot

Internal Details item returned to Ordering MUST contain canonical IDs:

```text
OfferItemId

ServiceDefinitionId      long
ProvisionId              long
SupplierId               long

ServiceDefinitionRef     // snapshot/display only
ServiceDefinitionVersion // snapshot/display only
SupplierName             // snapshot/display only when required
ServiceSubCode
ServiceTypeCode
GroupCode
SubGroupCode?
CommercialName

TravellerId?
CoverageScope
CoveredFlightIds[]
QuantityUnit
Quantity

FiledCurrencyId
FiledPriceLines[]
FiledUnitTotal
FiledTotal

SellingCurrencyId
SellingPriceLines[]
SellingUnitTotal
SellingTotal
RatesOfExchange[]

Document
Booking
Settlement
FulfillmentProviderKey
```

There is no `ReservationMode`.

The `FulfillmentProviderKey` in this Details contract is the **Ordering fulfillment provider key** (for example `Ancillary` or `FlightFlow`). It is not `Supplier.FulfillmentProviderKey`. The Supplier-internal external-adapter key must not leak through AirAvail or public surfaces.

`ServiceDefinitionRef + Version` is not used to resolve the entity during fulfillment.

---

# 13. Pricing and FX

Final monetary flow:

```text
Ancillary filed PriceLines (decimal(18,2) persisted)
-> AirAvail reads filed currency
-> AirAvail existing FX / monetary normalization
-> selling price for Surface
-> Details contains both filed and selling evidence + ROE
-> Ordering accepts/snapshots the sale
```

Ancillary never calls the currency converter for fulfillment.

Ordering never asks Ancillary Hold to prove that selling amount equals filed amount after FX.

ROE precision follows existing AirAvail/Ordering rate representation and is not reduced to money precision.

---

# 14. Flydubai benchmark mapping

Verified public OpenAPI behavior:

```text
POST /pricing/services
```

returns ancillary service quotes attached to segment/leg with fields including:

```text
code
categoryID
currency
amount
ruleID
maxCountFlightLevel
maxCountServiceLevel
quantity
```

and:

```text
POST /pricing/seats
```

returns a separate priced seat map including:

```text
seat
serviceCode
amount
currency
assigned
ruleID
isBlocked
isPreBlocked
```

Selected service commitment in `SummaryPNR` uses numeric `ServiceID` plus passenger/flight/amount/currency facts.

AeroTech intentionally follows the same high-level separation:

```text
shopping/pricing first
opaque/public offer selection
canonical internal IDs at trusted commitment boundary
seats separated from generic ancillary list
```

AeroTech does not copy flydubai payload naming or its session architecture.

---

# 15. Seat shopping is separate

Use separate seat offer operations:

```text
POST {surface}/v1/SeatOffers            // pre-order AirAvail surfaces
POST {surface}/v1/SeatOffers/Details
```

For post-order, the Surface enters through Ordering and Ordering calls AirAvail with the Order context.

Runtime seat composition:

```text
FlightFlow live seat map/status
+ Ancillary paid-seat Provisions
+ AirAvail evaluator/price/FX
= priced seat offer
```

AirInfo static seat map is authoring/reference only.

---

# 16. Availability authority

AirAvail never fabricates availability.

Use the real authority by service kind:

```text
physical seat -> FlightFlow live seat state
Ancillary-owned quota -> Ancillary stock projection/service evidence
unlimited service -> no RemainingQuantity required
supplier availability with no implemented live source -> OnRequest, never fake Available count
```

A successful shop is not a reservation. Availability may change before provider hold.

---

# 17. Post-order stale handling

The post-order offer is bound to Order `CommercialVersion`.

Ordering must reject/re-shop when:

```text
current Order CommercialVersion != offered ContextVersion
```

AirAvail Details also checks the version supplied by Ordering against the offer state.

No AirAvail callback to Ordering is used for this check.

---

# 18. Deterministic errors

Required semantics, expressed using existing project error conventions:

```text
OFFER_EXPIRED
OFFER_CONTEXT_INVALID
ORDER_VERSION_STALE
ANCILLARY_OFFER_EXPIRED
ANCILLARY_ITEM_NOT_FOUND
ANCILLARY_ITEM_NO_LONGER_ELIGIBLE
ANCILLARY_AVAILABILITY_CHANGED
ANCILLARY_QUANTITY_INVALID
ANCILLARY_CATALOG_UNAVAILABLE
SEAT_NO_LONGER_AVAILABLE
```

Do not introduce a new error framework solely for these names.
