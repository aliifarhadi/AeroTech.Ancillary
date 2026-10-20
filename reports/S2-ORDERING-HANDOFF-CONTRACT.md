# S2 → Ordering handoff contract (implementation-independent)

**The test-only Ordering simulator is not a production Ordering contract nor an implementation of Order persistence.**

| Item | Value |
|---|---|
| Ancillary base | `k8s-stg@f8e29ae`, plus one additive field in the working tree (section 1.3) |
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

**`results[]`** carries `pricedSelectionRef`, `status`, `reasonCodes[]`, `service?` and `includedSeat?`.
- `status` is one of `Valid | Stale | Expired | NeedsQuote | NeedsVerification | Unavailable | Rejected`.
- Only `Valid` carries `service` or `includedSeat`.

**`service`** (`ServiceResolvedServiceDto`):

| Group | Fields |
|---|---|
| Identity | `kind` (`Ancillary`/`SeatMap`), `serviceDefinitionRef`, `definitionVersionId`, `provisionId`, `pricingRevisionId?`, `pricingRateId?` |
| Codes | `rfic?`, `rfisc?`, `ssrCode?`, `variantCode`, `serviceTypeCode`, `serviceSubCode` |
| Coverage | `coverageScope` (`Sector`/`Portion`/`Journey`/`Order`), `orderTravellerIds[]` (long as string), `travellerRefs[]`, `flightIds[]`, `portionRefs[]` |
| Seat | `seatNumber?`, `flightCapacityId?` (paid seat only) |
| Options | `selectedOptions[]` |
| Price (`OfferExactPriceDto`) | `origin`, `pricingUnit`, `currencyId`, `filedCurrencyId`, `quantity`, `chargeableWeightKg`, `baseAmount`, `addedTaxes[]`, `includedTaxes[]`, `unitFees[]`, `sharedFees[]`, `unitTotal`, `lineTotal`, `fx`, `isTotalComplete`, `reasonCodes[]` |
| Booking and document | `requiresCheckAtReserve`, `bookingMethod` (`Ssr`/`AuxiliarySegment`/`DisplayPriceContactCarrierForBooking`/`NoBookingProcessRequired`/`PerServiceRecord`), `bookingRequired`, `documentRouting` (`NoAncillaryDocument`/`Emd`/`TicketOrExchange`), `documentType` (`None`/`EmdAssociated`/`EmdStandalone`), `documentRequired`, `confirmationRequirement` (`Immediate`/`SubjectToConfirmation`) |
| Provider (**new, additive**) | `fulfillmentProviderKey?` |
| Selection | `selection` (all 37 `AncillarySelection` fields, exactly as accepted) |

`fulfillmentProviderKey` is new in this change set:
- It is taken from the candidate's existing `Confirmation.FulfillmentProviderKey`, which comes from the provision's `Fulfillment` definition.
- It was added because Ordering requires a provider key on every `OrderService` (`Ordering.Final Domain/OrderAggregate/Entities/OrderService.cs:45`, max length 64 in `OrderServiceConfiguration.cs:16`).
- Its production code is `ServiceResolvedServiceDto.cs` and `ServiceSelectionResolutionMapper.cs`, one line each.

**`includedSeat`** (`ServiceIncludedSeatDto`): `orderTravellerId?`, `travellerRef`, `flightId`, `flightCapacityId`, `seatNumber`, `currencyId`, `fee` (0).
- It is a free seat with no commercial category, under the Owner seat rule.
- It carries no provider key. Ordering assigns a seat the provider of its associated air service, as `Order.Create.cs:404` does today.

**Reason codes a consumer will see:**

| Situation | Status | Reason code |
|---|---|---|
| Order version moved | Stale | `OrderVersionChanged` |
| Order source changed (fingerprint) | Stale | `SourceVersionChanged` |
| Recomputed facts differ | Stale | `SignedFactsChanged` |
| Same facts, new amount | Stale | `PriceChanged` |
| Item gone | Stale | `OfferItemNotOffered` |
| Seat gone or taken | Stale | `SeatSelectionNotOffered` |
| Bad signature | Rejected | `PricedSelectionNotAuthentic` |
| Ref signed for another Order | Rejected | `PricedSelectionForAnotherOrder` |
| Traveller without a unique positive `OrderTravellerId` | NeedsVerification | `OrderTravellerNotLinked` |
| Past the token expiry | Expired | — |
| Ordering source not reachable | whole request fails: HTTP 503, code 16815 | — |

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
3. Call `POST /Service/v1/Ancillaries/Selections/Resolve` with the refs and the expected version.
4. Accept only HTTP 200 with `allValid=true` and the same `orderId` and version.
5. For every result, require exactly one of `service` or `includedSeat`, then validate:
   - Beneficiary: one `orderTravellerIds` entry that belongs to the Order and matches `travellerRefs`.
   - Flights: every `flightIds` entry belongs to the Order.
   - A seat matches the traveller's air service `FlightCapacityId` and is not already assigned on that flight.
   - Price: `price.currencyId` equals the Order currency, `lineTotal` is present and `isTotalComplete=true`.
   - Provider: a service has a non-empty `fulfillmentProviderKey`. Never fall back to a constant.
6. Apply all lines in one transaction: OrderService or seat assignment, plus charge lines. Increment `CommercialVersion` exactly once.
   - Keep these exactly as returned: price, quantity, traveller, flights, seat number, selection, provider key, and the definition, provision and pricing ids.
   - Any failure inserts nothing.
7. Do not capture money, issue an EMD or ticket, or hold inventory in this step. Those belong to later stages.

**Consumer gaps found by the E2E (Ordering-side, not Ancillary):**
- An Order-scoped service (`coverageScope=Order`, `orderTravellerIds=[]`) resolves `Valid`, but `OrderService.TravellerId` is a non-nullable long, so it cannot be stored. Test C5_10 proves the Add is refused. This is an external consumer integration issue; no fake traveller is invented.
- Ordering creates `OrderSeatService` only at order creation (`Order.Create.cs:370-407`). Adding an included or paid seat later needs a new Ordering operation.
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
          "kind": "Ancillary",
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
          "seatNumber": null,
          "flightCapacityId": null,
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
            "…": "other AncillarySelection fields are sent as null/false"
          }
        },
        "includedSeat": null
      }
    ]
  },
  "errors": null
}
```

### Seats, positive (paid A07 3A for ADT-1 + included 3B for CHD-1)

Source: `C4_F4_a_paid_and_an_included_seat_are_assigned_to_the_right_passenger_flight_and_capacity`, call 4 (`ordering->ancillary`), HTTP 200.

Request `POST /Service/v1/Ancillaries/Selections/Resolve`:
```json
{
  "orderId": 7001,
  "expectedOrderCommercialVersion": 1,
  "pricedSelectionRefs": [
    "eyJhbGciOiJIUzI1NiIsInR5...<JWS>",
    "eyJhbGciOiJIUzI1NiIsInR5...<JWS>"
  ],
  "correlationId": "f4-seats"
}
```

Response:
```json
{
  "data": {
    "orderId": "7001",
    "orderCommercialVersion": 1,
    "correlationId": "f4-seats",
    "allValid": true,
    "results": [
      {
        "pricedSelectionRef": "eyJhbGciOiJIUzI1NiIsInR5...<JWS>",
        "status": "Valid",
        "reasonCodes": [],
        "service": {
          "kind": "SeatMap",
          "serviceDefinitionRef": "SEAT_STANDARD",
          "definitionVersionId": "1025",
          "provisionId": "1026",
          "pricingRevisionId": "1031",
          "rfic": "A",
          "rfisc": "ST1",
          "ssrCode": "RQST",
          "coverageScope": "Sector",
          "orderTravellerIds": [
            "1001"
          ],
          "travellerRefs": [
            "ADT-1"
          ],
          "flightIds": [
            "1558420526869774453"
          ],
          "portionRefs": [
            "B1"
          ],
          "seatNumber": "3A",
          "flightCapacityId": "1558420526869774478",
          "selectedOptions": [],
          "price": {
            "origin": "Filed",
            "pricingUnit": "PerSeat",
            "currencyId": 70,
            "filedCurrencyId": 70,
            "quantity": 1,
            "chargeableWeightKg": null,
            "baseAmount": 1500000,
            "addedTaxes": [],
            "includedTaxes": [],
            "unitFees": [],
            "sharedFees": [],
            "unitTotal": 1500000,
            "lineTotal": 1500000,
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
          "variantCode": "A07",
          "serviceTypeCode": "F",
          "serviceSubCode": "ST1",
          "bookingMethod": "Ssr",
          "bookingRequired": false,
          "documentRouting": "Emd",
          "documentType": "EmdAssociated",
          "documentRequired": true,
          "confirmationRequirement": "Immediate",
          "fulfillmentProviderKey": "Ancillary",
          "pricingRateId": "1032",
          "selection": {
            "seatNumber": "3A",
            "…": "other AncillarySelection fields are sent as null/false"
          }
        },
        "includedSeat": null
      },
      {
        "pricedSelectionRef": "eyJhbGciOiJIUzI1NiIsInR5...<JWS>",
        "status": "Valid",
        "reasonCodes": [],
        "service": null,
        "includedSeat": {
          "orderTravellerId": "1002",
          "travellerRef": "CHD-1",
          "flightId": "1558420526869774453",
          "flightCapacityId": "1558420526869774478",
          "seatNumber": "3B",
          "currencyId": 70,
          "fee": 0
        }
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
        "service": null,
        "includedSeat": null
      }
    ]
  },
  "errors": null
}
```

### Negative: seat taken after signing

Source: `C5_07_a_seat_taken_after_signing_is_stale_at_resolve_and_the_whole_seat_basket_inserts_nothing`, call 4 (`ordering->ancillary`), HTTP 200.

Request `POST /Service/v1/Ancillaries/Selections/Resolve`:
```json
{
  "orderId": 7001,
  "expectedOrderCommercialVersion": 1,
  "pricedSelectionRefs": [
    "eyJhbGciOiJIUzI1NiIsInR5...<JWS>",
    "eyJhbGciOiJIUzI1NiIsInR5...<JWS>"
  ],
  "correlationId": "taken"
}
```

Response:
```json
{
  "data": {
    "orderId": "7001",
    "orderCommercialVersion": 1,
    "correlationId": "taken",
    "allValid": false,
    "results": [
      {
        "pricedSelectionRef": "eyJhbGciOiJIUzI1NiIsInR5...<JWS>",
        "status": "Stale",
        "reasonCodes": [
          "SeatSelectionNotOffered"
        ],
        "service": null,
        "includedSeat": null
      },
      {
        "pricedSelectionRef": "eyJhbGciOiJIUzI1NiIsInR5...<JWS>",
        "status": "Valid",
        "reasonCodes": [],
        "service": null,
        "includedSeat": {
          "orderTravellerId": "1002",
          "travellerRef": "CHD-1",
          "flightId": "1558420526869774453",
          "flightCapacityId": "1558420526869774478",
          "seatNumber": "3B",
          "currencyId": 70,
          "fee": 0
        }
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
          "kind": "Ancillary",
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
          "seatNumber": null,
          "flightCapacityId": null,
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
            "planCode": "WIFI_FULL_FLIGHT",
            "deviceCount": 1,
            "…": "other AncillarySelection fields are sent as null/false"
          }
        },
        "includedSeat": null
      }
    ]
  },
  "errors": null
}
```

### Test Ordering Add result (simulator only)

Source: `C4_F2_an_existing_pre_issue_order_adds_two_pieces_for_one_traveller_with_its_exact_price_and_one_version_step`, call 5 (`test->ordering`), HTTP 201.

Request `POST /__e2e/orders/7001/ancillaries`:
```json
{
  "expectedOrderCommercialVersion": 1,
  "pricedSelectionRefs": [
    "eyJhbGciOiJIUzI1NiIsInR5...<JWS>"
  ],
  "idempotencyKey": "f2-add"
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
  "addedSeatIds": [],
  "replayed": false
}
```
