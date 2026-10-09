# Source-backed benchmark traceability (public API/airline evidence vs our modeling choices)

| Public source (official when possible) | Observed behavior | Evidence → proposed design, not copied provider schema |
|---|---|---|
| Amadeus developer flight APIs guide, https://github.com/amadeus4dev/developer-guides/blob/master/docs/resources/flights.md | Flight Offers Price returns baggage catalog by quantity or weight associated with passenger and segment; selected baggage repriced before booking; additional service may be unavailable | Commercial baggage entitlement != local kg Stock. Future OfferItem by passenger+segment; quote remains future, not Phase2 API |
| Iberia NDC ServiceList 17.2, https://transform.atlassian.net/wiki/spaces/NDCDOC/pages/4078698497/ServiceList%2B17.2 | ServiceList works pre- and post-sale, returns Bags, Special Equipment, Priority and Special Needs; carrier/channel purchase stages differ | Provision PurchaseStage and Booking confirmation descriptor, no universal stage hardcoding |
| Iberia NDC Ancillaries, https://transform.atlassian.net/wiki/spaces/NDCDOC/pages/3836838134/Ancillaries%2B17.2 | Group/SubGroup filters for bags, sports and pets; Offer/Order references and SeatAvailability separate | Keep SD Group/SubCode identity and typed eligibility; Seat state not duplicated |
| Iberia NDC Special Equipment, https://transform.atlassian.net/wiki/spaces/NDCDOC/pages/3894378548 | Examples for bike, golf, dive, ski etc; 15/23/32kg bags separate ServiceDefinitions and SSR/EMD encoding | Separate simple products/packages; typed baggage descriptor; no dynamic generic tariff matrix |
| Iberia NDC Special Needs SSR, https://transform.atlassian.net/wiki/spaces/NDCDOC/pages/3836706946/Special%2BService%2BRequest%2BSSR%2B17.2 | SSR OfferItem may be requestable but is subject to confirmation at booking; examples zero price; MaxQuantity per person/segment | Free Provision; booking SubjectToConfirmation; passenger usage/quantity distinct from stock |
| Iberia NDC ALaCarteOffer, https://transform.atlassian.net/wiki/spaces/NDCDOC/pages/4020142345/ALaCarteOffer | Offer item has passenger/segment association, unit base/tax/total and code-of-currency with monetary amount | Rate Money+components, future NDC projection outside phases 1/2 |
| flydubai OTA Modify Flow, https://developer1.flydubai.com/browse/api-doc-banner?apirefrence=modify_flow | Baggage and meal offer retrieval via Ancillary API, seat map via Seat API, servicing by booking context | One domain can author all, future separation of ServiceList and seat availability at boundary |
| Qatar Airways Excess Baggage, https://www.qatarairways.com/en-au/baggage/excess.html | Piece vs weight by route; extra weight in 10kg bundles; higher permitted amounts vary by aircraft and travel class | SKU/package quantities + route/flight/fare eligibility and limits, not assumed inventory |

## Industry code caution
ATPCO Optional Services codes / S5/S7 are **conceptual benchmark**, not permission to make up exact official subcodes or pricing category numbers. The source repository has a small `IndustryServiceSubCodeReference` (0BX, 0CC) and carrier-specific types. Any new Industry code must be confirmed against a valid source snapshot; otherwise mark CarrierDefined. Numeric enums of pre-existing core contracts cannot be silently changed.

## Separate source claims from our decisions
- The first two columns above state a published carrier/platform capability as checked in public docs in October 2026; provider terms may evolve.
- Our `AncillaryPricingRate`, `Money`, `PurchaseStage`, `ConfirmationRequirement` and typed Inventory policy names are **local domain design choices** inspired by the data contracts. They are NOT declared mandatory ATPCO/IATA record names.
- The public NDC sources do **not** prove that every airline has an active finite Count inventory for pets, bags or meal. Consequently auto-inference of local quantity is prohibited.
- We intentionally do not simulate Supplier, FlightFlow or reservation availability in Phase 2.
