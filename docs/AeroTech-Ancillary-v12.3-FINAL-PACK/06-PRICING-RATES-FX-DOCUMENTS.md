# 06 — Pricing and document rules retained from existing implementation

`AncillaryPricing` Aggregate (`Id`, `AncillaryProvisionId`, `Version`, `PricingUnit`, lifecycle/status). `AncillaryPricingRate` child per `(CurrencyId, PassengerTypeCode?, age lower?, age upper?)`: `BasePrice:Money`. `AncillaryPriceComponent`: `Category=Tax|Fee`, `Code`, `Name?`, `CountryId?`, `StationAirportId?`, `Amount:Money`, `FeeApplicationUnit?`, `TaxTreatment` (included vs added), currency consistency. One active revision per Provision. Price origin on Provision, not invented per Seat number stock item.

## Invariants
1. `Paid+Filed` requires actual filed, active rate in eligible currency/age/PTC context. `Paid+ExternalQuote` declares real quote provider and no fabricated rate. `Free` no Active Pricing and NO synthetic price 0. `NotAvailable` no purchasable rate.
2. Money persists `decimal(19,6)` with authoritative currency minor-unit source (0/2/3). Do not guess decimal scales. No manual FX if no verified rate. `CurrencyId` is not POS.
3. `GrossUnit = Base + AddedToBase taxes + Item fees`; `IncludedInBase` taxes informational and never summed twice. Ticket/OneWay/RoundTrip/nonitem fees separately `UnappliedFees`, with incompleteness until correctly evaluated.
4. Price by Seat commercial category from Definition + Provision + Pricing. `PricingUnit=PerSeat=5`; a live SeatNumber may choose category only if its characteristic/cabin facts were validated from a **trusted context**. Never infer from physical stock, occupancy, or FlightFlow calls in Ancillary.
5. A08 characteristics mandatory, A07 optional, A09 extra-seat purpose and external ticket routing semantics untouched. `DocumentRouting=Emd` requires EMD type + correct RFIC(1)/RFISC(3); `TicketOrExchange` requires Ancillary Document None, not fictitious EMD; `NoAncillaryDocument` None. Actual issuance P3 out of scope.
6. Active pricing immutability and copied revision mapping with new child IDs preserved; published Definition/Provision commercial semantics unchanged.

## Source-related failures
If currency reference is missing or inconsistent, use an explicit blocked/missing-price status, never `0`, `Unlimited`, or an arbitrary 2dp fallback. Missing external SeatMap must NOT affect standalone Filed or Free commercial pricing (unless a specific seat-number predicate cannot be verified; then that particular selection is **incomplete**, not free).

## Regression tests
PR-01 VAT included vs added; PR-02 Ticket Fee not multiplied; PR-03 EUR/JPY/KWD correct scales when valid reference; PR-04 missing reference fail closed; PR-05 filed and free Seat with absent policy; PR-06 nonSeat external quote incomplete without credible source; PR-07 Revise preserves rates and components; PR-08 currency/PTC/age rate selection and no POS mandate reintroduced.
