# Handoff to the Ordering agent: `GET /Service/v1/Orders/{orderId}/AncillaryContext`

From: AeroTech.Ancillary
To: the AeroTech.Ordering coding agent (repository `AeroTech.Ordering.Final`, branch `k8s-stg`, read at `cd50a2a`)
Date: 2026-10-27

## 1. Why this route exists

Ancillary will sell services (bags, meals, lounges, seats and so on) for an **existing order**. It needs the same facts for an order that it reads today from a priced flight offer. Ancillary calls this route, builds its shopping context from the answer, and prices on it. It never writes to Ordering and never calls Ordering for anything else in this flow.

## 2. The route

| Item | Rule |
|---|---|
| Route | `GET /Service/v1/Orders/{orderId}/AncillaryContext` (`orderId` is a `long`) |
| Surface | Service (service to service), tag `Service2Service`. **No authentication and no authorization policy**, like every Service route on the platform. |
| Placement | Follow your own pattern: a thin Service controller action, then a Query-side query, handler and DTO in your Query project, reading the order once (no tracking). |
| Side effects | None. Read only. No event, no outbox, no call to another service. |
| `200` | the platform envelope `{ "data": <AncillaryContextDto> }` |
| `404` | the order does not exist; use your existing "order not found" business error |
| Other errors | none expected |
| JSON | the platform default options of `AeroTech.Framework.Presentation`: camelCase, every `long` written as a string, every enum written by its **name**. Ancillary reads with the same options. |

## 3. Response fields

Every field below is read by Ancillary. Do not add fields Ancillary does not ask for.

### 3.1 Order header

| Field | Type | Source in Ordering | Rule |
|---|---|---|---|
| `orderId` | long | `Order.Id` | |
| `commercialVersion` | int | `Order.CommercialVersion` | Ancillary stores it with every price it gives, and refuses to read a price again after the version changed |
| `acceptsChanges` | bool | Ordering's own rule | `true` when Ordering allows new services to be added to this order now. Ordering owns this decision (§5). |
| `ticketedAt` | DateTimeOffset? | earliest `ElectronicTicket.IssuedAt` of the order's non-voided tickets | `null` while no ticket is issued |
| `sourceOfferId` | string | `Order.SourceOfferId` | |
| `currencyId` | int | `Order.CurrencyId` | prices are offered in this currency |
| `customerId` | long | `Order.CustomerId` | |
| `sales.channel` | `SalesChannel` (name) | `Order.SalesContext.Channel` | `BackOffice`, `IBE`, `PartnerAPI`, `AgencyPanel`, `GDS`, `System` |
| `sales.travelAgencyId` | long? | `Order.SalesContext.TravelAgencyId` | |

### 3.2 `travellers[]`

All travellers of the order whose `OrderTraveller.Status` is `Active`. `Transferred` travellers are left out.

| Field | Type | Source |
|---|---|---|
| `travellerId` | long | `OrderTraveller.Id` |
| `passengerTypeCode` | `AeroTech.Messages.AirPrice.Enums.PassengerTypeCode` (name) | `OrderTraveller.PassengerType` |
| `dateOfBirth` | DateOnly? | `TravellerProfileRevision.DateOfBirth` of `OrderTraveller.CurrentProfileRevisionId` |
| `infantParentTravellerId` | long? | `OrderTraveller.InfantParentTravellerId` |

### 3.3 `journeys[]`, their `segments[]` and the segments' `legs[]`

The sold itinerary, ordered by `sequence` at every level.

**Journey**

| Field | Type | Source |
|---|---|---|
| `boundId` | string | `OrderJourney.BoundId` |
| `sequence` | int | `OrderJourney.Sequence` |
| `originAirportId` | int | `OrderJourney.OriginAirportId` |
| `destinationAirportId` | int | `OrderJourney.DestinationAirportId` |
| `segments[]` | | below |

**Segment**

| Field | Type | Source |
|---|---|---|
| `segmentId` | long | `OrderSegment.Id` |
| `sequence` | int | `OrderSegment.Sequence` |
| `flightId` | long | `OrderSegment.FlightId` |
| `flightVersion` | int | `OrderSegment.FlightVersion` |
| `flightNumber` | string? | `OrderSegment.FlightNumber` |
| `originAirportId` | int | `OrderSegment.OriginAirportId` |
| `originAirportTerminalId` | int? | `OrderSegment.OriginAirportTerminalId` |
| `destinationAirportId` | int | `OrderSegment.DestinationAirportId` |
| `destinationAirportTerminalId` | int? | `OrderSegment.DestinationAirportTerminalId` |
| `operatingAirlineId` | int | `OrderSegment.OperatingAirlineId` |
| `marketingAirlineId` | int | `OrderSegment.MarketingAirlineId` |
| `departureAt` | DateTimeOffset | `OrderSegment.SoldDeparture` |
| `arrivalAt` | DateTimeOffset | `OrderSegment.SoldArrival` |
| `aircraftId` | int? | `OrderSegment.AircraftId` |
| `legs[]` | | below |

**Leg**

| Field | Type | Source |
|---|---|---|
| `legId` | long | `OrderSegmentLeg.LegId` |
| `sequence` | int | `OrderSegmentLeg.Sequence` |
| `originAirportId` | int | `OrderSegmentLeg.OriginAirportId` |
| `destinationAirportId` | int | `OrderSegmentLeg.DestinationAirportId` |
| `departureAt` | DateTimeOffset | `OrderSegmentLeg.DepartureDateTime` |
| `arrivalAt` | DateTimeOffset | `OrderSegmentLeg.ArrivalDateTime` |

### 3.4 `airServices[]`

One row per traveller and segment. This is `OrderService` with `ServiceType = AirTransportation`, joined with its `OrderAirTransportService`. Return **only** rows whose `CommercialStatus` is `Active`; replaced, cancelled and transferred rows are left out.

| Field | Type | Source |
|---|---|---|
| `orderServiceId` | long | `OrderService.Id` |
| `travellerId` | long | `OrderService.TravellerId` |
| `segmentId` | long | `OrderAirTransportService.SegmentId` |
| `flightCapacityId` | long | `OrderAirTransportService.FlightCapacityId` |
| `airFareId` | long? | `OrderAirTransportService.AirFareId` |
| `fareBasis` | string? | `OrderAirTransportService.FareBasis` |
| `fareFamily` | string? | `OrderAirTransportService.FareFamily` |
| `fareType` | `AeroTech.Messages.AirPrice.Enums.AirFareType` (name)? | `OrderAirTransportService.FareType` |
| `bookingClass` | string? | `OrderAirTransportService.BookingClass` |
| `rbdId` | long? | `OrderAirTransportService.RbdId` |
| `cabinClassId` | long? | `OrderAirTransportService.CabinClassId` |
| `checkedBaggageAllowance` | `{pieces:int, weight:decimal, unit:WeightUnit}`? | `OrderAirTransportService.CheckedBaggageAllowance` |
| `cabinBaggageAllowance` | same shape? | `OrderAirTransportService.CabinBaggageAllowance` |
| `isRefundable` | bool | `OrderAirTransportService.IsRefundable` |
| `isChangeable` | bool | `OrderAirTransportService.IsChangeable` |
| `isUpgradable` | bool | `OrderAirTransportService.IsUpgradable` |
| `couponOpenForUse` | bool? | from the current `TicketCoupon` of this order service (`CurrentOrderServiceId`): `true` when the coupon is open for use, `false` for every other coupon state, `null` when no ticket coupon exists |

### 3.5 `ancillaryServices[]`

The ancillary services already stored on the order. Return an **empty array** until Ordering stores ancillary services. When it does, one row per stored service:

| Field | Type | Meaning |
|---|---|---|
| `orderServiceId` | long | Ordering's id of the service |
| `travellerId` | long? | `null` for a service bought for the whole order |
| `serviceDefinitionRef` | string | the `serviceDefinitionRef` Ancillary returned for the product (Resolve output) |
| `segmentIds` | long[] | the segments the service covers; empty for a whole-order service |
| `quantity` | int | |
| `commercialState` | `Active` / `Cancelled` / `Refunded` | |

## 4. Example

```json
{
  "data": {
    "orderId": "1564500000000000001",
    "commercialVersion": 2,
    "acceptsChanges": true,
    "ticketedAt": null,
    "sourceOfferId": "1|1|1;…;3|1551571415767973888||70|1792915276",
    "currencyId": 70,
    "customerId": "1551571415767973888",
    "sales": { "channel": "AgencyPanel", "travelAgencyId": "1551571720488353792" },
    "travellers": [
      { "travellerId": "1564500000000000101", "passengerTypeCode": "ADT", "dateOfBirth": "1990-05-01", "infantParentTravellerId": null }
    ],
    "journeys": [
      {
        "boundId": "1", "sequence": 1, "originAirportId": 10, "destinationAirportId": 20,
        "segments": [
          {
            "segmentId": "1564500000000000201", "sequence": 1,
            "flightId": "1550116065952923736", "flightVersion": 3, "flightNumber": "1124",
            "originAirportId": 10, "originAirportTerminalId": null, "destinationAirportId": 20, "destinationAirportTerminalId": 2,
            "operatingAirlineId": 1, "marketingAirlineId": 1,
            "departureAt": "2026-11-01T08:00:00+03:30", "arrivalAt": "2026-11-01T10:30:00+03:00", "aircraftId": 3,
            "legs": [
              { "legId": "77001", "sequence": 1, "originAirportId": 10, "destinationAirportId": 20, "departureAt": "2026-11-01T08:00:00+03:30", "arrivalAt": "2026-11-01T10:30:00+03:00" }
            ]
          }
        ]
      }
    ],
    "airServices": [
      {
        "orderServiceId": "1564500000000000301", "travellerId": "1564500000000000101", "segmentId": "1564500000000000201",
        "flightCapacityId": "1550116065952923999", "airFareId": "1525580011535335424",
        "fareBasis": "YLOW", "fareFamily": "Basic", "fareType": "Public", "bookingClass": "Y", "rbdId": "12", "cabinClassId": "1",
        "checkedBaggageAllowance": { "pieces": 1, "weight": 23, "unit": "Kg" },
        "cabinBaggageAllowance": { "pieces": 1, "weight": 7, "unit": "Kg" },
        "isRefundable": false, "isChangeable": true, "isUpgradable": true, "couponOpenForUse": null
      }
    ],
    "ancillaryServices": []
  }
}
```

The values are illustrative. Real ids come from the order.

## 5. `acceptsChanges`

Ordering decides. Ancillary only refuses to shop when it is `false`.

Suggestion (Ordering to confirm against its own state machine):

| Order status | `acceptsChanges` |
|---|---|
| `Created`, `Confirmed`, `Paid`, `Ticketed` | `true` |
| Every failed, unconfirmed, cancelled, expired or refunded status | `false` |
| A status in progress (`Paying`, `Ticketing`) | `false` |

## 6. Tests the Ordering agent should add

1. An unknown order answers 404.
2. A created order with two travellers, one an infant, two journeys and a connection with two legs, returns every field of §3 with the values stored in the order. The infant's `infantParentTravellerId` points to its adult.
3. A replaced or cancelled air service is not in `airServices`.
4. A ticketed order returns `ticketedAt` and `couponOpenForUse = true` for an open coupon and `false` for a used or voided coupon.
5. `acceptsChanges` follows §5 for each status Ordering keeps.
6. The JSON uses strings for long ids and enum names (one serializer test on the DTO).

## 7. Out of scope for this handoff

- Any call from Ordering to Ancillary.
- Storing ancillary services in the order (only the empty `ancillaryServices` array is needed now).
- Authorization on the Service surface.
