# 08 — بنچ‌مارک PSS/NDC و تصمیم‌های خاص AeroTech

## Sources (public, not AeroTech implementation authority)
1. **IATA NDC:** https://www.iata.org/en/programs/airline-distribution/retailing/ndc/ — Offer/Order as commercial boundary.
2. **IATA 21.3 Implementation Guide:** https://guides.developer.iata.org/docs/21-3_ImplementationGuide.pdf — `ServiceListRQ/RS`, `SeatAvailabilityRQ/RS`, `OfferPriceRQ/RS`, `OrderCreateRQ/OrderViewRS` at shopping/ordering stages.
3. **Travelport JSON APIs Air v11 API References:** https://support.travelport.com/webhelp/jsonapis/airv11/content/air11/APIReferences.htm — Ancillary Shop, Ancillary Price, Seat Map/Book, Add Ancillary Offer, Cancel, EMD operations distinguished.
4. **Travelport Seats Guide:** https://support.travelport.com/webhelp/JSONAPIs/Airv11/Content/Air11/Seats/SeatsGuide.htm — NDC/GDS seat/booking workflow differences; carrier support matters; some seats can be added separately by GDS but not all NDC paths.
5. **Duffel Orders API v2:** https://duffel.com/docs/api/v2/orders — POST to add services to an existing Order with `add_services[{id,quantity}]` and payment; documentation warns this operation currently not supported for orders of type `hold`.
6. **Duffel Post-booking Bags Guide:** https://duffel.com/docs/guides/adding-post-booking-bags — available ancillary service ID + quantity; booked services accessible on order.

## Fact vs choice
| Public benchmark supports | AeroTech decision (local; not vendor standard) |
|---|---|
| NDC separates a-la-carte service/seat shopping and Order processing | Ancillary holds its commercial selections independent of Ordering; EMD, financial price, Order status in Ordering |
| Travelport has specialized seat map/book and ancillary shop/price/book flows | `SeatMaps/Holds` separate commercial facade; other families grouped in one typed Hold |
| Different carrier/PSS implementations have different restrictions | Unsupported A09/A13/A14/A17/A18/A19 and Supplier fail-closed, not assumed supported |
| Duffel can use concise ancillary ID/qty for simple bag products | AeroTech requires typed Options when choice (SSR/weight/appointment/device) cannot be reproduced by Provision ID and Quantity alone |
| NDC offers and orders model accepted products/services | v11 uses `provisionId` as local Product locator, snapshots selected details; this is **AeroTech-specific schema**, not a copy of NDC request message |
| Travelport separates seat assignment from EMD/booking flows | Commercial Seat Unit in Ancillary never directly means FlightFlow physical seat held; Ordering coordinates physical operation |

## Why not directly copy Amadeus/Duffel/Travelport?
Their API resources, Order ownership and carrier/provider capabilities differ. A static `serviceId+quantity` payload is adequate only when all product-specific choices are fixed within the referenced offer; A03 BagRef, A15 SSR and A20 appointment demonstrate why AeroTech must retain typed selections. Conversely, requiring twenty nullable fields for every family destroys contract clarity. `family -> variant -> product -> units` preserves Offers/Resolve naming while each family accepts only its relevant Options.

## Decision points without invention
- `CustomerSelectionContract` is the catalog semantic source, but only concrete supported variant and Definition fixed choice establish Hold schema. Future dynamic A12 menu choice or A24 plan choice requires new explicit Owner decision, not implicit extension.
- `Hold` does not prove an input belongs to a former `s_` Selection because user prohibited Price/Resolve cross-service revalidation. This is an accepted trust boundary and must be documented as such.
- `sp_getapplock`, Root/Hold statuses, Ledger/Expiry are local implementation details. Neither IATA NDC nor public Duffel pages mandate AeroTech's exact lock or route structure.
- No invented provider booking code/PNR, fictitious source mapping, or automatic third-party fulfillment.
