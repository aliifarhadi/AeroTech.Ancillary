# 03 — FINAL Backoffice Authoring Contracts v9.1

**Status:** implementation authority.

This document freezes Backoffice behavior and HTTP contracts only. It does not redefine project layering, folder layout, mediator conventions or persistence architecture; those are already established by source.

## 1. Operational goal

An airline ancillary/pricing analyst must be able to:

1. manage Suppliers;
2. define one or more supplier-specific ServiceDefinitions for the same broad ancillary/service sub-code;
3. author route/flight/date/aircraft/fare/PTC/POS applicability;
4. author filed prices;
5. select exact aircraft seats or PADIS-compatible seat characteristics for paid-seat pricing;
6. validate draft rows and overlaps before publication;
7. publish/revise without mutating historical sold rows;
8. simulate **published** behavior using the same AirAvail evaluator used by production shopping.

There is no separate Ancillary simulation engine.

---

# 2. Supplier Backoffice API

Base route:

```http
Backoffice/v1/Suppliers
```

## 2.1 List

```http
GET Backoffice/v1/Suppliers/Paginated
```

Filters:

```text
OwnerAirlineId?
FulfillmentKind?
Status?
Search?
PageNumber
PageSize
SortBy?
```

## 2.2 Detail

```http
GET Backoffice/v1/Suppliers/{supplierId}
```

Response includes:

```text
Id
OwnerAirlineId
Name
FulfillmentKind
FulfillmentProviderKey?
Status
CreatedAt
RetiredAt?
```

## 2.3 Register

```http
POST Backoffice/v1/Suppliers
```

Request:

```json
{
  "ownerAirlineId": 10,
  "name": "Airport Lounge Partner",
  "fulfillmentKind": "External",
  "fulfillmentProviderKey": "LoungePartnerA"
}
```

Response contains assigned `SupplierId`.

## 2.4 Rename

```http
PUT Backoffice/v1/Suppliers/{supplierId}
```

Request:

```json
{
  "name": "Airport Lounge Partner GmbH"
}
```

`FulfillmentKind` and `FulfillmentProviderKey` are immutable after Supplier registration. Endpoint/credential changes happen behind the stable provider key in integration configuration; changing fulfillment semantics requires a new Supplier identity rather than mutating historical routing.

## 2.5 Retire / reactivate

```http
POST Backoffice/v1/Suppliers/{supplierId}/Retire
POST Backoffice/v1/Suppliers/{supplierId}/Reactivate
```

Rules:

- `External` requires `FulfillmentProviderKey`; `Local` requires it to be null and must not depend on a hard-coded Supplier ID;
- provider key must resolve to a registered Ancillary supplier adapter before production activation;
- retired Supplier cannot be used for activation of a new ServiceDefinition;
- existing sold Orders referencing its ServiceDefinition IDs remain serviceable/cancellable;
- do not delete Supplier rows that have ever been referenced.

---

# 3. Industry ServiceSubCode reference API

Industry sub-code semantics are lookup data, not a mutable airline aggregate.

```http
GET Backoffice/v1/ReferenceData/IndustryServiceSubCodes
```

Filters:

```text
search?
code?
serviceTypeCode?
groupCode?
limit
```

Response item:

```text
Code
ServiceTypeCode
Rfic?
GroupCode
SubGroupCode?
Description1Code?
Description2Code?
CommercialName
DocumentType?  // only when authoritative reference provides it
```

Rules:

- no POST/PUT/DELETE for Industry entries;
- the production data source must be the organization-approved ATPCO/IATA industry reference dataset;
- donor hard-coded examples are not treated as the complete table;
- unknown Industry code cannot be published by guessing its meaning.

Carrier-defined sub-codes are authored as part of ServiceDefinition and therefore do not require a separate mutable ServiceSubCode master.

---

# 4. ServiceDefinition Backoffice API

Base:

```http
Backoffice/v1/AncillaryServiceDefinitions
```

## 4.1 List

```http
GET Backoffice/v1/AncillaryServiceDefinitions/Paginated
```

Filters:

```text
OwnerAirlineId?
SupplierId?
ServiceDefinitionRef?
ServiceSubCode?
ServiceTypeCode?
GroupCode?
Status?
Search?
PageNumber
PageSize
SortBy?
```

## 4.2 Detail

```http
GET Backoffice/v1/AncillaryServiceDefinitions/{serviceDefinitionId}
```

## 4.3 Define — Industry sub-code example

```http
POST Backoffice/v1/AncillaryServiceDefinitions
```

```json
{
  "ownerAirlineId": 10,
  "supplierId": 2001,
  "serviceDefinitionRef": "EXTRA_BAG_20KG_FZ",
  "serviceSubCode": "0CC",
  "subCodeSource": "Industry",
  "commercialName": "Extra baggage 20 kg",
  "description": "Prepaid extra checked baggage",
  "document": {
    "type": "EmdAssociated",
    "rfic": "C",
    "rfisc": "0CC"
  },
  "booking": {
    "method": "Ssr",
    "ssrCode": "XBAG",
    "ssimCode": null
  },
  "salesEffectiveFrom": "2026-11-01",
  "salesDiscontinueOn": null
}
```

For `SubCodeSource = Industry`, the request must **not** redefine `ServiceTypeCode/Group/SubGroup/Description*` semantics. The service resolves those values from `IndustryServiceSubCodeReference` and rejects contradictions.

## 4.4 Define — CarrierDefined example

For a CarrierDefined sub-code, request additionally supplies the required classification semantics:

```text
ServiceTypeCode
GroupCode
SubGroupCode?
Description1Code?
Description2Code?
```

Carrier-defined semantics are validated once and frozen on publication.

## 4.5 Draft change

```http
PUT Backoffice/v1/AncillaryServiceDefinitions/{serviceDefinitionId}
```

Allowed only while Draft.

`SupplierId` may be changed only while Draft.

## 4.6 Lifecycle

```http
POST Backoffice/v1/AncillaryServiceDefinitions/{id}/Activate
POST Backoffice/v1/AncillaryServiceDefinitions/{id}/Suspend
POST Backoffice/v1/AncillaryServiceDefinitions/{id}/Reactivate
POST Backoffice/v1/AncillaryServiceDefinitions/{id}/Retire
POST Backoffice/v1/AncillaryServiceDefinitions/{id}/Revise
```

`Revise` creates a **new ServiceDefinition ID** with incremented Version. It never reuses the old ID.

---

# 5. Provision Backoffice API

Base:

```http
Backoffice/v1/AncillaryProvisions
```

## 5.1 List

```http
GET Backoffice/v1/AncillaryProvisions/Paginated
```

Filters:

```text
ServiceDefinitionId
SupplierId?             // derived join filter for analyst convenience
Status?
Sequence?
SalesDate?
TravelDate?
PassengerTypeCode?
FlightId?
AircraftId?
FareFamilyId?
PointOfSaleId?
PageNumber
PageSize
SortBy?
```

## 5.2 Detail

```http
GET Backoffice/v1/AncillaryProvisions/{provisionId}
```

## 5.3 Define one Draft Provision

```http
POST Backoffice/v1/AncillaryProvisions
```

The request is the typed authoring representation of `AncillaryProvision`; no JSON rule DSL/EAV expression tree is accepted.

Example — first extra bag for BASIC fare family on DXB -> IST:

```json
{
  "serviceDefinitionId": 1001,
  "sequence": 100,
  "salesEffectiveFrom": "2026-10-01T00:00:00Z",
  "salesDiscontinueAt": "2026-12-31T23:59:59Z",
  "coverageScope": "Sector",
  "passenger": {
    "passengerTypeCodes": ["ADT"]
  },
  "sales": {
    "pointOfSaleIds": [101, 102]
  },
  "travel": {
    "routePairs": [
      {
        "originAirportId": 1,
        "destinationAirportId": 2,
        "direction": "Directional"
      }
    ],
    "travelFrom": "2026-11-01",
    "travelTo": "2026-12-20"
  },
  "fare": {
    "fareFamilyIds": [501]
  },
  "quantity": {
    "unit": "Piece",
    "minQuantity": 1,
    "maxQuantity": 3
  },
  "application": {
    "type": "Baggage",
    "firstExcessPiece": 1,
    "lastExcessPiece": 1,
    "weight": 20,
    "weightUnit": "Kilogram",
    "purchaseApplication": "Prepaid"
  },
  "outcome": {
    "disposition": "Paid",
    "documentRequired": true,
    "bookingRequired": true
  },
  "fee": {
    "applicationUnit": "Item",
    "currencyId": 978,
    "priceLines": [
      {
        "category": "Ancillary",
        "code": null,
        "name": "Extra baggage",
        "unitAmount": 35.00
      }
    ]
  },
  "settlement": {
    "reissueRefund": "Refundable",
    "formOfRefund": "OriginalPayment",
    "commissionable": false,
    "interlineSettlement": false
  },
  "availability": {
    "mustCheckAvailability": false
  },
  "fulfillment": {
    "fulfillmentProviderKey": "Ancillary"
  }
}
```

Money request values are CLR decimals; persistence uses the platform money convention `decimal(18,2)`.

## 5.4 Change Draft

```http
PUT Backoffice/v1/AncillaryProvisions/{provisionId}
```

Only Draft.

## 5.5 Lifecycle

```http
POST Backoffice/v1/AncillaryProvisions/{id}/Activate
POST Backoffice/v1/AncillaryProvisions/{id}/Suspend
POST Backoffice/v1/AncillaryProvisions/{id}/Reactivate
POST Backoffice/v1/AncillaryProvisions/{id}/Retire
POST Backoffice/v1/AncillaryProvisions/{id}/Revise
```

`Revise` creates a new Provision ID; it does not mutate the published row.

---

# 6. Pricing Matrix — bulk authoring only

Pricing Matrix is a Backoffice convenience contract, not a new domain model.

Each matrix row maps to exactly one Draft `AncillaryProvision`.

## 6.1 Preview

```http
POST Backoffice/v1/AncillaryProvisions/Preview
```

Request:

```text
ServiceDefinitionId
Rows[]
```

Response:

```text
NormalizedRows[]
Errors[]
Warnings[]
OverlapFindings[]
ShadowedRows[]
ReferenceValidation[]
```

Preview performs **structural/static authoring validation only**.

Hard errors include:

```text
unknown ServiceDefinitionId
retired/invalid Supplier for a new Draft intended for activation
invalid AirInfo/AirPrice reference
invalid Industry sub-code semantics
invalid seat number for selected AircraftIds
Paid with no fee
invalid currency
invalid date range
invalid quantity
invalid EMD/RFIC/RFISC relation
same active sequence conflict
```

Warnings include deterministic overlapping rows and broad/shadowed provisions.

Preview does not execute a second live eligibility evaluator.

## 6.2 Bulk Draft creation

```http
POST Backoffice/v1/AncillaryProvisions/Bulk
```

All-or-none creation after the same static validation.

## 6.3 Batch publish

```http
POST Backoffice/v1/AncillaryProvisions/Publish
```

```json
{
  "provisionIds": [100, 101, 102]
}
```

Behavior:

- revalidate references/state;
- perform sequence replacement atomically;
- publish all or none;
- no `ProvisionBatch` aggregate.

---

# 7. Live Backoffice simulation — AIRAVAIL ONLY

Removed v8 endpoint:

```text
POST Backoffice/v1/AncillaryProvisions/Simulate
```

It must **not** exist.

Final simulation endpoint:

```http
POST Backoffice/v1/AncillaryOffers
```

owned by **AirAvail**.

It accepts a trusted Backoffice evaluation context and uses the exact same `AncillaryOfferEvaluator` as production shopping.

Minimum context:

```text
AsOf
SellingCurrencyId
ResolvedPointOfSaleIds[]
CustomerId?
CustomerType?
Travellers[]
Flights[]
FareComponents[]
ExistingAncillaries[]
```

Backoffice-only diagnostic response may additionally expose:

```text
EvaluatedProvisionIds[]
Rejected[] { ProvisionId, Reasons[] }
WinnerProvisionId?
WinnerSequence?
```

These IDs must not leak to normal OTA/IBE responses.

Simulation evaluates **published active catalog state**. Draft-row structural checking belongs to Ancillary Preview. v9.1 intentionally does not build a draft-overlay evaluator.

---

# 8. ReferenceData lookup endpoints

Reuse/extend existing Ancillary ReferenceData conventions for authoring lookup only; no duplicate masters.

```http
GET Backoffice/v1/ReferenceData/Airlines
GET Backoffice/v1/ReferenceData/Airports
GET Backoffice/v1/ReferenceData/Cities
GET Backoffice/v1/ReferenceData/Countries
GET Backoffice/v1/ReferenceData/Currencies
GET Backoffice/v1/ReferenceData/Aircrafts
GET Backoffice/v1/ReferenceData/CabinClasses
GET Backoffice/v1/ReferenceData/Customers
GET Backoffice/v1/ReferenceData/TravelAgencies
GET Backoffice/v1/ReferenceData/TravelAgencyOffices
GET Backoffice/v1/ReferenceData/AirlineOffices
GET Backoffice/v1/ReferenceData/IndustryServiceSubCodes
```

Use canonical source IDs.

### Aircraft static SeatMap

```http
GET Backoffice/v1/ReferenceData/Aircrafts/{aircraftId}/SeatMap
```

maps/delegates to the verified AirInfo contract:

```text
GET v1/Aircrafts/{aircraftId}/DisplaySeatMap
```

### Fare/RBD/POS lookups

Do not recreate them in Ancillary. Use current AirPrice Backoffice contracts, including:

```text
GET Backoffice/v1/FareFamilies/Options
GET Backoffice/v1/Rbds/Options
GET Backoffice/v1/Rbds/ByCabinClass
GET Backoffice/v1/AirFares/Paginated
GET Backoffice/v1/PointOfSales/Options
```

---

# 9. Paid-seat authoring flow

Required practical workflow:

```text
1. choose Supplier / ServiceDefinition
2. choose AircraftId(s) from AirInfo
3. GET static SeatMap
4. select exact SeatNumbers and/or PADIS-compatible characteristics
5. choose route/flight/date/fare/PTC/POS conditions
6. enter filed price
7. Preview structural conflicts
8. Publish
9. test published behavior with AirAvail Backoffice/v1/AncillaryOffers
```

The Backoffice UI may present matrix/seat-map tools, but persistence remains ServiceDefinition + Provision.

---

# 10. Marketplace authoring example

Two lounge suppliers may independently define the same broad product:

```text
Supplier 2001 -> "Airline Lounge"
  ServiceDefinitionId 3001 -> sub-code 0BX -> SupplierId 2001

Supplier 2002 -> "Partner Lounge"
  ServiceDefinitionId 3002 -> sub-code 0BX -> SupplierId 2002
```

Each ServiceDefinition has its own Provisions, prices and availability policy. Supplier fulfillment routing comes from the referenced Supplier's typed `FulfillmentKind/FulfillmentProviderKey`, not from Supplier numeric ID.

AirAvail may return both as distinct OfferItems when both match.

No synthetic `SupplierVariant` or `MarketplaceProduct` aggregate is added.
