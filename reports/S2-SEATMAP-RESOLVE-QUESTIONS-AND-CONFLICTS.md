# SeatMap recoverable id + `Service/v1/Ancillaries/SeatMaps/Resolve` — questions and conflicts

Base: `k8s-stg@ca4242b` (local = origin, tree clean). No code written yet.

## Owner decisions already given (2026-10-21, chat)

- The seat id becomes recoverable from `travellerRef + flightRef + AncillaryPricing id + seatNumber`.
- A seat is free only when it has a pricing with amount 0; otherwise it is not selectable.
- New `POST Service/v1/Ancillaries/SeatMaps/Resolve`, input = list of ids plus `orderId` or `offerId`; stays beside the existing Resolve.
- The existing `Selections/Resolve` no longer needs to answer SeatMap lines.
- `provisionRef` / `pricingRef` / `price.total` are numbers.
- Changing only the `id` value is not a breach of the frozen SeatMapsDto.

## 1. How the current code and the sibling do it

| Topic | Evidence |
|---|---|
| Current seat id | `src/AeroTech.Ancillary.Query/Shopping/Common/OfferItemRef.cs:36-55` `OfSeat`: SHA-256 over channel, ownerAirline, POS, offerId, travellerRef, flightRef, seatNumber, definitionVersionId, provisionId, pricingRevisionId. Built in `SeatMaps/Queries/GetSeatMap/GetSeatMapService.cs:113`; the batch finds it by rebuilding the map (`EvaluateBatchService.cs:101-108`). |
| Sibling id pattern | AirAvail `AeroTech.AirOffer.Services/Offers/JourneyOfferIdCodec.cs:10-26`: plain self-describing text joined with `_`, `;`, `|`; not signed, not encrypted; re-priced on every read. |
| Free seat today | Engine `AncillaryShoppingEngine.SeatMaps.cs:107-109`: a seat with no category is `Available` with no candidate (price 0), per the Owner ruling of 2026-10-14. A category whose provision is `PriceOrigin.Free` has no `AncillaryPricing` row (Free provisions carry no pricing). |
| Zero-amount rate | `Domain/AncillaryPricingAggregate/ValueObjects/Money.cs:28` allows `amount >= 0`, so a Paid provision with a 0 rate is authorable. |
| Seat lines in the batch | `EvaluateBatchService.cs` handles `SelectionLineKind.SeatMap` in 6 places; `ApiSelectionLineDto.Seat`, `ServiceResolvedSelectionDto.IncludedSeat`; 9 files reference the included seat (incl. the E2E suite). |
| Service routes today | `Service/v1/Ancillaries` (holds) and `Service/v1/Ancillaries/Selections` (Resolve), both anonymous. |

## 2. Questions

**Q1. Free seat = which authoring shape?** Today "free" is a provision with `PriceOrigin.Free` and no pricing. Your sentence "a pricing with amount 0" fits a `Paid` provision with a 0 rate.
- (a) Free seat = Paid provision + Active pricing whose rate is 0 (id carries that pricing id); a `Free`-origin provision is NOT selectable because it has no pricing id.
- (b) Free seat = `Free`-origin provision, no pricing; the id carries the provision id and an empty pricing part.
- Recommendation: (a), because the id must point at a pricing and the price 0 is then authored data like every other price.
- Answer: a

**Q2. Seats with no seat category at all** (today AVAILABLE at 0 by the 2026-10-14 ruling). New rule makes them `BLOCKED` and the `includedSeat` concept disappears. Confirm this overrides the 2026-10-14 ruling and that the SeatMaps route may show them as BLOCKED.
- Answer: Confirm

**Q3. Id format.** Self-describing readable text like AirAvail (`{travellerRef}|{flightId}|{pricingId}|{seatNumber}` base64url-encoded), not signed. Resolve re-validates everything, so tampering only yields `Rejected`.
- Recommendation: the AirAvail shape.
- Answer: the AirAvail shape

**Q4. `flightRef` in the id and output** = the platform `flightId` (long), not `"B1:F1"`. Confirm.
- Answer:Confirm

**Q5. Seat lines in EvaluateBatch.** Since the old Resolve no longer answers seats:
- (a) remove `kind: SeatMap`, `seatMapSelectionId`, `exitRowTermsAccepted` and the `seat` member from EvaluateBatch on the three surfaces (contract change, goldens regenerated), and remove `includedSeat` from the old Resolve;
- (b) keep them for preview only, but the old Resolve rejects a seat ref with `SeatSelectionNotOffered`.
- Recommendation: (a) one seat path only.
- Answer: a

**Q6. Exit-row terms (A08).** With the batch gone for seats, where does `exitRowTermsAccepted` travel? Option: a per-id object in the new Resolve input `{ id, exitRowTermsAccepted }` instead of a bare string list.
- Answer: a per-id object in the new Resolve input `{ id, exitRowTermsAccepted }` instead of a bare string list. See how Amadeus handles exitRow, follow it.

**Q7. Output per seat.** Proposed, all numbers typed: `seatMapSelectionId, status, reasonCodes[], travellerRef, orderTravellerId? (only with orderId), flightId, flightCapacityId, seatNumber, serviceDefinitionId, provisionId, pricingId, fulfillmentProviderKey, price { currencyId, total }`. Confirm or trim.
- Answer: Confirm

**Q8. Status set.** `Valid | Stale | Rejected` only (no token, so no `Expired`; `Stale` = seat taken, pricing no longer active, price changed, order version/source changed; `Rejected` = malformed id, seat or pricing not in the map, traveller not in the order). `allValid` = every seat Valid.
- Answer: Confirm

**Q9. Price shape.** Only `{ currencyId, total }` as you wrote, or also the taxes/fees lines Ordering will need for charge lines (`OfferExactPriceDto` has them)?
- Recommendation: `{ currencyId, baseAmount, taxes[], fees[], total }` all numeric.
- Answer: Confirm Recommendation

**Q10. Order-bound checks with `orderId`.** Same as today: version and source fingerprint, `OrderTravellerId` link required. With `offerId`: preview only, never `Valid`? Or `Valid` is allowed on an offer (no Order binding needed because this route is S2S)?
- Recommendation: with `offerId` the answer is a priced preview with status `Valid` but `orderTravellerId` null; Ordering decides.
- Answer: Confirm Recommendation
