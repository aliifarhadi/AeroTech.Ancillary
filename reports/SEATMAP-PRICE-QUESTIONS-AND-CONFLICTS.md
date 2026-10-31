# SeatMap Price: questions and conflicts

Owner request (2026-10-31): an endpoint like `/Price`, only for SeatMap. It takes the selected seats and answers exactly like the current `/Price`:

```json
{ "data": { "selectionId": "XXX", "expiresAtUtc": "2026-10-31T07:30:07+00:00" }, "errors": null }
```

Base: `k8s-stg@769229f` plus the uncommitted Price performance change. No code has been written for this request.

## 1. How it works today

| Topic | Evidence |
|---|---|
| Seat map | `POST {S}/v1/SeatMaps` and `POST {S}/v1/SeatMaps/Orders`. Every available seat carries one `travelerPricing[].id` per traveller. |
| Seat id | Unsigned base64url of `travellerRef\|flightId\|pricingId\|seatNumber` (`SeatMapSelectionId.cs`). It has no expiry and no binding to the offer or order. |
| Ambiguous seat | If two points of sale give different prices, the map marks the seat `BLOCKED` with no id (`GetSeatMapService.BuildAsync`). So every id has exactly one price. |
| `/Price` sibling | `POST {S}/v1/AncillaryOffers/Price` and `.../Orders/Price`. They reload the source, re-evaluate every line, then write one `SelectionSnapshot` with child lines and return `selectionId` + `expiresAtUtc` (`PriceAncillaryOffersService.cs`). |
| Seat lines in snapshots | None today. `AncillaryOffers/Resolve` refuses a Seat line (`SelectionResolver.cs`: `definition.Profile == AncillaryProfile.Seat`). |
| Seat Resolve | `POST Service/v1/Ancillaries/SeatMaps/Resolve {ids}` is a pure catalog lookup. It returns all rates of the pricing, not one final price, and no point of sale. |
| Seat Hold | `POST Service/v1/Ancillaries/SeatMaps/Holds` needs `pointOfSaleId`, `currencyId` and per seat `seatMapSelectionId`. |
| Snapshot line | `OfferItemId` varchar(1024), `PricingRateId`, `CoverageScope`, `TravellerRef`, `CoverageRef`, `SourceBinding`, `SalesBinding`, `Quantity`, `SelectedJson`. There is no point-of-sale column; `SalesBinding` is only a hash. |
| Consumer | Ordering.Final `stage6a-ancillary-commercial` (`07b7158`) calls only `v1/AncillaryOffers/Resolve` in its code (`AncillaryProvider.cs`). Its pack v2.0 docs 06 and 11 propose `POST Service/v1/Ancillaries/SeatMaps/ValidateForAcceptance` (AC-03) because seat Resolve gives no single final price, expiry or point of sale. |

## 2. Planned without a question

- **Validation:** like `/Price`, the source is reloaded and the seat map is rebuilt with `IGetSeatMapService.BuildAsync`. A seat passes only if the rebuilt map shows it `AVAILABLE` with the identical id. A taken seat or a changed price is refused and nothing is written. One Price therefore costs about one SeatMaps call.
- **Basket rules:** the same as seat Resolve: at most 50 ids, each id once, a seat of a flight once, one seat per traveller per flight.
- **Storage:** one `SelectionSnapshot`, reusing `CreateForOffer` / `CreateForOrder`. Each line holds `OfferItemId` = seat id, `PricingRateId` from the priced candidate, scope Sector, `TravellerRef`, `CoverageRef` = flight ref, `Quantity` 1, `SelectedJson` null, and the bindings of the context that priced the seat. The seat number stays inside the seat id.
- **Expiry:** now + `Shopping:SelectionMaxAge`.
- **Layering:** one per-surface command and handler, one shared Application service, a port implemented in Query, like `/Price`. Route pin grows by the new routes.
- **Not built:** `GET .../Price/{selectionId}` for seats. Seat Hold is unchanged.

## 3. Questions

### Q1. Routes and input

Proposal, same words as the `/Price` routes:

```text
POST {S}/v1/SeatMaps/Price          { "offerId": "...", "ids": ["<travelerPricing id>", "..."] }
POST {S}/v1/SeatMaps/Orders/Price   { "orderId": "...", "ids": ["<travelerPricing id>", "..."] }
```

`{S}` = Backoffice, Api, OtaPanel. `ids` matches the `{ids}` input of seat Resolve. The Backoffice body also keeps the optional `options` of its SeatMaps request, otherwise the rebuilt map can run under another point of sale than the map the seats came from.

Options: (a) as above; (b) `selections: [{ "offerItemId": "..." }]` like `/Price`.

Recommendation: (a). A seat line has no quantity or options, so a wrapper object adds nothing.

Answer: Owner, 2026-10-31: (a). With Q3 (c) the Backoffice body carries no `options`.

### Q2. Who reads the new selectionId

Today nothing can read a seat snapshot: `AncillaryOffers/Resolve` refuses Seat lines and seat Resolve takes raw ids.

Options:

- (a) `SeatMaps/Resolve` input becomes `{ selectionId }`, `{ ids }` is removed, and each seat answers its one final rate, `pointOfSaleId`, `expiresAtUtc` and the seat id needed by Seat Hold. This also covers what Ordering's AC-03 `ValidateForAcceptance` asks for, so AC-03 is not built.
- (b) Same as (a), but `{ ids }` stays next to `{ selectionId }`.
- (c) Build only Price now; `selectionId` has no reader yet and Ordering keeps raw ids.

Recommendation: (a), the same pattern you chose for `AncillaryOffers/Resolve` (one id in). Ordering's code does not call seat Resolve yet, so no running consumer breaks; its pack v2.0 text would need updating.

Answer: Owner, 2026-10-31: (a).

### Q3. Point of sale on the snapshot line

Seat Hold requires `pointOfSaleId`, and Ordering's doc 06 requires it to come from the priced quote. The snapshot keeps only a hash of it, so it would be lost at Price.

Options:

- (a) Add a nullable `PointOfSaleId bigint` column to `SelectionSnapshotLines`, filled only for seat lines. Needs one migration. The active database is DB-TEST, so the migration needs your OK to apply.
- (b) Keep it in `SelectedJson`. No migration, but it puts a fact in the JSON column that CLAUDE rule 3 keeps as a narrow exception.
- (c) The seat id carries it as a fifth part: `travellerRef|flightId|pricingId|seatNumber|pointOfSaleId`. The seat map already knows it (`SeatTravellerOffer.PointOfSaleId`). No migration, Resolve reads it from the id, and the Backoffice Price body needs no `options`, because a named point of sale is taken unchecked there today. Api and OtaPanel still derive it from the token and header at Price and must match the id. It changes the seat id shape the Owner fixed on 2026-10-22.

Price itself never needs a point-of-sale input on any surface. This question is only about keeping it for Seat Hold, which runs on the Service surface with no token or header and has `pointOfSaleId` as a required input.

Recommendation: (c), replacing the earlier recommendation (a).

Answer: Owner, 2026-10-31: (c).
