# P3.2B — Source Audit & Architecture Verdict

**Status: proposed authority for coding agent; not implemented.** Date: 2026-10-11.

## Source pins (read, not guessed)

- Ancillary: `aliifarhadi/AeroTech.Ancillary`, `feat/ancillary-v12.2-phase1-phase2-rebuild@d598ff7082b14483940f32dab1a7a053eae47b5d` (P3.2A-R1). Contains one `Backoffice/v1/AncillaryOffers` controller, one surface-specific query and resolver, **common `GetAncillaryOffersService` returning `AncillaryOffersDto`**, and common `AncillaryOffersMapper` / `AncillaryOfferMapper`. No `Api/v1/AncillaryOffers` or `OtaPanel/v1/AncillaryOffers` and no offer-details routes. `src/AeroTech.Ancillary.Shopping/Engine/IAncillaryShoppingEngine.cs` already exposes `ShopAsync(AncillaryShoppingContext)` returning `CanonicalAncillaryOfferResult` and `EvaluateSelectionAsync(context, candidate, selection)` returning `CanonicalAncillarySelectionEvaluation`. The engine and canonical candidate types must be kept as the authoritative decision layer.
- AirAvail producer: `aliifarhadi/AeroTech.AirAvail`, `k8s-stg@ac840401d38cb407af339d6fa07f98e052b1bd04`. `ServiceController` exposes `POST Service/v1/FlightOffers/Details` with `{OfferId}` and BaseResult; this is the **only upstream Offer Details call** of new Ancillary. Producer re-prices a priced OfferId and returns flights, tickets, coupons, fares, baggage, prices and currency; `PricedAt` is decoded sales date, **not** a quote expiry or now. Search-only/unpriced ID is refused; well-formed unknown flights can return HTTP 200 with empty journey; consumer must refuse empty journey.
- Ordering consumer benchmark: `aliifarhadi/AeroTech.Ordering.Final`, `k8s-stg@cd50a2a5fd18a372f32f2d4efac08dee0510f6ba`. `src/AeroTech.Ordering.Providers/Offer/Services/OfferProvider.cs` posts relative `v1/FlightOffers/Details` with envelope, tolerant string-numbers and enum wire conversions. Do not call Ordering to obtain pre-order Offer; Ordering remains order/payment/document authority, out of this delivery.

## AirAvail surface evidence (important: not a theoretical model)

| Surface | Source entrypoint | Search/pricing input | Commercial identity |
| --- | --- | --- | --- |
| Backoffice | `RestApi/V1/AirOffers/Controllers/Backoffice/BackofficeController.cs` | `POST Backoffice/v1/FlightAvailability/Price` body `{offerId,options?}`; `BackofficeSearchOptionsRequest` = `customerId:string?`, `countryId:long?`, `pointOfSaleId:long?`, `salesDate:DateTimeOffset?`, `preferredCurrencyId:int?` | `BackofficeSearchFlightOffersService.CreateCommercialContextAsync`: airline user + **all** user's offices from Core, channel BackOffice; explicit POS is singleton *without an additional authorization validation*, otherwise **allowed set** from AirPrice conditions. Customer parsed if numeric; price can take sales date/currency options. |
| API | `RestApi/V1/AirOffers/Controllers/Ota/OtaController.cs` | `POST Api/v1/FlightOffers/Price` `{offerId:string}` | `OtaSearchFlightOffersService.CreateCommercialContextAsync`: requires `TravelAgencyId`, uses customer claim, country `X-Country`, first agency office claim, `Channel.API`, `CurrencyId=70` (source TODO), clock sales date, allowed POS **set**. |
| OtaPanel | `RestApi/V1/AirOffers/Controllers/OtaPanel/OtaPanelController.cs` | `POST OtaPanel/v1/FlightOffers/Price` `{offerId:string}` | **same** Ota service/context logic as API, with distinct route/surface policy. Not a Backoffice options model. |

Source `AirOfferIdentityService.cs`: `travel_agency_id`, `travel_agency_office_id`, `customer_id` claims and `X-Country` header; it currently parses header as `long?`. Ancillary `ClaimsCallerContext.cs` currently has agency claims but **no CountryId**; add a surface adapter to match source semantics. `SurfaceAuthorization` already defines `Backoffice`, `Ota`, `OtaPanel` policies; reuse them.

**Important parity nuance:** AirAvail's enums have distinct integer assignments across repo copies (`Channel.BackOffice` is not numerically identical). Integrate by named semantics or explicit conversion, never assume enum wire ordinal parity.

**Known source model gap:** `FlightOfferSalesScope` / `AncillaryShoppingContext` require *one* PointOfSaleId. AirAvail eligibility returns a **set** of POS. The existing `IPointOfSaleEligibilityReference` is bound to `NotConnectedPointOfSaleEligibilityReference`; Agency API and OtaPanel cannot claim full source parity until this is connected. Do not substitute constant 1, first POS, office id, or fake eligibility. In the source-audit report the development catalog used an office ID where actual POS IDs differ; catalog data must be reconciled separately, not papered over in code.

## Architecture defect (confirmed)

Current `GetAncillaryOffersService.ExecuteAsync` fetches the source, builds context, invokes Engine and then calls **`AncillaryOffersMapper.Map`** and returns **`AncillaryOffersDto`**. `AncillaryOffersDto`, `AncillaryOfferDto`, `OfferJourneyDto` live in shared `Query/AncillaryOffers/Dto`; they leak backoffice-only concepts to the shared boundary. The Engine is source-neutral, but the orchestration/result projection is not: a second and third surface would be forced to reuse Backoffice DTO, fork rules or copy the pipeline. Refactor service boundary, not engine internals.

## Price / completion facts that survive refactor

- `PriceAssessor` selects exactly one rate in target currency, on applicable PTC/age; `AncillaryPricingRate.UnitTotal` = Base + Added Taxes + Item Fees; Included Taxes are already inside base. Never add flight fare to ancillary prices.
- `UnappliedFees` (Ticket/OneWay/RoundTrip/SectorOrPortion) must never be multiplied per unit, hidden or presented as a complete order payable amount; `RequestedQuantityTotal` nullable when incomplete.
- `PriceOrigin.ExternalQuote` remains unpriced until trusted provider quote. `Free` has no invented 0-money components. `OfferReadiness.Selectable` is not guaranteed stock and **not** authorization to purchase.
- Suspected `PerKilogram` multiplication issue remains a **separate engine semantic gate**: current code multiplies `CompleteUnitTotal * selection.Quantity` universally, without demonstrating per-kg billing weight. Never assert a full amount without verified quantity basis; retain nullable total until the exact product/selection basis is established.
- Without airport IANA time zone, fare family numeric mapping, DOB, trusted usage/seat facts or delegated inventory, per-rule results may be `NeedsVerification`/`NeedsSelection`/`NeedsQuote`. Do not fake those facts for three pretty DTOs. `LastTicketingDate` != Ancillary offer expiration.

## Objective

Preserve the existing P3.1 engine and published Phase1/2 domain. Refactor to one canonical shared orchestration, **three independent input and output contracts**, three source-parity sales-context resolvers, and a first-class details/read-only selection flow; prove the same commercial decisions across all surfaces with golden fixtures and real HTTP where available. P3.2 closure only, NOT booking/reservation/EMD or complete NDC certification.
