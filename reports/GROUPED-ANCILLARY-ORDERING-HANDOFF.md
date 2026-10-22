# Grouped Ancillary — Ordering handoff (no Ordering code in this task)

| Item | Value |
|---|---|
| Ancillary branch | `feat/ancillary-grouped-api-gate-a`; Gate A = `f4b5145`, finish = `5d85304`, closure-audit corrections UNCOMMITTED on top |
| Scope | the facts Ordering must hold and the call order it must follow so that `POST Service/v1/Ancillaries/Service-Holds/Grouped` can leave `ORDERING_INTEGRATION_PENDING` |

## 1. The one dependency: the accepted service snapshot on the order service

`Application/Orders/OrderServiceFacts` carries six trailing optional members plus `Quantity`. The production reader (`IOrderCommercialContextReader`, today `NotConnectedOrderCommercialContextReader`, 503 / 16815) must fill them for every ancillary service it reports. They are the snapshot of the `service` block Ordering accepted from `ResolveGrouped` at create or add time, and they are the authority the hold binds to, so the hold still works after the order has moved to a later commercial version.

| Member | Type | Source in Ordering (the `service` block of the accepted `ResolveGrouped` entry) |
|---|---|---|
| `AcceptedSelectionId` | `string?` (max 4096, base64url) | the exact `selectionId` string, never re-issued, never re-encoded |
| `AcceptedCurrencyId` | `int?` | `service.price.currencyId` |
| `AcceptedTotal` | `decimal?` | `service.price.lineTotal` |
| `AcceptedServiceDefinitionId` | `long?` | `service.definitionVersionId` |
| `AcceptedProvisionId` | `long?` | `service.provisionId` |
| `AcceptedPricingRevisionId` | `long?` | `service.pricingRevisionId` |
| `Quantity` | `int` | `service.price.quantity` (units of the accepted selection: pieces for a baggage choice, 1 for a plan or a seat) |
| `TravellerRef`, `PortionRef`, `FlightRefs` | existing | `service.travellerRefs[0]` (empty for an Order-scoped service), `service.portionRefs[0]`, `service.flightRefs` |
| `CommercialState` | existing | `Active` for a sellable service; anything else refuses the hold |

Without the first three the grouped hold refuses every non-seat entry (422 / 16402 `…AcceptedSelectionId` or `…AcceptedTotal`); without the next three it refuses with `…AcceptedServiceDefinitionId`. Seats never need them.

`OrderService` in Ordering.Final (`cd50a2a`) has none of these columns today and no `Quantity`; they are additive there as well.

## 2. What the hold verifies against the snapshot

For every non-seat entry `{ orderServiceId, selectionId }` the verifier (`Query/…/VerifyGroupedSelections/GroupedSelectionVerifier`) checks, in this order, all refused as 422 / 16402 with the field `{Profile}[{orderServiceId}].SelectionId:{reason}`:

1. the id decodes canonically (`PricedSelectionNotAuthentic`), its profile matches the family array and is not Seat (`OfferItemNotOffered`), its `scope.pointOfSaleId` equals the header (`PointOfSaleNotAllowed`), and it names this order's offer or this order at a version not above the current one (`PricedSelectionForAnotherOrder`);
2. the order service exists, `AcceptedSelectionId` equals the sent id byte for byte, the accepted currency equals the header currency, the accepted definition and provision ids are set, the state is `Active`, and the accepted definition's variant equals the id's variant;
3. the order source is reopened at its current version without this batch's own services, the engine shops the id's variant, and the candidate with the accepted definition and provision for the service's traveller and flights must still be offered (`OfferItemNotOffered`); its portion must contain `PortionRef`;
4. the id's selection is re-evaluated on that candidate: accepted, same provision, purchasable, same pricing revision, same currency and `lineTotal` (`PriceChanged`), `price.units == Quantity`, no unapplied shared fee (`SharedFeeNotApplied`), and no grouped-hold blocker (`InventoryDelegatedCheckRequired` for Supplier stock, `InventorySourceNotConfigured` for Local stock, `InventoryNotConfigured`, `AvailabilityMustBeChecked`, `SeatSelectionNotOffered` for A09);
5. the traveller of the candidate must be linked to an order traveller id (`OrderTravellerNotLinked`) unless the service is Order-scoped.

The coverage (traveller, portion, flights) that is held is the one Ordering recorded on the order service, re-offered and re-priced; the id's `offerItemRef` is not re-derived at hold time because an Order-issued id's source version includes the service list that the acceptance itself changes. Ordering therefore has to store the `service` block of the entry it accepted, not a re-shopped one.

## 3. Call order for Ordering

1. Shopping on the frontend surfaces (`{Backoffice|OtaPanel}/v1/AncillaryOffers/Grouped` and the nine `…/{Family}/Details` routes) ends with a `selectionId` per chosen non-seat service and a SeatMap `id` per seat.
2. **Create order:** `POST Service/v1/Ancillaries/Selections/ResolveGrouped { offerId, seats[], baggages[] … }`. Store a service only when `meta.allValid` is true: keep the original `selectionId` and the whole `service` block (section 1). `allValid` is false for every entry the grouped hold would later refuse (Supplier or Local stock, unconfigured inventory, reserve-time checks, A09, Seat).
3. **Add to an order:** the same route with `{ orderId, expectedOrderCommercialVersion }`; ids must be Order-issued (shopped with `orderId`). Offer-issued ids are refused there with `PricedSelectionForAnotherOrder`; an id issued at an older version is `Stale` / `OrderVersionChanged` at resolve time, but an already accepted one still holds (section 2).
4. **Hold:** `POST Service/v1/Ancillaries/Service-Holds/Grouped` with the header of the existing seat hold plus one `{ orderServiceId, selectionId }` per family array. Seats keep the existing `SeatHoldSelectionInput`. The idempotency key is looked up before any verification: the same batch under the same key returns the stored hold, a different batch under the same key is 409 / 16403.
5. **Lifecycle:** the existing `Service-Holds/{holdId}` routes (Confirmations, DELETE, Cancellations) act on the whole hold.

Take the FlightFlow physical seat hold before step 4 and release it when step 4 fails; Ancillary does not compensate across providers.

## 4. What the id carries, and a release limitation

`v`, `source {kind, ref, orderCommercialVersion}`, `scope {channel, pointOfSaleId, airlineOfficeId, travelAgencyId, customerId, countryId}`, `offerItemRef`, `profile`, `variantCode`, `selection {only the chosen fields}`. No catalog, provision, pricing, supplier id, price or document text. It is unsigned; every consumer re-validates it against the live source.

The `scope` block is needed because `offerItemRef` is a hash over channel, owner, office/agency, point of sale, offer id, source version and candidate identity; without the scope the candidate cannot be found again. At hold time `scope.pointOfSaleId` must equal the header `pointOfSaleId`.

Release limitation: the id is readable by anyone who base64url-decodes it and therefore reveals the point of sale, office, agency, customer and country ids of the shopping scope. It carries no price, no catalog id and no personal data, and it cannot be used on another order, point of sale or source version. Do not show it in customer-facing documents; treat it like an opaque reference in logs. A sealed or signed form is a later change and is not part of this release.
