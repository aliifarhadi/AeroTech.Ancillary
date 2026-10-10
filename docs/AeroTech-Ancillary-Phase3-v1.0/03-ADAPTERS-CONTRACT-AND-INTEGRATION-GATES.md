# 03 - P3.2 Shopping Engine Adapters and real ingress (DESIGN NOW; NO P3.2 CODE BEFORE P3.1 APPROVAL)

## A. Responsibilities

Adapter converts **verified input facts** from a specific source to `AncillaryShoppingContext` v1. All adapters call the exact SAME P3.1 `Shop/EvaluateSelection`; they cannot contain proprietary `if (A13)...` eligibility, price calculations, inventory math, separate profile registry, or accept a client-supplied `IsAvailable`/Money as final. In P3.2 the application boundary also manages Shopping HTTP, QuoteSelection orchestration, authenticated source resolution, opaque service offer tokens, read-only provider quotes, expiration and revalidation. Physical reservation remains P3.3.

```
AirAvail FlightOffers/Details -- FlightOfferContextAdapter --+
                                                       |
Authorized Order snapshot ----- OrderContextAdapter ---+--> CanonicalContext --> Engine
                                                       |
Trusted itinerary/fare ---------- DirectContextAdapter -+
                                                       |
                                       Candidate + selection validation --> OfferTokenStore
```

## B. `FlightOfferContextAdapter` - verified source and mapping

Ordering real source at `k8s-stg@cd50a2a5`: `OfferProvider.GetByOfferIdAsync()` issues `POST v1/FlightOffers/Details` (relative to `Offer:BaseUrl`) with `{ OfferId = offerId }`. It expects `OfferEnvelope<FlightOfferDetailResponse>` and calls `OfferResponseMapper.ToDomain` before `Order.Create`. These are proof of *Ordering's existing external consumer contract*, NOT proof of AirAvail repository implementation or actual staging response (AirAvail repository not accessible to this audit). The owner-supplied request has `offerId` but the live endpoint was not invoked from the available tools. Never copy an auth bearer token into code, test fixtures, logs or documentation.

Precise mapping audit for P3.2:

| Existing Offer wire field | Context target | Source issues / required checks |
|---|---|---|
| `OfferId` | `SourceIdentity.TrustedSourceReference` | Must be resolved against authorized current sales scope, validated expiry; never trust arbitrary decoded token payload |
| `LastTicketingDate` | `SourceIdentity.ValidUntilUtc` ONLY IF that is its actual semantic (otherwise separate TicketingCutoff fact) | LastTicketingDate != universally OfferExpiresAt; NEVER silently treat as equivalent |
| `CurrencyId` | `CurrencyId` | Check authoritative currency reference; no guessed decimals |
| `AirTransports[].BoundId` | `ShoppingPortion.PortionRef` | BoundId is separate from BoundOfferId |
| `AirTransports[].BoundOfferId` | Adapter-local join key | Used by PricingUnit.CoveredBoundOfferIds; NOT surfaced as final PortionRef |
| `AirTransports[].Flights[].FlightId/FlightVersion` | `ShoppingFlight` IDs/version | Flight must be in verified matched Bound |
| `Flights[].FlightCapacityId` | Source-specific later reservation binding (if needed) | DO NOT require it in core Engine or confuse with FlightId; Ordering forwards it to FlightFlow as a string ID |
| `MarketingAirlineId/OperatingAirlineId` | Flight airline IDs | Offer uses `long`; Provision rules use `int`; checked numeric conversion and real reference semantics required, otherwise block not truncate |
| `AircraftId/CabinClassId/RbdId` | Flight and per-passenger Fare facts as appropriate | AircraftId source `long?`, provision `int`; no string label to ID inference |
| `Origin/DestinationAirportId`, `Legs` | Flight/Portion location and via stops | Source `long` vs Rule `int`; airport country/IANA/local time must come from trusted reference, not inferred from UTC offset |
| `PricingUnits[].CoveredBoundOfferIds` | PricingUnit -> Portion mapping | Resolve through BoundOfferId mapping exactly as `OfferResponseMapper.MapPricingUnits()`; orphan => reject |
| `PricingUnits[].FareComponents[]` | candidate Fare facts | `AirFareId`, `BoundId`, `FareBasis`, `FareFamily` string, `FareType` string, `BookingClass` string. NEVER invent FareFamilyId or enum AirFareType when only text exists |
| `Tickets[].TravellerRef/TravellerIndex/PTC` | Traveller identity/PTC | DateOfBirth, Gender/Office/Customer/actor NOT in Offer Detail; obtain from authorized sales/input scope |
| `Tickets[].Coupons[].BoundId/FlightId` | per-passenger Fare and allowance coverage | Join by TravellerRef+FlightId/Bound; do NOT spread one fare/allowance to all travellers |
| `Coupons[].BaggagePieces/Weight/Unit` and Cabin equivalents | `TravellerFlightBaggageFacts` | Missing Unit in Ordering mapper => null allowance; do not interpret missing as 0 |
| `Coupons[].Pricings` fare line `Reference` | Fare mapping | Ordering `OfferReader` resolves `AirFareId` from Fare-category line reference; use real contract, no randomly first fare if ambiguous |
| `RatesOfExchange`, `OrderCharges` | optional trusted price reference facts, not ancillary authored rates | Base Flight price is NOT an Ancillary filed rate. No FX without verified rule/scale |

Additional fields for `ShoppingTraveller` DOB/Gender/guardian etc must come from verified upstream customer/passenger request. `PointOfSaleId/CustomerId/CustomerType` from authenticated sales context and identity, never from an unauthenticated caller string. `LastTicketingDate` must remain a *ticketing deadline*, not automatically a quote expiry; add separate field to canonical SourceIdentity if real offer TTL differs. Reconcile versioned Offer changes with Ordering's `OfferId` guard.

**P3.2 blocker:** Without readable AirAvail source / captured sanitized real `/FlightOffers/Details` response, do not declare the adapter `SOURCE_VERIFIED` or claim exact end-to-end parity. It can be implemented against the witnessed Ordering contract with a candid `CONSUMER_CONTRACT_VERIFIED / PRODUCER_NOT_VERIFIED` status, then source-verified later by fixture from live authorized response. The access token originally shared by owner is not to be reproduced/reused in pack or checked into tests.

## C. `OrderContextAdapter` (post-booking, post-ticketed)

Only load Order via authorized Ordering read contract, not Ancillary's own copy/SQL cross-service join. Normalize actual `OrderTraveller`, `OrderJourney/Segment`, `OrderAirTransportService` (actual accepted fare and baggage), `OrderService`, `OrderFarePricingUnit`, issued/void/refunded documents where policy needs it. Use committed OrderCommercialVersion for quote binding. Scope by requesting seller/authorized office. Existing services and documents must have complete/correct source-reported status; unknown refund consumption must not automatically re-credit usage. `PostTicketed` is an existing Provision stage and should be used only when ticket issuance is proven. For a **created but not yet ticketed Order**, there is no automatically valid Stage enum `PreOrder` or `PostTicketed`; do not silently reinterpret. Record a REQUIRED owner contract decision or specifically proven policy before claiming post-create/pre-ticket shopping is supported.

`OrderContextAdapter` cannot create/change an Order. Ordering owns accepted Order prices, service addition, funding, issuance and refund. Adapter must never expose internal seller Office or actor identifiers to other customer scopes.

## D. `DirectContextAdapter`

For internal test/backoffice/approved partner caller who supplies selected Flights/Fares but no OfferId: every fact must come with explicit authority/provenance. Accepting `{FlightId, FareBasis}` from an arbitrary client does NOT establish a trusted Fare, Currency, POS, or flight schedule. Need verified source readers; if absent, reject `UntrustedFact`/`InsufficientContext`. Never give `Direct` a bypass of the same eligibility and price rules.

## E. Thin Shopping HTTP/application contract (PROPOSED; NOT EXISTING)

Preserve repository conventions (existing `Service/v1/Ancillaries/Service-Holds` route exists for shallow reservations). Proposed separate internal logical endpoints when P3.2 is authorized:

```
POST /Service/v1/Ancillaries/Offers/Search
  { source: { type: "Offer"|"Order"|"Direct", offerId?:..., orderId?:... },
    filters?: { variants?:["A01"], travellerRefs?:["T1"], flightRefs?:[...] } }
  -> CanonicalAncillaryOfferResult projection (never exposed untrusted internal CandidateIdentity as a valid buying token)

POST /Service/v1/Ancillaries/Offers/Selections/Quote
  { contextRef:opaque, candidateRef:opaque, selection:typed-profile-payload }
  -> BoundServiceOffer { serviceOfferId:opaque, price/availability/verification/expiry,... }

GET /Service/v1/Ancillaries/Offers/{serviceOfferId}
  -> authenticated token-scoped read/revalidation; never a free unscoped browse of other POS offers
```

Concrete URI/name and HTTP auth must be checked against current project conventions at P3.2 gate. **No public `POST /orders/{orderId}/services` in the Ancillary repository**: that is Ordering/agency surface. A thin public Add may take `serviceId/travelerId/quantity` only if selection and quote were already bound and verified in `serviceId` lookup.

P3.2 token model (authoritative server store or authenticated integrity-protected compact token) binds:
`OwnerAirlineId, POS, AuthorizedActorScope, SourceKind/Ref/Version, DefinitionVersionId, ProvisionId, PricingRevisionId+RateId if filed, TravellerRefs, Flight/PortionRefs, exact typed selection canonical digest, allowed accepted quantity, Money/currency/components including tax treatment and fee bases or supplier QuoteRef, Booking/Document routing, Quote expiry and context digest.` Must not contain raw PII or medical inputs. Immutable snapshot is not a guarantee of unchanged provider availability; verify before sale/Hold. TTL is the **minimum** of proven Offer validity / real pricing quote validity / explicit owned token TTL; do not invent external guarantee. Client cannot submit final price or substitute another traveller/office.

Validation: Token absent/expired/modified => reject. Changed OrderCommercialVersion, changed active published rule/price/provider quote => reject/requote per correct commercial rule; do not silently reprice an already accepted Order. Same-bound multi-flight offer holds one commercial line unless rule says otherwise.

QuoteSelection flow must evaluate typed input using P3.1, possibly query a *trusted* external quote/seat availability source if a real integration is wired, then produce bound offer or `QuotePending/NotAvailable/QuoteSourceUnavailable`. Supplier pricing `ExternalQuote` **never** falls back to 0 or customer amount. The Engine is still sole owner of eligibility/price-origin rule decisions; orchestration just supplies verified quote evidence and retries.

## F. P3.2 tests required for approval

1. Sanitized **real** AirAvail response (or explicit producer-blocked), compare adapter Context with known per-traveller coupon, BoundOfferId map, fareId and baggage; preserve 2-flight Portion.
2. FareFamily string vs numeric `FareFamilyId` mismatch => unresolved, never guessed; `AirFareType` text vs enum requires resolver; long->int overflow blocked.
3. `LastTicketingDate` not treated as Offer TTL unless explicitly proven.
4. Equivalent verified facts from Offer and Order paths yield equivalent shopping result where purchase stage and fare state are also equivalent.
5. Authorized POS vs forged customer/office; prevent cross-office leakage; no trust of user-submitted amount, customer ID or age.
6. All 24 typed forms survive HTTP validation; free/quote/selected-seat differ correctly; Offer token tampering/TTL and unknown versions fail closed.
7. Original `AirAvail` auth token is never logged, stored or reused in tests. No other repository modification.
8. Endpoint denied for deleted/retired/suspended product, stale pricing/price-origin contradiction, sold-out/unknown provider evidence.
9. Real call integration, bounded latency, idempotent reference issue, and no N+1 source calls; report producer/test environment availability separately.
