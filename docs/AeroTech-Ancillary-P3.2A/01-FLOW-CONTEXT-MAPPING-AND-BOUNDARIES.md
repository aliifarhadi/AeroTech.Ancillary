# 01 — Runtime flow, exact Field Mapping & Ownership

## 1. The ONLY authorized use case
`POST /Backoffice/v1/AncillaryOffers/` → Backoffice authorization → resolve owner/office/POS → AirAvail `POST Service/v1/FlightOffers/Details` `{offerId}` → validate response and commercial scope → `AirAvailFlightOfferContextAdapter` → existing `IAncillaryShoppingEngine.ShopAsync` → Backoffice mapper → response.

**No Order exists during this flow.** This is PRE-ORDER, does not store a booking, request an EMD, Hold a service, promise a seat/slot, or generate a purchasable `ServiceOfferId`. `CandidateIdentity` is for internal re-evaluation only. Later Quote/Selection and Order workflows are separate use cases.

```mermaid
sequenceDiagram
 participant BO as Backoffice operator
 participant API as Ancillary Backoffice API
 participant ACL as Auth/CommercialScope
 participant F as AirAvail FlightOffers Details
 participant M as FlightOfferContextAdapter
 participant E as Canonical Shopping Engine
 participant P as BackofficeOfferMapper
 BO->>API: POST /Backoffice/v1/AncillaryOffers/ {offerId}
 API->>ACL: authorize actor; owner; resolve unique POS
 API->>F: POST /Service/v1/FlightOffers/Details {offerId}
 F-->>API: data {airTransports, tickets, pricingUnits, charges}
 API->>ACL: validate owner/sales context vs resolved scope
 API->>M: map verified flight offer + scoped reference facts
 M-->>API: AncillaryShoppingContext, completeness evidence
 API->>E: ShopAsync(context)
 E-->>API: CanonicalAncillaryOfferResult
 API->>P: project without altering commercial facts
 P-->>BO: journey, travellers, ancillaryOffers, blocked, diagnostics
```

## 2. New components — smallest correct diff
- `IAirAvailFlightOfferDetailsProvider` + `AirAvailFlightOfferDetailsProvider`: typed `HttpClient` using project conventions, route derived from producer `Service/v1/FlightOffers/Details`, same JSON enum/number handling as Ordering, bounded timeout and cancellation, structured errors (timeout/invalid/missing/auth upstream), no silent fallback or retry storm. Never directly expose upstream error/PII.
- Local **consumer wire DTO**, validated against **AirAvail producer**, not by importing AirAvail assemblies or reusing Ordering Domain model. Prefer reuse exact compatible local wire types when available; no cross-repository project reference.
- `IFlightOfferShoppingContextMapper` + `FlightOfferShoppingContextMapper`: pure mapping from validated producer DTO, verified sale scope and optional authoritative reference data. NO rules/pricing/inventory or presentation conditionals.
- `IBackofficeAncillaryOffersService`: read-only orchestration, `IAncillaryShoppingEngine` invocation. Follow existing `Application`/`Query`/`RestApi` handlers and DI; avoid new aggregate/repository/table/event.
- `BackofficeAncillaryOffersMapper`: **presentation-only**. Must not re-run `Provision`, tax, customer pricing, quantity consumption, availability, or inventory rules.
- Controller `[Authorize(SurfaceAuthorization.Backoffice)]`, route and DTO specified in 02. Use existing framework response envelope conventions after verifying them.

## 3. Exact producer → canonical context mapping
| AirAvail producer field | Ancillary context field | Critical join / truth rule |
|---|---|---|
| `OfferId` | `SourceIdentity.TrustedSourceReference` | Must equal requested string after exact documented normalization; don't treat as signed token |
| `PricedAt` | **Diagnostic producer pricing/sale timestamp only** | Source mapper passes decoded `SalesDate`; **NOT reliable actual quote execution time** |
| `LastTicketingDate` | **Do not map to** `ValidUntilUtc` | Keep as output itinerary/ticketing condition if needed; NOT proof of offer expiry |
| `CurrencyId` | `CurrencyId` | Nonzero, verify scoped currency and positive reference scale; no fare-to-ancillary FX default |
| `AirTransports[].BoundId` | `ShoppingPortion.PortionRef` | Stable bound for output; distinct BoundOfferId |
| `AirTransports[].BoundOfferId` | join key only | Used to validate PricingUnits[].CoveredBoundOfferIds. Orphan/duplicate => source inconsistent |
| `AirTransports[].Sequence,Direction,Origin/DestinationAirportId` | `ShoppingPortion.Sequence,OriginAirportId,DestinationAirportId`; direction output | Context may carry `DirectionOrJourneyRef` separately if verified; don't fake Journey from direction code |
| `AirTransports[].Flights[].FlightId,FlightVersion` | `ShoppingFlight.FlightId,FlightVersion` | `FlightRef` deterministic and unique **within response**, e.g. opaque `F:<flightId>` only if no repeated ID within offer; otherwise include bound + sequence. Same flight in multiple bounds requires explicit disambiguation; don't let it silently collide |
| `Flights[].Sequence, Origin/Arrival, AirportIds, Marketing/OperatingAirlineId, AircraftId,CabinClassId,RbdId,BookingClass` | `ShoppingFlight` + traveler Fare fallback only when permitted by Engine | No unchecked `long->int` cast. Actual AirAvail ID types (`int` in producer) differ from Ordering consumer (`long`); use producer truth. Validate flight occurrence, ordering, timezone offsets, end > start |
| `Flights[].FlightCapacityId` | Adapter-local trusted flight reservation reference | **Not** used as flight identity, seat uniqueness or Ancillary local inventory. Never hold seats here |
| `Flights[].Legs[].LegId,Stop,terminal,time` | Itinerary display metadata and derived via only when semantically provable | Technical leg != sold flight segment/coupon; no double charge. May derive intermediate locations carefully from legs; do not invent via when evidence ambiguous |
| `Tickets[].TravellerRef,TravellerIndex,PassengerTypeCode` | `ShoppingTraveller.TravellerRef,PassengerTypeCode` | Validate PTC by **actual source strings/enum mapping**; unique refs, one identity per traveler; DOB null, verified age null, stable identity null unless provided by verified integration |
| `Tickets[].Coupons[].BoundId,FlightId` | `TravellerFareFacts` / `TravellerFlightBaggageFacts` per `(TravellerRef, FlightRef, PortionRef)` | Must join 1:1 to actual bound and occurrence. Reject orphan or duplicate coupon; traveler may lack coupons only if consumer contract allows it and handled explicitly |
| `Coupons[].Pricings` where `Category==Fare` and `Reference` numeric | `TravellerFareFacts.AirFareId` | Respect coupon fare reference **first**; match to `PricingUnits[].FareComponents[].AirFareId` scoped to bound. No silently taking first ambiguous fare |
| `PricingUnits[].CoveredBoundOfferIds` | validation of bound-to-fare coverage | Map `BoundOfferId → BoundId` using actual producer; distinguish `ThroughOneWay`/`RoundTripFare`. Multi-bound fare needs matched coupon assignment per traveler, not blanket replication |
| `FareComponents[].FareBasis,FareType,FareFamily,AirFareId,BookingClass,CabinClassId,RbdId` | `TravellerFareFacts` typed actual values | `FareFamily` is **string**: display/code only; `FareFamilyId=null` without trusted resolver. `FareType` string must parse known enum only; if unknown -> unknown, not arbitrary enum. Avoid default first fare across PTC where coupon ref exists |
| `Coupons[].BaggagePieces/Weight/BaggageUnit`, cabin equivalents | `TravellerFlightBaggageFacts.Checked/Cabin...`, `SourceCompleteness` | Only `Verified` if unit and semantics are valid; null/empty unit means **unknown even when amount 0**. Verify WeightUnit accepted source names and no silently assumed KG. Do not treat free included baggage as bought ancillary |
| Airline/airport country, origin IANA zone | `ShoppingFlight.OriginCountryId/DestinationCountryId/OriginIanaTimeZoneId` | Source DTO does not include zone/country: only from authoritative existing reference. Null => rule-dependent `NeedsVerification`. UTC offset is not IANA zone |
| `CustomerId,CustomerType,PointOfSaleId,OwnerAirlineId` | commercial fields of context | **Not** all in producer. Derived via authorized caller and verified commercial/POS/owner data, NOT client input and NOT blind reading of decoded OfferId |
| `OrderCharges`, `RatesOfExchange`, flight totals | Optional Backoffice source-summary only | Ancillary `PriceAssessment` comes from published AncillaryPricing; flight source charges never included in ancillary product total |
| `SourceIdentity.SourceKind` | `Offer` | `ShoppingStage=PreOrder`, `OrderId=null`, `ValidUntilUtc=null` unless a genuinely authoritative offer expiry source exists |
| `CoverageCompleteness` | verified per-topic fields | `TravellersComplete/ItineraryComplete` only when validated; `FareFactsComplete`, `BaggageFactsComplete`, `FareBenefitsComplete`, `ExistingServicesComplete` MUST NOT be marked true from absence of evidence. Pre-order source contains no historical orders/usage; absence != verified zero |

## 4. POS and tenant resolution algorithm — do not bypass
1. Authenticate `Surface.Backoffice`, require valid airline-human context + operator/home airline/office scope. Confirm actual existing `ICallerContext` actor and AirlineOfficeId and operator ref.
2. Request the **real provider** for flight details; treat decoded OfferId commercial segment only as an *untrusted hint* until validated against caller and authentic partner context. The upstream detail response itself does not echo POS or decoded Channel/Customer, so missing correlation is a material contract gap.
3. Look for **existing real** sale-context reference: `PointOfSaleId` uniquely mapped to authorized AirlineOfficeId + channel/customer/country of the priced offer. Verify no cross-airline leak; ambiguous or missing => `POS_CONTEXT_UNRESOLVED` and STOP real sellable-result claim. `ICallerContext` alone is insufficient; AirAvail OfferId alone is insufficient. Do not hardcode `POS=1`.
4. For itineraries with distinct operating airlines, owner catalog is selected only for explicit authorized owner and verified carrier scope; absent interline/third-party Ancillary agreement cannot be called sellable.
5. If owner POS cannot be resolved, endpoint may return controlled error (no guessed catalog). Report needed source contract in agent closure report, do not degrade to an incorrect list.

## 5. Full-journey correlation and coverage rules
- Iterate *all* bounds, flights, tickets and coupons, including outbound/inbound, through fare, multi-leg technical stops, child/infant and multiple travelers.
- Business coverage: `Sector` = one flight segment; `Portion` = one bound with one or more flights; `Journey` = whole itinerary where provision explicitly says Journey; `Order` is excluded because no Order exists.
- Never charge one portion-scoped ancillary again per connected segment. Multi-PAX price differs by PTC/age and needs per-traveller evaluation.
- Some commercial criteria cannot be certified from an OfferId-only request. Preserve null and return `NeedsVerification`/`NeedsSelection`; don't invent missing traveler DOB, personalized fare benefits or usage ledger.
- A source OfferId from FlightOffers **Search** (unpriced 2-section ID) is not accepted by producer Details; test that case. No re-implementing local codec.
- **Important:** Source dynamically reprices the graph on Details call, so changing flight price/flight version may yield different details; never assert immutable/signed offer. Validate response consistency and show `sourcePricing` separate from ancillary pricing.

## 6. Boundaries
`AirAvail = Flight Offer commercial producer` (read only). `Ancillary = product definition, provisions, pricing, customer selection and availability assessment`. `Ordering = future accepted order, mutations, financial settlement, eTicket/EMD`. `FlightFlow = flight capacity; do not allocate here`.
