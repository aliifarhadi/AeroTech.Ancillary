# 01 — Target Architecture, Dependencies, Commercial-Scope Parity

## Mandatory pipeline

```text
 BackofficeRequest + SurfaceAuthorization.Backoffice -> BackofficeScopeResolver --+
 ApiRequest        + SurfaceAuthorization.Ota        -> ApiScopeResolver        --+--[trusted sales context / authorized POS set]--+
 OtaPanelRequest   + SurfaceAuthorization.OtaPanel   -> OtaPanelScopeResolver   --+                                             |
                                                                                                                                v
                               IFlightOfferProvider (AirAvail Service/v1/FlightOffers/Details), typed wire -> trusted FlightOffer
                                                                      |
                                                       FlightOfferShoppingContextMapper
                                                                      |
                                        IAncillaryShoppingEngine.ShopAsync(context) (one POS scope at a time)
                                                                      |
                                      canonical ShoppingReadModel (source itinerary + raw canonical candidates + per-POS provenance)
                                                    /                 |                 \
                                             BO mapper            API mapper          Panel mapper
                                             BO DTO               API DTO             Panel DTO
```

**Shared orchestration must not depend on or instantiate any surface DTO.** The surface-specific mapper is the only stage converting canonical values to transport/view DTO. The shared orchestrator must return `CanonicalShoppingSnapshot` (a thin app-level aggregation: `FlightOffer`, verified context, `CanonicalAncillaryOfferResult` for each authorized POS; context/completeness; no duplicated pricing or rules). This wrapper is NOT a new Domain aggregate nor a second business engine. The engine remains the authority for eligibility, price, quantity, availability and selection; no mapper should recompute commercial outcomes.

## Suggested module boundaries (names flexible, invariants binding)

- `Shopping` assembly: **only** canonical `AncillaryShoppingContext`, `IAncillaryShoppingEngine`, result and typed `AncillarySelection`. Keep P3.1 reader and rule logic here. Current `Shopping/FlightOffers` integration-specific classes should migrate to an `Integration/FlightOffers` or `Providers`+`Query/Application` boundary; do not invert assembly references to create a dependency cycle. Port/model should be in an app-level contracts layer independent of the producer wire; do not violate existing `P1_A02` Domain guardrail or force a new global clean-architecture framework.
- `Providers`: AirAvail HTTP wire and its mapper to source-neutral typed FlightOffer. `ReferenceData`/POS reader integration. No UI DTO and no price logic.
- Shared Query/Application orchestration: source read, trusted fact enrichment, authorized POS-set fanout, Engine invocation and canonical aggregation. **No controller DTO type or surface enum dependency**. Read-only.
- `Query/AncillaryOffers/Backoffice`: `Backoffice...Request`, query, validator, `BackofficeSalesScopeResolver`, `Backoffice...Response`, mapper, handler.
- `Query/AncillaryOffers/Api`: `Api...Request`, query, `ApiSalesScopeResolver`, `Api...Response`, mapper, handler.
- `Query/AncillaryOffers/OtaPanel`: `OtaPanel...Request`, query, `OtaPanelSalesScopeResolver`, `OtaPanel...Response`, mapper, handler.
- `RestApi/V1/AncillaryOffers/Controllers`: **three** independently decorated Controllers. Do not expose a general `{surface}` route with switched roles. Preserve framework BaseResult envelope and ApiVersion.

## Surface parity with AirAvail — source is the authority

**Backoffice input:** `{offerId, options?}` with the five exact AirAvail fields/types. The explicit POS is accepted as AirAvail currently does; **do not unilaterally add an extra validator, replace with token, or ban a cross-channel OfferId**. These are cross-service policy decisions. Do not rewrite business policy in this scope.

**Api and OtaPanel input:** `{offerId}` exactly as their AirAvail `FlightOffers/Price` endpoints. Do not accept arbitrary `options`, `customerId`, `pointOfSaleId`, `currencyId` from the caller. Read agency `TravelAgencyId`, `CustomerId`, `TravelAgencyOfficeIds` from trusted claims and `X-Country` using source-equivalent semantics. Channel `API`, currency `70` (AirAvail current code TODO; centralize this parity mapping and document clearly; no permanent `magic` duplicated in resolvers), evaluation sale instant from clock. OtaPanel reuses same *commercial policy* via internal helper but has separate types/controller/mapper/policy. Trust `SurfaceAuthorization.Ota` and `.OtaPanel`; neither may authorize another surface's data.

**Backoffice office semantics:** source uses all actual airline offices resolved through Core reference for user, not the singular `airline_office_id` claim of current Ancillary. Preserve source parity; if the authoritative office lookup is unavailable, do not claim exact parity. Document `EXTERNAL_REFERENCE_BLOCKED` and fail closed for the POS-resolution branch; explicit POS branch should preserve AirAvail semantics.

**Named POS vs POS set:** P3.1 Engine requires one POS and published v12.2 provision has one POS. In shared orchestration, resolve a distinct authorized POS set and call the engine once per POS, then concatenate **distinct scope-preserving** results. Do not choose `First()`, infer a default, collapse several sellable offers to the cheapest, average prices or merge fees from different POS. Preserve `PointOfSaleId` as internal provenance in the canonical wrapper and tie the public `OfferItemRef` to it. If source returns empty set, return no applicable offers or source-parity error as justified, NOT an invented POS. If POS reference is unavailable, return typed unavailable, not fabricated success. Any enhancement that would alter AirAvail rules across services requires separate owner decision.

**Currency:** AirAvail API currently fixes currency 70. Backoffice uses preferred currency as supplied (0 means unspecified; use the actual source flight offer currency for Ancillary filed rates, never perform local FX). If desired currency has no published rate, `RateNotFound`/incomplete; do not silently switch currencies. Explain difference between source flight fare currency and ancillary price currency; never sum them without authoritative conversion and rounding.

**SalesDate:** match AirAvail request semantics. Maintain two concepts internally if needed: commercial/sales effective instant vs technical verification at current time. Never interpret AirAvail `PricedAt` as quote freshness. For source parity tests, assert source-derived commercial instant, but maintain expiry/freshness evidence independently; do not manufacture validUntil.

## Detail and selection read-model flow (same boundary)

- `GetDetails`: Surface request -> trusted sales scope -> fetch same source OfferId -> build same Context -> re-shop selected candidate in authorized POS scopes -> load **published** Definition/Provision/Price/Inventory details into canonical profile-specific `AncillaryOfferDetail` -> surface-specific DTO. This must not mutate orders or reserve stock. Re-check eligibility, fare/coverage and scope; never trust only IDs or cached client-supplied price.
- `EvaluateSelection`: accept typed/bounded customer selection and candidate reference from a previous Search, rebuild trusted context, call **existing** `IAncillaryShoppingEngine.EvaluateSelectionAsync`, return status, reasons, verified amount or `NeedsQuote`/`NeedsVerification`. Distinguish selection evaluation from final pricing/reprice and physical reservation; do not claim a bookable OfferItem/EMD until P3.3.
- CandidateRef/OfferItemRef is a read-only selection pointer, **not** a purchase token, hold, EMD or price guarantee. Must be bound to `offerId`, principal's permitted sales scope, traveler/covered flights and definition/provision and must be **re-evaluated** for each Detail/Selection call. A deterministic opaque ref or server lookup is acceptable; choose simplest repo-compatible approach; never trust raw client-supplied technical IDs as commercial authority. No arbitrary crypto with dev-secret fallback.

## Dependencies / non-goals

No changes in AirAvail, Ordering, FlightFlow; no Order/EMD/payment mutation; no new DB aggregates/migrations just for three surface DTOs. No accidental Phase3.3 implementation. Clean up old generic presentation DTO after migration and run deletion audit (all references/tests/DI/serialization). If POS eligibility, seat map or timezone authorities are not connected, expose typed blocked states instead of fake success; don't close those integrations by renaming `NotConnected` adapters.
