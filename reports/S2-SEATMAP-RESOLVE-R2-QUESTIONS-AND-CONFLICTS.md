# SeatMap Resolve R2 — id-only input, nested output, enums with value and title — questions

Base: `k8s-stg@ca4242b` plus the uncommitted SeatMap Resolve change set of 2026-10-21. No R2 code written.

> **Superseded 2026-10-22 by the Owner in chat:** the id stays `{travellerRef}|{flightId}|{pricingId}|{SEATNUMBER}`; the output carries nothing from the offer or the order, only the definition, provision and pricing the id names plus seat number, flight id and traveller ref, so Ordering can add the line, its amount and its reservation instruction, as it does with AirAvail. Q1–Q10 below are void. What was built: `S2-SEATMAP-RESOLVE-CONFORMANCE.md`, section R2.

## Owner order (2026-10-22, chat)

- The input and output must be specific to this endpoint; irrelevant fields go.
- The input needs no `offerId` and no `orderId`, exactly like AirAvail `POST Service/v1/FlightOffers/Details`: every fact the output depends on from the offer or the order must be inside the id itself.
- The output carries every field of the provision and the service definition that Ordering needs.
- Every enum is returned with its exact value and title.
- The output is not flat: every multi-level field is an object.

## 1. How the sibling and the current code do it

| Topic | Evidence |
|---|---|
| AirAvail route | `AirAvail/.../AeroTech.AirOffer.RestApi/V1/AirOffers/Controllers/Service2Service/ServiceController.cs:13`: `POST Service/v1/FlightOffers/Details`, anonymous, `BaseResult<ServiceFlightOfferDetailDto>`. |
| AirAvail input | `ServiceGetFlightOffersDetailRequest.cs`: one field, `OfferId`. Its comment: "the offer id is produced by a prior price call and already carries the commercial context (channel, customer, country, currency, sales date), so no commercial parameters are needed here." |
| AirAvail id | `AeroTech.AirOffer.Services/Offers/JourneyOfferIdCodec.cs`: **plain text**, not base64, not signed: `travellers;bounds;commercial`, bounds joined by `_`, capacity/fare rows by `~` and `-`, commercial `channel|customer|country|currency|salesUnix`. Example: `1|1|0;1558420526873968667~1469435200688619520_1550116065952924138~1469436768104218624;3|1551571415767973888||70|1792613830` (133 chars). |
| AirAvail output | `ServiceFlightOfferDetailDto.cs`: nested objects (`AirTransports[] → Flights[] → Legs[] → Stop`, `Tickets[] → Coupons[] → Pricings[]`), the offer id echoed at the root, longs written as strings. Its enums (`OfferBoundDirection`, `StopType`, `ServiceOfferPricingCategory`) are raw enums, not value+title — your order deviates from the sibling here, on purpose. |
| Enum shape in Ancillary | `src/AeroTech.Ancillary.Query/_Shared/Enums/EnumValueDto.cs`: `EnumValueDto(int Value, string Name, string Title)`, used by every Backoffice DTO. |
| Current seat id | `src/AeroTech.Ancillary.Query/Shopping/SeatMaps/SeatMapSelectionId.cs`: base64url of `travellerRef|flightId|pricingId|SEATNUMBER`; carries nothing from the offer or the order, so Resolve needs `offerId`/`orderId` beside it. Max length 256. |
| Current Resolve input | `offerId?`, `orderId?`, `expectedOrderCommercialVersion?`, `seats[{ id, exitRowTermsAccepted? }]`, `correlationId?`. |
| Current Resolve output | root `offerId, orderId, orderCommercialVersion, correlationId, allValid, seats[]`; each seat flat: `seatMapSelectionId, status (string), reasonCodes, travellerRef, orderTravellerId, flightId, flightCapacityId, seatNumber, serviceDefinitionId, provisionId, pricingId, fulfillmentProviderKey, price{currencyId, baseAmount, taxes[{code,amount}], fees[{code,amount}], total}`. |
| What Ordering consumes today | `AeroTech.Ordering.Final@cd50a2a` (read only): `Domain/OrderAggregate/Entities/OrderSeatService.cs` stores `TravellerId, SegmentId, AssociatedAirServiceId, SeatNumber, FulfillmentProviderKey` (+ base `OrderItemId, ServiceType=SeatAssignment, CommercialStatus`); `Order.Create.cs:370-407` finds the traveller by index, the segment by bound, and takes the provider key from the air service. Ordering has no paid-seat or ancillary pricing model yet, so "what Ordering needs" beyond these is a design choice, not something its code already reads. |
| What Ancillary already hands Ordering | `Selections/Resolve` `ServiceResolvedServiceDto.cs`: identity (`serviceDefinitionRef, definitionVersionId, provisionId, pricingRevisionId, pricingRateId`), codes (`rfic, rfisc, ssrCode, variantCode, serviceTypeCode, serviceSubCode`), coverage, exact price, booking and document (`requiresCheckAtReserve, bookingMethod, bookingRequired, documentRouting, documentType, documentRequired, confirmationRequirement`), `fulfillmentProviderKey`. It is flat and its enums are plain strings. |

## 2. Questions

**Q1. What the seat id carries (offer path).** The seat map built from a priced offer id gives each selectable seat:
`{offerId}` + `{travellerRef}|{flightId}|{pricingId}|{SEATNUMBER}`.
Resolve decodes the offer id, reads the offer from AirAvail Details (the call the seat map already makes), rebuilds that flight's map and re-prices the seat. Channel, customer, country, currency and sales date come from the offer id's commercial section, as in AirAvail. Nothing else is sent.
- Recommendation: yes.
- Answer:

**Q2. What the seat id carries (order path).** A seat map built from an `orderId` depends on the order (traveller links, existing services, ticket/coupon state, commercial version). Proposed id: `{orderId}|{orderCommercialVersion}` + `{travellerRef}|{flightId}|{pricingId}|{SEATNUMBER}`. Resolve reads the order through `IOrderCommercialContextReader` (production: still `NotConnected…`, 503 / 16815), answers `Stale OrderVersionChanged` when the version moved, and returns the `orderTravellerId`.
- (a) as proposed; (b) drop the order path: seat ids exist only on offer seat maps.
- Recommendation: (a), because seats added after booking are built on the order.
- Answer:

**Q3. Id text format.** AirAvail ids are plain text. Ours is base64url today.
- (a) plain text like AirAvail: `{context}#{travellerRef}|{flightId}|{pricingId}|{SEATNUMBER}` with a separator the offer id never uses (`#`); 172 chars for yesterday's seat 15D on the offer above;
- (b) keep base64url of the same text; 232 chars for the same seat.
- Recommendation: (a), the sibling shape; the maximum accepted length rises from 256 to 1024 either way.
- Answer:

**Q4. Input shape.** AirAvail takes one id. Proposed: `{ "ids": ["…", "…"] }`, 1–50 ids, unique, nothing else (`offerId`, `orderId`, `expectedOrderCommercialVersion`, `correlationId` removed).
- (a) `{ ids: [] }`; (b) one id per call `{ id }` exactly like AirAvail.
- Recommendation: (a), because one booking adds one seat per traveller per flight and the "same seat twice" check needs them together.
- Answer:

**Q5. `exitRowTermsAccepted`.** It is passenger input, not a fact of the offer or the order, so it cannot be inside the id. Today it is the only extra input and caused yesterday's `SelectionFieldNotAllowed` on a normal seat.
- (a) remove it: Resolve checks eligibility only and returns `seat.exitRow = true` plus `serviceDefinition.seat.requiresExitRowEligibility = true`; the terms are collected and kept by the seller/Ordering (my recollection of Amadeus Seat Map Display: exit rows are marked by characteristic `E`, no acceptance field in the seat map API — not verified against Amadeus documentation);
- (b) keep it per id, accepted only on exit-row seats and ignored (not rejected) elsewhere.
- Recommendation: (a).
- Answer:

**Q6. Output root.** Proposed: `{ "allValid": bool, "seats": [ … ] }`, one entry per id in request order. Removed from the root: `offerId`, `orderId`, `orderCommercialVersion`, `correlationId` (the ids carry them).
- Recommendation: yes.
- Answer:

**Q7. Output per seat (nested; every enum as `{ value, name, title }`).** Proposed:
```
{
  "id": "…",
  "status": { value, name, title },                      // Valid | Stale | Rejected
  "reasonCodes": [ … ],                                    // blocking codes only (see Q9)
  "traveller": { "travellerRef", "orderTravellerId"?, "passengerTypeCode": {enum} },
  "flight": { "flightId", "flightCapacityId", "flightNumber", "departureDateTime", "originAirportId", "destinationAirportId", "cabinClassId" },
  "seat": { "seatNumber", "characteristicCodes": [ … ], "exitRow": bool },
  "serviceDefinition": {
    "id", "ref", "version", "commercialName", "variantCode", "profile": {enum},
    "serviceTypeCode", "serviceSubCode", "subCodeSource": {enum}, "groupCode", "subGroupCode",
    "pricingUnit": {enum}, "supplierId",
    "booking": { "method": {enum}, "ssrCode", "ssimCode", "confirmationRequirement": {enum} },
    "document": { "type": {enum}, "routing": {enum}, "rfic", "rfisc" },
    "seat": { "purpose": {enum}, "requiresExitRowEligibility", "requiresAdjacentSeat", "extraSeatPurpose": {enum}?, "extraOccupiedSeatCount"?, "requiresExternalTicketAction" }
  },
  "provision": {
    "id", "sequence", "coverageScope": {enum},
    "quantity": { "unit": {enum}, "min", "max" },
    "disposition": {enum}, "priceOrigin": {enum}, "purchaseStage": {enum},
    "documentRequired", "bookingRequired", "mustCheckAvailability",
    "refund": { "reissueRefund": {enum}, "formOfRefund": {enum}? },
    "commissionable", "interlineSettlement",
    "fulfillmentProviderKey"
  },
  "pricing": {
    "id", "version", "rateId", "pricingUnit": {enum},
    "currencyId", "baseAmount",
    "taxes": [ { "code", "amount", "treatment": {enum} } ],
    "fees":  [ { "code", "amount", "applicationUnit": {enum} } ],
    "total"
  }
}
```
Not included: the provision's eligibility rules (passenger, sales, geography, flight, fare, dates, advance purchase, seat characteristics); Ancillary has already applied them and Ordering cannot act on them.
- Recommendation: this list.
- Answer:

**Q8. Provision cancellation rules.** `ProvisionCancellationRules` exist (refund/penalty windows). Ordering will need them for cancel/refund, which is a later stage.
- (a) include now as `provision.cancellation[]`; (b) leave for the stage that cancels.
- Recommendation: (b).
- Answer:

**Q9. Reason codes on a rejected seat.** Today a rejected seat also lists the candidate's informational codes (yesterday: `AvailabilityMustBeChecked` beside the real `SelectionFieldNotAllowed`).
- Recommendation: return only the codes that caused `Stale`/`Rejected`.
- Answer:

**Q10. `Selections/Resolve` (ancillaries).** It has the same pattern you are removing here: input `orderId + expectedOrderCommercialVersion + pricedSelectionRefs + correlationId` (the signed ref already carries order id and version), flat output, enums as plain strings.
- (a) give it the same R2 treatment in this round; (b) only SeatMaps/Resolve now.
- Recommendation: (b) now, (a) as the next step, so this round stays small.
- Answer:
