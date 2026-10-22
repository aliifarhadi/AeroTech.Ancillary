# Grouped Ancillary — Ordering handoff (no Ordering code in this task)

| Item | Value |
|---|---|
| Ancillary branch | `feat/ancillary-grouped-api-gate-a` on `49e9209` (Gate A = `f4b5145`), rest UNCOMMITTED |
| Scope | the facts Ordering must hold and the call order it must follow so that `POST Service/v1/Ancillaries/Service-Holds/Grouped` can leave `ORDERING_INTEGRATION_PENDING` |

## 1. The one dependency: accepted-selection binding on the order service

`Application/Orders/OrderServiceFacts` gained three trailing optional members. The production reader (`IOrderCommercialContextReader`, today `NotConnectedOrderCommercialContextReader`, 503 / 16815) must fill them for every ancillary service it reports:

| Member | Type | Source in Ordering |
|---|---|---|
| `AcceptedSelectionId` | `string?` (max 4096, base64url) | the exact `selectionId` string Ordering accepted from `ResolveGrouped` when it created the order service; never re-issued, never re-encoded |
| `AcceptedCurrencyId` | `int?` | the currency of the accepted `service.price` |
| `AcceptedTotal` | `decimal?` | the accepted `service.price.lineTotal` |

Without these three the grouped hold refuses every non-seat entry (422 / 16402 `…AcceptedSelectionId` or `…AcceptedTotal`). Seats never need them.

`OrderService` in Ordering.Final (`cd50a2a`) has no such columns today; they are additive there as well.

## 2. Call order for Ordering

1. Shopping on the frontend surfaces (`{Backoffice|OtaPanel}/v1/AncillaryOffers/Grouped` and the nine `…/{Family}/Details` routes) ends with a `selectionId` per chosen non-seat service and a SeatMap `id` per seat.
2. **Create order:** `POST Service/v1/Ancillaries/Selections/ResolveGrouped { offerId, seats[], baggages[] … }`. Store a service only when `meta.allValid` is true: keep the original `selectionId`, `service.price.currencyId` and `service.price.lineTotal`, plus provider, definition, provision and pricing ids from `service`.
3. **Add to an order:** the same route with `{ orderId, expectedOrderCommercialVersion }`; ids must be Order-issued (shopped with `orderId`). Offer-issued ids are refused there with `PricedSelectionForAnotherOrder`.
4. **Hold:** `POST Service/v1/Ancillaries/Service-Holds/Grouped` with the header of the existing seat hold plus one `{ orderServiceId, selectionId }` per family array. Seats keep the existing `SeatHoldSelectionInput`. Ancillary re-opens the source named inside the id, re-prices, checks the binding of step 1, and saves one `AncillaryReservation`.
5. **Lifecycle:** the existing `Service-Holds/{holdId}` routes (Confirmations, DELETE, Cancellations) act on the whole hold.

Take the FlightFlow physical seat hold before step 4 and release it when step 4 fails; Ancillary does not compensate across providers.

## 3. What the id carries

`v`, `source {kind, ref, orderCommercialVersion}`, `scope {channel, pointOfSaleId, airlineOfficeId, travelAgencyId, customerId, countryId}`, `offerItemRef`, `profile`, `variantCode`, `selection {only the chosen fields}`. No catalog, provision, pricing, supplier id, price or document text. It is unsigned; every consumer re-validates it against the live source.

The `scope` block is needed because `offerItemRef` is a hash over channel, owner, office/agency, point of sale, offer id, source version and candidate identity; without the scope the candidate cannot be found again. At hold time `scope.pointOfSaleId` must equal the header `pointOfSaleId`.
