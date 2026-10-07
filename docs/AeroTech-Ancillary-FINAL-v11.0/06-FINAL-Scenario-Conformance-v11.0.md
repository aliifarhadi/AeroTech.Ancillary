# 06 — FINAL Scenario Conformance v11.0

All scenarios in this file are **Phase 1 authoring scenarios**.

## A — Boundary

### A01
No file outside `AeroTech.Ancillary` is modified.

### A02
No new `AncillaryEvaluation` namespace/folder/service exists.

### A03
No existing `AncillaryReservationAggregate` functional source is changed by Phase 1.

### A04
No AirAvail/Ordering/FlightFlow integration DTO is added "for later".

### A05
No `/Simulate`, `/AncillaryEvaluations`, generic rules endpoint or runtime matcher is added.

## B — Supplier / marketplace authoring

### B01
Two Suppliers can own separate ServiceDefinitions for the same broad service/sub-code.

### B02
Supplier name is never a technical relationship key.

### B03
No credentials/URL/secrets are stored in Supplier domain state.

### B04
Existing typed fulfillment fields are preserved but no adapter/runtime behavior is implemented.

## C — ServiceDefinition

### C01
Industry sub-code must exist in `IndustryServiceSubCodeReference`.

### C02
Carrier-defined service can explicitly author classification fields without creating a new sub-code aggregate.

### C03
Draft -> Active requires Active Supplier.

### C04
Draft can be edited; Active semantic mutation is rejected.

### C05
Suspend/Reactivate/Retire lifecycle follows the existing enum only.

### C06
Revision creates a new immutable numeric ID/version if revision is implemented; historical version is not overwritten.

### C07
BookingDefinition and DocumentDefinition round-trip completely.

## D — Passenger pricing

### D01
Same ServiceDefinition supports separate ADT and CHD Provisions with different fixed prices.

### D02
INF can be Free or NotAvailable using a separate Provision.

### D03
There are no AdultPrice/ChildPrice/InfantPrice columns.

### D04
No age/FF/occurrence fields are added.

## E — Sales criteria

### E01
PointOfSaleIds round-trip.

### E02
CustomerIds round-trip.

### E03
CustomerTypes round-trip.

### E04
No PCC/raw channel duplicate is added.

## F — Travel criteria

### F01
Origin/Destination lists round-trip.

### F02
Directional and both-directions RoutePairs round-trip.

### F03
Via airport list round-trips.

### F04
TravelFrom/TravelTo round-trip and invalid reversed range is rejected.

### F05
DaysOfWeek and time window round-trip.

### F06
Marketing/Operating carrier lists round-trip.

### F07
FlightNumbers round-trip normalized according to source conventions.

### F08
Exact FlightIds round-trip.

### F09
AircraftIds round-trip.

## G — Fare criteria

### G01–G06
AirFareIds, AirFareTypes, FareFamilyIds, exact FareBasisCodes, CabinClassIds and RbdIds each round-trip and can coexist in one Provision.

### G07
FareFamilyId is numeric identity; no FareFamily display-name identity is introduced.

## H — Advance purchase

### H01
Period must be positive.

### H02
Unit reuses the canonical `AeroTech.Messages.AirPrice.Enums.TimeUnit`; no duplicate Ancillary time-unit enum is added.

### H03
No eligibility arithmetic is implemented in Ancillary Phase 1.

## I — Price

### I01
Paid requires Fee + price line(s).

### I02
Free/NotAvailable cannot carry a positive payable amount.

### I03
Amount persistence is decimal(18,2).

### I04
Tax line can carry code/country/station evidence.

### I05
No FX/ROE is calculated.

### I06
Unsupported percentage/per-kg fee formulas are not activated merely because enum values exist.

## J — Applications

### J01
Standard application requires no service-family-specific child object.

### J02
Baggage application typed fields round-trip.

### J03
Seat application requires seat numbers and/or characteristic codes.

### J04
Exact seat-number authoring requires AircraftIds.

### J05
No live seat state is added.

## K — Backoffice

### K01
Supplier list/detail works with pagination.

### K02
ServiceDefinition list/detail works with useful filters.

### K03
Provision list/detail works with useful filters.

### K04
All new typed criteria appear in Provision detail.

### K05
Draft update round-trips all fields.

### K06
Lifecycle endpoints do not create a second workflow/state machine.

### K07
No Preview/Bulk/Simulate requirement is introduced.

## Family proof

`FAM01` through `FAM13` from document 05 must each have explicit fixture/acceptance proof for authoring/read/lifecycle.

## Required price-stress fixtures

1. Lounge: ADT 25 EUR, CHD 15 EUR, INF NotAvailable.
2. Baggage: selected flight/date 30 EUR; route+fare-family 25 EUR; broad/default 20 EUR — all three rules must be authorable and readable, without running a matcher.
3. Two lounge suppliers with distinct ServiceDefinitionIds and prices.
4. Meal price differs by PTC and cabin.
5. Insurance plan price differs by PTC and travel window.
6. Seat rule differs by aircraft and characteristic.
