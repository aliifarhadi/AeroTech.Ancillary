# 05 — Shared Provision contract and eligibility

## Existing source topology (retain)
`AncillaryProvision` is the commercial rule instance referencing one specific `ServiceDefinitionId`. Fields: `Sequence`, `CoverageScope`, `PurchaseStage`, `PriceOrigin`, `QuoteProviderKey?`, `QuantityRule`, `ApplicationType`, `CommercialOutcome`, `SettlementDefinition`, `AvailabilityDefinition`, `FulfillmentDefinition`, Status, timestamps, 10 shared typed optional child rule groups and bounded Pet/AssistedTravel/AirportService rules. Active version immutable; Revise/Suspend/Reactivate/Retire policies stay current.

| Rule group | Fields/semantics; preserve all nested rows |
|---|---|
| PassengerEligibility | PTC codes, age bands; verified child age, no guessed DOB |
| SalesRestrictions | start/discontinue, PointsOfSale, CustomerIds, CustomerTypes |
| Geography | origin/destination/via, route pairs, airport/country/location |
| FlightApplication | marketing/operating carrier, FlightNumber/FlightId, AircraftIds |
| FareApplication | AirFareId, FareFamilyId, FareBasis, CabinId, RBD and type |
| TravelDate | permitted periods **and BlackoutPeriods**, blacklist precedence |
| DayTimeApplication | weekday local windows allow/deny; timezone correct |
| AdvancePurchase | minimum, optional maximum, AirPrice.TimeUnit, SameTimeAsTicketed |
| BaggageApplication | baggage charge/free pieces/tier/weight, Baggage-only |
| SeatApplication | eligible seat numbers and characteristic codes, Seat-only |

Optional special rule groups: `PetRule`, `AssistedTravelRule`, `AirportServiceRule` only on matching profiles. Preserve current narrow validations. Do not create default dummy rules or weaken wrong-profile guards.

## Current HEAD POS decision overrides v12.2
At HEAD `8fda5f1e`, `AncillaryProvision.Publication.cs` no longer demands any POS. `SqlActiveAncillaryProvisionReader` returns Active row if `SalesRestrictions == null OR PointsOfSale empty OR PointOfSaleId matches`. `AncillaryShoppingEngine.LoadAsync` applies the same predicate. **Do NOT revert this.** Empty POS does not bypass caller-to-airline tenant scope; all queries and commands must still isolate OwnerAirlineId.

## Quantity / price origin / dates
- `QuantityRule.MinQuantity >=1` and Max>=Min. Display 0 means skip selection, not 0 accepted units.
- Coverage `Sector=1`, `Portion=2`, `Journey=3`, `Order=4` (no invented Bound enum). One priced Portion may span multiple flight IDs and still be charged once per commercial rule.
- Seat A07/A08 max1 as coded; A09 extra-seat declared count separately.
- `AvailabilityDefinition.MustCheckAvailability` is metadata about external service readiness. For Seat, it must **not** turn into an Ancillary InventoryPolicy gate; may remain declared and surfaced as external context requirement without calling FlightFlow or blocking commercial price assessment. For other profiles use truthful policy signals.
- Outcomes align with `PriceOrigin`: Paid+Filed, Paid+ExternalQuote, Free+Free, NotAvailable+NotAvailable; no inconsistent combinations.
- Published valid `PointOfSale` missing -> unrestricted for authorized scope. No invisible POS cloning.

## Must-have regression
`TravelDate.BlackoutPeriods` GET/PUT identical round-trip; overlapping real effective/blackout logic; fare/age/cabin/flight rule correctness, rejected wrong family fields, optional POS sales semantics and pricing across two POS, no hidden inventory gate for Seat.
