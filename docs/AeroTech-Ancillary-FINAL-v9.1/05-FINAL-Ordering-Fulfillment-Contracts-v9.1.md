# 05 — FINAL Ordering & Fulfillment Contracts v9.1

**Status:** implementation authority.

## 1. Boundary

```text
AirAvail shops/evaluates/prices.
Ordering owns trusted Order context and commercial acceptance.
Ordering orchestrates fulfillment and accountable-document issuance.
Ancillary fulfils marketplace services whose FulfillmentProviderKey is Ancillary.
FlightFlow fulfils physical seat assignment.
```

No OTA/IBE/Backoffice client calls Ancillary fulfillment endpoints directly.

---

# 2. Post-order shopping starts from Ordering

The v8 internal endpoint:

```text
GET Service/v1/Bookings/{orderId}/AncillaryShoppingContext
```

is removed from the target design.

AirAvail must not call Ordering for context.

For each existing Ordering Surface, expose post-order ancillary-shopping operations under the existing Booking/Order route convention.

Required relative operations:

```text
POST {surface}/v1/Bookings/{orderId}/AncillaryOffers
POST {surface}/v1/Bookings/{orderId}/AncillaryOffers/Details
POST {surface}/v1/Bookings/{orderId}/Ancillaries
```

For the Backoffice controller whose current base is `Backoffice/v1/Orders`, follow that existing base rather than creating a second parallel controller style. The functional operations remain equivalent.

### AncillaryOffers

Ordering:

1. authorizes and loads the Order;
2. constructs the canonical context in Section 3 from Order source-of-truth data;
3. sends it to `AirAvail Service/v1/AncillaryOffers`;
4. returns the AirAvail-composed Surface result.

### AncillaryOffers/Details

Ordering:

1. loads current Order and CommercialVersion;
2. sends offer/selections + current version to AirAvail Service Details;
3. receives public detail + internal accepted evidence;
4. returns the Surface detail.

### Ancillaries — commit selected post-order service

Ordering:

1. verifies current `CommercialVersion` equals the offer/detail version;
2. verifies AirAvail Details evidence;
3. creates the Order item/service + pricing snapshot;
4. increments commercial version exactly once for the committed change;
5. triggers existing provider orchestration;
6. records OrderChange/history.

There is no AirAvail -> Ordering callback.

---

# 3. Canonical Order ancillary-shopping context

Ordering builds this internally from its source-of-truth state and sends it to AirAvail.

```text
Surface
OrderId                 long
CommercialVersion       int
AsOf                    DateTimeOffset
SellingCurrencyId       int
ResolvedPointOfSaleIds  long[]
CustomerId              long?
CustomerType            canonical value?

Travellers[]
  TravellerId           long
  TravellerRef          string
  Index                  int
  PassengerTypeCode     canonical PTC

Journeys[]
  JourneyId             long
  BoundId               string
  Sequence              int
  OriginAirportId       int
  DestinationAirportId  int

Flights[]
  FlightRef             string
  SegmentId             long
  JourneyId             long
  Sequence              int
  FlightId              long
  FlightCapacityId      long?
  FlightNumber          string
  MarketingAirlineId    int
  OperatingAirlineId    int
  OriginAirportId       int
  DestinationAirportId  int
  DepartureDateTime     DateTimeOffset
  ArrivalDateTime       DateTimeOffset
  AircraftId            int?
  CabinClassId          int?
  RbdId                 long?

FareComponents[]
  FlightIds[]           long[]
  AirFareId             long?
  AirFareType           canonical value?
  FareFamilyId          long?
  FareBasis             string?
  CabinClassId          int?
  RbdId                 long?

ExistingAncillaries[]
  OrderServiceId        long
  ServiceDefinitionId   long
  SupplierId            long
  TravellerId           long?
  CoveredFlightIds[]    long[]
  Quantity              int
  CommercialStatus
```

No DateOfBirth/FF-status/occurrence fields are required by v9.1 because those qualifiers are not implemented.

### Mandatory source addition

Current `OrderAirTransportService` has FareFamily display text but not `FareFamilyId`.

Add:

```text
FareFamilyId long?
```

through accepted AirOffer detail -> Order domain/read state so post-order ancillary FareFamily evaluation is deterministic.

---

# 4. Accepted ancillary commercial snapshot

AirAvail Service Details returns canonical IDs and commercial evidence.

For every accepted Ancillary Order service persist/snapshot at least:

```text
OfferItemId
ServiceDefinitionId
ProvisionId
SupplierId

ServiceDefinitionRef         // display/audit snapshot only
ServiceDefinitionVersion     // display/audit snapshot only
SupplierName                 // display/audit snapshot if required
CommercialName/classification

TravellerId
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
ROE evidence

DocumentDefinition
BookingDefinition
SettlementDefinition
FulfillmentProviderKey   // Ordering-level provider key
```

`Supplier.FulfillmentProviderKey` is intentionally not part of this AirAvail/Ordering accepted-sale contract. Ancillary resolves that internal adapter key from the persisted Supplier when fulfillment reaches Ancillary.

Rules:

- `ServiceDefinitionId`, `ProvisionId`, `SupplierId` are the runtime identities;
- never reconstruct sold identity from `ServiceDefinitionRef + Version`;
- `ServiceDefinitionRef/Version/SupplierName` are snapshot/display fields only;
- never store `ReservationMode` from Ancillary/AirAvail.

---

# 5. Price trust boundary

Ordering trusts AirAvail Service Details as the commercial acceptance source after verifying offer integrity/version.

Ordering stores both:

```text
filed price evidence
selling price evidence + FX/ROE evidence
```

Ancillary fulfillment is not asked to recompute or validate the selling amount.

Therefore the Ordering -> Ancillary Hold request contains **no**:

```text
AcceptedRevenue
SellingCurrencyId
FX rate
selling price line
```

This removes the v8 contradiction where Ancillary was expected to validate an amount produced by AirAvail conversion even though Ancillary owns no FX logic.

---

# 6. Provider capability ownership

Existing source pattern remains authority:

```text
OrderService.FulfillmentProviderKey
-> ReservationProviderResolver
-> IReservationProvider
-> CapabilityFor(OrderService)
```

`ReservationMode` remains Ordering provider capability state.

Typical provider keys:

```text
Ancillary   -> marketplace services committed through Ancillary
FlightFlow  -> physical seat assignment
```

When Ordering chooses `Ancillary`, supplier-specific behavior is decided **inside Ancillary** by loading `ServiceDefinition -> Supplier` and applying `Supplier.FulfillmentKind` plus, for External suppliers, `Supplier.FulfillmentProviderKey`. `SupplierId` is identity only. The selected Ancillary-side adapter must present the stable Hold/Confirm/Read/Validate/Release/Cancel semantics to Ordering; do not add a generic supplier capability matrix until a real integration proves the outer contract is insufficient.

---

# 7. Ordering -> Ancillary reservation contract — ID based

Only Order services whose `FulfillmentProviderKey == "Ancillary"` use this contract.

## 7.1 Hold / reserve

```http
POST Service/v1/Ancillaries/Service-Holds
```

Request:

```text
IdempotencyKey
OrderId
Reference
RequestedExpiresAt?
Services[]
  OrderServiceId        long
  ServiceDefinitionId   long
  ProvisionId           long
  TravellerId           long?
  CoverageScope
  CoveredFlightIds[]
  Quantity
```

No business-key identity. No price/FX payload.

Ancillary verifies:

```text
ServiceDefinitionId exists
ProvisionId belongs to ServiceDefinitionId
ServiceDefinition.SupplierId identifies the Supplier; Supplier.FulfillmentKind/FulfillmentProviderKey select behavior
OrderServiceId not already committed inconsistently
quantity/coverage
stock/quota if Ancillary-owned
idempotency/concurrency
supplier reserve behavior
```

It does not re-run commercial eligibility.

## 7.2 Read

```http
GET Service/v1/Ancillaries/Service-Holds/{holdId}
```

## 7.3 Confirm

```http
POST Service/v1/Ancillaries/Service-Holds/{holdId}/Confirmations
```

Supplier-specific confirm behavior is resolved from the ServiceDefinition's Supplier and its typed fulfillment routing fields.

## 7.4 Release

```http
DELETE Service/v1/Ancillaries/Service-Holds/{holdId}
```

## 7.5 Cancel confirmed service

```http
POST Service/v1/Ancillaries/Service-Confirmations/{holdId}/Cancellation
```

Request contains only the confirmed OrderService/unit identities and cancellation reason required by the existing contract style.

---

# 8. Supplier post-confirm activation/issuance — NOT IN v9.1 CORE

v9.1 intentionally defines **no** generic supplier-side `Issuance`, `Activation`, `VoucherIssue`, or no-op equivalent endpoint.

Reason:

- no current concrete Supplier integration contract proves that a distinct post-confirm operation is required;
- inventing an endpoint now would create an unused abstraction and force fake/no-op capability paths;
- reserve/confirm/read/validate/release/cancel are sufficient for the current CORE fulfillment contract.

If a real Supplier later proves a separate post-confirm operation, add it from that supplier contract with an explicit capability and conformance tests. Do not retrofit it by name only.

This restriction does **not** change accountable document issuance:

```text
ETKT / EMD issuance, stock, coupons, void/refund/exchange -> Ordering
```

Supplier identity and supplier fulfillment do not grant document authority.

# 9. Ancillary validation for Ordering issue

Before accountable document issue, Ordering may use provider validation according to its existing `IReservationProvider.ValidateAsync` pattern.

For the Ancillary provider, validation is Supplier-aware and routing-aware:

```text
OrderServiceId
-> AncillaryReservationUnit
-> ServiceDefinitionId
-> SupplierId
-> Supplier.FulfillmentKind
-> Supplier.FulfillmentProviderKey when External
-> supplier-specific validation
```

A validation read must not silently reprice the sold service.

---

# 10. Ancillary reservation state

Reservation Unit identity/persistence uses:

```text
OrderServiceId
ServiceDefinitionId
ProvisionId
TravellerId?
CoveredFlightIds[]
Quantity
ProviderUnitRef?
```

No:

```text
ServiceDefinitionRef foreign key
ServiceDefinitionVersion foreign key
AcceptedRevenue
selling CurrencyId
```

The sold ServiceDefinition's SupplierId is resolved from immutable ServiceDefinition ID and may also be copied to read/audit output, but does not need to be redundantly supplied by Ordering Hold.

---

# 11. Idempotency/concurrency

Required:

```text
same IdempotencyKey + same normalized body -> same result
same IdempotencyKey + different body       -> conflict
```

Quota/max-count transitions must be serialized across nodes using existing repository concurrency patterns.

Stable occurrence identity for a traveller service:

```text
ServiceDefinitionId
+ TravellerId
+ canonical sorted CoveredFlightIds
```

For order coverage:

```text
ServiceDefinitionId + OrderId
```

---

# 12. Stock/quota behavior

For Ancillary-owned quota:

```text
Hold        -> Held += qty
Confirm     -> Held -= qty; Confirmed += qty
Release     -> Held -= qty
Expire      -> Held -= qty exactly once
Cancel      -> Confirmed -= qty when the service rule allows releasing capacity
```

Invariant:

```text
Held + Confirmed <= Capacity
```

Stock identity:

```text
ServiceDefinitionId + FlightId
```

---

# 13. Paid-seat fulfillment

Paid-seat commercial price is Ancillary-authored but physical commitment is FlightFlow-owned.

```text
AirAvail SeatOffers
-> selection accepted by Ordering
-> OrderSeatService
-> FulfillmentProviderKey = FlightFlow
-> existing FlightFlow reservation provider
```

No `AncillaryReservation` is created for physical seat assignment.

A seat can become unavailable between shopping and hold. Provider hold failure causes re-shop/reselection; it is not repaired by pretending Ancillary stock owns the seat.

---

# 14. EMD policy

The sold Ancillary snapshot tells Ordering:

```text
Document.Type = None | EmdAssociated | EmdStandalone
RFIC
RFISC
Settlement policy
```

Ordering owns document issuance.

Examples:

```text
extra baggage -> normally EMD-A when airline policy requires
lounge        -> may be EMD-S
priority      -> may require no accountable document
```

Supplier settlement/fulfillment and EMD document issuance are separate decisions.

Lufthansa partner documentation independently confirms real optional-service settlement can include direct settlement, EMD-A/EMD-S and separate validating/provider concepts; v9.1 therefore does not assume "Supplier == document issuer".

---

# 15. Cancellation/refund

Ordering determines:

```text
which OrderService(s) are being cancelled
commercial/document/payment state
refund accounting using sold snapshot
```

Ordering invokes provider cancellation where required.

Ancillary receives provider-owned cancellation identity/reason and applies Supplier-specific cancellation behavior.

It does not re-read current price rules to decide the historical refund amount.

---

# 16. Historical integrity

After Supplier/ServiceDefinition/Provision retirement or revision:

- existing Order services retain their `ServiceDefinitionId`, `ProvisionId`, `SupplierId` snapshot;
- provider operations on old sales resolve the historical ServiceDefinition row by ID;
- supplier retirement does not make an already sold service un-cancellable/un-validatable;
- new shopping only uses current Active supplier/service/provision rows;
- refund/EMD logic uses the Order's sold commercial snapshot.
