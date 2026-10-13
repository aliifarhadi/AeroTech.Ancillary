# 11 — Mandatory v12.3 P1/P2 conformance and negative stress tests

## Actual evidence only
Agent must run tests and report exact counts and method names. Do not claim `PASS` for scenarios not exercised, fixtures faking a current external source, or old v12.2 numbers. `BLOCKED_EXTERNAL_SOURCE` may be reported only for nonSeat physical source integration not yet verified; Seat Definition/Provision/Pricing/Shopping must complete locally **without** FlightFlow and Seat Policy.

## Matrix: 24 variants x six groups
For each A01–A24, test `D` (typed Definition full CRUD), `R` (Provision + shared rules and optional POS), `P` (pricing/outcome/currency), `I` (true inventory/policy semantics, Seat NotApplicable), `Q` (QueryDb/HTTP read+EF round-trip), `N` (negative error code). 144 identifiable assertions/groups; same method may cover more than one but each group must be evidenced. Retain practical existing tests; update only those whose expected behavior was superseded by v12.3.

## Core cross-cutting required tests
| ID | Proof |
|---|---|
| CT-01 | A01–A24 exactly match 9 closed profiles, wrong specs rejected |
| CT-02 | Seat Definition `POST/GET/PUT` correct variant fields, A08 nonempty characteristics, A09 count/purpose |
| CT-03 | Seat Provision original nested four endpoints and all ten shared rule groups retain correct round-trip |
| CT-04 | Active/Revise semantics do not change product Profile or Variant |
| CT-05 | Seat Pricing Filed/Free/Quote/NotAvailable cases and correct documents |
| CT-06 | **Seat zero InventoryPolicy, no policy required for Define/Publish/Pricing/Shopping** |
| CT-07 | `Define/Change/Activate` nonSeat Policy works, Seat Policy refused at all entrypoints |
| CT-08 | old FlightFlow-specific enum/port/stub/runtime references removed, host runs with no `FlightFlow:BaseUrl` |
| CT-09 | Count/Weight/Slot true local inventory remains functional and fails closed when reference unavailable |
| CT-10 | No mandatory POS; null/empty POS permits authorized caller; nonempty POS limits |
| CT-11 | No owner leakage in Definition, Provision, Pricing, inventory or Shopping routes |
| CT-12 | `TravelDate.BlackoutPeriods` survives GET form and PUT identical |
| CT-13 | TaxIncluded vs AddedToBase, nonitem Fee, currency minor unit, no fabricated 0 |
| CT-14 | Shared domain Shopping Engine reused; not replaced by Seat-specific copy |
| CT-15 | Three surfaces SeatOffers and AncillaryOffers routes with correct auth and type partition |
| CT-16 | Seat commercial offer values remain valid without any SeatMap data; physical availability never claimed |
| CT-17 | No unverified seatNumber characteristic is treated as known, no arbitrary browser `SourceAuthority` accepted |
| CT-18 | Both families' `Details` and `Selections/Evaluate` are scoped; cross-family candidate refs refused |
| CT-19 | Existing `AncillaryOffers/SeatMaps` removed; API documentation and generated OpenAPI snapshot refreshed |
| CT-20 | A07/A08 quantity max1, A09 extra seats correctly modeled, no seat occupancy in Spec/Provision |
| CT-21 | NonSeat inventory missing policy != Unlimited; Seat NotApplicable != Unlimited |
| CT-22 | current FlightFlow-independent Contract builds without removing unrelated shared Contracts used by other code |
| CT-23 | command/query SQL migrations clean and projections up-to-date |
| CT-24 | P3 Reservation/Hold/Confirm files and contract untouched |

## Eight scenario stress journeys
S1: A07 free/A08 paid in same cabin: commercial prices correct with FlightFlow URL blank.
S2: multiple traveler/flight selection, different fares and pricing; seat categories distinct; no physical occupancy asserted.
S3: A09 extra seat with CabinBaggage vs PassengerComfort, requiresAdjacentSeat and document route compatible; no flight stock.
S4: A02 purchased 5+10kg baggage pack not mistaken for airplane weight capacity; no Seat leakage.
S5: PETC per flight supplier confirmation and true nonSeat counting family; blocked external source is honestly blocked.
S6: WCHR free SSR, must-check external readiness, no paid pricing or fabricated stock.
S7: CIP AirportSlot overlapping concurrent bookings/config prevented, nonSeat local stock preserved.
S8: mixed AirOffer seat and other services, one shopping execution implementation with surface-specific filtered endpoints; correct tax/currency/POS/owner scope.

## Source/runtime tests
CI build on real .NET solution, unit tests, SQL Server integration, HTTP smoke for Api/OtaPanel/Backoffice, OpenAPI contract snapshots, DI container, tests for no external HTTP Seat/FlightFlow invocation. Snapshot test route absence/presence. Do not conflate a green in-memory test with actual SQL migration proof.
