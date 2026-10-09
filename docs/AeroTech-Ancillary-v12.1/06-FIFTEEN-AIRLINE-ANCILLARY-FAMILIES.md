# Fifteen airline ancillary families — end-to-end commercial and stock authoring examples

Legend: `SD`=ServiceDefinition, `P`=Provision, `Price`=Pricing/Rate/Components; `IP`=InventoryPolicy, `U`=Unlimited local stock, `S`=Supplier, `F`=FlightFlow delegated, `LC`=Local FlightCount verified, `LW`=Local FlightWeight verified, `AS`=Local AirportSlot verified. `SubjectToConfirmation` is a metadata policy, NOT a completed booking state. Names/prices/examples illustrative unless explicitly identified as airline-published. **Each row requires a real source for any Local quota activation.**

| # | Family & real benchmark | SD + P rules & unit | Price authoring | IP default; permitted alternatives | Booking/purchase nuance |
|---|---|---|---|---|---|
| 01 | Extra baggage **Weight Concept**; Qatar 10kg bundles, Emirates 5kg increment | 5kg, 10kg, 20kg fixed pack identities, PerItem/Each; P route, baggage concept, Fare, AdvancePurchase, MaxQuantity. 10kg pack Quantity 2 = 20kg entitlement | one Price for each applicable P, rates by currency; potentially POS-specific P | U (no assumed local pool), S when externally enforced, LW **only proved** | PreOrder/PostTicketed by carrier; check cumulative allowance in future context |
| 02 | Extra baggage **Piece Concept**; Iberia 15kg/23kg/32kg, Qatar Africa/Americas piece | distinct piece product identities, PerPiece/Piece, weight-per-piece and quantity limits; P flight+fare+route | one base rate + tax/fee, by currency | U, S, LC only if factual piece quota | 15/23/32 are SKU characteristics, not three physical aircraft quota pools |
| 03 | Heavy/Oversize bag | `ChargeKind=Overweight|Oversize`, criteria by aircraft/route/allowance; PerItem/Each where fee is per bag | fee itself can be base rate of product; do not attach unknown percentage surcharge | U or S, LC only proved | Requires actual dimensions/weight in later request; never infer from amount |
| 04 | Special sports equipment (bike, ski, scuba, golf etc); Iberia ServiceList | special equipment identity per type; `ChargeKind=SpecialEquipment`; per piece and segment, SaleStage as carrier | PerPiece/Piece explicit Money; P may have NotAvailable override per equipment/aircraft | S default; LC only verified cargo allocation | May require advance registration/approval; example Iberia paid order requirement varies by product/channel |
| 05 | Pet cabin/hold | separate PETC/AVIH SDs, traveller+flight+carrier+route, quantity per passenger/flight | PerItem/Each; no made-up pet fare | S/request confirmation or verified LC `AnimalCarrier` | medical/docs/animal carrier restrictions in operational process; not guaranteed from catalog |
| 06 | Paid seat / extra-legroom / EXST | SD seat product, P seat trait+aircraft+RBD+PTC, PerSeat/Each. EXST distinct SD, flight coverage | selected paid Seat base or Free Provision; quote tied to exact seat future | F; no local seat stock | Seat map/status external real source; selling an offer ≠ seat allocated |
| 07 | Special / preorder meal (VGML/CHML, paid meal) | SD per meal type, P advance cutoff, eligible flight + PTC, PerPassenger/Each | Free SSR meal -> No price; prepaid hot meal -> Money rate | U or S; LC only real catering quotas | SSR meal can be Free and requestable; meal must not be assumed guaranteed |
| 08 | WCHR/WCHS/WCHC & special needs | SD service type/SSR, P passenger/flight, free Outcome; contact/assistance descriptors from product | no active price for Free | U + MustCheckAvailability if confirmation needed, or S | `SubjectToConfirmation`; never sell artificial paid wheelchair capacity |
| 09 | UMNR / accompanied minor | SD required documents/SSR, P age bands+route/flight, PreOrder/Both per carrier | Paid or Free depending carrier/P; Rate PerPassenger by age/ptc where paid | U with confirmation or S, LC only factual staff quota | special service request, operational acceptance pending future Phase3 |
| 10 | Priority Boarding (Iberia) | SD priority, P one per passenger/sector, allowed fare, stage per carrier; PerPassenger/Each | Paid / Free depending fare, currency rate per P | U typically | Some channel API permits only after ticketing; don't hardcode across carriers |
| 11 | Lounge access | SD facility/supplier/eligibility, ServiceDateBasis=ServiceStart, P airport/location/day/time | PerPassenger/Each, paid/free by fare; possible age selectors | S default; AS if actual owned lounge slot | Request or confirmed access per provider, not a count inferred from seats |
| 12 | CIP / Fast Track / Meet & Assist | distinct SD services by location, stage, occurrence time; P airport/facility/customer | PerPassenger/Each; potential same service different POS -> separate P | S or proven AS | Time slot is real only if agreed capacity/appointment system exists |
| 13 | Onboard Wi-Fi / messaging | SD Wi-Fi products, P flight/aircraft, PerItem/Each | Money per pass or Free entitlement by fare | U or S | activation/coverage varies; no invented session inventory |
| 14 | Travel insurance | SD type/coverage countries, ServiceDateBasis=CoverageStart, P customer/age/coverage, PerPassenger/Each | price per covered product; no fabricated percent premium | S default when underwriter controls issuance, U only if truly unconditional distribution | policy issuance later, not assumed guaranteed by commercial publication |
| 15 | eSIM/travel connectivity | SD country/region/validity package, ServiceDateBasis=Activation, P coverage, PerItem/Each | per package currency rate; no period multiplication without defined tariff | S provider allocation or U if contract says unlimited | activation may fail, reflected separately from local stock |

## Five mandatory worked examples (authoring recipes)

### A. 10kg baggage in DE versus TR POS
- SD `XBAG_WEIGHT_10KG`: `PricingUnit=PerItem`, Quantity Unit=Each. Baggage Weight=10 Kilogram and ChargeKind WeightPackage; applicable journey concept Weight only when sourced.
- P#100 Sequence=10, Sales PointOfSale DE, Route A→B, PurchaseStage Both, Outcome Paid, Qty 1..4, MaxAdvance 30 days, MinAdvance 6 hours where carrier rules support this. P#110 Sequence=20, Sales POS TR, same service & conditions, different price. P#900 generic if truly permitted; else none.
- PriceBook for P#100 Active: EUR rate + USD rate; P#110 Active TRY rate + EUR rate. No FX. FreeAllowance comes from Fare/Ticket context later and is not a stock decrement.
- IP stable `XBAG_WEIGHT_10KG` => Unlimited no local finite quota. A 500kg FlightWeight Inventory row must NOT be seeded merely because product says weight.

### B. Iberia-inspired baggage 15/23/32 kg
- Three definitions, PerPiece/Piece; P eligible flights/markets, purchase timing specific to channel. Rates are unit price per actual selected item; base USD/EUR independently authored; no blanket "all three available" rule. IP U/S unless actual allotment.

### C. WCHR Free
- SD booking SSR WCHR, ConfirmationRequirement SubjectToConfirmation, Document None if actual policy so dictates; P Free with `BookingRequired=true`, DateBasis FlightDeparture, PassengerEligible, No pricing. IP U with `MustCheckAvailability=true` allowed after D10 correction; no local wheelchair Stock.

### D. Seat 18A
- SD SeatSelection PerSeat/Each; P SeatApplication seat traits and eligible aircraft, Free or Paid by Fare/Passenger; price fixed Money when Paid; IP FlightFlow delegated, requiring actual delegation evidence. **18A physical status does not reside here**.

### E. Verified 12-person lounge slot
- SD Lounge ServiceStart, P airport/service location/time; PricingUnit PerPassenger/Each; IP Local AirportSlot only if real facility ownership/timezone evidence. Create verified FacilityId slot `[10:00Z,11:00Z)` CapacityPersons=12; overlapping `[10:30Z,11:30Z)` blocked by SQL; `[11:00Z,12:00Z)` allowed. Admin adjustment to 10 records immutable ledger; **no reservations or guarantee**.

## Benchmark caveat
Sources prove capabilities in their published carrier/channel flows, not universal airline business rules. Every example is a product configuration that this domain should express. Do not hardcode “Iberia post-ticket” for all sellers or infer a Qatar 10kg package to be available on all routes.
