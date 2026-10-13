# 14 — Frontend handoff (contract clarification, NO frontend repo changes)

Seat Backoffice: Definition -> multiple Provisions -> each Provision Pricings. A07/A08/A09 typed fields detailed in `04`. No Seat InventoryPolicy page, no activation UI, no provider key, no Seat capacity count or commercial pricing dependent on seatmap.

**Phase2 public Shopping:** generic Ancillary Offers listing and Seat Offers list are distinct endpoints/presentations; both share the existing backend engine. Seat Offers returns commercial price and terms by category/traveller/flight context, NOT actual seat layout or seat occupancy. External BFF/FlightFlow presentation can later merge physical map with Seat price but is outside this Ancillary-only backend task.

`POST /Api/v1/SeatOffers` for Seat commercial offers; `POST /Api/v1/AncillaryOffers` for all other services; equivalents for OtaPanel and Backoffice. Separate `/Details` and `/Selections/Evaluate`, preserving current errors/auth. Data not returned by Ancillary: available seat count, occupied/blocked seat number list, physical seat geometry, aircraft seat map; do NOT put mock values in FE.

No inference "Ancillary absent => Free seats". A truly Free Seat results from explicit published Free Provision. An unresolved SeatNumber-to-characteristic match is incomplete until trusted source facts exist. Ensure front receives real offer reference, traveler/flight/coverage, variant, price origin, PriceAssessment/price status, currency/taxes/fees, allowed selection fields; no guessed amount or availability.

**NonSeat Backoffice inventory remains** for products that actually have a local/supplier capacity policy. It is not part of Seat Definition editor. Existing unrelated frontend functionality stays unchanged until backend HTTP smoke passes.
