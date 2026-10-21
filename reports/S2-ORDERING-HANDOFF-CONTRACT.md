# S2 → Ordering handoff contract (implementation-independent)

**The test-only Ordering simulator is not a production Ordering contract nor an implementation of Order persistence.**

| Item | Value |
|---|---|
| Ancillary base | `k8s-stg@ca4242b`, plus the SeatMap Resolve change set in the working tree (sections 1.3 and 1b; see `S2-SEATMAP-RESOLVE-CONFORMANCE.md`) |
| Ordering source read (read-only) | `AeroTech.Ordering.Final@cd50a2a` |
| Example traffic | Captured in `tests/AeroTech.Ancillary.Stage2.EndToEndTests` (standalone HTTP E2E, see `S2-STANDALONE-HTTP-E2E-CONFORMANCE.md`) |

This document does not dictate an HTTP route inside Ordering.

---

## 1. Ancillary Resolve (exists today)

### 1.1 Endpoint

`POST /Service/v1/Ancillaries/Selections/Resolve`
- Controller: `src/AeroTech.Ancillary.RestApi/V1/AncillaryOffers/Controllers/ServiceController.cs`.
- It is anonymous S2S, like the existing Service routes.
- It has no side effect in Ancillary: no hold, no payment, no Order write.

### 1.2 Request (`ServiceResolveSelectionsQuery`)

| Field | Type | Rule |
|---|---|---|
| `orderId` | long | required, > 0 |
| `expectedOrderCommercialVersion` | int | required, > 0; must equal the Order's current `CommercialVersion` |
| `pricedSelectionRefs` | string[] | 1 to 50 refs, exactly as EvaluateBatch returned them |
| `correlationId` | string? | echoed back |

### 1.3 Response (`ApiResult<ServiceSelectionResolutionDto>`, wrapped in `{"data": …}`)

**Top level:** `orderId` (string), `orderCommercialVersion` (int?), `correlationId`, `allValid` (bool), and `results[]`. There is one result per requested ref, in the same order.

**`results[]`** carries `pricedSelectionRef`, `status`, `reasonCodes[]` and `service?`.
- `status` is one of `Valid | Stale | Expired | NeedsQuote | NeedsVerification | Unavailable | Rejected`.
- Only `Valid` carries `service`. Seats never pass through this endpoint any more (section 1b).

**`service`** (`ServiceResolvedServiceDto`):

| Group | Fields |
|---|---|
| Identity | `serviceDefinitionRef`, `definitionVersionId`, `provisionId`, `pricingRevisionId?`, `pricingRateId?` |
| Codes | `rfic?`, `rfisc?`, `ssrCode?`, `variantCode`, `serviceTypeCode`, `serviceSubCode` |
| Coverage | `coverageScope` (`Sector`/`Portion`/`Journey`/`Order`), `orderTravellerIds[]` (long as string), `travellerRefs[]`, `flightIds[]`, `portionRefs[]` |
| Options | `selectedOptions[]` |
| Price (`OfferExactPriceDto`) | `origin`, `pricingUnit`, `currencyId`, `filedCurrencyId`, `quantity`, `chargeableWeightKg`, `baseAmount`, `addedTaxes[]`, `includedTaxes[]`, `unitFees[]`, `sharedFees[]`, `unitTotal`, `lineTotal`, `fx`, `isTotalComplete`, `reasonCodes[]` |
| Booking and document | `requiresCheckAtReserve`, `bookingMethod` (`Ssr`/`AuxiliarySegment`/`DisplayPriceContactCarrierForBooking`/`NoBookingProcessRequired`/`PerServiceRecord`), `bookingRequired`, `documentRouting` (`NoAncillaryDocument`/`Emd`/`TicketOrExchange`), `documentType` (`None`/`EmdAssociated`/`EmdStandalone`), `documentRequired`, `confirmationRequirement` (`Immediate`/`SubjectToConfirmation`) |
| Provider (**new, additive**) | `fulfillmentProviderKey?` |
| Selection | `selection` (all 37 `AncillarySelection` fields, exactly as accepted) |

`fulfillmentProviderKey` is new in this change set:
- It is taken from the candidate's existing `Confirmation.FulfillmentProviderKey`, which comes from the provision's `Fulfillment` definition.
- It was added because Ordering requires a provider key on every `OrderService` (`Ordering.Final Domain/OrderAggregate/Entities/OrderService.cs:45`, max length 64 in `OrderServiceConfiguration.cs:16`).
- Its production code is `ServiceResolvedServiceDto.cs` and `ServiceSelectionResolutionMapper.cs`, one line each.

`includedSeat`, `kind`, `seatNumber` and `flightCapacityId` were removed on 2026-10-21 (Owner Q5 = a). A seat is resolved only by section 1b.

**Reason codes a consumer will see:**

| Situation | Status | Reason code |
|---|---|---|
| Order version moved | Stale | `OrderVersionChanged` |
| Order source changed (fingerprint) | Stale | `SourceVersionChanged` |
| Recomputed facts differ | Stale | `SignedFactsChanged` |
| Same facts, new amount | Stale | `PriceChanged` |
| Item gone | Stale | `OfferItemNotOffered` |
| Bad signature | Rejected | `PricedSelectionNotAuthentic` |
| Ref signed for another Order | Rejected | `PricedSelectionForAnotherOrder` |
| Traveller without a unique positive `OrderTravellerId` | NeedsVerification | `OrderTravellerNotLinked` |
| Past the token expiry | Expired | — |
| Ordering source not reachable | whole request fails: HTTP 503, code 16815 | — |

## 1b. SeatMap Resolve (new, 2026-10-21)

### 1b.1 Endpoint

`POST /Service/v1/Ancillaries/SeatMaps/Resolve`
- Controller: `src/AeroTech.Ancillary.RestApi/V1/SeatMaps/Controllers/ServiceController.cs`, anonymous S2S beside the selection Resolve.
- Service: `src/AeroTech.Ancillary.Query/Shopping/SeatMaps/Queries/ResolveSeatMapSelections/ResolveSeatMapSelectionsService.cs`.
- No side effect: no hold, no payment, no Order write. It rebuilds the seat map from the current source and re-prices every seat.

### 1b.2 The seat id

The `id` of every selectable seat in the SeatMaps answer (`travelerPricing[].id`) is now recoverable, like AirAvail's `JourneyOfferIdCodec`:
`base64url( travellerRef | flightId | ancillaryPricingId | SEATNUMBER )` (`SeatMapSelectionId.cs`). It is not signed; Resolve re-validates everything.
- A seat has an id only when it is `AVAILABLE` with a priced category. A seat without any category is `BLOCKED` and has no id (Owner Q2: `includedSeat` no longer exists; a free seat is a Paid provision whose Active pricing rate is 0).
- The id is the same on all three SeatMaps surfaces.

### 1b.3 Request (`ServiceResolveSeatMapSelectionsQuery`)

| Field | Type | Rule |
|---|---|---|
| `offerId` | string? | exactly one of `offerId` / `orderId` (else 422 / 16801) |
| `orderId` | long? | > 0; the order's source offer id is read from Ordering |
| `expectedOrderCommercialVersion` | int? | required with `orderId`, > 0 |
| `seats[]` | `{ id, exitRowTermsAccepted? }` | 1 to 50, ids non-empty and unique |
| `correlationId` | string? | echoed back |

Sales scope: channel, customer and country from the offer id's commercial section; home airline from `IOperatorAirlineResolver`; points of sale = those named by the provisions of the pricings in the ids, else every known point of sale (none known: 409 / 16808).

### 1b.4 Response (`ApiResult<ServiceSeatMapResolutionDto>`)

**Top level:** `offerId`, `orderId?`, `orderCommercialVersion?`, `correlationId?`, `allValid` (= every seat `Valid`), `seats[]` in request order.

**`seats[]`** (`ServiceResolvedSeatDto`): `seatMapSelectionId`, `status` (`Valid | Stale | Rejected`), `reasonCodes[]`, and only on `Valid`: `travellerRef`, `orderTravellerId?` (null on the offer path), `flightId`, `flightCapacityId`, `seatNumber`, `serviceDefinitionId`, `provisionId`, `pricingId`, `fulfillmentProviderKey`, `price { currencyId, baseAmount, taxes[{code, amount}], fees[{code, amount}], total }`.
- Ids are `long` in the contract; the platform JSON converter renders every `long` as a string on the wire (as it does for `orderId` today). Amounts are numbers.

| Situation | Status | Reason code |
|---|---|---|
| Order version moved | Stale | `OrderVersionChanged` (every seat, nothing priced) |
| Seat taken, blocked or no longer priced | Stale | `SeatSelectionNotOffered` |
| Same seat still offered under another pricing | Stale | `PriceChanged` |
| Id unreadable, or names a flight/seat/traveller not on the map | Rejected | `SeatSelectionNotOffered` |
| Same seat for two travellers / two seats for one traveller on one flight | Rejected | `SeatSelectedTwice` / `TravellerSeatSelectedTwice` |
| Order path, traveller without a positive `OrderTravellerId` | Rejected | `OrderTravellerNotLinked` |
| Exit row without accepted terms, or any other rule failure | Rejected | the engine's reason codes (for example `ExitRowTermsNotAccepted`) |
| Fee that could not be applied | Rejected | `SharedFeeNotApplied` |
| Ordering not reachable / unknown order | HTTP 503 / 16815, HTTP 403 / 16809 | — |

## 2. What Ancillary needs from Ordering (`IOrderCommercialContextReader`)

```csharp
public interface IOrderCommercialContextReader
{
    Task<OrderCommercialContext?> FindAsync(long orderId, CancellationToken cancellationToken = default);
}
public sealed record OrderCommercialContext(long OrderId, int CommercialVersion, bool AcceptsChanges, DateTimeOffset? TicketedAtUtc,
    FlightOffer Air, IReadOnlyList<OrderTravellerFacts> Travellers, IReadOnlyList<OrderServiceFacts> Services);
public sealed record OrderTravellerFacts(string TravellerRef, long OrderTravellerId, DateOnly? DateOfBirth, string? InfantOfTravellerRef);
public sealed record OrderServiceFacts(long OrderServiceId, string TravellerRef, string ServiceRef, string? PortionRef,
    IReadOnlyList<string> FlightRefs, int Quantity, ExistingServiceCommercialState CommercialState);
```
- Production registration: `NotConnectedOrderCommercialContextReader`, which fails closed with 503 / 16815.
- The E2E host replaces it in test DI only. Test C2_01 asserts that production keeps `NotConnected…`.
- `null` means an unknown or foreign Order and gives HTTP 403 (16809).

| Fact | Ancillary use | Owner | Present in Ordering.Final cd50a2a |
|---|---|---|---|
| `OrderId`, `CommercialVersion:int` | binding, stale check | Ordering | yes: `Order.CommercialVersion int` (`Order.cs:64`); no Service read endpoint (`OrderAggregate/Controllers/ServiceController.cs` has no action) |
| `AcceptsChanges` | refuse closed orders (422 / 16801) | Ordering | **absent**; must be derived from `OrderStatus` |
| `TicketedAtUtc` | PreOrder vs PostTicketed | Ordering / ticketing | **absent on Order**; per ticket `ElectronicTicket.IssuedAt` |
| `Air` (`FlightOffer`: bounds, flights with `FlightCapacityId`, fare components, tickets with coupons and baggage, `CustomerId`, `SalesChannel`, `SalesCountryId`, `CurrencyId`) | the whole engine context; the scope guard compares channel, customer and country | AirAvail produced it at pricing; Ordering keeps `SourceOfferId` and per-service air facts (`OrderAirTransportService`) | **partially**; there is no serialized snapshot in this shape |
| coupon `IsOpenForUse` | post-ticket eligibility | Ordering / ticketing (`TicketCoupon.FinancialStatus`) | present as a status; the mapping to open-for-use is not defined |
| travellers: `TravellerRef` → unique positive `OrderTravellerId`, DOB, infant link | B5 binding, PTC age rules | Ordering (`OrderTraveller.SourceTravellerRef`, `Id long`, `InfantParentTravellerId`) | id and ref present; **DOB not on the entity** (profile revision not verified) |
| existing services | cumulative limits, existing-service facts | Ordering (`OrderService`) | present for air and seat; no ancillary subclass |

## 3. What a real Ordering Add use case should do

This is future Ordering code. It is not an Ancillary change. The simulator implements these rules for the E2E only.

1. Check, under the Order lock, that `expectedOrderCommercialVersion == Order.CommercialVersion`. If not, answer a conflict before calling Ancillary.
2. Record the idempotency key with a digest of the payload.
   - Same key and same payload: return the first result and do not insert again.
   - Same key and another payload: conflict.
3. Call `POST /Service/v1/Ancillaries/Selections/Resolve` with the refs and the expected version, and `POST /Service/v1/Ancillaries/SeatMaps/Resolve` with the seat ids (and `exitRowTermsAccepted` per id) and the same expected version. Either list may be empty, not both.
4. Accept only HTTP 200 with `allValid=true` and the same `orderId` and version, from each call that was made.
5. For every service result require `service`, then validate:
   - Beneficiary: one `orderTravellerIds` entry that belongs to the Order and matches `travellerRefs`.
   - Flights: every `flightIds` entry belongs to the Order.
   - Price: `price.currencyId` equals the Order currency, `lineTotal` is present and `isTotalComplete=true`.
   - Provider: a non-empty `fulfillmentProviderKey`. Never fall back to a constant.
   For every seat result validate: `orderTravellerId` and `flightId` name an air service of the Order, `flightCapacityId` equals that air service's capacity, the seat is not already assigned on that flight, `price.currencyId` equals the Order currency with `total` present, and `serviceDefinitionId`, `provisionId`, `pricingId` and `fulfillmentProviderKey` are present.
6. Apply all lines in one transaction: OrderService and seat assignments, plus charge lines. Increment `CommercialVersion` exactly once.
   - Keep these exactly as returned: price, quantity, traveller, flights, seat number, selection, provider key, and the definition, provision and pricing ids.
   - Any failure inserts nothing.
7. Do not capture money, issue an EMD or ticket, or hold inventory in this step. Those belong to later stages.

**Consumer gaps found by the E2E (Ordering-side, not Ancillary):**
- An Order-scoped service (`coverageScope=Order`, `orderTravellerIds=[]`) resolves `Valid`, but `OrderService.TravellerId` is a non-nullable long, so it cannot be stored. Test C5_10 proves the Add is refused. This is an external consumer integration issue; no fake traveller is invented.
- Ordering creates `OrderSeatService` only at order creation (`Order.Create.cs:370-407`). Adding a resolved seat later (paid or zero-rated) needs a new Ordering operation.
- Ordering has no ancillary `OrderService` subclass and no EMD/RFIC/RFISC fields. `OrderServiceType` lists ancillary kinds, but none of them has an entity.

## 4. Example traffic (captured from the E2E run)

JWS values are shortened to `…<JWS>`. All ids are test ids.

### Baggage, positive (A01 x2 for CHD-1)

Source: `C4_F2_an_existing_pre_issue_order_adds_two_pieces_for_one_traveller_with_its_exact_price_and_one_version_step`, call 4 (`ordering->ancillary`), HTTP 200.

Request `POST /Service/v1/Ancillaries/Selections/Resolve`:
```json
{
  "orderId": 7001,
  "expectedOrderCommercialVersion": 1,
  "pricedSelectionRefs": [
    "eyJhbGciOiJIUzI1NiIsInR5...<JWS>"
  ],
  "correlationId": "f2-add"
}
```

Response:
```json
{
  "data": {
    "orderId": "7001",
    "orderCommercialVersion": 1,
    "correlationId": "f2-add",
    "allValid": true,
    "results": [
      {
        "pricedSelectionRef": "eyJhbGciOiJIUzI1NiIsInR5...<JWS>",
        "status": "Valid",
        "reasonCodes": [],
        "service": {
          "serviceDefinitionRef": "EXTRA_BAG",
          "definitionVersionId": "1002",
          "provisionId": "1003",
          "pricingRevisionId": "1007",
          "rfic": "C",
          "rfisc": "XB1",
          "ssrCode": "XBAG",
          "coverageScope": "Portion",
          "orderTravellerIds": [
            "1002"
          ],
          "travellerRefs": [
            "CHD-1"
          ],
          "flightIds": [
            "1558420526869774453"
          ],
          "portionRefs": [
            "B1"
          ],
          "selectedOptions": [],
          "price": {
            "origin": "Filed",
            "pricingUnit": "PerPiece",
            "currencyId": 70,
            "filedCurrencyId": 70,
            "quantity": 2,
            "chargeableWeightKg": null,
            "baseAmount": 5000000,
            "addedTaxes": [],
            "includedTaxes": [],
            "unitFees": [],
            "sharedFees": [],
            "unitTotal": 5000000,
            "lineTotal": 10000000,
            "fx": {
              "rateOfExchangeTableId": "70",
              "fromCurrencyId": 70,
              "toCurrencyId": 70,
              "rate": 1,
              "decimalPlaces": 0
            },
            "isTotalComplete": true,
            "reasonCodes": []
          },
          "requiresCheckAtReserve": false,
          "variantCode": "A01",
          "serviceTypeCode": "F",
          "serviceSubCode": "XB1",
          "bookingMethod": "Ssr",
          "bookingRequired": false,
          "documentRouting": "Emd",
          "documentType": "EmdAssociated",
          "documentRequired": true,
          "confirmationRequirement": "Immediate",
          "fulfillmentProviderKey": "Ancillary",
          "pricingRateId": "1008",
          "selection": {
            "quantity": 2,
            "packageProductRef": null,
            "bagRef": null,
            "measuredWeightKg": null,
            "dimensions": null,
            "equipmentKind": null,
            "weightKg": null,
            "seatNumber": null,
            "exitRowTermsAccepted": null,
            "purpose": null,
            "targetCabinId": null,
            "quoteRef": null,
            "mealCode": null,
            "menuItemRef": null,
            "animalType": null,
            "otherAnimalCode": null,
            "animalAgeWeeks": null,
            "combinedWeightKg": null,
            "carrierDimensions": null,
            "documentAcknowledgements": null,
            "sizeBracket": null,
            "assistanceSsrCode": null,
            "equipmentCode": null,
            "evidenceDocumentRefs": null,
            "oxygenUnits": null,
            "infantRef": null,
            "guardianRef": null,
            "childRef": null,
            "guardianHandoffContactProvided": false,
            "guardianPickupContactProvided": false,
            "airportId": null,
            "facilityRef": null,
            "timeWithOffset": null,
            "guestCount": null,
            "optIn": null,
            "planCode": null,
            "deviceCount": null
          }
        }
      }
    ]
  },
  "errors": null
}
```

### Seats, positive by order (paid A07 3A for ADT-1 + zero-rated 3B for CHD-1)

Source: `C3_02_a_paid_and_a_zero_rated_seat_of_the_order_resolve_valid_together_and_the_same_seat_twice_resolves_nothing`, call 4 (`anonymous->ancillary`), HTTP 200.

Request `POST /Service/v1/Ancillaries/SeatMaps/Resolve`:
```json
{
  "offerId": null,
  "orderId": 7001,
  "expectedOrderCommercialVersion": 1,
  "seats": [
    {
      "id": "QURULTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
      "exitRowTermsAccepted": null
    },
    {
      "id": "Q0hELTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDQwfDNC",
      "exitRowTermsAccepted": null
    }
  ]
}
```

Response:
```json
{
  "data": {
    "offerId": "1|1|0;9000000000000000001~9000000000000000101_9000000000000000002~9000000000000000102;3|9000000000000000900||70|1791706372",
    "orderId": "7001",
    "orderCommercialVersion": 1,
    "correlationId": null,
    "allValid": true,
    "seats": [
      {
        "seatMapSelectionId": "QURULTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
        "status": "Valid",
        "reasonCodes": [],
        "travellerRef": "ADT-1",
        "orderTravellerId": "1001",
        "flightId": "1558420526869774453",
        "flightCapacityId": "1558420526869774478",
        "seatNumber": "3A",
        "serviceDefinitionId": "1025",
        "provisionId": "1026",
        "pricingId": "1031",
        "fulfillmentProviderKey": "Ancillary",
        "price": {
          "currencyId": 70,
          "baseAmount": 1500000,
          "taxes": [],
          "fees": [],
          "total": 1500000
        }
      },
      {
        "seatMapSelectionId": "Q0hELTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDQwfDNC",
        "status": "Valid",
        "reasonCodes": [],
        "travellerRef": "CHD-1",
        "orderTravellerId": "1002",
        "flightId": "1558420526869774453",
        "flightCapacityId": "1558420526869774478",
        "seatNumber": "3B",
        "serviceDefinitionId": "1034",
        "provisionId": "1035",
        "pricingId": "1040",
        "fulfillmentProviderKey": "Ancillary",
        "price": {
          "currencyId": 70,
          "baseAmount": 0,
          "taxes": [],
          "fees": [],
          "total": 0
        }
      }
    ]
  },
  "errors": null
}
```


### Negative: the same seat for two travellers

Source: `C3_02_a_paid_and_a_zero_rated_seat_of_the_order_resolve_valid_together_and_the_same_seat_twice_resolves_nothing`, call 5 (`anonymous->ancillary`), HTTP 200.

Request `POST /Service/v1/Ancillaries/SeatMaps/Resolve`:
```json
{
  "offerId": null,
  "orderId": 7001,
  "expectedOrderCommercialVersion": 1,
  "seats": [
    {
      "id": "QURULTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
      "exitRowTermsAccepted": null
    },
    {
      "id": "Q0hELTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
      "exitRowTermsAccepted": null
    }
  ]
}
```

Response:
```json
{
  "data": {
    "offerId": "1|1|0;9000000000000000001~9000000000000000101_9000000000000000002~9000000000000000102;3|9000000000000000900||70|1791706372",
    "orderId": "7001",
    "orderCommercialVersion": 1,
    "correlationId": null,
    "allValid": false,
    "seats": [
      {
        "seatMapSelectionId": "QURULTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
        "status": "Rejected",
        "reasonCodes": [
          "SeatSelectedTwice"
        ],
        "travellerRef": null,
        "orderTravellerId": null,
        "flightId": null,
        "flightCapacityId": null,
        "seatNumber": null,
        "serviceDefinitionId": null,
        "provisionId": null,
        "pricingId": null,
        "fulfillmentProviderKey": null,
        "price": null
      },
      {
        "seatMapSelectionId": "Q0hELTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
        "status": "Rejected",
        "reasonCodes": [
          "SeatSelectedTwice"
        ],
        "travellerRef": null,
        "orderTravellerId": null,
        "flightId": null,
        "flightCapacityId": null,
        "seatNumber": null,
        "serviceDefinitionId": null,
        "provisionId": null,
        "pricingId": null,
        "fulfillmentProviderKey": null,
        "price": null
      }
    ]
  },
  "errors": null
}
```


### Negative: Order commercial version moved after signing

Source: `C5_01_a_commercial_change_after_signing_makes_resolve_stale_and_the_add_inserts_nothing`, call 5 (`ordering->ancillary`), HTTP 200.

Request `POST /Service/v1/Ancillaries/Selections/Resolve`:
```json
{
  "orderId": 7001,
  "expectedOrderCommercialVersion": 2,
  "pricedSelectionRefs": [
    "eyJhbGciOiJIUzI1NiIsInR5...<JWS>"
  ],
  "correlationId": "stale-version"
}
```

Response:
```json
{
  "data": {
    "orderId": "7001",
    "orderCommercialVersion": 2,
    "correlationId": "stale-version",
    "allValid": false,
    "results": [
      {
        "pricedSelectionRef": "eyJhbGciOiJIUzI1NiIsInR5...<JWS>",
        "status": "Stale",
        "reasonCodes": [
          "OrderVersionChanged"
        ],
        "service": null
      }
    ]
  },
  "errors": null
}
```

### Negative: seat taken after the map was shown (call made by the Ordering simulator)

Source: `C5_07_a_seat_taken_after_the_map_was_shown_is_stale_at_resolve_and_the_whole_add_inserts_nothing`, call 7 (`ordering->ancillary`), HTTP 200.

Request `POST /Service/v1/Ancillaries/SeatMaps/Resolve`:
```json
{
  "orderId": 7001,
  "expectedOrderCommercialVersion": 1,
  "seats": [
    {
      "id": "QURULTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
      "exitRowTermsAccepted": null
    },
    {
      "id": "Q0hELTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDQwfDNC",
      "exitRowTermsAccepted": null
    }
  ],
  "correlationId": "taken"
}
```

Response:
```json
{
  "data": {
    "offerId": "1|1|0;9000000000000000001~9000000000000000101_9000000000000000002~9000000000000000102;3|9000000000000000900||70|1791706372",
    "orderId": "7001",
    "orderCommercialVersion": 1,
    "correlationId": "taken",
    "allValid": false,
    "seats": [
      {
        "seatMapSelectionId": "QURULTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
        "status": "Stale",
        "reasonCodes": [
          "SeatSelectionNotOffered"
        ],
        "travellerRef": null,
        "orderTravellerId": null,
        "flightId": null,
        "flightCapacityId": null,
        "seatNumber": null,
        "serviceDefinitionId": null,
        "provisionId": null,
        "pricingId": null,
        "fulfillmentProviderKey": null,
        "price": null
      },
      {
        "seatMapSelectionId": "Q0hELTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDQwfDNC",
        "status": "Valid",
        "reasonCodes": [],
        "travellerRef": "CHD-1",
        "orderTravellerId": "1002",
        "flightId": "1558420526869774453",
        "flightCapacityId": "1558420526869774478",
        "seatNumber": "3B",
        "serviceDefinitionId": "1034",
        "provisionId": "1035",
        "pricingId": "1040",
        "fulfillmentProviderKey": "Ancillary",
        "price": {
          "currencyId": 70,
          "baseAmount": 0,
          "taxes": [],
          "fees": [],
          "total": 0
        }
      }
    ]
  },
  "errors": null
}
```


### Negative: exit-row terms refused

Source: `C3_04_an_exit_row_seat_resolves_only_for_a_test_verified_adult_who_accepts_the_terms_and_a_seat_is_no_batch_line`, call 5 (`anonymous->ancillary`), HTTP 200.

Request `POST /Service/v1/Ancillaries/SeatMaps/Resolve`:
```json
{
  "offerId": null,
  "orderId": 7001,
  "expectedOrderCommercialVersion": 1,
  "seats": [
    {
      "id": "QURULTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
      "exitRowTermsAccepted": false
    }
  ]
}
```

Response:
```json
{
  "data": {
    "offerId": "1|1|0;9000000000000000001~9000000000000000101_9000000000000000002~9000000000000000102;3|9000000000000000900||70|1791706372",
    "orderId": "7001",
    "orderCommercialVersion": 1,
    "correlationId": null,
    "allValid": false,
    "seats": [
      {
        "seatMapSelectionId": "QURULTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
        "status": "Rejected",
        "reasonCodes": [
          "ExitRowTermsNotAccepted"
        ],
        "travellerRef": null,
        "orderTravellerId": null,
        "flightId": null,
        "flightCapacityId": null,
        "seatNumber": null,
        "serviceDefinitionId": null,
        "provisionId": null,
        "pricingId": null,
        "fulfillmentProviderKey": null,
        "price": null
      }
    ]
  },
  "errors": null
}
```


### Seats by priced offer (no order): one valid, one hand-made id for a blocked seat

Source: `C3_05_a_priced_offer_resolves_its_seats_without_an_order_and_a_hand_made_id_for_a_blocked_seat_is_refused`, call 2 (`anonymous->ancillary`), HTTP 200.

Request `POST /Service/v1/Ancillaries/SeatMaps/Resolve`:
```json
{
  "offerId": "1|1|0;9000000000000000001~9000000000000000101_9000000000000000002~9000000000000000102;3|9000000000000000900||70|1791706372",
  "orderId": null,
  "expectedOrderCommercialVersion": null,
  "seats": [
    {
      "id": "QURULTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
      "exitRowTermsAccepted": null
    },
    {
      "id": "Q0hELTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDFB",
      "exitRowTermsAccepted": null
    }
  ]
}
```

Response:
```json
{
  "data": {
    "offerId": "1|1|0;9000000000000000001~9000000000000000101_9000000000000000002~9000000000000000102;3|9000000000000000900||70|1791706372",
    "orderId": null,
    "orderCommercialVersion": null,
    "correlationId": null,
    "allValid": false,
    "seats": [
      {
        "seatMapSelectionId": "QURULTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
        "status": "Valid",
        "reasonCodes": [],
        "travellerRef": "ADT-1",
        "orderTravellerId": null,
        "flightId": "1558420526869774453",
        "flightCapacityId": "1558420526869774478",
        "seatNumber": "3A",
        "serviceDefinitionId": "1025",
        "provisionId": "1026",
        "pricingId": "1031",
        "fulfillmentProviderKey": "Ancillary",
        "price": {
          "currencyId": 70,
          "baseAmount": 1500000,
          "taxes": [],
          "fees": [],
          "total": 1500000
        }
      },
      {
        "seatMapSelectionId": "Q0hELTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDFB",
        "status": "Stale",
        "reasonCodes": [
          "SeatSelectionNotOffered"
        ],
        "travellerRef": null,
        "orderTravellerId": null,
        "flightId": null,
        "flightCapacityId": null,
        "seatNumber": null,
        "serviceDefinitionId": null,
        "provisionId": null,
        "pricingId": null,
        "fulfillmentProviderKey": null,
        "price": null
      }
    ]
  },
  "errors": null
}
```


### Negative for the consumer: Order-scoped service (Valid in Ancillary, not representable in Ordering)

Source: `C5_10_an_order_scoped_service_resolves_valid_but_the_ordering_simulator_refuses_it_because_ordering_has_no_traveller_less_service`, call 4 (`ordering->ancillary`), HTTP 200.

Request `POST /Service/v1/Ancillaries/Selections/Resolve`:
```json
{
  "orderId": 7001,
  "expectedOrderCommercialVersion": 1,
  "pricedSelectionRefs": [
    "eyJhbGciOiJIUzI1NiIsInR5...<JWS>"
  ],
  "correlationId": "order-scope"
}
```

Response:
```json
{
  "data": {
    "orderId": "7001",
    "orderCommercialVersion": 1,
    "correlationId": "order-scope",
    "allValid": true,
    "results": [
      {
        "pricedSelectionRef": "eyJhbGciOiJIUzI1NiIsInR5...<JWS>",
        "status": "Valid",
        "reasonCodes": [],
        "service": {
          "serviceDefinitionRef": "WIFI_FULL_FLIGHT",
          "definitionVersionId": "1012",
          "provisionId": "1019",
          "pricingRevisionId": "1022",
          "rfic": "E",
          "rfisc": "WF1",
          "ssrCode": null,
          "coverageScope": "Order",
          "orderTravellerIds": [],
          "travellerRefs": [],
          "flightIds": [
            "1558420526869774453",
            "1550116065952923910"
          ],
          "portionRefs": [
            "B1",
            "B2"
          ],
          "selectedOptions": [
            {
              "fieldName": "planCode",
              "value": "WIFI_FULL_FLIGHT",
              "optionRef": "Ia6WuvHPSSjie0r4nB1u6lGdZojHWHGgxUu3kdoenY0"
            }
          ],
          "price": {
            "origin": "Filed",
            "pricingUnit": "PerItem",
            "currencyId": 70,
            "filedCurrencyId": 70,
            "quantity": 1,
            "chargeableWeightKg": null,
            "baseAmount": 900000,
            "addedTaxes": [],
            "includedTaxes": [],
            "unitFees": [],
            "sharedFees": [],
            "unitTotal": 900000,
            "lineTotal": 900000,
            "fx": {
              "rateOfExchangeTableId": "70",
              "fromCurrencyId": 70,
              "toCurrencyId": 70,
              "rate": 1,
              "decimalPlaces": 0
            },
            "isTotalComplete": true,
            "reasonCodes": []
          },
          "requiresCheckAtReserve": false,
          "variantCode": "A24",
          "serviceTypeCode": "F",
          "serviceSubCode": "WF1",
          "bookingMethod": "NoBookingProcessRequired",
          "bookingRequired": false,
          "documentRouting": "Emd",
          "documentType": "EmdStandalone",
          "documentRequired": true,
          "confirmationRequirement": "Immediate",
          "fulfillmentProviderKey": "Ancillary",
          "pricingRateId": "1023",
          "selection": {
            "quantity": null,
            "packageProductRef": null,
            "bagRef": null,
            "measuredWeightKg": null,
            "dimensions": null,
            "equipmentKind": null,
            "weightKg": null,
            "seatNumber": null,
            "exitRowTermsAccepted": null,
            "purpose": null,
            "targetCabinId": null,
            "quoteRef": null,
            "mealCode": null,
            "menuItemRef": null,
            "animalType": null,
            "otherAnimalCode": null,
            "animalAgeWeeks": null,
            "combinedWeightKg": null,
            "carrierDimensions": null,
            "documentAcknowledgements": null,
            "sizeBracket": null,
            "assistanceSsrCode": null,
            "equipmentCode": null,
            "evidenceDocumentRefs": null,
            "oxygenUnits": null,
            "infantRef": null,
            "guardianRef": null,
            "childRef": null,
            "guardianHandoffContactProvided": false,
            "guardianPickupContactProvided": false,
            "airportId": null,
            "facilityRef": null,
            "timeWithOffset": null,
            "guestCount": null,
            "optIn": null,
            "planCode": "WIFI_FULL_FLIGHT",
            "deviceCount": 1
          }
        }
      }
    ]
  },
  "errors": null
}
```

### Test Ordering Add request and result with a bag reference and two seat ids (simulator only)

Source: `C4_F4_a_paid_and_a_zero_rated_seat_are_assigned_to_the_right_passenger_flight_and_capacity_beside_a_bag`, call 7 (`test->ordering`), HTTP 201.

Request `POST /__e2e/orders/7001/ancillaries`:
```json
{
  "expectedOrderCommercialVersion": 1,
  "pricedSelectionRefs": [
    "eyJhbGciOiJIUzI1NiIsInR5...<JWS>"
  ],
  "seats": [
    {
      "id": "QURULTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDMxfDNB",
      "exitRowTermsAccepted": null
    },
    {
      "id": "Q0hELTF8MTU1ODQyMDUyNjg2OTc3NDQ1M3wxMDQwfDNC",
      "exitRowTermsAccepted": null
    }
  ],
  "idempotencyKey": "f4-seats"
}
```

Response:
```json
{
  "orderId": "7001",
  "commercialVersion": 2,
  "addedServiceIds": [
    "50005"
  ],
  "addedSeatIds": [
    "50006",
    "50007"
  ],
  "replayed": false
}
```
