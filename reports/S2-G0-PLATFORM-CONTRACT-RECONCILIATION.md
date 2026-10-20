# S2 G0 — Platform Contract Reconciliation (no code)

| Item | Value |
|---|---|
| Base | `k8s-stg@5e99a3a49e77950bf5e81fda33b90a60703eb2db` "B1–B8 execution amendment v1.2"; working tree clean (`git status --short` = 0 lines); 520e493..5e99a3a = 72 files (`git diff --stat`) |
| Assignment | `02-S2-G0-NO-CODE-RECONCILIATION-AGENT-REQUEST.md`, after `01-S2-ROOT-CAUSE-AND-PLATFORM-STABILIZATION-v1.0.md` (chat attachments, 2026-10-20) |
| Code changed in this task | none. This report is the only new file. |
| Siblings read (read-only, local disk) | Ordering.Final `cd50a2a` (k8s-stg); AirAvail `590e21e` (branch `fix/offerId`); FlightFlow `962f418`; AirInfo `4587ce1`; AirPrice `eb4d504` (3 uncommitted ROE files, not used here); Dcs `d2a0408` (a stub). The GitHub 404 in document 01 does not apply to these local clones, but a local clone may differ from what is deployed. Every sibling fact below is labelled with its SHA. |

Labels used on every arrow and fact:
- `Current code`: present in HEAD source and wired in DI.
- `Fake-tested`: proven only by unit or acceptance tests with fakes.
- `External pending`: the runtime dependency does not exist or is not connected.
- `Design only`: written in a document, not in code.

---

## 0. Correction to my own earlier report

`reports/S2-CORRECTION-B1-B8-v1.2-CONFORMANCE.md` is inside 5e99a3a, and three of its statements do not hold at this SHA.

1. **Stale metadata.** It says the work is "in the working tree on `c865ccc`, not committed". The Owner has since committed and pushed it as `5e99a3a`.
2. **Wrong F-2 finding.** It says "A07/A08 in the generic list show `DetailsOnDemand`/`TypedInputRequired`". That is not what the real routes return:
   - Every List, Details and Single-Evaluate handler passes `OfferFamilyFilters.Services`. Batch does the same for its ancillary lines. Evidence:
     - `src/AeroTech.Ancillary.Query/Shopping/AncillaryOffers/Queries/GetAncillaryOffers/{Api,Backoffice,OtaPanel}/*QueryHandler.cs` (lines 24, 22 and 21);
     - the Details handlers (lines 28, 21 and 21);
     - the EvaluateSelection handlers (lines 32, 26 and 26);
     - `EvaluateBatchService.cs:57`.
   - That filter removes A07 and A08 (`Common/OfferFamilyFilters.cs:8,12-15`; `AncillaryVariant.StandardSeat="A07"`, `PreferredSeat="A08"`).
   - The finding came from the golden fixture, which shops with `ShoppingFilter.None` (`tests/.../ThreeSurfaceTests.cs:132` → `GetAncillaryOffersService.cs:35-36`). So the List goldens contain A07 items that no route returns.
3. **The test counts were claims.** The 360, 259 and 224 counts came from my working tree. I re-ran them on 5e99a3a in this task (section 5.4).

---

## 1. Architecture map (current flow)

### 1.1 OfferId flows

```
Client (Backoffice / Api / OtaPanel, authorized surface)
 │ POST {S}/v1/AncillaryOffers {offerId}                      [Current code]  RestApi/V1/AncillaryOffers/Controllers/*Controller.cs:25
 │ POST {S}/v1/AncillaryOffers/Details {offerId,offerItemRef} [Current code]  :29
 │ POST {S}/v1/AncillaryOffers/Selections/Evaluate {offerId,offerItemRef,selection}  [Current code] :33 (offerId only: *EvaluateSelectionQuery.cs:12-13 `OrderId => null`)
 │ POST {S}/v1/AncillaryOffers/Selections/EvaluateBatch {offerId,selections[]}       [Current code] :37
 ▼
SalesScope resolver (caller → owner airline, channel, POS ids, customer/agency)       [Current code] Common/SalesScope/*
 ▼
ShoppingSource (offerId XOR orderId)                                                  [Current code] Common/ShoppingSource.cs:19-25
 ▼
FlightOfferShoppingSource.LoadAsync → IFlightOfferProvider                            [Current code] Common/FlightOffers/FlightOfferShoppingSource.cs:31-39
 ▼
AirAvail POST Service/v1/FlightOffers/Details {offerId}                                [Current code → live AirAvail; Fake-tested with captured wire tests/.../Fixtures/Wire/flight-offer-details.json]
   Providers/Offer/Services/FlightOfferProvider.cs:12; AirAvail 590e21e .../Service2Service/ServiceController.cs:10,13
 ▼
PricedOfferScopeGuard (channel, customer, country)                                    [Current code] Common/SalesScope/PricedOfferScopeGuard.cs:55-65
 ▼
Engine (per POS) → List / Details / Evaluate result
EvaluateBatch with offerId: basket.allReady=false + OrderBindingRequired, no token   [Current code; Fake-tested] SelectionBasketRules.cs:64-67, EvaluateBatchService.cs:73
```

### 1.2 OrderId flows

```
Client  POST {S}/v1/AncillaryOffers | /Details | /Selections/EvaluateBatch | {S}/v1/SeatMaps  {orderId}   [Current code]
 ▼
ShoppingSource → OrderShoppingSource.LoadAsync                                        [Current code] Common/Orders/OrderShoppingSource.cs:19-37
 ▼
IOrderCommercialContextReader.FindAsync(orderId)
   DI = NotConnectedOrderCommercialContextReader → throws 16815 / HTTP 503           [Current code] Providers/DependencyInjection.cs:55; Providers/Orders/NotConnectedOrderCommercialContextReader.cs:68-69
   Ordering endpoint that would serve it: NONE                                       [External pending] Ordering.Final cd50a2a: OrderAggregate/Controllers/ServiceController.cs and InternalController.cs have constructors only, no actions
 ▼ (fake only from here)
FlightOfferShoppingSource.ContextualizeAsync(order.Air …)  — Air is a full FlightOffer supplied by Ordering, NOT re-read from AirAvail   [Fake-tested] OrderShoppingSource.cs:29
OrderShoppingContextMapper: ShoppingStage, OrderId, CommercialVersion, OrderTravellerId (unambiguous links only), ExistingServiceFacts   [Fake-tested] OrderShoppingContextMapper.cs:12-55
 ▼
EvaluateBatch: Settle → Link (B5) → Basket (B8) → JWS per line only when allReady   [Fake-tested] EvaluateBatchService.cs:64-74
```

### 1.3 SeatMap → Batch → token → Resolve

```
POST {S}/v1/SeatMaps {offerId|orderId}                                   [Current code] RestApi/V1/SeatMaps/Controllers/*:22 (SeatMapsDto unchanged)
  AirInfo GET Service/v1/Aircrafts/{id}/DisplaySeatMap                   [Current code → live; Fake-tested] Providers/AirInfo/Services/AirInfoAircraftSeatMapProvider.cs:11
  FlightFlow POST Service/v1/Flights/OccupiedSeats                       [Current code → live; Fake-tested] Providers/FlightFlow/Services/FlightFlowOccupiedSeatProvider.cs:11
  → 43-char seat id per AVAILABLE travelerPricing row
EvaluateBatch {orderId, selections:[{kind:SeatMap, seatMapSelectionId, exitRowTermsAccepted?}]}
  free seat (no commercial category) → Accepted, canPurchase, no engine call   [Current code] EvaluateBatchService.cs:110-119
  paid seat → engine re-evaluation in its POS context                          [Current code] EvaluateBatchService.cs:121-133
  token (JWS HS256)                                                            [Fake-tested] Common/PricedSelections/PricedSelectionTokens.cs:34-53
POST Service/v1/Ancillaries/Selections/Resolve {orderId, expectedOrderCommercialVersion:int, pricedSelectionRefs[], correlationId?}   [Current code, anonymous] RestApi/V1/AncillaryOffers/Controllers/ServiceController.cs:53,60
  re-runs EvaluateBatch per signed scope on the live Order (needs the Ordering reader again → 503 today)   [Fake-tested] ResolveSelectionsService.cs:61-72
  → per ref: Valid | Stale | Expired | NeedsQuote | NeedsVerification | Unavailable | Rejected; service{…} or includedSeat{…}   [Fake-tested] ResolveSelectionsService.cs:86-127
```

### 1.4 OrderChange / OrderService handoff (Ordering side)

```
Ordering calls Resolve                                         [External pending] Ordering.Final cd50a2a has no Ancillary client, no "Ancillary" config key; grep "Ancillar|pricedSelection|Selections/Resolve" over src = only an error message (Domain/_Shared/Resources/ExceptionMessages.cs:200)
Ordering adds OrderService for an ancillary                    [External pending] OrderService subclasses = OrderAirTransportService, OrderSeatService only (OrderAggregate/Entities); OrderItem created only as ProductType.AirFare (Order.Create.cs:246); OrderChangeType AddProduct exists as enum, not produced
Ordering adds/changes a seat after order creation              [External pending] OrderSeatService created only in Order.Create.cs:370-407; no add/change method; seat-only cancel refused (Order.Cancellation.cs:25-26)
Ordering Stage 6 "Ancillary/EMD"                               [Design only] Ordering docs/AeroTech-Ordering-Master-Domain-ADR-PRD-v2.0…:384-406 (§6.7 OrderAncillaryService "materialize in Ancillary stage, not now"), :2239-2245
```

---

## 2. Evidence table — consumed source facts and Resolve outputs

Owner authority codes:
- **O-SM**: Owner seat ruling, 2026-10-14/15 (no category = free AVAILABLE).
- **O-v1.1**: the Owner accepted v1.1 in chat on 2026-10-20 ("برای نکته 1 و 3 پیشنهادت قبول است. شروع کن").
- **v1.2-claimed**: delivered by the Owner as an attachment with no text. Document 01 RC-02 says the Assistant wrote it.
- **none**: no Owner decision found.

| flow/scenario | exact DTO field/type | origin owner | current source path | consumer target | verified? | failure behavior | Owner authority |
|---|---|---|---|---|---|---|---|
| all / sales scope | `AncillarySalesScope(int OwnerAirlineId, Channel, IReadOnlyList<long> PointOfSaleIds, long? TravelAgencyId, long? AirlineOfficeId, long? CustomerId, long? CountryId)` | Identity caller context; AirPrice POS conditions (`Service/v1/PointOfSales`, AirPrice eb4d504 PointOfSaleAggregate ServiceController:10-21) synced to ReferenceData | `Common/SalesScope/AncillarySalesScope.cs:81-91`; resolvers `Backoffice/Api/OtaPanelSalesScopeResolver.cs`, `AgencySalesScopeService.cs:25-45` | engine POS eligibility; token `Scope` | code-verified; live verified 2026-10-19 on 71f4470 only (S2-01) | no caller → 403 (ShoppingCallerNotAuthorized); no POS resolved → PointOfSaleContextUnresolved (`AuthorizedPointsOfSale.cs:19`) | earlier S1/S2-01 rulings |
| Customer/Agency POS | agency query `(Channel.PartnerAPI, CountryId, [], TravelAgencyOfficeIds.FirstOrDefault())` | Ancillary | `AgencySalesScopeService.cs:40` | POS list | **difference from AirAvail**: AirAvail fills only Country + Channel for fare visibility (AirAvail 590e21e `Legacy/Services/OfferSalesContextFactory.cs:30-36`), so the two services can select different POS for the same agency | — | none |
| offer ownership | `FlightOffer.SalesChannel`, `CustomerId`, `SalesCountryId` (decoded from the priced offerId; AirAvail S2S DTO has none of them) | AirAvail id codec | `Providers/Offer/Wire/PricedOfferId.cs`; guard `PricedOfferScopeGuard.cs:57-64` | guard | code-verified | 403 | 2026-10-15 ruling (memory seatmap-state) |
| Passenger | `FlightOfferTicket(TravellerRef, TravellerIndex, PassengerTypeCode, Coupons)` | AirAvail (`ServiceOfferTicketDto` 108-119) | `Application/FlightOffers/FlightOfferTicket.cs` | engine travellers | code-verified | — | — |
| Passenger DOB / infant link | `OrderTravellerFacts(TravellerRef, long OrderTravellerId, DateOnly? DateOfBirth, string? InfantOfTravellerRef)` | Order only. AirAvail has no DOB or infant link (grep `DateOfBirth\|BirthDate\|InfantOf\|AssociatedAdult` = 0 in AirAvail) | `Application/Orders/OrderTravellerFacts.cs`; mapped `OrderShoppingContextMapper.cs:35-39` | A18/A19 eligibility, PTC ages | Ordering `OrderTraveller` has `InfantParentTravellerId long?`, `AgeRange`, no DOB field on the entity (Ordering.Final `Entities/OrderTraveller.cs:32-46`); a DOB inside the profile revision: **UNVERIFIED** | missing → Unknown, NeedsVerification | none |
| OrderTravellerId | `long OrderTravellerId` (> 0, unique) | Ordering `OrderTraveller : Entity<long>`; `SourceTravellerRef string?` maps offer ref → id (Order.Create.cs:138-144, 289) | `SelectionBasketRules.Link` :35-50 | Resolve `orderTravellerIds[]`, `includedSeat.orderTravellerId` | Ordering type verified; transport **External pending** | unlinked/ambiguous → Incomplete + `OrderTravellerNotLinked` → Resolve NeedsVerification | v1.2-claimed (Q3) |
| Order version | `int CommercialVersion` | Ordering `Order.CommercialVersion int` (Order.cs:64; +1 on cancel Order.Cancellation.cs:55,65; OrderChange keeps its own) | `OrderCommercialContext.cs`; token `OrderCommercialVersion`; Resolve `expectedOrderCommercialVersion:int` | Resolve stale check :90-91 | type verified in Ordering source | mismatch → Stale `OrderVersionChanged` | O-v1.1 D2 |
| Order state | `bool AcceptsChanges`, `DateTimeOffset? TicketedAtUtc` | **No such fields in Ordering** (grep `AcceptsChanges\|TicketedAt` = 0). Nearest: `OrderStatus`; `ElectronicTicket.IssuedAt` per ticket | `OrderCommercialContext.cs` | refuse closed orders; PreOrder vs PostTicketed | **contract invented on Ancillary side; Ordering must derive them** | — | none |
| Order air facts | `FlightOffer Air` (full AirAvail-shaped offer incl. tickets/coupons) | Ordering would have to return it. Ordering stores `SourceOfferId string(512)` (Order.cs:56) and per-service air facts on `OrderAirTransportService` | `OrderShoppingSource.cs:26-29` | engine | **External pending; shape not agreed with Ordering** | empty OfferId → 422 (:26-27) | O-v1.1 D2 (port only) |
| Coupon states | `FlightOfferCoupon.IsOpenForUse bool?` | Ordering `TicketCoupon.FinancialStatus` Open/Used/Void/Exchanged/Refunded/Suspended + `ControlStatus`; IATA `CouponStatus` enum defined but unused in Ordering src | `Application/FlightOffers/FlightOfferCoupon.cs`; engine CouponUsability | post-ticket eligibility | Ordering types verified; mapping **not agreed** | null → candidate unverified (FLOW_09 test) | O-v1.1 |
| Flight refs / FlightCapacityId | `FlightOfferFlight(… long FlightCapacityId, long FlightId, int FlightVersion …)` | AirAvail `ServiceOfferFlightDto.FlightCapacityId` (58), set `cap?.FlightCapacityId ?? 0` (mapper :81) | `Providers/Offer/Services/FlightOfferResponseMapper.cs:85` requires > 0 | seat ids, Resolve `flightIds`, `flightCapacityId` | verified | 0 → 502 16807 FlightOfferIsInconsistent | S2-01 |
| FareBasis / RBD / cabin / fare family | `FlightOfferFareComponent(AirFareId, BoundId, CabinClassId?, RbdId?, BookingClass?, FareBasis?, FareFamily?, FareType?)`; per flight `CabinClassId?, RbdId?, BookingClass?` | AirAvail `ServiceOfferFareComponentDto` 155-174 | ContextMapper; fare family ids via ReferenceData `ReferenceDataAirFareReference` | provision fare rules | verified | missing → rule Unknown | — |
| Seat eligibility (exit row) | `ShoppingTraveller.VerifiedExitRowEligible bool?` | **no source anywhere**: AirAvail, FlightFlow, Dcs (stub) and Ordering have no per-passenger exit-row fact (greps listed in sub-reports); AirInfo has deck `ExitRowsX` + DB-defined `CharacteristicsCodes` only | engine `ProvisionEvaluation.ProfileRules.cs:190-191` | A08 | verified absent | null → Unknown `ExitRowEligibilityNotVerified` → seat BLOCKED in map | v1.2-claimed (Q5) |
| Seat occupancy | FlightFlow `FlightOccupiedSeatsDto{FlightId, Seats[]}` | FlightFlow 962f418 handler: **excludes seats held under allotment capacity** (`GetFlightsOccupiedSeatsQueryHandler.cs:23` `FlightCapacityAllotmentId == null`) | `FlightFlowOccupiedSeatProvider.cs` | SeatMap AVAILABLE/OCCUPIED | verified | missing flight → 16814 / 502 | O-SM |
| Fare benefit (free seat, priority, lounge) | `FareEntitlementFacts` | AirAvail has `IsSeatSelectionFree`, `HasLoungeAccess` internally (Persistence `FareOfferQueryResult.cs:28-29`) but **not in the S2S DTO** | ContextMapper | A23 included, seat free entitlement | verified absent from DTO | Unknown → not included | none |
| Baggage allowance | AirAvail `int BaggagePieces; int BaggageWeight; string BaggageUnit` (non-nullable, both always sent; `ServiceFlightOfferDetailDto.cs:131-136`) → `FlightOfferBaggage(int Pieces, decimal Weight, WeightUnit)` | AirAvail | `FlightOfferShoppingContextMapper.cs:152-170` | engine `SharedRules.cs:344-359` derives concept as `pieces is null ? Weight : Piece` | verified | pieces never null ⇒ concept always Piece ⇒ weight-concept products (A02) fail `BaggageAllowanceConceptMismatch` | none |
| Quantity | `AncillarySelection.Quantity int?`; provision min/max; cumulative limits | Ancillary | engine; `SelectionBasketRules.Settle` | Resolve `price.quantity` | Fake-tested | above max → `QuantityAboveMaximum`; split → `QuantitySplitAcrossLines` | v1.2-claimed (Q2) |
| Shared fee | `PriceAssessment.UnappliedFeeLines` = every fee whose `FeeApplicationUnit != Item` (OneWay, RoundTrip, SectorOrPortion, Ticket) | Ancillary pricing | `Shopping/Engine/PriceAssessor.cs:131,176`; `SelectionBasketRules.cs:54-62` | basket | Fake-tested | any such fee ⇒ `SharedFeeNotApplied`, allReady=false, no token for the whole basket | v1.2-claimed (B8 check list) |
| FX | `IRateOfExchangeReference.FindRatesAsync` (ReferenceData) | AirPrice ROE (synced) | `ServiceHost/ReferenceData/ReferenceDataRateOfExchangeReference.cs`; engine | price conversion | code-verified; the offer's own `RatesOfExchange` are carried but not used by pricing (they only enter the fingerprint) | missing rate → price incomplete | earlier pricing rulings |
| fulfillmentProviderKey | engine has `Confirmation.FulfillmentProviderKey`; Resolve DTO does **not** expose it | Ordering **requires** it on every `OrderService` (`string FulfillmentProviderKey`, required, max 64; Entities/OrderService.cs:45; Persistence OrderServiceConfiguration.cs:11-16); keys today: `FlightFlow`, `LocalDocumentAuthority` | `ServiceResolvedServiceDto.cs` (absent) | Ordering OrderService | verified both sides | Ordering cannot create an OrderService from Resolve output without a provider key | v1.2-claimed (Q4) vs Master 01:873 |
| document routing | `documentRouting, documentType, documentRequired, rfic, rfisc` | Ancillary definition | `ServiceSelectionResolutionMapper.cs` | Ordering EMD | Fake-tested; Ordering has no EMD/RFIC/RFISC code (grep = 0) | — | v1.2-claimed (Q4) |
| tax/refund properties | coupon `IsRefundable/IsChangeable/IsUpgradable`; no per-tax refundable flag in AirAvail (`ServiceOfferPricingLineDto` 176-188) | AirAvail | carried into fingerprint (`FlightOfferFingerprint.cs:93-95`); `CouponUpgradable` used by A10 (`FlightOfferShoppingContextMapper.cs:147`) | — | verified | — | — |
| Resolve service output | `ServiceResolvedServiceDto` (29 members incl. `OrderTravellerIds long[]`, `FlightIds long[]`, `FlightCapacityId?`, `SeatNumber?`, `Price`, `Selection`) | Ancillary | `Dto/Service/ServiceResolvedServiceDto.cs` | Ordering | **no consumer exists** | — | O-v1.1 + v1.2-claimed |
| Resolve included seat | `ServiceIncludedSeatDto(long? OrderTravellerId, TravellerRef, FlightId, FlightCapacityId, SeatNumber, CurrencyId, decimal Fee)` | Ancillary | `Dto/Service/ServiceIncludedSeatDto.cs` | Ordering `OrderSeatService(SegmentId, AssociatedAirServiceId, SeatNumber)` | Ordering can create seats only at order creation | — | O-v1.1 D3 |
| Order-scoped beneficiary | A22/A24 Order scope → `OrderTravellerIds = []` | Ancillary | `ServiceSelectionResolutionMapper.cs` | Ordering `OrderService.TravellerId long` **non-nullable** (Entities/OrderService.cs:41) | verified | Ordering cannot persist a traveller-less service | Master 01:886 records the conflict |

---

## 3. Authority conflicts (clauses quoted; HEAD shown; nothing decided)

| Topic | Clause A | Clause B | HEAD implements |
|---|---|---|---|
| Stored quote vs stateless | `docs/AeroTech-Ancillary-Master-Pack-v13.2-FINAL/08-S2-PRICED-SELECTION-IDENTITY-AND-VALIDITY.md:11` "`PricedSelectionSnapshot` [TARGET] — short-lived lookup" | v1.1 D1 (chat attachment, scratch copy `v11.md:27-32`): "Stateless reference … compact, self-contained, short-lived **signed** `pricedSelectionRef` … no DB" | JWS HS256, no table (`PricedSelectionTokens.cs:34-53`) |
| Signed vs encrypted token | `docs/ancillary_s2_execution_pack/03-API-AND-INTEGRATION-CONTRACT-FREEZE.md:121` "AES-GCM authenticated encryption or framework Data Protection" | v1.1 `v11.md:30` "Signing-only is sufficient because the token holds IDs and non-secret commercial values" | signed only; the payload claim is the full `PricedSelection` JSON (`PricedSelectionTokens.cs:45-49`) |
| OfferId after Order creation | Master `01-S2-OWNER-DESIGN-MASTER-v1.0.md:200` and `:62`: Offer path remains for preview; Add needs re-evaluation with `orderId` | the same Master, `:229-231`: two List URLs (`/Orders/{id}` route) and Evaluate batch on the **same** `Selections/Evaluate` route (`:221`); v1.1 D5/D6 (`v11.md:57,64-66`): separate `EvaluateBatch` route, no `/Orders/` routes, `offerId XOR orderId` on existing routes | v1.1 shape (D5/D6) |
| CommercialVersion type | `04-ORDERING-CONSUMER-AND-EXTERNAL-DEPENDENCIES.md:31` "`orderCommercialVersion` string", `:59,:77` `"V17"`; `03-…FREEZE.md:112` `"V17"` | v1.1 D2 `v11.md:38` "is an integer (`int`), not string `"V17"`"; Ordering.Final `Order.cs:64` `int CommercialVersion` | `int` |
| Basket tokens | `03-…FREEZE.md:90` "`allReady=false` means no basket Add, irrespective of individual status" (token per line not forbidden) | v1.2 Q1 (`v12-decisions.md`, B8): no `pricedSelectionRef` on any line unless the whole basket is committable | v1.2 (all-or-nothing) |
| Order scope traveller nullable | Master `:886`: Ordering `OrderService.TravellerId` non-nullable; "Ancillary must not invent a TravelerId"; Order-scoped commit = `EXTERNAL_PENDING` | v1.2 Q3: "A true Order-scoped A22/A24 line may have no traveller" (token issued) | token issued with `orderTravellerIds: []` (test `B5_a_true_order_scoped_service_has_no_beneficiary_and_still_gets_its_reference`) |
| Repeated same candidate | Master `:458`: duplicate exact candidate → **422** `DuplicateSelectionScope`, or canonical merge with real cumulative limits | v1.2 Q2: reject exact duplicates and quantity splits, distinct instances only if a domain rule allows | per-line `Rejected` with `DuplicateSelection` / `QuantitySplitAcrossLines` / `RepeatedInstanceNotAllowed`, HTTP 200 (`SelectionBasketRules.cs:14-23,73-83`) |
| Seat with no commercial category | `09-S2-SEATMAP-FROZEN-AND-VERIFIED-BEHAVIOR.md:43` "Physical Empty + no commercial product/unknown quote/source → `BLOCKED`"; `:42` free only with verified entitlement | Owner seat ruling 2026-10-14/15 "no category = free AVAILABLE" (O-SM); v1.1 D3 keeps it | free AVAILABLE, price 0 (`AncillaryShoppingEngine.SeatMaps.cs:108`; `EvaluateBatchService.cs:110-119`) |
| Supplier/provider handoff | Master `:836`, `:859`, `:873`: `fulfillmentProviderKey` "Y when required", S2S only; `03-…FREEZE.md:115` lists it inside `service{}` | v1.2 Q4: "No `supplierId` unless required. Do NOT add `fulfillmentProviderKey`" | absent from the Resolve DTO |

**Authority ambiguity:**
- v1.1 was accepted explicitly in chat, together with two of my suggestions.
- v1.2 arrived as a file with no text. Document 01 (RC-02) says the Assistant wrote it.
- The Owner then committed and pushed its implementation (`5e99a3a`). That is evidence the Owner accepted the code. It is not an explicit decision on each clause.
- Neither v1.1 nor v1.2 is stored in the repository. Only the pack, the S2 execution pack, the audit and the targeted prompt are under `docs/`.

---

## 4. Contract examples (field names from HEAD DTOs; `<…>` = placeholder, not a real value)

### E1 — Offer-only preview, Api, baggage
**Request**
```json
POST /Api/v1/AncillaryOffers/Selections/EvaluateBatch
{ "offerId": "<priced AirAvail offerId>",
  "selections": [ { "clientSelectionId": "bag-1", "kind": "Ancillary",
                    "offerItemRef": "<43-char ref from List>", "selection": { "quantity": 1 } } ] }
```
**Response**
```json
{ "data": { "type": "ancillary-selection-batch", "flightOfferId": "<offerId>",
  "lines": [ { "clientSelectionId": "bag-1", "kind": "Ancillary", "status": "Accepted", "reasonCodes": [],
               "canPurchase": true, "pricedSelectionRef": null, "validUntilUtc": null, "ancillary": { "…": "…" }, "seat": null } ],
  "basket": { "allReady": false, "currencyId": "<int>", "total": "<decimal>", "sharedFees": [], "reasonCodes": ["OrderBindingRequired"] } } }
```
- **Negative case:** a wrong ref gives `status: "Rejected"` with `OfferItemNotOffered`.
- **Proof today:** fake-tested only (`B8_an_offer_preview_is_priced_but_never_ready_and_never_needs_the_signing_key`).
- **Provable without Ordering:** yes. It needs live AirAvail and an agency token. **No live run on 5e99a3a.**
- **What Ordering consumes:** nothing.

### E2 — Pre-issue Order, baggage purchase
**EvaluateBatch request**
```json
POST /OtaPanel/v1/AncillaryOffers/Selections/EvaluateBatch
{ "orderId": "<orderId>",
  "selections": [ { "clientSelectionId": "bag-1", "kind": "Ancillary",
                    "offerItemRef": "<ref>", "selection": { "quantity": 1 } } ] }
```
**Response today (runtime):** HTTP 503, code 16815 `OrderSourceUnavailable`.

**Response with a fake reader:** `pricedSelectionRef: "<JWS>"`, `basket.allReady: true`.

**Resolve request**
```json
POST /Service/v1/Ancillaries/Selections/Resolve
{ "orderId": "<orderId>", "expectedOrderCommercialVersion": 3,
  "pricedSelectionRefs": ["<JWS>"], "correlationId": "<uuid>" }
```
**Resolve response**
```json
{ "orderId": "<orderId>", "orderCommercialVersion": 3, "correlationId": "<uuid>", "allValid": true,
  "results": [ { "pricedSelectionRef": "<JWS>", "status": "Valid", "reasonCodes": [], "includedSeat": null,
    "service": { "kind": "Ancillary", "serviceDefinitionRef": "<ref>", "definitionVersionId": "<id>", "provisionId": "<id>",
      "pricingRevisionId": "<id>", "rfic": "<rfic>", "rfisc": "<rfisc>", "ssrCode": null, "coverageScope": "<scope>",
      "orderTravellerIds": ["<OrderTraveller.Id>"], "travellerRefs": ["<ref>"], "flightIds": ["<id>"], "portionRefs": ["<bound>"],
      "seatNumber": null, "flightCapacityId": null, "selectedOptions": [], "price": { "…": "OfferExactPriceDto" },
      "requiresCheckAtReserve": false, "variantCode": "A01", "serviceTypeCode": "<code>", "serviceSubCode": "<code>",
      "bookingMethod": "<BookingMethod>", "bookingRequired": "<bool>", "documentRouting": "<DocumentRouting>", "documentType": "<DocumentType>",
      "documentRequired": "<bool>", "confirmationRequirement": "<ConfirmationRequirement>", "pricingRateId": "<id or null>",
      "selection": { "quantity": 1, "…": "all other fields null/false" } } } ] }
```
**Negative cases**
- Order version moved: Stale `OrderVersionChanged`.
- Flight changed: Stale `SourceVersionChanged`.
- Traveller not linked: NeedsVerification with `OrderTravellerNotLinked`.

**Provable without Ordering:** no.

**What Ordering would consume:**
- the `service` block;
- `fulfillmentProviderKey`, which is missing and required by Ordering's `OrderService`.

### E3 — Post-issue (ticketed) Order, second service
**Request:** the same as E2, with an Order whose `TicketedAtUtc` is set.

**Engine behaviour**
- The engine runs with `ShoppingStage.PostTicketed`.
- A coupon that is not open for use closes only that traveller and flight (`FLOW_03_FLOW_10`).
- An unknown coupon state leaves the candidate unverified (`FLOW_09`).
- Services the Order already holds reach the engine as `ExistingServiceFacts` (`S2_ORDER_…`).

**Status:** `External pending`. Ordering has no `TicketedAtUtc` on the Order and no coupon "open for use" read. The mapping from `TicketCouponFinancialStatus` is not agreed.

**Provable without Ordering:** no.

### E4 — SeatMap with free, paid and exit-row seats (Order source)
**SeatMap request:** `POST /Api/v1/SeatMaps {"orderId":"<orderId>"}`. With an `offerId` this step is live-capable.

**SeatMap response:** `SeatMapsDto` is unchanged. Each AVAILABLE `travelerPricing` row has an opaque 43-char `id`.
- A seat with no category is `AVAILABLE` at `0.00`.
- An A07 category seat is `AVAILABLE` at its price.
- A08 exit-row seats are `BLOCKED` for every traveller, because `VerifiedExitRowEligible` is never set in production.

**Batch request**
```json
{ "orderId": "<orderId>",
  "selections": [ { "clientSelectionId": "s1", "kind": "SeatMap", "seatMapSelectionId": "<free seat id>" },
                  { "clientSelectionId": "s2", "kind": "SeatMap", "seatMapSelectionId": "<paid seat id>" } ] }
```
**Resolve response**
- Free seat: `includedSeat {"orderTravellerId": "<id>", "travellerRef": "<ref>", "flightId": "<id>", "flightCapacityId": "<id>", "seatNumber": "14C", "currencyId": "<int>", "fee": 0}`
- Paid seat: `service {… "seatNumber": "3A", "selection": {"seatNumber": "3A", "exitRowTermsAccepted": null, …}}`

**Negative cases**
- A seat taken after signing: Stale `SeatSelectionNotOffered`.
- The same seat on two lines: `SeatSelectedTwice`.
- An ancillary line carrying `exitRowTermsAccepted`: 422.

**Proof**
- A positive A08 is fake-tested only, with a test decorator that sets eligibility (`tests/.../Fixtures/SeatMapDesk.cs`).
- The SeatMap step on an `offerId` is provable live with AirInfo and FlightFlow. The Order and Resolve steps are not.

**What Ordering would consume:** an add or change of `OrderSeatService`. Ordering has no such method today.

### E5 — Basket with a shared fee, then a price change
**Shared fee**
- Example: an A24 provision priced with a `TKT` fee whose `FeeApplicationUnit.Ticket` is set. That fee lands in `UnappliedFeeLines`.
- Result: `basket {"allReady": false, "sharedFees": [<line>], "reasonCodes": ["SharedFeeNotApplied"]}` and no ref on any line (`B8_an_unapplied_shared_fee_withholds_every_reference_although_each_line_is_accepted`).
- This applies to every fee unit except `Item`.

**Price change**
- A price change at Resolve with the same source version should give Stale `PriceChanged` (`ResolveSelectionsService.cs:124-126`).
- **No test asserts `PriceChanged`:** a grep over the test folder returned 0 hits. This path is untested.

**Provable without Ordering:** the shared-fee part only, with an offerId.

**Surface parity:**
- Backoffice, Api and OtaPanel batch requests share `SelectionLineRequest` (`*SelectionLineInputMapper.cs`) and one `EvaluateBatchService`.
- The three response mappers differ only in presentation.
- Parity is proven by tests through mappers, not through HTTP.

---

## 5. Test truth table

Column meanings:
- **Engine:** a positive or fail-closed engine result with a synthetic catalogue and context (`S2VariantMatrixTests` row, engine only, `Trip.Context()`).
- **Source:** the real source supplies the needed fact (sections 2 and 6).
- **Committable:** an Order-bound token is possible in code.
- **Operational:** Ordering can persist the service. That is **No for all 24**, because no Ordering consumer exists.

### 5.1 A01–A24

| Code | Engine | Source | Committable (code) | Operational | Evidence / gap |
|---|---|---|---|---|---|
| A01 | Accepted | yes (pieces) | yes | no | Matrix S2M-A01; B1 round-trip |
| A02 | Accepted only with `Trip.WeightAllowance()` | **no** | no from real sources | no | AirAvail always sends `int` pieces → concept Piece (`SharedRules.cs:358`); test `B1_a_weight_package_is_not_offered…` |
| A03 | Accepted per piece; per-kg no amount | per-kg **no** (no chargeable weight) | per piece only | no | `P32B_ENG_07`, `PRICE_04` |
| A04 | Incomplete (combination not verified) | no rule source | no | no | Matrix |
| A05 | Accepted | yes | yes | no | Matrix |
| A06 | Accepted, check at reserve | yes | yes | no | Matrix; B6 |
| A07 | Accepted (via SeatMap only) | layout + occupancy yes; allotment-held seats invisible (FlightFlow handler :23) | yes | no (Ordering cannot add a seat after creation) | B1 paid seat; **filtered out of generic List by `OfferFamilyFilters.Services`** |
| A08 | Accepted only with decorator-set eligibility | **no eligibility source** | no in production | no | `S2ExitRowSeatTests` uses `SeatMapDesk.VerifiedExitRows`; filtered out of generic List |
| A09 | Incomplete (adjacency) | no | no | no | Matrix |
| A10 | Incomplete — NeedsQuote | no quote provider | no | no | Matrix; B6 |
| A11 | Accepted, free | yes | yes | no | Matrix |
| A12 | Accepted with authored menu | authored only | yes | no | Domain refuses a definition without `MenuItemRef` |
| A13/A14 | Incomplete (animal age) | no evidence source | no | no | Matrix |
| A15 | Accepted, free, 48 h lead time | yes | yes outside lead time | no | Matrix (captured offer is inside lead time → blocked) |
| A16 | Accepted, free | yes | yes | no | Matrix; B3 |
| A17 | Incomplete (medical approval) | no | no | no | Matrix |
| A18 | Accepted with verified infant-guardian | DOB/link only from Order (AirAvail has none; Ordering DOB UNVERIFIED) | Order only | no | Matrix |
| A19 | Incomplete (guardian contacts) | no contact evidence | no | no | Matrix |
| A20/A21 | Accepted, check at reserve | facility reference `NotConnectedAirportFacilityReference` (Providers DI :53) | yes if authored | no | Matrix |
| A22 | Accepted per passenger | as A20 | yes; Order scope token issued with no traveller | no (Ordering `TravellerId` non-nullable) | Matrix; B5 |
| A23 | Accepted, free | fare benefit not in AirAvail S2S DTO | yes (as paid/free product), "included" unknown | no | Matrix |
| A24 | Accepted | yes | yes, unless priced with a non-Item fee (P-07) | no | Matrix; B1/B3/B6 |

### 5.2 A07/A08 filtering

- Proof in code: `OfferFamilyFilters.cs:8,12-15` plus the ten call sites listed in section 0.
- **No test exercises a query handler or `OfferFamilyFilters`.** A grep of `OfferFamilyFilters|QueryHandler(` over tests returned 0.
- The List goldens include A07, because they use `ShoppingFilter.None`.

### 5.3 Gaps named in the assignment

| Gap | Cause | Evidence |
|---|---|---|
| A02 weight | No allowance-concept flag in the AirAvail S2S DTO. The engine infers the concept from a nullable piece count that the source never sends as null. | Section 2, baggage row |
| A08 exit-row evidence | No owner of the fact in any sibling. | Section 2, seat eligibility row |
| A10 quote | No quote provider port is implemented. | Matrix row S2M-A10, "NeedsQuote" |
| A19 contacts | `Guardian…Provided=true` is never accepted as proof, and there is no evidence source. | Matrix row S2M-A19 |
| Shared fees | Every non-Item fee is never applied. There is no owner for allocating it. | `PriceAssessor.cs:131` |
| Provider assignment | Ordering requires `FulfillmentProviderKey`, and Resolve does not send it. | Section 2 |

### 5.4 Executed vs reported (re-run on 5e99a3a in this task)

| Command | Result |
|---|---|
| `dotnet build AeroTech.Ancillary.sln --no-incremental` | Build succeeded, 0 errors, 62 warnings |
| Shopping | discovered 360 (`--list-tests`) / executed 360, passed 360, failed 0, skipped 0 |
| Domain | discovered 259 / executed 259, passed 259, failed 0, skipped 0 |
| Application acceptance | discovered 224 / executed 224, passed 224, failed 0, skipped 0, no aborted line (2 min 6 s) |

- These are fake-backed suites. No live HTTP run was made for any S2 code after 71f4470. The S2-01 live run on 2026-10-19 covered 12/12 List/Details calls only.

---

## 6. Lean-code impact (concrete items only)

The 72-file increment (5e99a3a = 68 paths: 43 under `src/`, 24 under `tests/`, 1 report; c865ccc = 4 docs/report paths) maps one-to-one to B1–B8. I found no dead adapter and no wrong-layer placement. These are the concrete issues:

| # | Kind | Path:line | Observable consequence |
|---|---|---|---|
| L1 | contract mismatch (leak) | `Common/PricedSelections/PricedSelectionTokens.cs:45-49` (claim = full `PricedSelection` JSON, signed not encrypted); returned to agencies by `EvaluateBatch/Api/ApiSelectionBatchMapper.cs:26` | Any Api caller can base64-decode `pricedSelectionRef` and read `provisionId`, `pricingRevisionId`, `definitionVersionId`, `scope.customerId`, `travelAgencyId`, `pointOfSaleIds`. This contradicts the Api no-internal-id pin (`P32B_SRF_02`, List only) and v1.2 Q7.4. It follows v1.1 D1. Authority conflict, section 3. |
| L2 | contract mismatch (leak) | `GetAncillaryOfferDetail/Api/ApiAncillaryOfferDetailMapper.cs:33` uses the shared `OfferSelectionDetailsMapper.OptionGroups` | Api Details choices carry `sourceVersion "D{definitionVersionId}-P{provisionId}"` and `pricePreview.currencyId`, while the Api List omits both (since 7efee95). |
| L3 | contract mismatch (Api batch) | `Dto/Api/ApiSelectionBatchDataDto.cs` → `OfferBasketDto(…int CurrencyId…, SharedFees: OfferPriceLineDto[])`; `ApiSelectionLineDto.Seat: OfferSeatSelectionDto(… long FlightId, long FlightCapacityId, … int CurrencyId …)` | The Api batch answer exposes `currencyId` and internal flight and capacity ids that the Api List deliberately hides. The same surface follows two policies. |
| L4 | test not route-representative | `tests/.../ThreeSurfaceTests.cs:132` (`ShoppingFilter.None`) | The List goldens pin A07 items that the routes never return. My F-2 statement was based on this. No handler test protects the filter. |
| L5 | duplicated work | `ResolveSelectionsService.cs:63` → `EvaluateBatchService.cs:73-74` | Resolve re-runs the batch, and the batch signs fresh tokens when ready. Resolve then discards them: one HMAC per line per Resolve, never returned. It is harmless but unnecessary. |
| L6 | inferred rule vs source | `Shopping/Engine/ProvisionEvaluation.SharedRules.cs:358` | A weight-concept baggage product can never pass on real AirAvail data (section 5, A02). |
| L7 | invented port fields | `Application/Orders/OrderCommercialContext.cs` (`AcceptsChanges`, `TicketedAtUtc`, `FlightOffer Air`) | Ordering has no such fields or endpoint. The port shape is an Ancillary proposal, not a consumer-verified contract (RC-03). |

I have no other duplication or wrong-layer finding backed by evidence. I recommend no refactor.

---

## 7. Questions for the Owner (three groups)

Already answered, so not asked again:
- `int` version (O-v1.1 D2, matches Ordering);
- separate EvaluateBatch route and no `/Orders/` routes (O-v1.1 D5/D6);
- free seat with no category (O-SM);
- Ordering stays untouched (Owner, 2026-10-07).

### A — Shopping source and basket (P-01, P-02, P-03; L1)

**A1. P-01 and P-02: binding and token form.**
- **Option a:** keep the Order-bound stateless token, but make it **opaque to callers**. Either encrypt it, as `03-…FREEZE.md:121` asks, or move the readable payload behind a server key.
- **Option b:** keep signed-only JWS (v1.1 D1) and accept that agencies can read internal ids.
- **Compatibility:** both keep the routes and DTOs. Option a changes only the token string.
- **Recommendation:** a.
- **Earliest scenario affected:** E2 on Api.

**A2. P-03: all-or-nothing basket.**
- **Option a:** keep it (v1.2 Q1).
- **Option b:** issue tokens per purchasable line, and let Ordering decide atomicity, as v1.1 §3 says ("Ordering decides atomicity").
- **Compatibility:** b changes only when `pricedSelectionRef` is non-null.
- **Recommendation:** a until Ordering defines partial Add; P-07 decides how often a basket can ever be ready.
- **Earliest scenario affected:** E5.

### B — Ordering handoff (P-04, P-05; the Order-scope traveller)

Facts from Ordering.Final cd50a2a:
- No read endpoint for another service.
- Seats can be created only at order creation.
- `OrderService.FulfillmentProviderKey` is required.
- `TravellerId` is non-nullable.

**B1. Who defines the Ordering consumer contract, and when?**
- **Option a:** Ordering Stage 6 defines `OrderAncillaryService`, Add-from-selection and seat add/change, and Ancillary adapts to it, including whether Resolve returns `fulfillmentProviderKey` and how Order-scoped services carry no traveller.
- **Option b:** the Owner approves the current Resolve DTO as the target now, and Ordering implements to it.
- **Compatibility:** with a, the Resolve DTO may change (additively, at least `fulfillmentProviderKey`). With b, Ordering's `OrderService` must accept a nullable traveller and an ancillary provider key.
- **Recommendation:** a. The current DTO has never met a consumer.
- **Earliest scenario affected:** E2 and E4.

**B2. Commercial-context read.**
- **Option a:** Ordering exposes a Service read with the Order's air facts, coupon states, traveller ids and version. Ancillary maps it.
- **Option b:** Ancillary re-reads AirAvail by `SourceOfferId` and gets only travellers, version and coupons from Ordering.
- **Compatibility:** b changes `OrderShoppingSource` and shrinks the port.
- **Recommendation:** a decision is needed before any further Order-path code. I lean to b: AirAvail already serves the air facts, and Ordering would not have to re-serialize them.
- **Earliest scenario affected:** E2.

### C — Multi-occurrence, shared fee, closure (P-06, P-07, P-08)

**C1. P-06: repeated candidate.**
- **Option a:** keep the per-line rejection (HTTP 200 with reasons).
- **Option b:** follow Master `:458`, which returns 422 for the whole request.
- **Compatibility:** b changes the error behaviour of the batch.
- **Recommendation:** a. No Domain rule allows multiple instances today.
- **Earliest scenario affected:** A12 or A22 bought twice.

**C2. P-07: non-Item fees** (OneWay, RoundTrip, SectorOrPortion, Ticket).
- **Option a:** declare them unsupported for S2. The basket is never ready, and authoring could be warned.
- **Option b:** name the owner of allocation (Ancillary per Order, or Ordering) and define the rule.
- **Compatibility:** a changes nothing. b adds pricing logic.
- **Recommendation:** a for S2, with a list of affected dev-catalog provisions. I did not count them in this task.
- **Earliest scenario affected:** E5.

**C3. P-08: closure criteria.** Define the two closures as in section 8.

---

## 8. Closure recommendation

**`S2_INTERNAL_CLOSED`: not yet.** These are needed first:
1. The Owner answers group A. A1 decides whether L1 is a defect.
2. L2, L3 and L4 are resolved or explicitly accepted.
3. A `PriceChanged` test exists.
4. A live HTTP run on the current SHA covers the Offer paths (E1, the SeatMap part of E4, E5) on all three surfaces.

The internal code paths exist, and their fake-backed suites are green at 5e99a3a (section 5.4).

**`S2_OPERATIONAL_E2E_CLOSED`: not reachable now.**
- Ordering.Final has no read endpoint, no Resolve client, no ancillary OrderService and no post-creation seat add.
- The Order source is `NotConnected` (503).
- No production source exists for A08 exit-row eligibility, the A02 weight concept, A10 quotes or A19 contacts.

I made no code change and no commit. I stop here as instructed.
