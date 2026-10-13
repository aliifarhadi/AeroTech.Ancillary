# 09 — Phase 2 is IMPLEMENTED: separate Seat Offers surface from other Ancillary Offers

## Why the split? — essential design decision
**YES to independent Seat shopping endpoints, NO to a second engine.** The current P2 `AncillaryShoppingEngine` already consumes trusted AirOffer, generates `AncillaryShoppingContext`, evaluates Definition/Provision/Pricing and converts candidates for `Api`, `OtaPanel`, `Backoffice`. Seat has a distinct seat-map UX and per-seat-characteristic price explanation, while ordinary service-list UI has quantity/forms. NDC distinguishes `SeatAvailabilityRQ/RS` and `ServiceListRQ/RS`; this supports external surface separation, not duplicate internal commercial rules.

## Current actual routes (reviewed HEAD)
All three surfaces currently expose `POST /{Surface}/v1/AncillaryOffers`, `POST /{Surface}/v1/AncillaryOffers/Details`, `POST /{Surface}/v1/AncillaryOffers/Selections/Evaluate`, `POST /{Surface}/v1/AncillaryOffers/SeatMaps`. `SeatMaps` is **NOT WORKING**: `GetSeatMapService.ExecuteAsync` calls a disconnected `IFlightSeatMapReference` and always throws `SeatMapSourceUnavailable`. Current `ApiAncillaryOffersMapper` groups bags separately, but maps Seat as another `OtherService`; `AncillaryShoppingEngine` erroneously loads and checks Seat inventory policy. Preserve the useful implemented Shopping P2, correct these routes and code paths.

## v12.3 target external contract (proposed NEW routes, implement in this change)
For each authorized `Surface` in `Api`, `OtaPanel`, `Backoffice`:

| HTTP | Route suffix | Behavior |
|---|---|---|
| POST | `/{Surface}/v1/AncillaryOffers` | Only nonSeat A01-A06/A10-A24 candidates; existing mapper/surface-specific output remains otherwise compatible |
| POST | `/{Surface}/v1/AncillaryOffers/Details` | Only details of nonSeat candidate, reject Seat refs with existing not-found/invalid-scope error |
| POST | `/{Surface}/v1/AncillaryOffers/Selections/Evaluate` | Only nonSeat candidate evaluation; preserve all P2 behavior |
| POST | `/{Surface}/v1/SeatOffers` | ONLY Seat A07/A08/A09, **commercial price and eligibility**, no physical seat list |
| POST | `/{Surface}/v1/SeatOffers/Details` | Seat-only variant/PriceOrigin/pricing unit, rule breakdown, requirements and applicability |
| POST | `/{Surface}/v1/SeatOffers/Selections/Evaluate` | Reuse current selection engine; Seat-only candidate, do not assert availability or mark unverified number as fully validated |
| REMOVE | `/{Surface}/v1/AncillaryOffers/SeatMaps` | Bad stub is retired; do not rename/proxy external FlightFlow seatmap under Ancillary |

`{Surface}` examples are literal path prefixes `Api`, `OtaPanel`, `Backoffice`, not a new dynamic wildcard controller. Keep `[Authorize(SurfaceAuthorization.*)]` and exactly current per-surface sales scope resolution, JWT policies and tenant boundaries. No changing the external FlightFlow service or other repository.

## Reuse existing core exactly
1. Keep `IAncillaryShoppingEngine.ShopAsync(AncillaryShoppingContext, ShoppingFilter?, CancellationToken)` and `EvaluateSelectionAsync`. Its `ShoppingFilter.Profiles` already permits family filtering. Use `Profiles={Seat}` for Seat routes and an explicit **excluded Seat** predicate/family filter for nonSeat; if existing filter supports inclusions only, pass the finite eight nonSeat profiles. Apply before expensive inventory checks/price computation, not merely after serialization.
2. Reuse `IGetAncillaryOffersService` and AirOffer/airport/airfare source + per-surface `SalesScopeResolver` and mapper patterns. Extend service contract to accept explicit family scope in a nonbreaking manner for internal callers; new lightweight Query Handlers should pass Seat or nonSeat filtering. Centralize family check so Details and selection evaluation cannot cross routes by supplying another family's `OfferItemRef`.
3. Do NOT create `SeatPricingEngine`, `SeatRuleEngine`, duplicate entity hierarchy, or a second definition/pricing reader. One canonical candidate (`DefinitionVersionId`, `ProvisionId`, `PricingRevisionId`, traveller/coverage, price, eligibility, selection contract) used by both surfaces.
4. Seat commercial output must include `OfferItemRef`, `AirOfferId`, traveller and flight/segment/portion scope, A07/A08/A09, commercial name, applicable cabin and characteristics, PricingUnit=PerSeat, PriceOrigin, currency and filed amount/tax/fee breakdown or Free/QuoteRequired/Incomplete, selection requirement for SeatNumber and exit-row acknowledgement, readiness and reason codes. Project with variant-specific DTOs, not an arbitrary payload bag. Do not promise list of physically available 12A/12B seats.
5. **Per-seat-number pricing**: same candidate price can apply to all actual seats matching its commercial category. To decide whether number 12A matches `A08(E)`, engine needs trusted seat characteristics, not just flight offer ID. `AncillaryShoppingContext.SeatCharacteristicFacts` already models `FlightRef`, `SeatNumber`, `CharacteristicCodes`, `SourceCompleteness`, `SourceAuthority`. Current `FlightOfferShoppingContextMapper` does **not** populate these facts. Thus initial SeatOffers must be a commercial type/rule list; exact seat-number selection without trusted facts stays `Incomplete/NeedsVerification` and must NOT invent price applicability. If implementing trusted facts ingestion via internal BFF is outside repository/current auth contract, report this gap explicitly rather than accepting browser-supplied characteristics as verified. Do not build the external BFF or a direct FlightFlow connection here.
6. **Readiness for Seat without policy**: bypass Seat internal InventoryPolicy and `AvailabilityAssessor` provider checks, and policy-driven `QuantityAssessor` cumulative checks. Enforce Provision min/max quantity and price/eligibility rules. Canonical availability may use appended `InventoryCapacityReadState.NotApplicable=8` strictly for commercial Seat; `Authority=null`, `CheckedState=NotChecked`, `IsGuaranteed=false`; omit misleading availability fields from specialized Seat public DTO. `MustCheckAvailability=true` remains external check metadata, not a pricing blocker.
7. Missing pricing/FX reference => no fabricated zero or selectable total. `PriceOrigin=Free` is the only basis for an explicitly free Seat. BFF failure strategy and seat stock rendering are outside this service scope; no SeatMap fallback or guessed ``all free``.

## Canonical response example (SCHEMATIC, not current wire DTO)
The Agent MUST inspect current `ApiAncillaryOffersDto`, OTA panel and Backoffice envelope conventions and produce real typed DTOs accordingly. The following shows REQUIRED information and missing-inventory semantics, not a mandate to copy names blindly:

```json
{
  "offerId": "<trusted-air-offer-id>",
  "seatOffers": [
    {
      "offerItemRef": "<server-generated-scoped-candidate-ref>",
      "travellerRefs": ["T1"],
      "flightRefs": ["B1:F1"],
      "variantCode": "A08",
      "seatCharacteristicCodes": ["E"],
      "applicableCabinIds": [1],
      "pricing": {
        "origin": "Filed",
        "unit": "PerSeat",
        "currencyId": 1,
        "completeUnitTotal": 35,
        "pricingRevisionId": "<current-pricing-revision-id>"
      },
      "selectionFields": ["SeatNumber", "ExitRowTermsAccepted"],
      "physicalSeatAvailability": "NOT_PART_OF_THIS_API"
    }
  ]
}
```

`35`, currency `1`, cabin `1`, `E` are **illustrative only**, not computed or valid fixture defaults; do not hardcode them. In the REAL public DTO, omit `physicalSeatAvailability` entirely, rather than serializing this demonstration string. Keep appropriate price code/tax/fee, eligibility/readiness/status and source identity. An `OfferItemRef` is not a SeatNumber reservation. If AirOffer alone cannot verify a seat-number-to-characteristic match, do not claim that match; return a type-level offer or explicit Incomplete selection status.

**Boundary test:** `SeatOffers` must function with FlightFlow/SeatMap services absent; `AncillaryOffers` must still price A01/A13/etc. with their current policy rules. This is the core invariant, not a P3 booking task.

## Tests: SH-01..SH-16
- SH-01: Current supported AirOffer + active A07/A08/A09 yields SeatOffers only; old AncillaryOffers no Seat candidates.
- SH-02: Bag/Pet/Meal/Lounge/WiFi etc remain in nonSeat response; same POS/currency as before.
- SH-03: three surface routes and real HTTP auth + owner scoped, no scope leak or cross-family `OfferItemRef` replay.
- SH-04: A08 with valid filed rate yields correct category price with **no policy/no FlightFlow HTTP**.
- SH-05: A07 Free shows explicit origin Free, not inferred zero due to failure.
- SH-06: no filed price/invalid currency remains incomplete, not 0.
- SH-07: A08 seat characteristic mismatch rejected on trusted facts; absent facts -> NeedsVerification (not silently matched).
- SH-08: A09 extra seat purpose/count validated from definition; never invents occupancy.
- SH-09: exit-row age/safety check remains and missing facts do not auto-pass.
- SH-10: new SeatOffers selection evaluation preserves PricingRevisionId, accepted/changed Provision and reason codes.
- SH-11: generic nonSeat `/Details` and `/Selections/Evaluate` reject Seat `OfferItemRef`; reverse also rejects nonSeat.
- SH-12: no Ancillary `/SeatMaps` route remains, no SeatMap stub in DI or query.
- SH-13: zero Seat InventoryPolicies in DB; commercial readiness not NotConfigured.
- SH-14: optional POS filter absent => unrestricted within caller scope; explicit POS filters apply.
- SH-15: mixed AirOffer (2 travelers, 2 flight legs) candidate coverage/currency per traveler/flight correct.
- SH-16: complete TaxTreatment, per-item fees, no double tax for Seat and regression nonSeat.

Tests must run actual existing Shopping unit tests plus new route/SQL integration tests. **Shopping is P2 and MUST NOT be deferred to P3.** P3 alone covers Hold/Reservations and physical seat allocation.
