# AeroTech Ancillary — Phase 3.2.A / Backoffice OfferId Shopping
**Version:** v1.0 — 2026-10-11. **Status:** Implementation specification; NOT a claim that this endpoint already works. **Scope:** One READ-ONLY Backoffice endpoint only, backed by real AirAvail FlightOffers/Details and existing Ancillary Shopping Engine. No P3.3, purchase token, Hold, Confirm, Ordering write, payment, EMD or changes to external repositories.

## 1. Authority and reviewed source (pinned)
1. Owner requirement: `POST /Backoffice/v1/AncillaryOffers/` receives exactly an AirAvail priced `OfferId` and returns evaluated ancillary catalog with detailed Backoffice projection.
2. Canonical Ancillary v12.2 Phase1/2 model and Phase3 pack in existing Ancillary repo (`docs/AeroTech-Ancillary-v12.2/`, `docs/AeroTech-Ancillary-Phase3-v1.0/`). Preserve source of truth and enum IDs.
3. Live source inspected 2026-10-11:
   - AirAvail `aliifarhadi/AeroTech.AirAvail`, `k8s-stg@8c6f5d18e17caf9eb47dfd039c20df11f76840f9`. **Actually accessible and inspected in this audit**. Repository folder is `AeroTech.AirAvail/AirOffer/`, real namespace `AeroTech.AirOffer`.
   - Ordering `aliifarhadi/AeroTech.Ordering.Final`, `k8s-stg@cd50a2a5fd18a372f32f2d4efac08dee0510f6ba`.
   - Ancillary `aliifarhadi/AeroTech.Ancillary`, branch `feat/ancillary-v12.2-phase1-phase2-rebuild@856ccbe8b41bb46439d8d27b52ad647c4cf9c054` (P3.1 R1 committed); recheck HEAD at implementation start.
4. Public airline/PSS pages and Lufthansa JSON fixtures are **behavior benchmarks**, not exact AeroTech data classes.

### Exact source paths
**AirAvail** (prefix `AeroTech.AirAvail/AirOffer/src/`):
- `AeroTech.AirOffer.RestApi/V1/AirOffers/Controllers/Service2Service/ServiceController.cs`: `[Route("Service/v{version}")] [HttpPost("FlightOffers/Details")]`, returns `BaseResult<ServiceFlightOfferDetailDto>`.
- `AeroTech.AirOffer.Services/Dto/Service2Service/ServiceGetFlightOffersDetailRequest.cs`: `{OfferId:string}`.
- `AeroTech.AirOffer.Services/Services/Service2Service/ServiceFlightOfferDetailService.cs`: requires `JourneyOfferIdCodec.TryDecode(...).Commercial != null`; extracts channel, customer, country, currency, sales date; **re-prices dynamically**, then maps.
- `AeroTech.AirOffer.Services/Dto/Service2Service/ServiceFlightOfferDetailDto.cs`: producer wire contract, 199 lines.
- `AeroTech.AirOffer.Services/Mappings/ServiceFlightOfferDetailMapper.cs`: actual fare/coupon/baggage joins, `PricedAt` and LTD.
- `AeroTech.AirOffer.Services/Offers/JourneyOfferIdCodec.cs`: three-section priced OfferId; commercial segment is a plain encoding, **NOT a signed authorization token** and contains NO `PointOfSaleId`/buyer DOB.
- `AeroTech.AirOffer.Services/Services/OfferPrice/OfferPricePipeline.cs`: reconstructs current offer graph/prices; no immutable upstream quote guaranteed by that read alone.

**Ordering** (prefix `src/`):
- `AeroTech.Ordering.Providers/Offer/Services/OfferProvider.cs`: `HttpClient.PostAsJsonAsync("v1/FlightOffers/Details", new { OfferId = offerId })`, `OfferEnvelope<FlightOfferDetailResponse>` with JsonNumberHandling.AllowReadingFromString and JsonStringEnumConverter.
- `AeroTech.Ordering.Providers/Offer/Wire/FlightOfferDetailResponse.cs`: Ordering **consumer**, not producer, contract. Less detailed than latest AirAvail producer for PricingUnit/currency/stop/etc.
- `AeroTech.Ordering.Providers/Offer/Services/OfferResponseMapper.cs`: BoundOfferId -> BoundId lookup, maps traveller/ticket/coupon/flight/fare/price, no trust of orphan BoundOfferId.
- `AeroTech.Ordering.Domain/Providers/Offer/OfferReader.cs`: `ResolveCouponAirFareId` prefers coupon Fare pricing line `Reference`, falls back to first fare component; **for Shopping adapter use stricter ambiguity rejection where required**, no arbitrary fare assignment.
- `AeroTech.Ordering.Providers/DependencyInjection.cs`: `AddHttpClient<IOfferProvider, OfferProvider>` with `Offer:BaseUrl`. Consumer posts relative `v1/...`; producer route is `Service/v1/...`: effective URL is correct only when BaseAddress includes service prefix. Avoid assumed URI concatenation.

**Ancillary**:
- `src/AeroTech.Ancillary.Shopping/Context/*`: canonical context; `ShoppingStage.PreOrder`, travellers/portions/flights/per-traveller fare and baggage facts, completeness, source metadata.
- `src/AeroTech.Ancillary.Shopping/Engine/IAncillaryShoppingEngine.cs`: `ShopAsync(context,filter)` and `EvaluateSelectionAsync`.
- `src/AeroTech.Ancillary.Shopping/Results/*`: canonical candidacy, eligibility/price/readiness/selection schema, inventory and documentary metadata.
- `src/AeroTech.Ancillary.RestApi/V1/AncillaryServiceDefinitionAggregate/Controllers/BackofficeController.cs`: `[Authorize(SurfaceAuthorization.Backoffice)]`, `Backoffice/v{version}/...`; copy patterns, not business logic.
- `src/AeroTech.Ancillary.Domain/_Shared/Contracts/ICallerContext.cs`, `src/AeroTech.Ancillary.ServiceHost/CallerContext/ClaimsCallerContext.cs`: actor/office/company claims, **not a PointOfSaleId**.
- `src/AeroTech.Ancillary.RestApi/V1/_Shared/SurfaceAuthorization.cs`: real Backoffice authorization policy.
- `src/AeroTech.Ancillary.Providers/DependencyInjection.cs`: existing HTTP DI style; implement typed AirAvail provider here, not in Domain/Engine.

### Critical extra source discovery: do NOT use AirAvail's legacy AncillaryOffers engine
The same AirAvail ServiceController has `POST Service/v1/AncillaryOffers` and `POST Service/v1/AncillaryOffers/Details`, backed by **another** `AncillaryOfferEvaluator`, `AncillaryCatalogProvider`, `ServiceAncillaryOfferDetailDto` with old ProductId/PriceRuleId and code paths demonstrably limited to `ExtraBaggage` and `LoungeAccess` (source: `AeroTech.AirOffer.Services/Services/AncillaryOffers/AncillaryOfferEvaluator.cs`, `IsOfferedOn`). They are not the authoritative v12.2 Ancillary catalog; calling them or merging their returned items would introduce duplicate pricing/eligibility. The new endpoint must fetch **only** `FlightOffers/Details` from AirAvail and read products from Ancillary's published v12.2 catalog via existing P3.1 engine.

## 2. Proven vs not supplied
| Input need | AirAvail `FlightOffers/Details` | Mapping decision |
|---|---|---|
| AirTransport/BoundId, BoundOfferId, route/flight dates, operating/marketing airline, aircraft/cabin/RBD, FlightCapacityId, legs | Yes | Verified joins; legs are itinerary detail, not extra coupon/service charging units |
| Travellers `TravellerRef`, index, PTC and coupons by traveller/flight | Yes | Distinct travellers/coupon joins; do not collapse multi-PAX |
| Fare components per bound, AirFareId, FareBasis, FareFamily **text**, FareType **text**, BookingClass | Yes | Map verified per coupon; numeric FareFamilyId **not supplied** |
| Included baggage per traveller/coupon (pieces, weight, unit) | Yes | `0` legitimate if unit known; absent/empty unit = unknown, never free entitlement |
| `CurrencyId`, rates, flight totals, ticket totals, tax/fee lines | Yes | Price of FLIGHT only; not ancillary authored price; ancillary price from Engine |
| DOB, passenger age, guardian relation, exit-row eligibility | **No** | Unknown; no default adult DOB/age |
| PointOfSaleId, airline office and authorized seller / customer type | **No** | Resolve using trusted caller / owner-specific POS reference; cannot be client-supplied silently |
| Airport IANA zone, country, FareFamilyId authoritative code mapping | **No / not fully** | Trusted reference reader if exists, else unknown and explain impact |
| Seat-map / per-seat physical availability, consumed customer entitlement, provider quote | **No** | `NeedsVerification`/`NeedsSelection`/`NeedsQuote`; no fake guaranteed supply |
| Offer TTL | **Not exposed** | `LastTicketingDate` is last ticketing deadline **NOT OfferExpiresAt**; `PricedAt` mapper receives encoded `commercial.SalesDate`, **NOT evidence of fresh supplier quote**, so no fabricated TTL |
| Valid authentication/authorization merely because OfferId decodes | **No** | Authenticate Backoffice, bind owner/office/POS and validate full upstream result |

## 3. Mandatory integration blockers before saying DONE
1. **POS/actor resolution:** `ICallerContext` has airline office but no POS. Locate verified `AirPrice` PointOfSale reference/lookup keyed by owner + office + channel/country/customer; require unique resolution matching returned offer commercial context. If no real mapping exists, do not use first active POS or add POS to public request; return `POS_CONTEXT_UNRESOLVED`, mark gate BLOCKED and ask for the exact reference contract.
2. **Owner boundary:** `IOperatorAirlineResolver` can resolve home airline, but do not silently treat Marketing/Operating airline IDs as same. Validate allowed owner/operating relationship and cross-carrier support; when no verified agreement, mark other carrier services unsupported, never leak their tariffs.
3. **Authenticated upstream HTTP** and a sanitized real AirAvail response/test environment are required for `LIVE_E2E_PASS`, not just an Ordering DTO fixture. Configured URL without operational token is not proof.
4. **Timezone/country/fare-family sources:** missing facts may still permit product discovery but may not validate restricted offers. Preserve missing evidence, never fabricate fields.
5. **P3.1 R1:** report claims `238` Domain, `110` Shopping, `418` Acceptance passing on agent machine; report is committed, but those figures are not independently reproduced here. Keep new Backoffice acceptance tests separate from that claim.

## 4. Actual source links
- AirAvail producer controller: https://github.com/aliifarhadi/AeroTech.AirAvail/blob/8c6f5d18e17caf9eb47dfd039c20df11f76840f9/AeroTech.AirAvail/AirOffer/src/AeroTech.AirOffer.RestApi/V1/AirOffers/Controllers/Service2Service/ServiceController.cs
- AirAvail DTO: https://github.com/aliifarhadi/AeroTech.AirAvail/blob/8c6f5d18e17caf9eb47dfd039c20df11f76840f9/AeroTech.AirAvail/AirOffer/src/AeroTech.AirOffer.Services/Dto/Service2Service/ServiceFlightOfferDetailDto.cs
- AirAvail codec: https://github.com/aliifarhadi/AeroTech.AirAvail/blob/8c6f5d18e17caf9eb47dfd039c20df11f76840f9/AeroTech.AirAvail/AirOffer/src/AeroTech.AirOffer.Services/Offers/JourneyOfferIdCodec.cs
- Ordering consumer: https://github.com/aliifarhadi/AeroTech.Ordering.Final/blob/cd50a2a5fd18a372f32f2d4efac08dee0510f6ba/src/AeroTech.Ordering.Providers/Offer/Services/OfferProvider.cs
- Ordering mapper: https://github.com/aliifarhadi/AeroTech.Ordering.Final/blob/cd50a2a5fd18a372f32f2d4efac08dee0510f6ba/src/AeroTech.Ordering.Providers/Offer/Services/OfferResponseMapper.cs
- Ancillary Engine: https://github.com/aliifarhadi/AeroTech.Ancillary/blob/856ccbe8b41bb46439d8d27b52ad647c4cf9c054/src/AeroTech.Ancillary.Shopping/Engine/AncillaryShoppingEngine.cs
